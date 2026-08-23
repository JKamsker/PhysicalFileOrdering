namespace PhysicalFileOrdering.MacOS;

/// <summary>Locates FAT files through the Darwin logical-to-physical operation.</summary>
public sealed class FatFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "FAT/FAT32";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        MacOsLog2Phys.Locate(path, approximate: true);
}
