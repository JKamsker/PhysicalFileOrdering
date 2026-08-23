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
        const int bufferSize = 20;

        IntPtr buffer = Marshal.AllocHGlobal(bufferSize);

        try
        {
            long logicalOffset = 0;

            while (logicalOffset < fileLength)
            {
                // The two off_t fields are deliberately only 4-byte aligned.
                // Marshal.Read/WriteInt64 handle the unaligned native access.
                Marshal.WriteInt32(buffer, 0, 0);
                Marshal.WriteInt64(buffer, contigBytesOffset, fileLength - logicalOffset);
                Marshal.WriteInt64(buffer, deviceOffsetOffset, logicalOffset);

                if (fcntl(fd, FLog2PhysExt, buffer) == -1)
                    return null;

                long contiguousBytes = Marshal.ReadInt64(buffer, contigBytesOffset);
                long deviceOffset = Marshal.ReadInt64(buffer, deviceOffsetOffset);
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

    [DllImport("libSystem.B.dylib", SetLastError = true)]
    private static extern int fcntl(int fd, int command, IntPtr argument);
}
