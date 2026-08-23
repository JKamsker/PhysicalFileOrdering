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

    [Theory]
    [InlineData(4, 0)]
    [InlineData(4, 1234567890123456789)]
    [InlineData(12, -1)]
    [InlineData(12, long.MinValue)]
    public void PackedInt64HelpersRoundTripAtUnalignedOffsets(int offset, long expected)
    {
        IntPtr buffer = Marshal.AllocHGlobal(32);

        try
        {
            MacOsLog2Phys.WriteInt64Unaligned(buffer, offset, expected);

            Assert.Equal(expected, MacOsLog2Phys.ReadInt64Unaligned(buffer, offset));
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }
}
