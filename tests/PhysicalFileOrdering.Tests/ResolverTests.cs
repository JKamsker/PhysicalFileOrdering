using PhysicalFileOrdering.Linux;
using PhysicalFileOrdering.MacOS;
using PhysicalFileOrdering.Windows;

namespace PhysicalFileOrdering.Tests;

public sealed class ResolverTests
{
    [Theory]
    [InlineData("ext2", typeof(Ext2FilePlacementProvider))]
    [InlineData("ext3", typeof(Ext3FilePlacementProvider))]
    [InlineData("ext4", typeof(Ext4FilePlacementProvider))]
    [InlineData("xfs", typeof(XfsFilePlacementProvider))]
    [InlineData("btrfs", typeof(BtrfsFilePlacementProvider))]
    [InlineData("f2fs", typeof(F2fsFilePlacementProvider))]
    public void LinuxProvidersHaveExpectedFileSystemNames(string expected, Type providerType)
    {
        var provider = (IFilePlacementProvider)Activator.CreateInstance(providerType)!;

        Assert.Equal(expected, provider.FileSystemName, ignoreCase: true);
    }

    [Fact]
    public void PlatformSpecificProvidersRejectTheWrongOperatingSystem()
    {
        string path = Path.GetFullPath("file.bin");

        if (!OperatingSystem.IsWindows())
            Assert.Throws<PlatformNotSupportedException>(() => new NtfsFilePlacementProvider().Locate(path));

        if (!OperatingSystem.IsLinux())
            Assert.Throws<PlatformNotSupportedException>(() => new Ext4FilePlacementProvider().Locate(path));

        if (!OperatingSystem.IsMacOS())
            Assert.Throws<PlatformNotSupportedException>(() => new ApfsFilePlacementProvider().Locate(path));
    }

    [Fact]
    public void DefaultFactoryReturnsAUsableOrderer()
    {
        IPhysicalFileOrderer orderer = PhysicalFileOrderers.CreateDefault();

        string path = Assert.Single(orderer.Sort(["file.bin"]));

        Assert.Equal(Path.GetFullPath("file.bin"), path);
    }
}
