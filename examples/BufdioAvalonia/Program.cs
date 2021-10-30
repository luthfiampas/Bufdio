using System;
using System.Runtime.InteropServices;
using Avalonia;
using Bufdio;

namespace BufdioAvalonia
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            InitPortAudio();
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }

        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
        }

        private static void InitPortAudio()
        {
#if DEBUG
            const string root = "../../../../../libs/PortAudio";

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var rid = Environment.Is64BitOperatingSystem ? "win-x64" : "win-x86";
                BufdioLib.InitializePortAudio($"{root}/{rid}/portaudio.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                BufdioLib.InitializePortAudio($"{root}/linux-x64/libportaudio.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                BufdioLib.InitializePortAudio($"{root}/osx-x64/libportaudio.dylib");
            }
#else
            var root = System.IO.Path.Combine(Environment.CurrentDirectory, "libs");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                BufdioLib.InitializePortAudio($"{root}/portaudio.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                BufdioLib.InitializePortAudio($"{root}/libportaudio.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                BufdioLib.InitializePortAudio($"{root}/libportaudio.dylib");
            }
#endif
            try
            {
                // Try initialize FFmpeg using system-wide libraries
                BufdioLib.InitializeFFmpeg();
            }
            catch
            {
            }
        }
    }
}
