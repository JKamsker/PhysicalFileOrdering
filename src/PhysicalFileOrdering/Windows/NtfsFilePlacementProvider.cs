namespace PhysicalFileOrdering.Windows;

/// <summary>Locates NTFS files through Windows retrieval pointers.</summary>
public sealed class NtfsFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "NTFS";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        WindowsNative.LocateByRetrievalPointers(path);
}
