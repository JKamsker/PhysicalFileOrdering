namespace PhysicalFileOrdering;

/// <summary>Resolves the placement provider that applies to a path.</summary>
public interface IFilePlacementProviderResolver
{
    /// <summary>Resolves a placement provider for a path.</summary>
    /// <param name="path">The path whose filesystem should be resolved.</param>
    /// <returns>A provider, or the fallback provider when the filesystem is unsupported.</returns>
    IFilePlacementProvider Resolve(string path);
}
