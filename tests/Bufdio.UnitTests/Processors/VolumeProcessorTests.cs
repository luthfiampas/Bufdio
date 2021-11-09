using Bufdio.Processors;
using Xunit;

namespace Bufdio.UnitTests.Processors;

public class VolumeProcessorTests
{
    [Fact]
    public void Process_Returns_Correct_Sample()
    {
        const float volume = 0.08f;

        var samples = new[] { 0.2f, 1.0f, 3.0f };
        var processor = new VolumeProcessor(volume);

        foreach (var sample in samples)
        {
            Assert.Equal(volume * sample, processor.Process(sample));
        }
    }
}
