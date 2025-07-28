using System.Text.Json.Serialization;
using VoiceProcessor.Data;

namespace VoiceProcessorServer;

public class VoiceLineRequest(GenerationInput input, string jobId)
{
    [JsonPropertyName("input")]
    public GenerationInput Input { get; } = input;
    [JsonPropertyName("job_id")]
    public string JobId { get; set; } = jobId;

    public override string ToString()
    {
        return Input.ToString();
    }
}