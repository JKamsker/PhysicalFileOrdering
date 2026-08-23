namespace PhysicalFileOrdering;

/// <summary>Orders files using placement providers selected by a resolver.</summary>
public sealed class PhysicalFileOrderer : IPhysicalFileOrderer
{
    private readonly IFilePlacementProviderResolver _resolver;

    /// <summary>Initializes a new physical file orderer.</summary>
    /// <param name="resolver">The resolver used to select a provider for each file.</param>
    public PhysicalFileOrderer(IFilePlacementProviderResolver resolver)
    {
        _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Sort(IEnumerable<string> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        string[] input = files.Select(Path.GetFullPath).ToArray();
        if (input.Length < 2)
            return input;

        var rows = new List<Row>(input.Length);
        var volumeOrder = new Dictionary<string, int>(StringComparer.Ordinal);
        int nextVolumeGroup = 0;

        for (int i = 0; i < input.Length; i++)
        {
            string path = input[i];
            FilePlacement placement;

            try
            {
                placement = _resolver.Resolve(path).Locate(path);
            }
            catch
            {
                placement = FallbackFilePlacementProvider.Instance.Locate(path);
            }

            int volumeGroup;
            if (string.IsNullOrWhiteSpace(placement.VolumeId))
            {
                // Invalid volume IDs are deliberately not entered in the map. Each
                // one remains independent and cannot collide with a provider ID.
                volumeGroup = nextVolumeGroup++;
            }
            else if (!volumeOrder.TryGetValue(placement.VolumeId, out volumeGroup))
            {
                volumeGroup = nextVolumeGroup++;
                volumeOrder.Add(placement.VolumeId, volumeGroup);
            }

            rows.Add(new Row(path, i, volumeGroup, placement.Position));
        }

        rows.Sort(static (a, b) =>
        {
            int result = a.VolumeGroup.CompareTo(b.VolumeGroup);
            if (result != 0)
                return result;

            // Known physical/logical positions sort before unsupported entries.
            result = b.Position.HasValue.CompareTo(a.Position.HasValue);
            if (result != 0)
                return result;

            if (a.Position.HasValue && b.Position.HasValue)
            {
                result = a.Position.Value.CompareTo(b.Position.Value);
                if (result != 0)
                    return result;
            }

            // Stable fallback.
            return a.OriginalIndex.CompareTo(b.OriginalIndex);
        });

        return rows.Select(row => row.Path).ToArray();
    }

    private readonly record struct Row(
        string Path,
        int OriginalIndex,
        int VolumeGroup,
        ulong? Position);
}
