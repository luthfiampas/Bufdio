using System;
using Bufdio.Players;
using Xunit;

namespace Bufdio.UnitTests.Players
{
    public class AudioPlayerLogTests
    {
        [Fact]
        public void Create_From_Static_Methods()
        {
            var info = AudioPlayerLog.Info("info");
            var warning = AudioPlayerLog.Warning("warning");
            var error = AudioPlayerLog.Error("error");
            
            Assert.Equal(AudioPlayerLogType.Info, info.Type);
            Assert.Equal(AudioPlayerLogType.Warning, warning.Type);
            Assert.Equal(AudioPlayerLogType.Error, error.Type);
            
            Assert.Equal("info", info.Message);
            Assert.Equal("warning", warning.Message);
            Assert.Equal("error", error.Message);
        }

        [Fact]
        public void ToString_Returns_Expected_String()
        {
            var date = DateTime.ParseExact("05/12/1996 23:00:12", "dd/MM/yyyy HH:mm:ss", null);
            var log = new AudioPlayerLog(date, "message", AudioPlayerLogType.Warning);
            Assert.Equal("[Warning] [12/05/1996 23:00:12] - message", log.ToString());
        }
    }
}
