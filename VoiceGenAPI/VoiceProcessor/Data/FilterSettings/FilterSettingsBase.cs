using System.Text.Json.Serialization;
using CSCore;

namespace VoiceProcessor.Data.FilterSettings;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(FlangerSettings), "flanger")]
[JsonDerivedType(typeof(TimePaddingSettings), "time_padding")]
[JsonDerivedType(typeof(FlangusSettings), "flangus")]
[JsonDerivedType(typeof(EqualizerSettings), "equalizer")]
[JsonDerivedType(typeof(ChorusSettings), "chorus")]
public abstract class FilterSettingsBase
{
    public abstract ISampleSource Apply(ISampleSource input);
}