namespace PhysicalFileOrdering.MacOS;

/// <summary>
/// Opt-in provider for other Darwin filesystems that implement F_LOG2PHYS_EXT.
/// It is not selected automatically because support is filesystem-specific.
/// </summary>
public sealed class GenericLog2PhysFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "F_LOG2PHYS_EXT-compatible";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        MacOsLog2Phys.Locate(path, approximate: true);
}
