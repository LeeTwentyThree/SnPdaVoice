using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class FlangerSettings : FilterSettingsBase
{
    [JsonPropertyName("depth")]
    public float Depth { get; set; } = 1.0f;

    [JsonPropertyName("lfo_frequency")]
    public float LfoFrequency { get; set; } = 0.5f;

    [JsonPropertyName("base_delay_ms")]
    public float BaseDelayMs { get; set; } = 2.0f;

    [JsonPropertyName("spread")]
    public float Spread { get; set; } = 0.5f;

    [JsonPropertyName("cross_mix")]
    public float CrossMix { get; set; } = -6.0f;

    [JsonPropertyName("dry_gain_db")]
    public float DryGainDb { get; set; } = 0.0f;

    [JsonPropertyName("wet_gain_db")]
    public float WetGainDb { get; set; } = -3.0f;
    [JsonPropertyName("feedback")]
    public float Feedback { get; set; } = 0.5f;

    public override ISampleSource Apply(ISampleSource input)
    {
        return new FlangerEffect(input, this);
    }
}