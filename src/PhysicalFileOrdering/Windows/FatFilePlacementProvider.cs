namespace PhysicalFileOrdering.Windows;

/// <summary>Locates FAT files through Windows retrieval pointers.</summary>
public sealed class FatFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "FAT/FAT32";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        WindowsNative.LocateByRetrievalPointers(path);
}
