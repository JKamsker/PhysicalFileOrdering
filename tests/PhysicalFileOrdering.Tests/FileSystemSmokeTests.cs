namespace PhysicalFileOrdering.Tests;

public sealed class FileSystemSmokeTests
{
    [Fact]
    public void DefaultOrdererRetainsAllRealFiles()
    {
        using var directory = new TemporaryDirectory();
        string[] paths =
        [
            directory.CreateFile("one.bin", 4096),
            directory.CreateFile("two.bin", 8192),
            directory.CreateFile("three.bin", 2048)
        ];

        IReadOnlyList<string> result = PhysicalFileOrderers.CreateDefault().Sort(paths);

        Assert.Equal(paths.Length, result.Count);
        Assert.Equal(
            paths.Order(StringComparer.Ordinal),
            result.Order(StringComparer.Ordinal));
        Assert.All(result, path => Assert.True(Path.IsPathFullyQualified(path)));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        private readonly string _path = Path.Combine(
            Path.GetTempPath(),
            "PhysicalFileOrdering.Tests",
            Guid.NewGuid().ToString("N"));

        public TemporaryDirectory()
        {
            Directory.CreateDirectory(_path);
        }

        public string CreateFile(string name, int length)
        {
            string path = Path.Combine(_path, name);
            File.WriteAllBytes(path, new byte[length]);
            return path;
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(_path, recursive: true);
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
