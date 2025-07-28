using System.Net;
using System.Runtime.Versioning;
using VoiceProcessor;

namespace VoiceProcessorServer;

[SupportedOSPlatform("windows")]
public class VoiceLineQueue(Dictionary<string, VoiceLineGenerator> voices)
{
    private readonly Queue<VoiceLineRequest> _requests = new();

    public void EnqueueVoiceLineRequest(VoiceLineRequest request)
    {
        _requests.Enqueue(request);
    }
    
    internal async Task ProcessNextQueueElement()
    {
        if (_requests.Count == 0)
        {
            return;
        }

        var request = _requests.Dequeue();
        
        Console.WriteLine("Handling request: " + request);

        if (string.IsNullOrEmpty(request.Input.VoiceId))
        {
            ContactRequester(request, $"Invalid ID: '{request.Input.VoiceId}'", false);
            return;
        }
        
        if (!voices.TryGetValue(request.Input.VoiceId, out var generator))
        {
            ContactRequester(request, $"Failed to find voice by ID '{request.Input.VoiceId}'", false);
            return;
        }

        try
        {
            var result = await generator.GenerateVoiceLine(request.Input);
            if (result.StatusCode != HttpStatusCode.OK)
            {
                ContactRequester(request, "An internal exception occurred", false);
                return;
            }
            ContactRequester(request, "Success", true);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            ContactRequester(request, "An internal exception occurred", false);
        }
    }

    private static void ContactRequester(VoiceLineRequest request, string responseMessage, bool success)
    {
        Console.WriteLine($"{{PLACEHOLDER}}: {request}, message: {responseMessage}, success: {success}");
    }
}