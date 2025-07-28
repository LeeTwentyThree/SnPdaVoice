using System.Text.Json.Serialization;

namespace VoiceProcessor.Data;

public class GenerationResult
{
    public GenerationResult()
    {
        
    }

    public GenerationResult(string jobId, string fileName, bool success)
    {
        JobId = jobId;
        FileName = fileName;
        Success = success;
    }

    [JsonPropertyName("job_id")]
    public string JobId { get; set; }
    [JsonPropertyName("filename")]
    public string FileName { get; set; }
    [JsonPropertyName("success")]
    public bool Success { get; set; }
}