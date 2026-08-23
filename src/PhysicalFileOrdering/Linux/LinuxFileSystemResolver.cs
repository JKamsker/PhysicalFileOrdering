namespace PhysicalFileOrdering.Linux;

/// <summary>Resolves placement providers using Linux mount metadata.</summary>
public sealed class LinuxFileSystemResolver : IFilePlacementProviderResolver
{
    private static readonly IFilePlacementProvider Ext2 = new Ext2FilePlacementProvider();
    private static readonly IFilePlacementProvider Ext3 = new Ext3FilePlacementProvider();
    private static readonly IFilePlacementProvider Ext4 = new Ext4FilePlacementProvider();
    private static readonly IFilePlacementProvider Xfs = new XfsFilePlacementProvider();
    private static readonly IFilePlacementProvider Btrfs = new BtrfsFilePlacementProvider();
    private static readonly IFilePlacementProvider F2fs = new F2fsFilePlacementProvider();

    /// <inheritdoc />
    public IFilePlacementProvider Resolve(string path)
    {
        string? fs = LinuxMountInfo.Resolve(path)?.FileSystemType;

        return fs?.ToLowerInvariant() switch
        {
            "ext2" => Ext2,
            "ext3" => Ext3,
            "ext4" => Ext4,
            "xfs" => Xfs,
            "btrfs" => Btrfs,
            "f2fs" => F2fs,
            _ => FallbackFilePlacementProvider.Instance
        };
    }
}
