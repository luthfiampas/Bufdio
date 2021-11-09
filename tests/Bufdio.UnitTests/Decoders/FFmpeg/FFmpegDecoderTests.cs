using System;
using System.IO;
using Bufdio.Decoders.FFmpeg;
using Xunit;

namespace Bufdio.UnitTests.Decoders.FFmpeg;

public class FFmpegDecoderTests
{
    [Fact]
    public void Construct_Throws_ArgumentNullException()
    {
        var ex1 = Assert.Throws<ArgumentNullException>(() => new FFmpegDecoder((string)null));
        var ex2 = Assert.Throws<ArgumentNullException>(() => new FFmpegDecoder((Stream)null));

        Assert.Equal("url", ex1.ParamName);
        Assert.Equal("stream", ex2.ParamName);
    }
}
