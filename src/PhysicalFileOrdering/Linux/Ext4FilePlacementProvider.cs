namespace PhysicalFileOrdering.Linux;

/// <summary>Locates ext4 files through Linux FIEMAP.</summary>
public sealed class Ext4FilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "ext4";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        LinuxFiemap.Locate(path, approximate: false);
}
