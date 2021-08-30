using System;
using Bufdio.Utilities;
using Xunit;

namespace Bufdio.UnitTests.Utilities
{
    public class LibraryLoaderTests
    {
        [Fact]
        public void Construct_Throws_ArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new LibraryLoader(null));
            Assert.Equal("libraryName", ex.ParamName);
        }
    }
}
