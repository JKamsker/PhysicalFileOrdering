namespace PhysicalFileOrdering.Linux;

internal readonly record struct LinuxMount(
    string DeviceId,
    string MountPoint,
    string FileSystemType);

internal static class LinuxMountInfo
{
    private static readonly Lazy<IReadOnlyList<LinuxMount>> Mounts = new(LoadMounts);

    public static LinuxMount? Resolve(string path)
    {
        return Resolve(path, Mounts.Value);
    }

    internal static LinuxMount? Resolve(string path, IReadOnlyList<LinuxMount> mounts)
    {
        path = Path.GetFullPath(path);
        LinuxMount? best = null;

        foreach (LinuxMount mount in mounts)
        {
            if (!IsInside(path, mount.MountPoint))
                continue;

            if (best is null || mount.MountPoint.Length > best.Value.MountPoint.Length)
                best = mount;
        }

        return best;
    }

    private static IReadOnlyList<LinuxMount> LoadMounts()
    {
        IReadOnlyList<LinuxMount> parsed = Array.Empty<LinuxMount>();

        try
        {
            parsed = ParseMountInfo(File.ReadLines("/proc/self/mountinfo"));
        }
        catch
        {
            // Fall back to DriveInfo if mountinfo is unavailable.
        }

        if (parsed.Count > 0)
            return parsed;

        var fallback = new List<LinuxMount>();

        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            try
            {
                fallback.Add(new LinuxMount(
                    "mount:" + drive.RootDirectory.FullName,
                    NormalizeRoot(drive.RootDirectory.FullName),
                    drive.DriveFormat));
            }
            catch
            {
                // A mount may disappear while DriveInfo is enumerating it.
            }
        }

        return fallback;
    }

    internal static IReadOnlyList<LinuxMount> ParseMountInfo(IEnumerable<string> lines)
    {
        var result = new List<LinuxMount>();

        foreach (string line in lines)
        {
            int separator = line.IndexOf(" - ", StringComparison.Ordinal);
            if (separator < 0)
                continue;

            string[] left = line[..separator].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            string[] right = line[(separator + 3)..].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (left.Length < 5 || right.Length < 1)
                continue;

            result.Add(new LinuxMount(
                left[2], // major:minor
                NormalizeRoot(Unescape(left[4])),
                right[0]));
        }

        return result;
    }

    private static string Unescape(string value) =>
        value.Replace("\\040", " ", StringComparison.Ordinal)
             .Replace("\\011", "\t", StringComparison.Ordinal)
             .Replace("\\012", "\n", StringComparison.Ordinal)
             .Replace("\\134", "\\", StringComparison.Ordinal);

    private static string NormalizeRoot(string root)
    {
        if (root == "/")
            return root;

        return root.TrimEnd('/');
    }

    private static bool IsInside(string path, string root)
    {
        if (root == "/")
            return true;

        return path.Equals(root, StringComparison.Ordinal) ||
               (path.Length > root.Length &&
                path.StartsWith(root, StringComparison.Ordinal) &&
                path[root.Length] == '/');
    }
}
