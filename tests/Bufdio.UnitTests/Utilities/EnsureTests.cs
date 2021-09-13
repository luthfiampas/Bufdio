using System;
using Bufdio.Utilities;
using Xunit;

namespace Bufdio.UnitTests.Utilities
{
    public class EnsureTests
    {
        [Fact]
        public void That_Throws_Correct_Exception_Type_With_Correct_Exception_Message()
        {
            var ex1 = Assert.Throws<InvalidCastException>(() => Ensure.That<InvalidCastException>(false, "invalid"));
            var ex2 = Assert.Throws<OverflowException>(() => Ensure.That<OverflowException>(false));
            
            Assert.Equal("invalid", ex1.Message);
            Assert.Equal("", ex2.Message);
        }
        
        [Fact]
        public void NotNull_Works_Properly()
        {
            // Should not throws
            Ensure.NotNull("abc", "param");
            
            // Should throws
            var ex = Assert.Throws<ArgumentNullException>(() => Ensure.NotNull(null, "param"));
            Assert.Equal("param", ex.ParamName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void NotNull_Use_Default_ParamName(string paramName)
        {
            var ex = Assert.Throws<ArgumentNullException>(() => Ensure.NotNull(null, paramName));
            Assert.Equal("argument", ex.ParamName);
        }
    }
}
