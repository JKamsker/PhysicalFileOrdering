using PhysicalFileOrdering.Linux;
using PhysicalFileOrdering.MacOS;
using PhysicalFileOrdering.Windows;

namespace PhysicalFileOrdering;

/// <summary>Creates physical file orderers for common configurations.</summary>
public static class PhysicalFileOrderers
{
    /// <summary>Creates an orderer using the resolver for the current operating system.</summary>
    /// <returns>A platform-appropriate physical file orderer.</returns>
    public static IPhysicalFileOrderer CreateDefault()
    {
        IFilePlacementProviderResolver resolver = OperatingSystem.IsWindows()
            ? new WindowsFileSystemResolver()
            : OperatingSystem.IsLinux()
                ? new LinuxFileSystemResolver()
                : OperatingSystem.IsMacOS()
                    ? new MacOsFileSystemResolver()
                    : new FallbackResolver();

        return new PhysicalFileOrderer(resolver);
    }

    private sealed class FallbackResolver : IFilePlacementProviderResolver
    {
        public IFilePlacementProvider Resolve(string path) =>
            FallbackFilePlacementProvider.Instance;
    }
}
