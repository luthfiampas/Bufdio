using Bufdio.Processors;
using NWaves.Effects;

namespace BufdioAvalonia.Processors
{
    public class EchoProcessor : SampleProcessorBase
    {
        private readonly EchoEffect _echo = new EchoEffect(44100, 0.2f, 0.6f) { Wet = 1f };
        
        public override float Process(float sample)
        {
            return _echo.Process(sample);
        }
    }
}
