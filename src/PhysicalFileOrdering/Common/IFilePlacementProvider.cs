namespace PhysicalFileOrdering;

/// <summary>Locates files on a particular filesystem.</summary>
public interface IFilePlacementProvider
{
    /// <summary>Gets a human-readable name for the supported filesystem.</summary>
    string FileSystemName { get; }

    /// <summary>Locates the first usable storage position for a file.</summary>
    /// <param name="path">The path of the file to locate.</param>
    /// <returns>The reported file placement.</returns>
    FilePlacement Locate(string path);
}
