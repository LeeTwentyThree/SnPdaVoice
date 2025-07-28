using System.Text.Json.Serialization;
using VoiceProcessor.Data;

namespace VoiceProcessorServer;

public class VoiceLineRequest(GenerationInput input)
{
    [JsonPropertyName("input")]
    public GenerationInput Input { get; } = input;

    public override string ToString()
    {
        return Input.ToString();
    }
}