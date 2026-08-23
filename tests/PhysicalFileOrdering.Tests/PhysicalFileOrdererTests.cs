namespace PhysicalFileOrdering.Tests;

public sealed class PhysicalFileOrdererTests
{
    [Fact]
    public void ConstructorRejectsNullResolver()
    {
        Assert.Throws<ArgumentNullException>(() => new PhysicalFileOrderer(null!));
    }

    [Fact]
    public void SortRejectsNullInput()
    {
        var orderer = new PhysicalFileOrderer(new DelegateResolver(_ =>
            new DelegateProvider(_ => default)));

        Assert.Throws<ArgumentNullException>(() => orderer.Sort(null!));
    }

    [Fact]
    public void SortReturnsEmptyInput()
    {
        var orderer = new PhysicalFileOrderer(new DelegateResolver(_ =>
            throw new InvalidOperationException("Resolver should not be called.")));

        Assert.Empty(orderer.Sort(Array.Empty<string>()));
    }

    [Fact]
    public void SinglePathIsNormalizedWithoutResolvingPlacement()
    {
        string relative = Path.Combine("fixtures", "one.bin");
        var orderer = new PhysicalFileOrderer(new DelegateResolver(_ =>
            throw new InvalidOperationException("Resolver should not be called.")));

        string result = Assert.Single(orderer.Sort([relative]));

        Assert.Equal(Path.GetFullPath(relative), result);
    }

    [Fact]
    public void SortsKnownPositionsWithinAVolume()
    {
        string[] paths = Paths("third.bin", "first.bin", "second.bin");
        var placements = new Dictionary<string, FilePlacement>
        {
            [paths[0]] = new("volume-a", 300),
            [paths[1]] = new("volume-a", 100),
            [paths[2]] = new("volume-a", 200)
        };

        IReadOnlyList<string> result = CreateOrderer(placements).Sort(paths);

        Assert.Equal([paths[1], paths[2], paths[0]], result);
    }

    [Fact]
    public void KeepsVolumesInFirstSeenOrder()
    {
        string[] paths = Paths("b2.bin", "a2.bin", "b1.bin", "a1.bin");
        var placements = new Dictionary<string, FilePlacement>
        {
            [paths[0]] = new("volume-b", 20),
            [paths[1]] = new("volume-a", 20),
            [paths[2]] = new("volume-b", 10),
            [paths[3]] = new("volume-a", 10)
        };

        IReadOnlyList<string> result = CreateOrderer(placements).Sort(paths);

        Assert.Equal([paths[2], paths[0], paths[3], paths[1]], result);
    }

    [Fact]
    public void PlacesKnownPositionsBeforeUnknownPositionsStably()
    {
        string[] paths = Paths("unknown-1.bin", "known-2.bin", "unknown-2.bin", "known-1.bin");
        var placements = new Dictionary<string, FilePlacement>
        {
            [paths[0]] = new("volume-a", null),
            [paths[1]] = new("volume-a", 20),
            [paths[2]] = new("volume-a", null),
            [paths[3]] = new("volume-a", 10)
        };

        IReadOnlyList<string> result = CreateOrderer(placements).Sort(paths);

        Assert.Equal([paths[3], paths[1], paths[0], paths[2]], result);
    }

    [Fact]
    public void PreservesInputOrderWhenKnownPositionsAreEqual()
    {
        string[] paths = Paths("one.bin", "two.bin", "three.bin");
        var placements = paths.ToDictionary(
            path => path,
            _ => new FilePlacement("volume-a", 42));

        IReadOnlyList<string> result = CreateOrderer(placements).Sort(paths);

        Assert.Equal(paths, result);
    }

    [Fact]
    public void BlankVolumeIdCannotCollideWithProviderVolumeId()
    {
        string[] paths = Paths("blank.bin", "named.bin");
        var placements = new Dictionary<string, FilePlacement>
        {
            [paths[0]] = new("", 100),
            [paths[1]] = new("unknown:0", 1)
        };

        IReadOnlyList<string> result = CreateOrderer(placements).Sort(paths);

        Assert.Equal(paths, result);
    }

    [Fact]
    public void ResolverFailureFallsBackWithoutDiscardingAPath()
    {
        string[] paths = Paths("one.bin", "two.bin");
        var orderer = new PhysicalFileOrderer(new DelegateResolver(_ =>
            throw new IOException("Synthetic resolver failure.")));

        IReadOnlyList<string> result = orderer.Sort(paths);

        Assert.Equal(paths, result);
    }

    [Fact]
    public void ProviderFailureFallsBackWithoutDiscardingAPath()
    {
        string[] paths = Paths("one.bin", "two.bin");
        var orderer = new PhysicalFileOrderer(new DelegateResolver(_ =>
            new DelegateProvider(_ => throw new UnauthorizedAccessException())));

        IReadOnlyList<string> result = orderer.Sort(paths);

        Assert.Equal(paths, result);
    }

    [Fact]
    public void EnumeratesInputOnlyOnce()
    {
        string[] paths = Paths("one.bin", "two.bin");
        int enumerationCount = 0;

        IEnumerable<string> Input()
        {
            enumerationCount++;
            foreach (string path in paths)
                yield return path;
        }

        var placements = paths.ToDictionary(
            path => path,
            _ => new FilePlacement("volume-a", 1));

        CreateOrderer(placements).Sort(Input());

        Assert.Equal(1, enumerationCount);
    }

    private static PhysicalFileOrderer CreateOrderer(
        Dictionary<string, FilePlacement> placements)
    {
        var provider = new DelegateProvider(path => placements[path]);
        return new PhysicalFileOrderer(new DelegateResolver(_ => provider));
    }

    private static string[] Paths(params string[] names) =>
        names.Select(name => Path.GetFullPath(Path.Combine("fixtures", name))).ToArray();

    private sealed class DelegateResolver(
        Func<string, IFilePlacementProvider> resolve) : IFilePlacementProviderResolver
    {
        public IFilePlacementProvider Resolve(string path) => resolve(path);
    }

    private sealed class DelegateProvider(
        Func<string, FilePlacement> locate) : IFilePlacementProvider
    {
        public string FileSystemName => "test";

        public FilePlacement Locate(string path) => locate(path);
    }
}
