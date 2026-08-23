namespace PhysicalFileOrdering.MacOS;

internal readonly record struct MacOsMount(
    string MountPoint,
    string FileSystemType);

internal static class MacOsMountInfo
{
    private static readonly Lazy<IReadOnlyList<MacOsMount>> Mounts = new(LoadMounts);

    public static MacOsMount? Resolve(string path)
    {
        path = Path.GetFullPath(path);
        MacOsMount? best = null;

        foreach (MacOsMount mount in Mounts.Value)
        {
            if (!IsInside(path, mount.MountPoint))
                continue;

            if (best is null || mount.MountPoint.Length > best.Value.MountPoint.Length)
                best = mount;
        }

        return best;
    }

    private static List<MacOsMount> LoadMounts()
    {
        var result = new List<MacOsMount>();

        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            try
            {
                result.Add(new MacOsMount(
                    NormalizeRoot(Path.GetFullPath(drive.RootDirectory.FullName)),
                    drive.DriveFormat));
            }
            catch
            {
            }
        }

        return result;
    }

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
