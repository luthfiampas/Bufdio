using System;
using Bufdio.Utilities;
using Xunit;

namespace Bufdio.UnitTests.Utilities;

public class LoopHelperTests
{
    [Fact]
    public void While_Throws_ArgumentNullException()
    {
        var ex1 = Assert.Throws<ArgumentNullException>(() => LoopHelper.While(null, () => true));
        Assert.Equal("condition", ex1.ParamName);

        var ex2 = Assert.Throws<ArgumentNullException>(() => LoopHelper.While(() => false, null));
        Assert.Equal("breaker", ex2.ParamName);
    }
}
