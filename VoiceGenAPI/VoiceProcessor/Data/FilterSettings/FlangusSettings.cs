using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class FlangusSettings : FilterSettingsBase
{
    [JsonPropertyName("order")]
    public int Order { get; set; } = 4; // Number of flanger voices stacked

    [JsonPropertyName("depth")]
    public float Depth { get; set; } = 0.45f; // Flange depth per flanger

    [JsonPropertyName("speed")]
    public float Speed { get; set; } = 0.5f; // Base LFO speed (Hz)

    [JsonPropertyName("delay")]
    public float Delay { get; set; } = 0.01f; // Base delay in seconds

    [JsonPropertyName("spread")]
    public float Spread { get; set; } = 0.5f; // Spread 0..1 affects variation across voices

    [JsonPropertyName("cross")]
    public float StereoCross { get; set; } = 0f; // -1.0 (inverted L <-> R) to 1.0 (L <-> R)

    [JsonPropertyName("dry")]
    public float Dry { get; set; } = 1f; // -1.0 to 1.0 (dry signal polarity and mix)

    [JsonPropertyName("wet")]
    public float Wet { get; set; } = 1f; // -1.0 to 1.0 (wet signal polarity and mix)
    
    [JsonPropertyName("feedback")]
    public float Feedback { get; set; } = 1f;

    [JsonPropertyName("effect_amount")] 
    public float EffectAmount { get; set; } = 1f;

    public override ISampleSource Apply(ISampleSource input)
    {
        return new FlangusEffect(input, this);
    }
}