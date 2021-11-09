using System;

namespace Bufdio.ConsoleTests;

public class AudioDeviceTests : IConsoleTests
{
    public string Name => "Retrieve audio device information";

    public void Run()
    {
        Console.Write("Path to PortAudio native library: ");
        BufdioLib.InitializePortAudio(Console.ReadLine());

        Console.WriteLine("\r\n# Default Output Device");
        PrintDevice(BufdioLib.DefaultOutputDevice);

        Console.WriteLine("\r\n# Available Output Devices");
        foreach (var device in BufdioLib.OutputDevices)
        {
            PrintDevice(device);
            Console.WriteLine();
        }
    }

    private static void PrintDevice(AudioDevice device)
    {
        Console.WriteLine($"> Device Index: {device.DeviceIndex}");
        Console.WriteLine($"> Name: {device.Name}");
        Console.WriteLine($"> Maximum channels: {device.MaxOutputChannels}");
        Console.WriteLine($"> Default low latency : {device.DefaultLowOutputLatency}");
        Console.WriteLine($"> Default high latency : {device.DefaultHighOutputLatency}");
        Console.WriteLine($"> Default sample rate : {device.DefaultSampleRate}");
    }
}
