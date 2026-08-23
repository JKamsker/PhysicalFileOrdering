namespace PhysicalFileOrdering.Windows;

/// <summary>Locates UDF files through Windows retrieval pointers.</summary>
public sealed class UdfFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "UDF";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        WindowsNative.LocateByRetrievalPointers(path);
}
