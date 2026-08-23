namespace PhysicalFileOrdering.Linux;

/// <summary>Locates ext2 files through Linux FIEMAP.</summary>
public sealed class Ext2FilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "ext2";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        LinuxFiemap.Locate(path, approximate: false);
}
