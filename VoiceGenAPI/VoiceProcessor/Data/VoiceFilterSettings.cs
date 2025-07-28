using System.Text.Json.Serialization;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Data;

public class VoiceFilterSettings
{
    [JsonPropertyName("pitch_shift")]
    public float PitchShift { get; set; }
    [JsonPropertyName("filters")]
    public FilterSettingsBase[] Filters { get; set; }
}