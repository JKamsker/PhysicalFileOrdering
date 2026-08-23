namespace PhysicalFileOrdering.Linux;

/// <summary>Locates Btrfs files through Linux FIEMAP.</summary>
public sealed class BtrfsFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "Btrfs";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        LinuxFiemap.Locate(path, approximate: true);
}
