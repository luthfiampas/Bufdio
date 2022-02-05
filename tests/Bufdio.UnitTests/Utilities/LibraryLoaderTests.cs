using System;
using Bufdio.Utilities;
using Xunit;

namespace Bufdio.UnitTests.Utilities;

public class LibraryLoaderTests
{
    [Fact]
    public void Construct_Null_LibraryName_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("libraryName", () => new LibraryLoader(null));
    }
}
