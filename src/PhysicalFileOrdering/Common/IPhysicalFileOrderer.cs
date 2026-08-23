namespace PhysicalFileOrdering;

/// <summary>Orders files using host-reported storage positions.</summary>
public interface IPhysicalFileOrderer
{
    /// <summary>Orders file paths by volume and first usable storage position.</summary>
    /// <param name="files">The file paths to order.</param>
    /// <returns>Normalized absolute paths in the resulting order.</returns>
    IReadOnlyList<string> Sort(IEnumerable<string> files);
}
