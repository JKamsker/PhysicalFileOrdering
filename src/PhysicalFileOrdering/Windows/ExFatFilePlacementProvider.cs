namespace PhysicalFileOrdering.Windows;

/// <summary>Locates exFAT files through Windows retrieval pointers.</summary>
public sealed class ExFatFilePlacementProvider : IFilePlacementProvider
{
    /// <inheritdoc />
    public string FileSystemName => "exFAT";

    /// <inheritdoc />
    public FilePlacement Locate(string path) =>
        WindowsNative.LocateByRetrievalPointers(path);
}
