using Bufdio.Utilities.Extensions;
using Xunit;

namespace Bufdio.UnitTests.Utilities.Extensions;

public class PortAudioExtensionsTests
{
    [Theory]
    [InlineData(0, false)]
    [InlineData(-1, true)]
    [InlineData(int.MaxValue, false)]
    public static void PaIsError_Should_Returns_Expected_Value(int code, bool expected)
    {
        Assert.Equal(expected, code.PaIsError());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public static void PaGuard_Should_Returns_Given_Value(int code)
    {
        Assert.Equal(code, code.PaGuard());
    }
}
