using System;
using System.IO;
using System.Reflection;
using Bufdio.Decoders;
using Bufdio.Decoders.FFmpeg;

namespace Bufdio.ConsoleTests;

public class FFmpegDecoderTests : IConsoleTests
{
    private const string StreamManifest = "Bufdio.ConsoleTests.Resources.test.mp3";
    private readonly bool _useUrl;
    private IAudioDecoder _decoder;
    private Stream _stream;

    public FFmpegDecoderTests(bool useUrl, string name)
    {
        _useUrl = useUrl;
        Name = name;
    }

    public string Name { get; }

    public void Run()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Leave this empty to use system-wide libraries.");
        Console.ResetColor();

        Console.Write("> Path to FFmpeg libraries: ");
        BufdioLib.InitializeFFmpeg(Console.ReadLine());

        if (_useUrl)
        {
            Console.Write("> Audio URL or path to audio file: ");
            _decoder = new FFmpegDecoder(Console.ReadLine());
        }
        else
        {
            _stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(StreamManifest);
            _decoder = new FFmpegDecoder(_stream);
        }

        PrintStreamInfo();
        DecodeFrames();
        Seek();
        DecodeFrames();
        EndTest();
    }

    private void PrintStreamInfo()
    {
        Console.WriteLine($"> Sample rate: {_decoder.StreamInfo.SampleRate}");
        Console.WriteLine($"> Channels: {_decoder.StreamInfo.Channels}");
        Console.WriteLine($"> Duration: {_decoder.StreamInfo.Duration}");
    }

    private void DecodeFrames()
    {
        Console.WriteLine("# Decoding audio frames..");

        var decodedFrames = 0;
        var lastPts = 0d;

        while (true)
        {
            var result = _decoder.DecodeNextFrame();

            if (result.IsEOF)
            {
                break;
            }

            if (!result.IsSucceeded)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write(result.ErrorMessage);
                Console.ResetColor();
            }
            else
            {
                lastPts = result.Frame.PresentationTime;
                decodedFrames++;
            }
        }

        Console.WriteLine($"> Decoded frames: {decodedFrames}");
        Console.WriteLine($"> Last pts: {TimeSpan.FromMilliseconds(lastPts)}");
    }

    private void Seek()
    {
        Console.WriteLine("# Seeks to beginning of audio stream..");

        if (_decoder.TrySeek(TimeSpan.Zero, out var error))
        {
            return;
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(error);
        Console.ResetColor();
    }

    private void EndTest()
    {
        Console.WriteLine("# Disposing..");

        _decoder.Dispose();
        _stream?.Dispose();
    }
}
