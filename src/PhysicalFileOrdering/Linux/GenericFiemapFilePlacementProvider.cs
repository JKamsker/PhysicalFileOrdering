namespace PhysicalFileOrdering.Linux;

/// <summary>
/// Opt-in provider for other Linux filesystems that implement FS_IOC_FIEMAP.
/// It is not selected automatically because layered/network filesystems can
/// expose mappings that are not meaningful for HDD seek ordering.
/// </summary>
public sealed class GenericFiemapFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "FIEMAP-compatible";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        LinuxFiemap.Locate(path, approximate: true);
}
