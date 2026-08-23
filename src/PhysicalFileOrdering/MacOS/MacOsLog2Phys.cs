using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PhysicalFileOrdering.MacOS;

internal static class MacOsLog2Phys
{
    private const int FLog2PhysExt = 65;

    // Darwin's public fcntl.h explicitly wraps struct log2phys in
    // #pragma pack(4), producing a 20-byte layout on 64-bit macOS.
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct Log2Phys
    {
        public uint Flags;
        public long ContiguousBytes;
        public long DeviceOffset;
    }

    public static FilePlacement Locate(string path, bool approximate = false)
    {
        if (!OperatingSystem.IsMacOS())
            throw new PlatformNotSupportedException();

        path = Path.GetFullPath(path);
        MacOsMount? mount = MacOsMountInfo.Resolve(path);
        string volumeId = mount is { } m
            ? "macos:" + m.MountPoint
            : VolumeIdentity.ForPath(path);

        using SafeFileHandle handle = File.OpenHandle(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        int fd = handle.DangerousGetHandle().ToInt32();
        long fileLength = RandomAccess.GetLength(handle);
        return new FilePlacement(
            volumeId,
            GetFirstDeviceOffset(fd, fileLength),
            approximate);
    }

    private static ulong? GetFirstDeviceOffset(int fd, long fileLength)
    {
        const int contigBytesOffset = 4;
        const int deviceOffsetOffset = 12;
        // The public structure is 20 bytes, but over-allocate to tolerate the
        // native operation writing ABI padding on some macOS/runtime versions.
        const int bufferSize = 32;

        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            long logicalOffset = 0;

            while (logicalOffset < fileLength)
            {
                // The two off_t fields are deliberately only 4-byte aligned.
                // Use byte-wise helpers so the runtime never emits an aligned load.
                Marshal.WriteInt32(buffer, 0, 0);
                WriteInt64Unaligned(buffer, contigBytesOffset, fileLength - logicalOffset);
                WriteInt64Unaligned(buffer, deviceOffsetOffset, logicalOffset);

                if (fcntl(fd, FLog2PhysExt, buffer) == -1)
                    return null;

                long contiguousBytes = ReadInt64Unaligned(buffer, contigBytesOffset);
                long deviceOffset = ReadInt64Unaligned(buffer, deviceOffsetOffset);
                if (contiguousBytes <= 0)
                    return null;

                if (deviceOffset >= 0)
                    return checked((ulong)deviceOffset);

                if (logicalOffset > long.MaxValue - contiguousBytes)
                    return null;

                logicalOffset += contiguousBytes;
            }

            return null;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    internal static long ReadInt64Unaligned(IntPtr buffer, int offset)
    {
        ulong value = 0;

        for (int byteIndex = 0; byteIndex < sizeof(long); byteIndex++)
        {
            int nativeIndex = BitConverter.IsLittleEndian
                ? byteIndex
                : sizeof(long) - 1 - byteIndex;
            value |= (ulong)Marshal.ReadByte(buffer, offset + nativeIndex) << (byteIndex * 8);
        }

        return unchecked((long)value);
    }

    internal static void WriteInt64Unaligned(IntPtr buffer, int offset, long value)
    {
        ulong bits = unchecked((ulong)value);

        for (int byteIndex = 0; byteIndex < sizeof(long); byteIndex++)
        {
            int nativeIndex = BitConverter.IsLittleEndian
                ? byteIndex
                : sizeof(long) - 1 - byteIndex;
            Marshal.WriteByte(
                buffer,
                offset + nativeIndex,
                unchecked((byte)(bits >> (byteIndex * 8))));
        }
    }

    // The public fcntl symbol is variadic. On Apple Silicon, its variadic third
    // argument uses a different ABI from a fixed P/Invoke argument. __fcntl is
    // libSystem's non-variadic entry point and safely accepts the native buffer.
    [DllImport("libSystem", EntryPoint = "__fcntl", SetLastError = true)]
    private static extern int fcntl(int fd, int command, IntPtr argument);
}
