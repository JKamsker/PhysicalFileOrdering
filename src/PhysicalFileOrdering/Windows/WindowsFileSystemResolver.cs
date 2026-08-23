namespace PhysicalFileOrdering.Windows;

/// <summary>Resolves placement providers using Windows volume metadata.</summary>
public sealed class WindowsFileSystemResolver : IFilePlacementProviderResolver
{
    private static readonly IFilePlacementProvider Ntfs = new NtfsFilePlacementProvider();
    private static readonly IFilePlacementProvider Refs = new RefsFilePlacementProvider();
    private static readonly IFilePlacementProvider Fat = new FatFilePlacementProvider();
    private static readonly IFilePlacementProvider ExFat = new ExFatFilePlacementProvider();
    private static readonly IFilePlacementProvider Udf = new UdfFilePlacementProvider();

    /// <inheritdoc />
    public IFilePlacementProvider Resolve(string path)
    {
        string? fs = WindowsNative.GetFileSystemName(path);

        return fs?.ToUpperInvariant() switch
        {
            "NTFS" => Ntfs,
            "REFS" => Refs,
            "FAT" or "FAT32" => Fat,
            "EXFAT" => ExFat,
            "UDF" => Udf,
            _ => FallbackFilePlacementProvider.Instance
        };
    }
}
