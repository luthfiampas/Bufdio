using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Bufdio.Decoders;
using Bufdio.Decoders.FFmpeg;
using Bufdio.Engines;
using Bufdio.Exceptions;
using Bufdio.Processors;
using Bufdio.Utilities;
using Bufdio.Utilities.Extensions;

namespace Bufdio.Players
{
    /// <summary>
    /// Provides functionalities for loading and controlling playback from given audio file.
    /// The source audio can be remote audio URL, local file and .NET Stream.
    /// <para>Implements: <see cref="IAudioPlayer"/>.</para>
    /// </summary>
    public class AudioPlayer : IAudioPlayer
    {
        private const int MinQueueSize = 3;
        private const int MaxQueueSize = 10;
        private readonly ConcurrentQueue<AudioFrame> _queue;
        private readonly VolumeProcessor _volumeProcessor;
        private readonly IEnumerable<ISampleProcessor> _customProcessors;
        private readonly IAudioEngine _engine;
        private IAudioDecoder _currentDecoder;
        private Thread _currentDecoderThread;
        private Thread _currentEngineThread;
        private bool _eof;
        private bool _seeking;
        private bool _disposed;
        
        /// <summary>
        /// Instantiate a new <see cref="AudioPlayer"/> instance.
        /// </summary>
        /// <param name="engine">An audio engine instance.</param>
        /// <param name="customProcessors">
        /// Custom sample processors to change audio samples before writing them to output device.
        /// </param>
        public AudioPlayer(IAudioEngine engine = default, IEnumerable<ISampleProcessor> customProcessors = default)
        {
            CurrentState = AudioPlayerState.Stopped;
            CurrentVolume = 1.0f;

            _engine = engine ?? new PortAudioEngine();
            _customProcessors = customProcessors;
            
            _queue = new ConcurrentQueue<AudioFrame>();
            _volumeProcessor = new VolumeProcessor(CurrentVolume);
        }

        /// <inheritdoc />
        public event EventHandler AudioLoaded;
        
        /// <inheritdoc />
        public event EventHandler StateChanged;
        
        /// <inheritdoc />
        public event EventHandler PositionChanged;
        
        /// <inheritdoc />
        public event EventHandler PlaybackCompleted;
        
        /// <inheritdoc />
        public event EventHandler<AudioPlayerLog> LogCreated;
        
        /// <inheritdoc />
        public event EventHandler<AudioFrame> FrameDecoded;
        
        /// <inheritdoc />
        public event EventHandler<AudioFrame> FramePresented;
        
        /// <inheritdoc />
        public bool IsAudioLoaded { get; private set; }
        
        /// <inheritdoc />
        public TimeSpan? TotalDuration { get; private set; }
        
        /// <inheritdoc />
        public TimeSpan CurrentPosition { get; private set; }
        
        /// <inheritdoc />
        public float CurrentVolume { get; private set; }
        
        /// <inheritdoc />
        public AudioPlayerState CurrentState { get; private set; }
        
        /// <summary>
        /// Gets or sets current audio URL.
        /// </summary>
        protected string CurrentUrl { get; set; }
        
        /// <summary>
        /// Gets or sets current audio input stream.
        /// </summary>
        protected Stream CurrentStream { get; set; }
        
        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">Thrown when given url is null.</exception>
        public void Load(string url)
        {
            LoadInternal(url, null, true);
        }

        /// <inheritdoc />
        /// <exception cref="ArgumentNullException">Thrown when given stream is null.</exception>
        public void Load(Stream stream)
        {
            LoadInternal(null, stream, false);
        }

        /// <inheritdoc />
        /// <exception cref="BufdioException">Thrown when audio is not loaded to this player.</exception>
        public void Play()
        {
            if (!IsAudioLoaded)
            {
                throw new BufdioException("This player does not have loaded audio for playback.");
            }
            
            if (CurrentState is AudioPlayerState.Playing or AudioPlayerState.Buffering)
            {
                return;
            }
            
            if (CurrentState == AudioPlayerState.Paused)
            {
                CurrentState = AudioPlayerState.Playing;
                OnStateChanged();
                return;
            }
            
            _eof = false;
            
            CurrentState = AudioPlayerState.Playing;
            OnStateChanged();
            
            _currentDecoderThread = new Thread(RunDecoder) { IsBackground = true };
            _currentEngineThread = new Thread(RunEngine) { IsBackground = true };
            _currentDecoderThread.Start();
            _currentEngineThread.Start();
        }

        /// <inheritdoc />
        public void Pause()
        {
            if (CurrentState != AudioPlayerState.Playing)
            {
                return;
            }
            
            CurrentState = AudioPlayerState.Paused;
            OnStateChanged();
        }

        /// <inheritdoc />
        public void Stop()
        {
            if (CurrentState == AudioPlayerState.Stopped)
            {
                return;
            }

            CurrentState = AudioPlayerState.Stopped;
            EnsureThreadsDone();
            OnStateChanged();
            
            Seek(TimeSpan.Zero);
        }

        /// <inheritdoc />
        public void SetVolume(float volume)
        {
            CurrentVolume = _volumeProcessor.Volume = volume.VerifyVolume();
        }

        /// <inheritdoc />
        public void Seek(TimeSpan position)
        {
            if (!IsAudioLoaded || _seeking || _currentDecoder == null)
            {
                return;
            }
            
            OnLogCreated(AudioPlayerLog.Info($"Seeking to {position}."));
            
            _seeking = true;
            
            if (!_currentDecoder.TrySeek(position, out var error))
            {
                OnLogCreated(AudioPlayerLog.Warning($"Unable to seek audio stream: {error}"));
                _seeking = false;
                return;
            }

            _queue.Clear();
            
            // Do not raise position changed event.
            // Decoder does not guarantee that audio stream will gets seeked instantly.
            CurrentPosition = position;
            OnLogCreated(AudioPlayerLog.Info("Successfully seeks."));
            
            _seeking = false;
        }

        /// <summary>
        /// Creates a new audio decoder by loading audio source from specified URL.
        /// By default this will returns a new <see cref="FFmpegDecoder"/> instance.
        /// </summary>
        /// <param name="url">Audio URL or path to audio file.</param>
        /// <returns>A new instance of <see cref="FFmpegDecoder"/>.</returns>
        protected virtual IAudioDecoder CreateDecoder(string url)
        {
            return new FFmpegDecoder(url);
        }

        /// <summary>
        /// Creates a new audio decoder by loading audio source from specified stream.
        /// By default this will returns a new <see cref="FFmpegDecoder"/> instance.
        /// </summary>
        /// <param name="stream">Source of audio stream to loads to.</param>
        /// <returns>A new instance of <see cref="FFmpegDecoder"/>.</returns>
        protected virtual IAudioDecoder CreateDecoder(Stream stream)
        {
            return new FFmpegDecoder(stream);
        }
        
        /// <summary>
        /// Executed when the decoder failed to decode audio frame.
        /// By default, this will re-instantiate current decoder and seeks to the current position.
        /// </summary>
        /// <param name="result">Failed audio decoder result.</param>
        /// <returns><c>true</c> to continue decoder thread, <c>false</c> to break.</returns>
        protected virtual bool OnDecoderFailed(AudioDecoderResult result)
        {
            _queue.Clear();

            OnLogCreated(AudioPlayerLog.Error($"Failed to decode frame: {result.ErrorMessage}. Retrying.."));
            
            _currentDecoder.Dispose();
            _currentDecoder = null;

            IsAudioLoaded = false;

            while (_currentDecoder == null)
            {
                if (CurrentState == AudioPlayerState.Stopped)
                {
                    return false;
                }

                try
                {
                    _currentDecoder = CurrentUrl != null ? CreateDecoder(CurrentUrl) : CreateDecoder(CurrentStream);

                    IsAudioLoaded = true;
                    OnAudioLoaded();
                }
                catch (FFmpegException)
                {
                    _currentDecoder = null;
                    Thread.Sleep(10);
                }
            }
            
            Seek(CurrentPosition);
            return true;
        }

        /// <summary>
        /// Invokes <see cref="AudioLoaded"/> event,
        /// </summary>
        protected virtual void OnAudioLoaded()
        {
            AudioLoaded?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes <see cref="StateChanged"/> event.
        /// </summary>
        protected virtual void OnStateChanged()
        {
            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes <see cref="PositionChanged"/> event.
        /// </summary>
        protected virtual void OnPositionChanged()
        {
            PositionChanged?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes <see cref="PlaybackCompleted"/> event.
        /// </summary>
        protected virtual void OnPlaybackCompleted()
        {
            PlaybackCompleted?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes <see cref="LogCreated"/> event.
        /// </summary>
        /// <param name="log">Log object.</param>
        protected virtual void OnLogCreated(AudioPlayerLog log)
        {
            LogCreated?.Invoke(this, log);
        }

        /// <summary>
        /// Invokes <see cref="OnFrameDecoded"/> event.
        /// </summary>
        /// <param name="frame">Decoded audio frame.</param>
        protected virtual void OnFrameDecoded(AudioFrame frame)
        {
            FrameDecoded?.Invoke(this, frame);
        }

        /// <summary>
        /// Invokes <see cref="OnFramePresented"/> event.
        /// </summary>
        /// <param name="frame">Presented audio frame.</param>
        protected virtual void OnFramePresented(AudioFrame frame)
        {
            FramePresented?.Invoke(this, frame);
        }

        private void LoadInternal(string url, Stream stream, bool useUrl)
        {
            Ensure.NotNull(useUrl ? url : stream, useUrl ? nameof(url) : nameof(stream));
            
            if (CurrentState != AudioPlayerState.Stopped)
            {
                Stop();
            }
            
            OnLogCreated(AudioPlayerLog.Info("Loading audio stream."));

            try
            {
                _currentDecoder?.Dispose();
                _currentDecoder = null;
                _currentDecoder = useUrl ? CreateDecoder(url) : CreateDecoder(stream);
                
                TotalDuration = _currentDecoder.StreamInfo.Duration;
                IsAudioLoaded = true;
                
                OnAudioLoaded();
                OnLogCreated(AudioPlayerLog.Info("Audio is loaded."));
            }
            catch (FFmpegException fex)
            {
                _currentDecoder = null;
                
                IsAudioLoaded = false;
                TotalDuration = null;

                OnLogCreated(AudioPlayerLog.Error(fex.Message));
            }
            
            _queue.Clear();
            
            CurrentPosition = TimeSpan.Zero;
            OnPositionChanged();
            
            CurrentUrl = url;
            CurrentStream = stream;
        }
        
        private void RunDecoder()
        {
            OnLogCreated(AudioPlayerLog.Info("Audio decoder thread is started."));
            
            while (CurrentState != AudioPlayerState.Stopped)
            {
                if (_seeking)
                {
                    Thread.Sleep(10);
                    continue;
                }
                
                var result = _currentDecoder.DecodeNextFrame();
                
                if (result.IsEOF)
                {
                    _eof = true;
                    break;
                }
                
                if (!result.IsSucceeded)
                {
                    if (OnDecoderFailed(result))
                    {
                        continue;
                    }

                    break;
                }
                
                while (_queue.Count >= MaxQueueSize)
                {
                    if (CurrentState == AudioPlayerState.Stopped)
                    {
                        break;
                    }
                    
                    Thread.Sleep(10);
                }
                
                RunCustomSampleProcessors(result.Frame);
                OnFrameDecoded(result.Frame);
                
                _queue.Enqueue(result.Frame);
            }
            
            OnLogCreated(AudioPlayerLog.Info("Audio decoder thread is completed."));
        }
        
        private void RunEngine()
        {
            OnLogCreated(AudioPlayerLog.Info("Audio engine thread is started."));
            
            while (CurrentState != AudioPlayerState.Stopped)
            {
                if (CurrentState == AudioPlayerState.Paused)
                {
                    Thread.Sleep(10);
                    continue;
                }
                
                if (_queue.Count < MinQueueSize)
                {
                    if (_eof)
                    {
                        break;
                    }

                    if (CurrentState != AudioPlayerState.Buffering)
                    {
                        OnLogCreated(AudioPlayerLog.Info("Insufficient queue, buferring.."));
                        CurrentState = AudioPlayerState.Buffering;
                        OnStateChanged();
                    }

                    Thread.Sleep(100);
                    continue;
                }
                
                if (!_queue.TryDequeue(out var frame))
                {
                    continue;
                }
                
                if (CurrentState != AudioPlayerState.Playing)
                {
                    CurrentState = AudioPlayerState.Playing;
                    OnStateChanged();
                }

                var samples = MemoryMarshal.Cast<byte, float>(frame.Data);

                for (var i = 0; i < samples.Length; i++)
                {
                    samples[i] = _volumeProcessor.Process(samples[i]);
                }

                _engine.Send(samples);
                OnFramePresented(frame);
                
                CurrentPosition = frame.PresentationTime.Milliseconds();
                OnPositionChanged();
            }

            if (_eof)
            {
                Seek(TimeSpan.Zero);
            }
            
            if (CurrentPosition != TimeSpan.Zero)
            {
                CurrentPosition = TimeSpan.Zero;
                OnPositionChanged();
            }
            
            if (CurrentState != AudioPlayerState.Stopped)
            {
                CurrentState = AudioPlayerState.Stopped;
                OnStateChanged();
            }

            if (_eof)
            {
                OnPlaybackCompleted();
            }
            
            OnLogCreated(AudioPlayerLog.Info("Audio engine thread is completed."));
        }
        
        private void RunCustomSampleProcessors(AudioFrame frame)
        {
            if (_customProcessors == null)
            {
                return;
            }

            var samples = MemoryMarshal.Cast<byte, float>(frame.Data);

            foreach (var processor in _customProcessors)
            {
                if (!processor.IsEnabled)
                {
                    continue;
                }

                for (var i = 0; i < samples.Length; i++)
                {
                    samples[i] = processor.Process(samples[i]);
                }
            }
        }

        private void EnsureThreadsDone()
        {
            _currentDecoderThread?.EnsureThreadDone();
            _currentDecoderThread = null;
            _currentEngineThread?.EnsureThreadDone();
            _currentEngineThread = null;
        }

        /// <inheritdoc />
        public virtual void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            CurrentState = AudioPlayerState.Stopped;
            EnsureThreadsDone();

            _engine.Dispose();
            _currentDecoder?.Dispose();
            _queue.Clear();

            GC.SuppressFinalize(this);
            
            _disposed = true;
        }
    }
}
