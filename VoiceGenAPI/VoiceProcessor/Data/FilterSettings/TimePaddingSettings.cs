using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class TimePaddingSettings : FilterSettingsBase
{
    [JsonPropertyName("ms_before")] public int MillisecondsBefore { get; set; }
    [JsonPropertyName("ms_after")] public int MillisecondsAfter { get; set; }

    public override ISampleSource Apply(ISampleSource input)
    {
        return new TimePaddingEffect(input, this);
    }
}