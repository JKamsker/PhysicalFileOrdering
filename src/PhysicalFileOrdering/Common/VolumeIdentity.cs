namespace PhysicalFileOrdering;

internal static class VolumeIdentity
{
    public static string ForPath(string path)
    {
        path = Path.GetFullPath(path);

        if (OperatingSystem.IsWindows())
        {
            return "mount:" + (Path.GetPathRoot(path) ?? "unknown").ToUpperInvariant();
        }

        string? best = null;

        foreach (DriveInfo drive in DriveInfo.GetDrives())
        {
            try
            {
                string root = NormalizeRoot(Path.GetFullPath(drive.RootDirectory.FullName));

                if (IsInside(path, root) && (best is null || root.Length > best.Length))
                    best = root;
            }
            catch
            {
                // Mount may have disappeared or may not be readable.
            }
        }

        return best is null ? "mount:unknown" : "mount:" + best;
    }

    private static string NormalizeRoot(string root)
    {
        string slash = Path.DirectorySeparatorChar.ToString();
        if (root == slash)
            return root;

        return root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    private static bool IsInside(string path, string root)
    {
        string slash = Path.DirectorySeparatorChar.ToString();
        if (root == slash)
            return true;

        if (path.Equals(root, StringComparison.Ordinal))
            return true;

        return path.Length > root.Length &&
               path.StartsWith(root, StringComparison.Ordinal) &&
               path[root.Length] == Path.DirectorySeparatorChar;
    }
}
