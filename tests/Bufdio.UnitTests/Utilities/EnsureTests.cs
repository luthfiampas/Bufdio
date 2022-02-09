using System;
using Bufdio.Utilities;
using Xunit;

namespace Bufdio.UnitTests.Utilities;

public class EnsureTests
{
    [Fact]
    public void That_Condition_Is_True_Should_Not_Throws()
    {
        Ensure.That<ArgumentException>(true);
    }

    [Fact]
    public void That_Condition_Is_False_Should_Throws_Correct_Exception_Type_With_Default_Exception_Message()
    {
        var ex = Assert.Throws<InvalidCastException>(() => Ensure.That<InvalidCastException>(false));
        Assert.Equal(new InvalidCastException().Message, ex.Message);
    }

    [Fact]
    public void That_Condition_Is_False_Should_Throws_Correct_Exception_Type_With_Specified_Exception_Message()
    {
        const string message = "message";

        var ex = Assert.Throws<InvalidCastException>(() => Ensure.That<InvalidCastException>(false, message));
        Assert.Equal(message, ex.Message);
    }

    [Fact]
    public void NotNull_Argument_Not_Null_Should_Not_Throws()
    {
        Ensure.NotNull("abc", "param");
    }

    [Fact]
    public void NotNull_Null_Argument_Should_Throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => Ensure.NotNull((string)null, "param"));
        Assert.Equal("param", ex.ParamName);
    }
}
