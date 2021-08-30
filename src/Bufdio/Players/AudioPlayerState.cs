namespace Bufdio.Players
{
    /// <summary>
    /// Enumeration represents audio player playback state.
    /// </summary>
    public enum AudioPlayerState
    {
        /// <summary>
        /// Player is currently playing an audio.
        /// </summary>
        Playing,
        
        /// <summary>
        /// Buffering, usually waiting for audio frame. Useful when working with remote audio file.
        /// </summary>
        Buffering,
        
        /// <summary>
        /// Player is currently suspended for writing buffers to output device.  
        /// </summary>
        Paused,
        
        /// <summary>
        /// Player has been stopped, or might be completed.
        /// </summary>
        Stopped
    }
}
