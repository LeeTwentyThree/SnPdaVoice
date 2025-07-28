using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class FlangerSettings : FilterSettingsBase
{
    [JsonPropertyName("depth")]
    public float Depth { get; set; } = 5.0f;    // ms
    [JsonPropertyName("rate")]
    public float Rate { get; set; } = 0.25f;    // Hz
    [JsonPropertyName("delay")]
    public float Delay { get; set; } = 2.0f;    // Base delay in ms
    [JsonPropertyName("feedback")]
    public float Feedback { get; set; } = 0.5f; // -1.0 to +1.0
    [JsonPropertyName("mix")]
    public float Mix { get; set; } = 0.7f;  // 0.0 to 1.0
    [JsonPropertyName("waveform")]
    public string Waveform { get; set; } = "sine";
    
    public override ISampleSource Apply(ISampleSource input)
    {
        return new FlangerEffect(input, this);
    }
}