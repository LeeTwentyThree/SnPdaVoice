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
        return ConvertToString(50);
    }

    public string ConvertToString(int maxCharactersFromInputtedMessage)
    {
        var messageString = Message == null ? "null" : Message.Truncate(maxCharactersFromInputtedMessage);
        var escapedString = messageString.Replace("\\", "\\\\").Replace("\"", "\\\"");
        return "{ \"message\": \"" + escapedString + "\", \"use_ssml\": " +  UseSsml + ", \"voice_id\": \"" + VoiceId + "\" }";
    }
}