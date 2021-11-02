using System.Collections.Generic;

namespace Bufdio.Decoders.FFmpeg
{
    /// <summary>
    /// Options for decoding (and, or) resampling specified audio source that can be passed
    /// through <see cref="FFmpegDecoder"/> class. This class cannot be inherited.
    /// </summary>
    public sealed class FFmpegDecoderOptions
    {
        /// <summary>
        /// Instantiate a new <see cref="FFmpegDecoderOptions"/>.
        /// </summary>
        /// <param name="channels">Desired audio channel count.</param>
        /// <param name="sampleRate">Desired audio sample rate.</param>
        /// <param name="demuxerOptions">Options passed to the demuxer.</param>
        public FFmpegDecoderOptions(int channels = 2, int sampleRate = 44100, IReadOnlyDictionary<string, string> demuxerOptions = null)
        {
            Channels = channels;
            SampleRate = sampleRate;
            DemuxerOptions = demuxerOptions;
        }

        /// <summary>
        /// Gets destination audio channel count.
        /// </summary>
        public int Channels { get; }
        
        /// <summary>
        /// Gets destination audio sample rate.
        /// </summary>
        public int SampleRate { get; }

        /// <summary>
        /// Gets the options which are passed to the demuxer.
        /// </summary>
        public IReadOnlyDictionary<string, string> DemuxerOptions { get; }
    }
}
