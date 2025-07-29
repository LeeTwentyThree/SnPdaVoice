using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class ChorusSettings : FilterSettingsBase
{
    [JsonPropertyName("delay_ms")]
    public float DelayMs { get; set; } = 10f;

    [JsonPropertyName("depth_ms")]
    public float DepthMs { get; set; } = 0f;

    [JsonPropertyName("stereo_phase_deg")]
    public float StereoPhaseDeg { get; set; } = 0f;

    [JsonPropertyName("lfo_frequency_hz")]
    public float LfoFrequencyHz { get; set; } = 5f;

    [JsonPropertyName("cross_cutoff_hz")]
    public float CrossCutoffHz { get; set; } = 353f;

    [JsonPropertyName("wet_mix")]
    public float WetMix { get; set; } = 0.5f;

    [JsonPropertyName("dry_mix")]
    public float DryMix { get; set; } = 0.5f;

    public override ISampleSource Apply(ISampleSource input)
    {
        return new ChorusEffect(input, this);
    }
}