namespace PhysicalFileOrdering.MacOS;

/// <summary>Locates exFAT files through the Darwin logical-to-physical operation.</summary>
public sealed class ExFatFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "exFAT";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        MacOsLog2Phys.Locate(path, approximate: true);
}
