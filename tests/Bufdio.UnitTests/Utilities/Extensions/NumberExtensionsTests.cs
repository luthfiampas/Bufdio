using Bufdio.Utilities.Extensions;
using Xunit;

namespace Bufdio.UnitTests.Utilities.Extensions;

public class NumberExtensionsTests
{
    [Fact]
    public void Milliseconds_Should_Returns_Expected_Value()
    {
        var ts = 10.00.Milliseconds();
        Assert.Equal(10.00, ts.TotalMilliseconds);
    }

    [Theory]
    [InlineData(-0.5f, 0.0f)]
    [InlineData(2.5f, 1.0f)]
    [InlineData(1.0f, 1.0f)]
    [InlineData(0.08f, 0.08f)]
    public void VerifyVolume_Should_Returns_Expected_Value(float volume, float expected)
    {
        Assert.Equal(expected, volume.VerifyVolume());
    }
}
