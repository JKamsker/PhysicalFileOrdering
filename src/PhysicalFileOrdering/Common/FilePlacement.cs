namespace PhysicalFileOrdering;

/// <summary>Describes the first usable storage position reported for a file.</summary>
/// <param name="VolumeId">An identity whose positions are comparable with each other.</param>
/// <param name="Position">The reported position, or <see langword="null"/> when unavailable.</param>
/// <param name="IsApproximate">Whether the position is only a best-effort approximation.</param>
public readonly record struct FilePlacement(
    string VolumeId,
    ulong? Position,
    bool IsApproximate = false);
