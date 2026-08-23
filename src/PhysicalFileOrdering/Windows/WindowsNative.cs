using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace PhysicalFileOrdering.Windows;

internal static class WindowsNative
{
    private const uint FsctlGetRetrievalPointers = 0x00090073;
    private const int ErrorMoreData = 234;
    private const int OutputBufferSize = 64 * 1024;

    public static FilePlacement LocateByRetrievalPointers(string path)
    {
        if (!OperatingSystem.IsWindows())
            throw new PlatformNotSupportedException();

        path = Path.GetFullPath(path);
        string volumeId = GetVolumeId(path);

        using SafeFileHandle handle = File.OpenHandle(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete);

        return new FilePlacement(volumeId, GetFirstAllocatedLcn(handle));
    }

    public static string? GetFileSystemName(string path)
    {
        if (!OperatingSystem.IsWindows())
            return null;

        path = Path.GetFullPath(path);
        string? mountPoint = GetMountPoint(path);
        if (mountPoint is null)
            return null;

        var fsName = new char[64];

        bool ok = GetVolumeInformationW(
            mountPoint,
            null,
            0,
            out _,
            out _,
            out _,
            fsName,
            fsName.Length);

        return ok ? ReadNullTerminated(fsName) : null;
    }

    private static ulong? GetFirstAllocatedLcn(SafeFileHandle handle)
    {
        IntPtr output = Marshal.AllocHGlobal(OutputBufferSize);

        try
        {
            long startingVcn = 0;

            while (true)
            {
                bool success = DeviceIoControl(
                    handle,
                    FsctlGetRetrievalPointers,
                    ref startingVcn,
                    sizeof(long),
                    output,
                    OutputBufferSize,
                    out uint bytesReturned,
                    IntPtr.Zero);

                int error = success ? 0 : Marshal.GetLastWin32Error();
                if (!success && error != ErrorMoreData)
                    return null;

                if (bytesReturned < 16)
                    return null;

                int extentCount = Marshal.ReadInt32(output, 0);
                if (extentCount <= 0)
                    return null;

                // RETRIEVAL_POINTERS_BUFFER header is 16 bytes with default Win32 packing.
                int availableExtents = Math.Max(0, ((int)bytesReturned - 16) / 16);
                extentCount = Math.Min(extentCount, availableExtents);

                long nextStartingVcn = startingVcn;

                for (int i = 0; i < extentCount; i++)
                {
                    int offset = 16 + i * 16;
                    long nextVcn = Marshal.ReadInt64(output, offset);
                    long lcn = Marshal.ReadInt64(output, offset + 8);

                    // Negative LCN values represent sparse/unallocated regions.
                    if (lcn >= 0)
                        return checked((ulong)lcn);

                    if (nextVcn > nextStartingVcn)
                        nextStartingVcn = nextVcn;
                }

                if (success || nextStartingVcn <= startingVcn)
                    return null;

                startingVcn = nextStartingVcn;
            }
        }
        finally
        {
            Marshal.FreeHGlobal(output);
        }
    }

    private static string GetVolumeId(string path)
    {
        string? mountPoint = GetMountPoint(path);
        if (mountPoint is null)
            return "windows:" + (Path.GetPathRoot(path) ?? "unknown").ToUpperInvariant();

        var volumeName = new char[1024];
        if (GetVolumeNameForVolumeMountPointW(mountPoint, volumeName, volumeName.Length))
            return "windows:" + ReadNullTerminated(volumeName).ToUpperInvariant();

        return "windows:" + mountPoint.ToUpperInvariant();
    }

    private static string? GetMountPoint(string path)
    {
        var mountPoint = new char[1024];
        return GetVolumePathNameW(path, mountPoint, mountPoint.Length)
            ? ReadNullTerminated(mountPoint)
            : null;
    }

    private static string ReadNullTerminated(char[] buffer)
    {
        int length = Array.IndexOf(buffer, '\0');
        return new string(buffer, 0, length < 0 ? buffer.Length : length);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeviceIoControl(
        SafeFileHandle hDevice,
        uint dwIoControlCode,
        ref long lpInBuffer,
        int nInBufferSize,
        IntPtr lpOutBuffer,
        int nOutBufferSize,
        out uint lpBytesReturned,
        IntPtr lpOverlapped);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumePathNameW(
        string fileName,
        [Out] char[] volumePathName,
        int bufferLength);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumeNameForVolumeMountPointW(
        string volumeMountPoint,
        [Out] char[] volumeName,
        int bufferLength);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetVolumeInformationW(
        string rootPathName,
        [Out] char[]? volumeNameBuffer,
        int volumeNameSize,
        out uint volumeSerialNumber,
        out uint maximumComponentLength,
        out uint fileSystemFlags,
        [Out] char[] fileSystemNameBuffer,
        int fileSystemNameSize);
}
