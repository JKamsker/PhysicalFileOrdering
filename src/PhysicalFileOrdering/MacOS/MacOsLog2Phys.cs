using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PhysicalFileOrdering.MacOS;

internal static class MacOsLog2Phys
{
    private const int FLog2PhysExt = 65;

    // Darwin aligns off_t fields on 8-byte boundaries on supported 64-bit
    // platforms. The default sequential packing produces the native 24-byte
    // layout (offsets 0, 8 and 16).
    [StructLayout(LayoutKind.Sequential)]
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

        var info = new Log2Phys
        {
            Flags = 0,
            ContiguousBytes = 1,
            DeviceOffset = 0 // input: byte offset into file
        };

        if (fcntl(fd, FLog2PhysExt, ref info) == -1 || info.DeviceOffset < 0)
            return new FilePlacement(volumeId, null, approximate);

        return new FilePlacement(volumeId, checked((ulong)info.DeviceOffset), approximate);
    }

    [DllImport("libSystem.B.dylib", SetLastError = true)]
    private static extern int fcntl(int fd, int command, ref Log2Phys argument);
}
