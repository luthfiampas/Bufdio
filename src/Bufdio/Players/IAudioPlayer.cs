using System;
using System.IO;

namespace Bufdio.Players
{
    /// <summary>
    /// An interface that provides functionalities for loading and controlling playback from given audio file.
    /// <para>Implements: <see cref="IDisposable"/>.</para>
    /// </summary>
    public interface IAudioPlayer : IDisposable
    {
        /// <summary>
        /// Event that is raised when specified audio source has been loaded.
        /// </summary>
        event EventHandler AudioLoaded;
        
        /// <summary>
        /// Event that is raised when playback state has been changed.
        /// </summary>
        event EventHandler StateChanged;

        /// <summary>
        /// Event that is raised when the current playback position has been changed.
        /// </summary>
        event EventHandler PositionChanged;

        /// <summary>
        /// Event that is raised only when the decoder reach end-of-file.
        /// </summary>
        event EventHandler PlaybackCompleted;

        /// <summary>
        /// Event that is raised when the player create a log object.
        /// </summary>
        event EventHandler<AudioPlayerLog> LogCreated;

        /// <summary>
        /// Event that is raised when audio frame is decoded and ready for queue.
        /// </summary>
        event EventHandler<AudioFrame> FrameDecoded;

        /// <summary>
        /// Event that is raised when audio frame has been written to output device.
        /// </summary>
        event EventHandler<AudioFrame> FramePresented;

        /// <summary>
        /// Gets whether or not the player have loaded audio for playback.
        /// </summary>
        bool IsAudioLoaded { get; }
        
        /// <summary>
        /// Gets total duration of loaded audio file.
        /// Should returns <c>null</c> if <see cref="IsAudioLoaded"/> is <c>false</c>.
        /// </summary>
        TimeSpan? TotalDuration { get; }
        
        /// <summary>
        /// Gets current playback position.
        /// </summary>
        TimeSpan CurrentPosition { get; }
        
        /// <summary>
        /// Gets current audio volume, to change the audio volume, use <see cref="SetVolume"/>.
        /// </summary>
        float CurrentVolume { get; }
        
        /// <summary>
        /// Gets current playback state.
        /// </summary>
        AudioPlayerState CurrentState { get; }

        /// <summary>
        /// Loads audio from specified URL to the player. The URL might be HTTP URL or local file path.
        /// </summary>
        /// <param name="url">Audio URL or audio file path.</param>
        void Load(string url);

        /// <summary>
        /// Loads audio stream to the player.
        /// </summary>
        /// <param name="stream">Source audio stream.</param>
        void Load(Stream stream);

        /// <summary>
        /// Starts audio playback.
        /// </summary>
        void Play();

        /// <summary>
        /// Pause or suspends the player for writing buffers or sample to output device.
        /// </summary>
        void Pause();

        /// <summary>
        /// Stop the playback, this should not invokes <see cref="PlaybackCompleted"/> event.
        /// </summary>
        void Stop();

        /// <summary>
        /// Sets the audio volume. Volume range should between 0f to 1f.
        /// </summary>
        /// <param name="volume">Desired audio volume.</param>
        void SetVolume(float volume);

        /// <summary>
        /// Seeks loaded audio to the specified position.
        /// </summary>
        /// <param name="position">Desired seek position.</param>
        void Seek(TimeSpan position);
    }
}
