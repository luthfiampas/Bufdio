using Bufdio.Engines;
using Xunit;

namespace Bufdio.UnitTests.Engines
{
    public class AudioEngineOptionsTests
    {
        [Fact]
        public void Construct_Handle_Maximum_Output_Channels()
        {
            var device = new AudioDevice(1, "", 2, 0d, 0, 0);
            var options = new AudioEngineOptions(device, 4, 44100, 0);
            Assert.Equal(2, options.Channels);
        }
    }
}
