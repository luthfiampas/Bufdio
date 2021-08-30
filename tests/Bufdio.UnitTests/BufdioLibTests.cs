using System;
using Xunit;

namespace Bufdio.UnitTests
{
    public class BufdioLibTests
    {
        [Fact]
        public void InitializePortAudio_Throws_ArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => BufdioLib.InitializePortAudio(null));
            Assert.Equal("portAudioPath", ex.ParamName);
        }
    }
}
