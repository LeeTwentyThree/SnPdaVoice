using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Filters;

namespace VoiceProcessor.Data.FilterSettings;

public class EqualizerSettings : FilterSettingsBase
{
    [JsonPropertyName("bands")]
    public EqualizerBandSettings[] Bands { get; set; }

    public override ISampleSource Apply(ISampleSource input)
    {
        return new EqualizerEffect(input, this);
    }
}