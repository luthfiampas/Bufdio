# Bufdio
[![NuGet](https://img.shields.io/nuget/v/Bufdio)](https://www.nuget.org/packages/Bufdio/)

A cross-platform audio playback library that targets .NET Standard 2.1. The main purpose of this project is to provide easy to use API for playing and streaming audio especially in a desktop environment.

This video demonstrate how to integrate Bufdio and [NWaves](https://github.com/ar1st0crat/NWaves/) to create realtime echo effect during playback (source code can be found at [examples directory](https://github.com/luthfiampas/Bufdio/tree/main/examples/BufdioAvalonia)).

[![Bufdio Sample](https://i.ibb.co/ZmJMjJF/2021-11-02-02-46.png)](https://youtu.be/Bx22X20Tkj0)

Behind the scene, it uses [FFmpeg](https://www.ffmpeg.org/) to decode audio frames (so, it is possible to play video files by taking its audio stream only). And [PortAudio](https://github.com/PortAudio/portaudio) for sending buffer data to output device using blocking calls mechanism.

## Getting Started
This repository include pre-compiled PortAudio binaries for Windows, Linux, macOS that can be found at the [libs directory](https://github.com/luthfiampas/Bufdio/tree/main/libs/PortAudio).

```csharp
BufdioLib.InitializePortAudio("path/to/portaudio");
BufdioLib.InitializeFFmpeg("path/to/ffmpeg/libraries");

// Or just use system-wide libraries
BufdioLib.InitializePortAudio();
BufdioLib.InitializeFFmpeg();
```

With PortAudio initialized, now we can retrieve available output devices.

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
Bufdio provides high level interface for loading audio and control its playback state.

```csharp
using IAudioPlayer player = new AudioPlayer();

// Methods
player.LoadAsync("audio-url");
player.LoadAsync(fileStream);
player.Play();
player.Pause();
player.Stop();
player.Seek(TimeSpan.FromSeconds(2));

// Properties
player.Volume;
player.CustomSampleProcessor;
player.Logger;

// Properties (read-only)
player.State;
player.IsLoaded;
player.Duration;
player.Position;

// Events
player.StateChanged += OnStateChanged;
player.PositionChanged += OnPositionChanged;
```

## Generate Sine Wave
Bufdio also exposes low level `IAudioEngine` interface for sending or writing samples to an output device.

```csharp
const int SampleRate = 8000;
const float Frequency = 350f;
const float Amplitude = 0.35f * short.MaxValue;

var samples = new float[SampleRate];
var options = new AudioEngineOptions(1, SampleRate);

using IAudioEngine engine = new PortAudioEngine(options);

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
- Still need more unit tests

## Credits
- [PortAudio](https://github.com/PortAudio/portaudio/)
- [FFmpeg](https://www.ffmpeg.org/)
- [FFmpeg.AutoGen](https://github.com/Ruslan-B/FFmpeg.AutoGen)
- [Avalonia](https://github.com/AvaloniaUI/Avalonia)
- [NWaves](https://github.com/ar1st0crat/NWaves/)
- [YoutubeExplode](https://github.com/Tyrrrz/YoutubeExplode)

## Similar Projects
- [SharpAudio](https://github.com/feliwir/SharpAudio)

## License
Bufdio is licenced under the [MIT license](https://github.com/luthfiampas/Bufdio/blob/main/LICENSE).
