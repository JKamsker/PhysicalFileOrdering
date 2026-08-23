namespace PhysicalFileOrdering.Linux;

/// <summary>Locates F2FS files through Linux FIEMAP.</summary>
public sealed class F2fsFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "F2FS";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        LinuxFiemap.Locate(path, approximate: false);
}
