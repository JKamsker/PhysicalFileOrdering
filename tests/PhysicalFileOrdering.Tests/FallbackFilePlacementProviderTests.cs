namespace PhysicalFileOrdering.Tests;

public sealed class FallbackFilePlacementProviderTests
{
    [Fact]
    public void ExposesOneSharedUnsupportedProvider()
    {
        Assert.Same(
            FallbackFilePlacementProvider.Instance,
            FallbackFilePlacementProvider.Instance);
        Assert.Equal("unsupported", FallbackFilePlacementProvider.Instance.FileSystemName);
    }

    [Fact]
    public void LocateReturnsUnknownPositionAndAStableVolume()
    {
        string path = Path.Combine(Path.GetTempPath(), "physical-ordering", "file.bin");

        FilePlacement first = FallbackFilePlacementProvider.Instance.Locate(path);
        FilePlacement second = FallbackFilePlacementProvider.Instance.Locate(path);

        Assert.False(string.IsNullOrWhiteSpace(first.VolumeId));
        Assert.StartsWith("mount:", first.VolumeId, StringComparison.Ordinal);
        Assert.Null(first.Position);
        Assert.False(first.IsApproximate);
        Assert.Equal(first, second);
    }
}
