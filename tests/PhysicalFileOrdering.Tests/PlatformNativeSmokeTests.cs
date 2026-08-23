using System.Security.Cryptography;
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
    public void MacOsResolverAndLog2PhysCanQueryHostedApfs()
    {
        if (!OperatingSystem.IsMacOS())
            return;

        using var file = new TemporaryFile();
        IFilePlacementProvider provider = new MacOsFileSystemResolver().Resolve(file.Path);

        Assert.IsType<ApfsFilePlacementProvider>(provider);

        FilePlacement placement = provider.Locate(file.Path);
        Assert.StartsWith("macos:", placement.VolumeId, StringComparison.Ordinal);
        Assert.True(placement.IsApproximate);

        // GitHub's virtual APFS device may legitimately withhold its physical
        // address. Reaching this point still validates the native ABI and call.
    }

    private sealed class TemporaryFile : IDisposable
    {
        public TemporaryFile()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"PhysicalFileOrdering-{Guid.NewGuid():N}.bin");

            byte[] contents = RandomNumberGenerator.GetBytes(64 * 1024);
            using var stream = new FileStream(Path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            stream.Write(contents);
            stream.Flush(flushToDisk: true);
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
