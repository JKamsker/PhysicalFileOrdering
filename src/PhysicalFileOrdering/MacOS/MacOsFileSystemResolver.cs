namespace PhysicalFileOrdering.MacOS;

/// <summary>Resolves placement providers using macOS drive metadata.</summary>
public sealed class MacOsFileSystemResolver : IFilePlacementProviderResolver
{
    private static readonly IFilePlacementProvider Apfs = new ApfsFilePlacementProvider();
    private static readonly IFilePlacementProvider HfsPlus = new HfsPlusFilePlacementProvider();
    private static readonly IFilePlacementProvider ExFat = new ExFatFilePlacementProvider();
    private static readonly IFilePlacementProvider Fat = new FatFilePlacementProvider();

    /// <inheritdoc />
    public IFilePlacementProvider Resolve(string path)
    {
        string? fs = MacOsMountInfo.Resolve(path)?.FileSystemType;

        return fs?.ToLowerInvariant() switch
        {
            "apfs" => Apfs,
            "hfs" or "hfs+" or "hfsplus" => HfsPlus,
            "exfat" => ExFat,
            "msdos" or "fat" or "fat32" => Fat,
            _ => FallbackFilePlacementProvider.Instance
        };
    }
}
