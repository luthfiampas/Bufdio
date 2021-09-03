using Bufdio.Decoders;
using Bufdio.Engines;
using Bufdio.Exceptions;
using Bufdio.Players;
using Bufdio.Processors;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace Bufdio.UnitTests.Players
{
    public class AudioPlayerTests
    {
        [Fact]
        public void Load_Throws_ArgumentNullException()
        {
            var player = new AudioPlayer(Mock.Of<IAudioEngine>());
            var ex1 = Assert.Throws<ArgumentNullException>(() => player.Load((string)null));
            var ex2 = Assert.Throws<ArgumentNullException>(() => player.Load((Stream)null));

            Assert.Equal("url", ex1.ParamName);
            Assert.Equal("stream", ex2.ParamName);
        }

        [Fact]
        public void Load_Loaded_And_Have_Correct_Duration()
        {
            var player = new AudioPlayerStub(Mock.Of<IAudioEngine>());
            var streamInfo = new AudioStreamInfo(2, 44100, TimeSpan.FromMinutes(4.5));
            Mock.Get(player.Decoder).SetupGet(x => x.StreamInfo).Returns(streamInfo);

            player.Load("audio-url");

            Assert.True(player.IsAudioLoaded);
            Assert.Equal(TimeSpan.FromMinutes(4.5), player.TotalDuration);
        }

        [Fact]
        public void Play_Does_Not_Have_Loaded_Audio_Throws_BufdioException()
        {
            var player = new AudioPlayerStub(Mock.Of<IAudioEngine>());
            var ex = Assert.Throws<BufdioException>(() => player.Play());
            Assert.Contains("loaded audio", ex.Message);
        }

        private class AudioPlayerStub : AudioPlayer
        {
            public AudioPlayerStub(IAudioEngine engine, IEnumerable<ISampleProcessor> processors = default) : base(engine, processors)
            {
                Decoder = Mock.Of<IAudioDecoder>();
            }

            public IAudioDecoder Decoder { get; }

            protected override IAudioDecoder CreateDecoder(string url)
            {
                return Decoder;
            }

            protected override IAudioDecoder CreateDecoder(Stream stream)
            {
                return Decoder;
            }
        }
    }
}
