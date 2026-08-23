namespace PhysicalFileOrdering.Linux;

/// <summary>Locates ext3 files through Linux FIEMAP.</summary>
public sealed class Ext3FilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "ext3";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        LinuxFiemap.Locate(path, approximate: false);
}
