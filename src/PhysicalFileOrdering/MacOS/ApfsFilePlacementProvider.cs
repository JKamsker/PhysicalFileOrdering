namespace PhysicalFileOrdering.MacOS;

/// <summary>Locates APFS files through the Darwin logical-to-physical operation.</summary>
public sealed class ApfsFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "APFS";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        MacOsLog2Phys.Locate(path, approximate: true);
}
