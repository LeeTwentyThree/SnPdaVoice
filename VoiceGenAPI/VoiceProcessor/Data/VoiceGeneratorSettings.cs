using System.Text.Json.Serialization;

namespace VoiceProcessor.Data;

public class VoiceGeneratorSettings
{
    [JsonPropertyName("voice_id")]
    public string VoiceId { get; set; }

    // Uses locally installed SAPI5 voice names
    [JsonPropertyName("voice_name")]
    public string VoiceName { get; set; }
    
    // Range of 0 to 100
    [JsonPropertyName("volume")]
    public int Volume { get; set; }
    
    // Range of -10 to 10
    [JsonPropertyName("rate")]
    public int Rate { get; set; }
    
    [JsonPropertyName("filter_settings")]
    public VoiceFilterSettings FilterSettings { get; set; }
}