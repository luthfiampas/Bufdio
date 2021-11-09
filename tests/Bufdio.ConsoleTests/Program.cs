using System;
using System.Collections.Generic;

namespace Bufdio.ConsoleTests;

public static class Program
{
    private static readonly Dictionary<int, IConsoleTests> Tests = new Dictionary<int, IConsoleTests>
    {
        { 1, new FFmpegDecoderTests(true, "FFmpeg decoder tests (from url)") },
        { 2, new FFmpegDecoderTests(false, "FFmpeg decoder tests (from stream)") },
        { 3, new AudioDeviceTests() },
        { 4, new PortAudioEngineTests() }
    };

    public static void Main(string[] args)
    {
        while (true)
        {
            foreach (var (num, test) in Tests)
            {
                Console.WriteLine($"{num}. {test.Name}");
            }

            Console.Write("\r\nSelect test: ");
            var valid = int.TryParse(Console.ReadLine(), out var id) && Tests.ContainsKey(id);

            if (!valid)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid test number.");
                Console.ResetColor();
            }
            else
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Running {Tests[id].Name}\r\n");
                Console.ResetColor();

                Tests[id].Run();
            }

            Console.WriteLine("\r\nPress ESC to exit, or any key to continue..");
            
            if (Console.ReadKey().Key == ConsoleKey.Escape)
            {
                break;
            }

            Console.Clear();
        }
    }
}
