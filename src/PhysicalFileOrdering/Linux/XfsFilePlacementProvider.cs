namespace PhysicalFileOrdering.Linux;

/// <summary>Locates XFS files through Linux FIEMAP.</summary>
public sealed class XfsFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "XFS";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        LinuxFiemap.Locate(path, approximate: false);
}
