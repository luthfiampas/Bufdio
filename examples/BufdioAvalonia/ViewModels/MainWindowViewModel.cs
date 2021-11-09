using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using Bufdio;
using Bufdio.Common;
using Bufdio.Players;
using BufdioAvalonia.Common;
using BufdioAvalonia.Framework;
using BufdioAvalonia.Processors;
using BufdioAvalonia.Services;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace BufdioAvalonia.ViewModels;

public class MainWindowViewModel : BindableBase, ILogger
{
    private readonly YoutubeClient _youtube = new YoutubeClient();
    private readonly IInputDialogService _input = new InputDialogService();
    private readonly IFileDialogService _fd = new FileDialogService();
    private readonly IAudioPlayer _player;
    private readonly EchoProcessor _echo;
    private bool _isStopRequested;
    private string _title;
    private TimeSpan _duration;
    private TimeSpan _position;
    private int _volume = 100;
    private string _playPauseText;
    private bool _isEchoEnabled;
    private bool _isRepeatEnabled;
    private bool _isFFmpegInitialized;
    private bool _isBuferring;

    public MainWindowViewModel()
    {
        _echo = new EchoProcessor { IsEnabled = IsEchoEnabled };
        _player = new AudioPlayer { CustomSampleProcessor = _echo, Logger = this };

        _player.StateChanged += OnStateChanged;
        _player.PositionChanged += OnPositionChanged;

        Title = "Bufdio Sample";
        PlayPauseText = "Play";

        Logs = new ObservableCollection<Log>();
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
                _player.Volume = value / 100f;
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

    public ObservableCollection<Log> Logs { get; }

    public ICommand OpenFileCommand { get; }

    public ICommand OpenUrlCommand { get; }

    public ICommand InitFFmpegCommand { get; }

    public ICommand PlayPauseCommand { get; }

    public ICommand StopCommand { get; }

    public ICommand ClearLogsCommand { get; }

    public void LogInfo(string message)
    {
        Dispatcher.UIThread.InvokeAsync(() => Logs.Add(new Log(message, Log.LogType.Info)));
    }

    public void LogWarning(string message)
    {
        Dispatcher.UIThread.InvokeAsync(() => Logs.Add(new Log(message, Log.LogType.Warning)));
    }

    public void LogError(string message)
    {
        Dispatcher.UIThread.InvokeAsync(() => Logs.Add(new Log(message, Log.LogType.Error)));
    }

    public void Seek(double ms)
    {
        if (_player.IsLoaded)
        {
            Task.Run(() => _player.Seek(TimeSpan.FromMilliseconds(ms)));
        }
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

    private void OnStateChanged(object sender, EventArgs e)
    {
        IsBuferring = _player.State == PlaybackState.Buffering;
        PlayPauseText = _player.State is PlaybackState.Playing or PlaybackState.Buffering ? "Pause" : "Play";

        if (!_isStopRequested && _player.State == PlaybackState.Idle && IsRepeatEnabled)
        {
            _player.Play();
        }
    }

    private void OnPositionChanged(object sender, EventArgs e)
    {
        // Prevent high CPU usage
        // https://github.com/AvaloniaUI/Avalonia/issues/2012

        if (_player.IsSeeking)
        {
            return;
        }

        if ((_player.Position - Position).TotalSeconds > 1 || Position > _player.Position)
        {
            Position = _player.Position;
        }
    }

    private async void ExecuteOpenFileCommand()
    {
        var path = await _fd.OpenAsync();

        if (!File.Exists(path))
        {
            return;
        }

        _isStopRequested = true;
        _player.Stop();

        if (!await _player.LoadAsync(path))
        {
            return;
        }

        Title = $"Bufdio Sample: {path}";
        Duration = _player.Duration;
        Position = TimeSpan.Zero;

        _player.Play();
        _isStopRequested = false;
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

        _isStopRequested = true;
        _player.Stop();

        var title = $"Bufdio Sample: {url}";
        var isyt = url.Contains("//youtu.be/", StringComparison.OrdinalIgnoreCase) ||
                   url.Contains("youtube.com/watch?", StringComparison.OrdinalIgnoreCase);

        if (isyt)
        {
            LogInfo("Loading YouTube URL.");

            try
            {
                var manifest = await _youtube.Videos.Streams.GetManifestAsync(VideoId.Parse(url));
                url = manifest.GetAudioOnlyStreams().GetWithHighestBitrate().Url;
            }
            catch (Exception ex)
            {
                LogError($"Error loading Youtube URL: {ex.Message}");
                return;
            }
        }

        if (!await _player.LoadAsync(url))
        {
            return;
        }

        Title = title;
        Duration = _player.Duration;
        Position = TimeSpan.Zero;

        _player.Play();
        _isStopRequested = false;
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
        catch (Exception ex)
        {
            LogInfo($"Cannot initialize FFmpeg: {ex.Message}");
        }

        IsFFmpegInitialized = BufdioLib.IsFFmpegInitialized;
    }

    private bool CanExecuteInitializeFFmpegCommand()
    {
        return !IsFFmpegInitialized;
    }

    private void ExecutePlayPauseCommand()
    {
        if (!_player.IsLoaded)
        {
            return;
        }

        _isStopRequested = false;

        if (_player.State is PlaybackState.Paused or PlaybackState.Idle)
        {
            _player.Play();
        }
        else
        {
            _player.Pause();
        }
    }

    private bool CanExecutePlayPauseCommand()
    {
        return IsFFmpegInitialized;
    }

    private void ExecuteStopCommand()
    {
        _isStopRequested = true;
        _player.Stop();
    }

    private void ExecuteClearLogsCommand()
    {
        Logs.Clear();
    }
}
