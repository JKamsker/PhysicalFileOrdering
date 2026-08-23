using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PhysicalFileOrdering.Linux;

internal static class LinuxFiemap
{
    // _IOWR('f', 11, struct fiemap), where sizeof(struct fiemap) == 32.
    // This is the standard value on mainstream Linux .NET targets (x64/arm64).
    private static readonly nuint FsIocFiemap = (nuint)0xC020660B;

    private const int FiemapHeaderSize = 32;
    private const int FiemapExtentSize = 56;
    private const int ExtentCapacity = 128;
    private const int BufferSize = FiemapHeaderSize + FiemapExtentSize * ExtentCapacity;

    private const uint ExtentLast = 0x00000001;
    private const uint ExtentUnknown = 0x00000002;
    private const uint ExtentDelalloc = 0x00000004;
    private const uint ExtentEncoded = 0x00000008;
    private const uint ExtentEncrypted = 0x00000080;
    private const uint ExtentInline = 0x00000200;
    private const uint ExtentTail = 0x00000400;

    private const uint UnusableForOrdering =
        ExtentUnknown |
        ExtentDelalloc |
        ExtentEncoded |
        ExtentEncrypted |
        ExtentInline |
        ExtentTail;

    public static FilePlacement Locate(string path, bool approximate = false)
    {
        if (!OperatingSystem.IsLinux())
            throw new PlatformNotSupportedException();

        path = Path.GetFullPath(path);
        LinuxMount? mount = LinuxMountInfo.Resolve(path);
        string volumeId = mount is { } m ? "linux:" + m.DeviceId : VolumeIdentity.ForPath(path);

        using SafeFileHandle handle = File.OpenHandle(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        int fd = handle.DangerousGetHandle().ToInt32();
        return new FilePlacement(volumeId, GetFirstPhysicalOffset(fd), approximate);
    }

    private static ulong? GetFirstPhysicalOffset(int fd)
    {
        IntPtr buffer = Marshal.AllocHGlobal(BufferSize);

        try
        {
            ulong start = 0;

            while (true)
            {
                // struct fiemap
                WriteUInt64(buffer, 0, start);
                WriteUInt64(buffer, 8, ulong.MaxValue - start);
                Marshal.WriteInt32(buffer, 16, 0);              // fm_flags
                Marshal.WriteInt32(buffer, 20, 0);              // fm_mapped_extents (out)
                Marshal.WriteInt32(buffer, 24, ExtentCapacity); // fm_extent_count
                Marshal.WriteInt32(buffer, 28, 0);              // fm_reserved

                if (ioctl(fd, FsIocFiemap, buffer) < 0)
                    return null;

                int mapped = Math.Min(Marshal.ReadInt32(buffer, 20), ExtentCapacity);
                if (mapped <= 0)
                    return null;

                ulong nextStart = start;
                bool sawLast = false;

                for (int i = 0; i < mapped; i++)
                {
                    int offset = FiemapHeaderSize + i * FiemapExtentSize;
                    ulong logical = ReadUInt64(buffer, offset);
                    ulong physical = ReadUInt64(buffer, offset + 8);
                    ulong length = ReadUInt64(buffer, offset + 16);
                    uint flags = unchecked((uint)Marshal.ReadInt32(buffer, offset + 40));

                    if ((flags & UnusableForOrdering) == 0)
                        return physical;

                    if ((flags & ExtentLast) != 0)
                        sawLast = true;

                    ulong end = logical > ulong.MaxValue - length
                        ? ulong.MaxValue
                        : logical + length;

                    if (end > nextStart)
                        nextStart = end;
                }

                if (sawLast || nextStart <= start || nextStart == ulong.MaxValue)
                    return null;

                start = nextStart;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    private static ulong ReadUInt64(IntPtr pointer, int offset) =>
        unchecked((ulong)Marshal.ReadInt64(pointer, offset));

    private static void WriteUInt64(IntPtr pointer, int offset, ulong value) =>
        Marshal.WriteInt64(pointer, offset, unchecked((long)value));

    [DllImport("libc", SetLastError = true)]
    private static extern int ioctl(int fd, nuint request, IntPtr argument);
}
