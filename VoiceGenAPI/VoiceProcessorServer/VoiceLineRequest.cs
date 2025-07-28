using System.Text.Json.Serialization;
using VoiceProcessor.Data;

namespace VoiceProcessorServer;

public class VoiceLineRequest(GenerationInput input, string jobId)
{
    [JsonPropertyName("input")] public GenerationInput Input { get; } = input;
    [JsonPropertyName("job_id")] public string JobId { get; set; } = jobId;

    // For logging purposes
    public override string ToString()
    {
        var messageLength = input.Message?.Length ?? 0;
        
        return "{ \"job_id\": \"" + JobId + "\", \"message length\": " + messageLength + ", \"input\": \"" + Input.ConvertToString(12) + "\" }";
    }
}