namespace PhysicalFileOrdering;

/// <summary>Provides stable volume grouping when physical placement is unavailable.</summary>
public sealed class FallbackFilePlacementProvider : IFilePlacementProvider
{
    /// <summary>Gets the shared fallback provider.</summary>
    public static FallbackFilePlacementProvider Instance { get; } = new();

    /// <inheritdoc />
    public string FileSystemName => "unsupported";

    private FallbackFilePlacementProvider()
    {
    }

    /// <inheritdoc />
    public FilePlacement Locate(string path)
    {
        path = Path.GetFullPath(path);
        return new FilePlacement(VolumeIdentity.ForPath(path), null);
    }
}
