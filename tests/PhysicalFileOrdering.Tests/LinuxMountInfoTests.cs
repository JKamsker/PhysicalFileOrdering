using PhysicalFileOrdering.Linux;

namespace PhysicalFileOrdering.Tests;

public sealed class LinuxMountInfoTests
{
    [Fact]
    public void ParseMountInfoReadsDeviceMountAndFileSystem()
    {
        string[] lines =
        [
            "36 25 8:1 / / rw,relatime - ext4 /dev/sda1 rw",
            "37 25 8:2 / /media/My\\040Disk rw,nosuid - xfs /dev/sdb1 rw"
        ];

        IReadOnlyList<LinuxMount> mounts = LinuxMountInfo.ParseMountInfo(lines);

        Assert.Equal(2, mounts.Count);
        Assert.Equal(new LinuxMount("8:1", "/", "ext4"), mounts[0]);
        Assert.Equal(new LinuxMount("8:2", "/media/My Disk", "xfs"), mounts[1]);
    }

    [Fact]
    public void ParseMountInfoIgnoresMalformedLines()
    {
        string[] lines =
        [
            "not mount info",
            "1 2 3 - ext4",
            ""
        ];

        Assert.Empty(LinuxMountInfo.ParseMountInfo(lines));
    }

    [Fact]
    public void ResolveChoosesLongestMountPointAtAPathBoundary()
    {
        if (!OperatingSystem.IsLinux())
            return;

        LinuxMount[] mounts =
        [
            new("1:1", "/", "ext4"),
            new("2:2", "/mnt", "xfs"),
            new("3:3", "/mnt/data", "btrfs")
        ];

        Assert.Equal("3:3", LinuxMountInfo.Resolve("/mnt/data/file.bin", mounts)?.DeviceId);
        Assert.Equal("2:2", LinuxMountInfo.Resolve("/mnt/database/file.bin", mounts)?.DeviceId);
        Assert.Equal("1:1", LinuxMountInfo.Resolve("/other/file.bin", mounts)?.DeviceId);
    }
}
