using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class FlangusSettings : FilterSettingsBase
{
    [JsonPropertyName("feedback")] public float Feedback { get; set; } = 0.6f;
    [JsonPropertyName("max_delay_ms")] public float MaxDelayMs { get; set; } = 8.0f;
    [JsonPropertyName("lfo_frequency")] public float LfoFrequency { get; set; } = 0.3f;
    [JsonPropertyName("wet_mix")] public float WetMix { get; set; } = 0.5f;

    public override ISampleSource Apply(ISampleSource input)
    {
        return new FlangusEffect(input, this);
    }
}