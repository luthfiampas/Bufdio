# Bufdio
A cross-platform audio playback library that targets .NET Standard 2.1. The main purpose of this project is to provide easy to use API for playing and streaming audio especially in desktop environment.

[![NuGet](https://img.shields.io/nuget/v/Bufdio)](https://www.nuget.org/packages/Bufdio/)

This video demonstrate how to integrate Bufdio and [NWaves](https://github.com/ar1st0crat/NWaves/) to create realtime echo effect during playback (source code can be found at [examples directory](https://github.com/luthfiampas/Bufdio/tree/main/examples/BufdioAvalonia)).

[![Bufdio Sample](https://i.ibb.co/FgWN3jt/bufdio-sample.png)](https://youtu.be/YM5CK2zG5T0)

Behind the scene, it uses [FFmpeg](https://www.ffmpeg.org/) to decode audio frames (so it is possible to play video files by taking only audio stream). And [PortAudio](https://github.com/PortAudio/portaudio) for sending buffer data or samples to output device using blocking calls mechanism.

## Getting Started
This repository include pre-compiled PortAudio binaries for Windows, Linux, macOS that can be found at [libs directory](https://github.com/luthfiampas/Bufdio/tree/main/libs/PortAudio) (unfortunately, the Windows binaries does not include ASIO).

As for FFmpeg, you can either install FFmpeg native libraries as system-wide libraries, or just instantiate Bufdio by providing path to FFmpeg libraries.

```csharp
BufdioLib.InitializePortAudio("path/to/portaudio");
BufdioLib.InitializeFFmpeg("path/to/ffmpeg/libraries");

// Or just use system-wide libraries
BufdioLib.InitializeFFmpeg();
```

With PortAudio initialized, we can retrieve available output devices.

```csharp
var defaultDevice = BufdioLib.DefaultOutputDevice;
Console.WriteLine(defaultDevice.Name);
Console.WriteLine(defaultDevice.MaxOutputChannels);
Console.WriteLine(defaultDevice.DefaultSampleRate);
Console.WriteLine(defaultDevice.DefaultHighOutputLatency);

// Retrieve all available output devices
foreach (var device in BufdioLib.OutputDevices)
{
    Console.WriteLine(device.Name);
}
```

## Playing Audio Files
Bufdio provides high level interface for loading audio and control its playback state. The audio source can be loaded from local file, URL or .NET `System.IO.Stream`.

```csharp
using IAudioPlayer player = new AudioPlayer();

// Methods
player.Load("audio-url");
player.Load(fileStream);
player.Play();
player.Pause();
player.Stop();
player.SetVolume(0.9f);
player.Seek(TimeSpan.FromSeconds(2));

// Properties (read-only)
var state = player.CurrentState; // (Playing, Buferring, Paused, Stopped)
var loaded = player.IsAudioLoaded;
var duration = player.TotalDuration;
var position = player.CurrentPosition;
var volume = player.CurrentVolume;

// Events
player.AudioLoaded += OnAudioLoaded;
player.StateChanged += OnStateChanged;
player.PositionChanged += OnPositionChanged;
player.PlaybackCompleted += OnPlaybackCompleted;
player.LogCreated += OnLogCreated;
player.FrameDecoded += OnFrameDecoded;
player.FramePresented += OnFramePresented;
```

The `AudioPlayer` constructor allows you to specify list of custom sample processors. `ISampleProcessor` interface is intended to modify audio sample that will be executed before writing audio frame to output device. The most simple processor is `VolumeProcessor` that simply multiply given sample by desired volume.

```csharp
var processors = new[] { new EchoProcessor(), new DistortionProcessor() };
using IAudioPlayer player = new AudioPlayer(customProcessors: processors);
```

## Generate Sine Wave
Bufdio also exposes low level `IAudioEngine` interface for sending or writing samples to native output device.

```csharp
const int SampleRate = 8000;
const float Frequency = 350f;
const float Amplitude = 0.35f * short.MaxValue;

var samples = new float[SampleRate];
var options = new AudioEngineOptions(1, SampleRate);

using IAudioEngine engine = new PortAudioEngine();

for (var i = 0; i < samples.Length; i++)
{
    samples[i] = (float)(Amplitude * Math.Sin(2 * Math.PI * i * Frequency / SampleRate));
}

Console.WriteLine("Playing 10 times with 1 second delay..");

for (var i = 0; i < 10; i++)
{
    engine.Send(samples);
    Thread.Sleep(1000);
}
```

## TODO
- Currently only resampling to `Float32` format
- Still need more unit tests

## Credits
- [PortAudio](https://github.com/PortAudio/portaudio/)
- [FFmpeg](https://www.ffmpeg.org/)
- [FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen) (project dependency)
- [NWaves](https://github.com/ar1st0crat/NWaves/) (used in sample project)
- [Avalonia](https://github.com/AvaloniaUI/Avalonia) (used in sample project)

## Similar Projects
- [SharpAudio](https://github.com/feliwir/SharpAudio)

## License
Bufdio is licenced under the [MIT license](https://github.com/luthfiampas/Bufdio/blob/main/LICENSE).
