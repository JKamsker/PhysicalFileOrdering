using System.Runtime.InteropServices;
using PhysicalFileOrdering.MacOS;

namespace PhysicalFileOrdering.Tests;

public sealed class MacOsNativeLayoutTests
{
    [Fact]
    public void Log2PhysMatchesDarwin64BitLayout()
    {
        Assert.Equal(20, Marshal.SizeOf<MacOsLog2Phys.Log2Phys>());
        Assert.Equal(0, Marshal.OffsetOf<MacOsLog2Phys.Log2Phys>(nameof(MacOsLog2Phys.Log2Phys.Flags)).ToInt32());
        Assert.Equal(4, Marshal.OffsetOf<MacOsLog2Phys.Log2Phys>(nameof(MacOsLog2Phys.Log2Phys.ContiguousBytes)).ToInt32());
        Assert.Equal(12, Marshal.OffsetOf<MacOsLog2Phys.Log2Phys>(nameof(MacOsLog2Phys.Log2Phys.DeviceOffset)).ToInt32());
    }
}
