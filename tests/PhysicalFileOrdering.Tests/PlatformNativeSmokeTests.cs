using PhysicalFileOrdering.MacOS;
using PhysicalFileOrdering.Windows;

namespace PhysicalFileOrdering.Tests;

public sealed class PlatformNativeSmokeTests
{
    [Fact]
    public void WindowsResolverAndRetrievalPointersWorkOnHostedNtfs()
    {
        if (!OperatingSystem.IsWindows())
            return;

        using var file = new TemporaryFile();
        IFilePlacementProvider provider = new WindowsFileSystemResolver().Resolve(file.Path);

        Assert.IsType<NtfsFilePlacementProvider>(provider);

        FilePlacement placement = provider.Locate(file.Path);
        Assert.StartsWith("windows:", placement.VolumeId, StringComparison.Ordinal);
        Assert.NotNull(placement.Position);
    }

    [Fact]
    public void MacOsResolverAndLog2PhysWorkOnHostedApfs()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        using var file = new TemporaryFile();
        IFilePlacementProvider provider = new MacOsFileSystemResolver().Resolve(file.Path);

        Assert.IsType<ApfsFilePlacementProvider>(provider);

        FilePlacement placement = provider.Locate(file.Path);
        Assert.StartsWith("macos:", placement.VolumeId, StringComparison.Ordinal);
        Assert.NotNull(placement.Position);
        Assert.True(placement.IsApproximate);
    }

    private sealed class TemporaryFile : IDisposable
    {
        public TemporaryFile()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"PhysicalFileOrdering-{Guid.NewGuid():N}.bin");
            File.WriteAllBytes(Path, new byte[4096]);
        }

        public string Path { get; }

        public void Dispose()
        {
            try
            {
                File.Delete(Path);
            }
            catch (IOException)
            {
                // A diagnostic test failure should not be masked by cleanup.
            }
            catch (UnauthorizedAccessException)
            {
                // Windows antivirus/indexing can briefly retain a file handle.
            }
        }
    }
}
