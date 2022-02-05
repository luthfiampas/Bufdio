using System;
using System.IO;
using Bufdio.Decoders.FFmpeg;
using Xunit;

namespace Bufdio.UnitTests.Decoders.FFmpeg;

public class FFmpegDecoderTests
{
    [Fact]
    public void Construct_Null_Url_Should_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("url", () => new FFmpegDecoder((string)null));
    }

    [Fact]
    public void Construct_Null_Stream_Should_Throws_ArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>("stream", () => new FFmpegDecoder((Stream)null));
    }
}
