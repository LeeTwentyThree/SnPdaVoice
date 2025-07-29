using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class CompressorSettings : FilterSettingsBase
{
    [JsonPropertyName("threshold_db")]
    public float ThresholdDb { get; set; } = -18.0f;

    [JsonPropertyName("makeup_gain_db")]
    public float MakeupGainDb { get; set; } = 6.0f;

    [JsonPropertyName("knee_width_db")]
    public float KneeWidthDb { get; set; } = 6.0f;

    [JsonPropertyName("ratio")]
    public float Ratio { get; set; } = 4.0f;

    [JsonPropertyName("smoothing")]
    public float Smoothing { get; set; } = 0.01f;

    public override ISampleSource Apply(ISampleSource input)
    {
        return new CompressorEffect(input, this);
    }
}