using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Bufdio.Decoders;
using Bufdio.Engines;
using Bufdio.Exceptions;
using Bufdio.Players;
using Moq;
using Xunit;

namespace Bufdio.UnitTests.Players;

public class AudioPlayerTests
{
    [Fact]
    public void Construct_Throws_ArgumentNullException()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => new AudioPlayer(null));
        Assert.Equal("engine", ex.ParamName);
    }

    [Fact]
    public void Construct_Properties_Default_Value()
    {
        var player = new AudioPlayer(Mock.Of<IAudioEngine>());
        Assert.Equal(1.0f, player.Volume);
        Assert.Equal(PlaybackState.Idle, player.State);
        Assert.Equal(TimeSpan.Zero, player.Position);
        Assert.Equal(TimeSpan.Zero, player.Duration);
        Assert.False(player.IsLoaded);
    }

    [Fact]
    public async Task LoadAsync_Throws_ArgumentNullException()
    {
        var player = new AudioPlayer(Mock.Of<IAudioEngine>());

        var ex1 = await Assert.ThrowsAsync<ArgumentNullException>(async () => await player.LoadAsync((string)null));
        var ex2 = await Assert.ThrowsAsync<ArgumentNullException>(async () => await player.LoadAsync((Stream)null));

        Assert.Equal("url", ex1.ParamName);
        Assert.Equal("stream", ex2.ParamName);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LoadAsync_Dispose_Decoder_First(bool useUrl)
    {
        var player = new AudioPlayerStub(Mock.Of<IAudioEngine>());
        var decoder1 = Mock.Of<IAudioDecoder>();
        var decoder2 = Mock.Of<IAudioDecoder>();

        player.SetCurrentDecoder(decoder1);
        player.SetCreateDecoderMock(decoder2);

        if (useUrl)
        {
            await player.LoadAsync("url");
        }
        else
        {
            await player.LoadAsync(new MemoryStream());
        }

        Mock.Get(decoder1).Verify(d => d.Dispose(), Times.Once);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LoadAsync_Decoder_Created(bool useUrl)
    {
        var decoder = Mock.Of<IAudioDecoder>();
        Mock.Get(decoder).SetupGet(d => d.StreamInfo).Returns(new AudioStreamInfo(2, 44100, TimeSpan.FromSeconds(2)));

        var player = new AudioPlayerStub(Mock.Of<IAudioEngine>());
        player.SetCreateDecoderMock(decoder);

        if (useUrl)
        {
            await player.LoadAsync("url");
        }
        else
        {
            await player.LoadAsync(new MemoryStream());
        }

        Assert.True(player.IsLoaded);
        Assert.Equal(decoder.StreamInfo.Duration, player.Duration);
        Assert.Equal(TimeSpan.Zero, player.Position);
        Assert.Equal(decoder, player.GetCurrentDecoder());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task LoadAsync_Handle_Decoder_Creation_Error(bool useUrl)
    {
        var decoder = Mock.Of<IAudioDecoder>();
        Mock.Get(decoder).SetupGet(x => x.StreamInfo).Throws<Exception>();

        var player = new AudioPlayerStub(Mock.Of<IAudioEngine>());
        player.SetCreateDecoderMock(decoder);

        if (useUrl)
        {
            await player.LoadAsync("url");
        }
        else
        {
            await player.LoadAsync(new MemoryStream());
        }

        Assert.Null(player.GetCurrentDecoder());
        Assert.False(player.IsLoaded);
        Assert.Equal(TimeSpan.Zero, player.Duration);
        Assert.Equal(TimeSpan.Zero, player.Position);
    }

    [Fact]
    public void Play_Audio_Not_Loaded_Throws_BufdioException()
    {
        var player = new AudioPlayerStub(Mock.Of<IAudioEngine>());
        Assert.Throws<BufdioException>(() => player.Play());
    }

    [Fact]
    public async Task Play_Resume_When_Paused()
    {
        var player = new AudioPlayerStub(Mock.Of<IAudioEngine>());
        player.SetCreateDecoderMock(Mock.Of<IAudioDecoder>());

        await player.LoadAsync("url");

        player.SetPlaybackState(PlaybackState.Paused);
        player.Play();

        Assert.Equal(PlaybackState.Playing, player.State);
    }

    [Fact]
    public async Task Play_Play_All_Frames_And_Raise_Correct_Position()
    {
        var engine = new AudioEngineStub();
        var decoder = Mock.Of<IAudioDecoder>();
        var positions = new List<TimeSpan>();
        var results = new List<AudioDecoderResult>();
        var resultIndex = 0;

        // 100 audio frames
        for (var i = 0; i < 100; i++)
        {
            var frame = new AudioFrame((i + 1) * 1000, Array.Empty<byte>());
            results.Add(new AudioDecoderResult(frame, true, false));
        }

        // End of file
        results.Add(new AudioDecoderResult(null, true, true));

        Mock.Get(decoder).Setup(d => d.DecodeNextFrame()).Returns(() =>
        {
            var result = results[resultIndex];
            resultIndex++;
            return result;
        });

        var player = new AudioPlayerStub(engine);
        player.PositionChanged += (s, e) => positions.Add(player.Position);

        player.SetCreateDecoderMock(decoder);
        await player.LoadAsync("url");
        player.Play();

        while (player.State is PlaybackState.Playing or PlaybackState.Buffering)
        {
        }

        Mock.Get(decoder).Verify(d => d.DecodeNextFrame(), Times.Exactly(101));
        Assert.Equal(100, engine.GetSendCalledCount());
        Assert.Equal(TimeSpan.FromMilliseconds(100 * 1000), positions[^2]);
        Assert.Equal(PlaybackState.Idle, player.State);
    }

    private class AudioPlayerStub : AudioPlayer
    {
        private IAudioDecoder _createDecoderMock;

        public AudioPlayerStub(IAudioEngine engine) : base(engine)
        {
        }

        public void SetPlaybackState(PlaybackState state)
        {
            State = state;
        }

        public void SetCreateDecoderMock(IAudioDecoder decoder)
        {
            _createDecoderMock = decoder;
        }

        public IAudioDecoder GetCurrentDecoder()
        {
            return CurrentDecoder;
        }

        public void SetCurrentDecoder(IAudioDecoder decoder)
        {
            CurrentDecoder = decoder;
        }

        protected override IAudioDecoder CreateDecoder(string url)
        {
            return _createDecoderMock;
        }

        protected override IAudioDecoder CreateDecoder(Stream stream)
        {
            return _createDecoderMock;
        }
    }

    // Moq cannot work with Span<T>
    private class AudioEngineStub : IAudioEngine
    {
        private int _sendCalled;

        public void Send(Span<float> samples)
        {
            _sendCalled++;
        }

        public int GetSendCalledCount()
        {
            return _sendCalled;
        }

        public void Dispose()
        {
        }
    }
}
