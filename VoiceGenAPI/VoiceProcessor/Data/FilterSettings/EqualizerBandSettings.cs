using System.Text.Json.Serialization;

namespace VoiceProcessor.Data.FilterSettings;

public class EqualizerBandSettings
{
    [JsonPropertyName("frequency_hz")]
    public int FrequencyHz { get; set; }

    [JsonPropertyName("gain_db")]
    public float GainDb { get; set; }

    [JsonPropertyName("q")]
    public float Q { get; set; } = 0.7f;
}