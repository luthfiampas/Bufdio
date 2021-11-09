using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Avalonia;
using Bufdio;

namespace BufdioAvalonia;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Debug.WriteLine("Attempting to initialize PortAudio using system-wide library.");
            BufdioLib.InitializePortAudio();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        try
        {
            Debug.WriteLine("Attempting to initialize FFmpeg using system-wide libraries.");
            BufdioLib.InitializeFFmpeg();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }

        if (!BufdioLib.IsPortAudioInitialized)
        {
            var ridext = GetRidAndLibExtensions();
            string path = null;
#if RELEASE
            path = System.IO.Path.Combine(Environment.CurrentDirectory, "libs", $"libportaudio.{ridext.Item2}");
#else
            path = $"../../../../../libs/PortAudio/{ridext.Item1}/libportaudio.{ridext.Item2}";
#endif
            BufdioLib.InitializePortAudio(path);
        }

        AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace().StartWithClassicDesktopLifetime(args);
    }

    private static (string, string) GetRidAndLibExtensions()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var rid = Environment.Is64BitOperatingSystem ? "win-x64" : "win-x86";
            return (rid, "dll");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return ("linux-x64", "so");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return ("osx-x64", "dylib");
        }
        else
        {
            throw new NotSupportedException();
        }
    }
}
