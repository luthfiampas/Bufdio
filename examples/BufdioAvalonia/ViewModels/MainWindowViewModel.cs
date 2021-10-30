using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Bufdio;
using Bufdio.Players;
using BufdioAvalonia.Framework;
using BufdioAvalonia.Processors;
using BufdioAvalonia.Services;

namespace BufdioAvalonia.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IInputDialogService _input = new InputDialogService();
        private readonly IFileDialogService _fd = new FileDialogService();
        private readonly IAudioPlayer _player;
        private readonly EchoProcessor _echo;
        private string _title = "Bufdio Sample";
        private TimeSpan _duration;
        private TimeSpan _position;
        private int _volume = 100;
        private string _playPauseText = "Play";
        private bool _isEchoEnabled;
        private bool _isRepeatEnabled;
        private bool _isFFmpegInitialized;
        private bool _isBuferring;

        public MainWindowViewModel()
        {
            _echo = new EchoProcessor { IsEnabled = IsEchoEnabled };

            _player = new AudioPlayer(customProcessors: new[] { _echo });
            _player.AudioLoaded += OnAudioLoaded;
            _player.LogCreated += OnLogCreated;
            _player.StateChanged += OnStateChanged;
            _player.PositionChanged += OnPositionChanged;
            _player.PlaybackCompleted += OnPlaybackCompleted;

            Logs = new ObservableCollection<AudioPlayerLog>();
            OpenFileCommand = new DelegateCommand(ExecuteOpenFileCommand, CanExecuteOpenFileCommand);
            OpenUrlCommand = new DelegateCommand(ExecuteOpenUrlCommand, CanExecuteOpenUrlCommand);
            InitFFmpegCommand = new DelegateCommand(ExecuteInitFFmpegCommand, CanExecuteInitializeFFmpegCommand);
            PlayPauseCommand = new DelegateCommand(ExecutePlayPauseCommand, CanExecutePlayPauseCommand);
            StopCommand = new DelegateCommand(ExecuteStopCommand);
            ClearLogsCommand = new DelegateCommand(ExecuteClearLogsCommand);

            // FFmpeg might gets initialized in Program.cs
            IsFFmpegInitialized = BufdioLib.IsFFmpegInitialized;
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public TimeSpan Duration
        {
            get => _duration;
            set => SetProperty(ref _duration, value);
        }

        public TimeSpan Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public int Volume
        {
            get => _volume;
            set
            {
                if (SetProperty(ref _volume, value))
                {
                    _player.SetVolume(value / 100f);
                }
            }
        }

        public string PlayPauseText
        {
            get => _playPauseText;
            set => SetProperty(ref _playPauseText, value);
        }

        public bool IsEchoEnabled
        {
            get => _isEchoEnabled;
            set
            {
                if (SetProperty(ref _isEchoEnabled, value))
                {
                    _echo.IsEnabled = value;
                }
            }
        }

        public bool IsRepeatEnabled
        {
            get => _isRepeatEnabled;
            set => SetProperty(ref _isRepeatEnabled, value);
        }

        public bool IsFFmpegInitialized
        {
            get => _isFFmpegInitialized;
            set => SetProperty(ref _isFFmpegInitialized, value);
        }

        public bool IsBuferring
        {
            get => _isBuferring;
            set => SetProperty(ref _isBuferring, value);
        }

        public ObservableCollection<AudioPlayerLog> Logs { get; }

        public ICommand OpenFileCommand { get; }

        public ICommand OpenUrlCommand { get; }

        public ICommand InitFFmpegCommand { get; }

        public ICommand PlayPauseCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand ClearLogsCommand { get; }

        public async void Seek(double ms)
        {
            await Task.Run(() => _player.Seek(TimeSpan.FromMilliseconds(ms)));
        }

        public void DisposePlayer()
        {
            _player.Dispose();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            ((DelegateCommandBase)OpenFileCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommandBase)OpenUrlCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommandBase)InitFFmpegCommand)?.RaiseCanExecuteChanged();
            ((DelegateCommandBase)PlayPauseCommand)?.RaiseCanExecuteChanged();
        }

        private void OnAudioLoaded(object sender, EventArgs e)
        {
            Duration = _player.TotalDuration ?? TimeSpan.Zero;
            Position = TimeSpan.Zero;

            _player.Play();
        }

        private void OnLogCreated(object sender, AudioPlayerLog e)
        {
            Dispatcher.UIThread.InvokeAsync(() => Logs.Add(e));
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            IsBuferring = _player.CurrentState == AudioPlayerState.Buffering;
            PlayPauseText = _player.CurrentState == AudioPlayerState.Playing ? "Pause" : "Play";
        }

        private void OnPositionChanged(object sender, EventArgs e)
        {
            Position = _player.CurrentPosition;
        }

        private void OnPlaybackCompleted(object sender, EventArgs e)
        {
            if (IsRepeatEnabled)
            {
                _player.Play();
            }
        }

        private async void ExecuteOpenFileCommand()
        {
            var path = await _fd.OpenAsync();

            if (!File.Exists(path))
            {
                return;
            }

            _player.Load(path);
        }

        private bool CanExecuteOpenFileCommand()
        {
            return IsFFmpegInitialized;
        }

        private async void ExecuteOpenUrlCommand()
        {
            var url = await _input.OpenAsync("Open Audio From URL");

            if (url == null)
            {
                return;
            }

            await Task.Run(() => _player.Load(url));
        }

        private bool CanExecuteOpenUrlCommand()
        {
            return IsFFmpegInitialized;
        }

        private async void ExecuteInitFFmpegCommand()
        {
            var dir = await _input.OpenAsync("Initialize FFmpeg", "Leave this empty to use system-wide libraries");

            try
            {
                BufdioLib.InitializeFFmpeg(dir);
            }
            catch
            {
                // ignored
            }

            IsFFmpegInitialized = BufdioLib.IsFFmpegInitialized;
        }

        private bool CanExecuteInitializeFFmpegCommand()
        {
            return !IsFFmpegInitialized;
        }

        private void ExecutePlayPauseCommand()
        {
            if (!_player.IsAudioLoaded)
            {
                return;
            }

            if (_player.CurrentState is AudioPlayerState.Paused or AudioPlayerState.Stopped)
            {
                _player.Play();
            }
            else if (_player.CurrentState == AudioPlayerState.Playing)
            {
                _player.Pause();
            }
        }

        private bool CanExecutePlayPauseCommand()
        {
            return IsFFmpegInitialized && !IsBuferring;
        }

        private async void ExecuteStopCommand()
        {
            await Task.Run(_player.Stop);
        }

        private void ExecuteClearLogsCommand()
        {
            Logs.Clear();
        }
    }
}
