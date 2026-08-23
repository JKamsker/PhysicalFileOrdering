namespace PhysicalFileOrdering.MacOS;

/// <summary>Locates HFS+ files through the Darwin logical-to-physical operation.</summary>
public sealed class HfsPlusFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "HFS+";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        MacOsLog2Phys.Locate(path, approximate: false);
}
