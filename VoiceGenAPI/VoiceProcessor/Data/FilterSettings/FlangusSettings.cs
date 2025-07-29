using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class FlangusSettings : FilterSettingsBase
{
    [JsonPropertyName("depth")] 
    public float Depth { get; set; } = 0.45f;

    [JsonPropertyName("speed")] 
    public float Speed { get; set; } = 0.68f;

    [JsonPropertyName("delay")] 
    public float Delay { get; set; } = 0.0625f;

    [JsonPropertyName("spread")] 
    public float Spread { get; set; } = 0.47f;

    [JsonPropertyName("cross")] 
    public float Cross { get; set; } = 0.375f;

    [JsonPropertyName("dry")] 
    public float Dry { get; set; } = 0.83f;

    [JsonPropertyName("wet")] 
    public float Wet { get; set; } = 0.86f;

    [JsonPropertyName("feedback")] 
    public float Feedback { get; set; } = 0.1f;

    [JsonPropertyName("waveform")] 
    public string Waveform { get; set; } = "sine"; // NOT IMPLEMENTED

    [JsonPropertyName("effect_amount")] 
    public float EffectAmount { get; set; } = 1f;

    public override ISampleSource Apply(ISampleSource input)
    {
        return new FlangusEffect(input, this);
    }
}