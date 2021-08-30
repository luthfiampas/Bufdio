using System;
using System.Threading;
using Bufdio.Engines;

namespace Bufdio.ConsoleTests
{
    public class PortAudioEngineTests : IConsoleTests
    {
        private const int SampleRate = 8000;
        private const float Frequency = 350f;
        private const float Amplitude = 0.35f * short.MaxValue;

        public string Name => "PortAudio engine sine generator";
        
        public void Run()
        {
            Console.Write("Path to PortAudio native library: ");
            BufdioLib.InitializePortAudio(Console.ReadLine());
            
            var samples = new float[SampleRate];
            var engine = new PortAudioEngine(new AudioEngineOptions(1, SampleRate));
            
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
        }
    }
}
