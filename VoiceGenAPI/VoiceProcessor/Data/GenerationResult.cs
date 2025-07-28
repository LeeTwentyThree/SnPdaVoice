using System.Net;

namespace VoiceProcessor.Data;

public class GenerationResult(string filePath, HttpStatusCode statusCode)
{
    public string FilePath { get; } = filePath;
    public HttpStatusCode StatusCode { get; } = statusCode;
}