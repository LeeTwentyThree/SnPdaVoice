using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class ReverbSettings : FilterSettingsBase
{
    [JsonPropertyName("delay_milliseconds")]
    public float DelayMilliseconds { get; set; } = 50f;

    [JsonPropertyName("decay")]
    public float Decay { get; set; } = 0.5f;

    [JsonPropertyName("mix")]
    public float Mix { get; set; } = 0.5f;

    public override ISampleSource Apply(ISampleSource input)
    {
        return new ReverbEffect(input, this);
    }
}