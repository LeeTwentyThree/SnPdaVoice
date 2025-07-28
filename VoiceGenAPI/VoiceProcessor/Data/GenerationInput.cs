using System.Text.Json.Serialization;
using VoiceProcessor.Utilities;

namespace VoiceProcessor.Data;

public class GenerationInput
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
    [JsonPropertyName("use_ssml")]
    public bool UseSsml { get; set; }
    [JsonPropertyName("voice_id")]
    public string? VoiceId { get; set; }

    public override string ToString()
    {
        var messageString = Message == null ? "null" : Message.Truncate(50);
        return $"[message: {messageString}, id: {VoiceId}, useSsml: {UseSsml}]]";
    }
}