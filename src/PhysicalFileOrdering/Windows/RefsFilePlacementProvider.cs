namespace PhysicalFileOrdering.Windows;

/// <summary>Locates ReFS files through Windows retrieval pointers.</summary>
public sealed class RefsFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "ReFS";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        WindowsNative.LocateByRetrievalPointers(path);
}
