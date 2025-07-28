using System.Net;
using System.Runtime.Versioning;
using VoiceProcessor;
using VoiceProcessor.Data;

namespace VoiceProcessorServer;

[SupportedOSPlatform("windows")]
public class VoiceLineQueue(Dictionary<string, VoiceLineGenerator> voices)
{
    private readonly CompletedFileNotifier _notifier = new(new HttpClient(), 8005, "api/notify-file-ready");
    private readonly Queue<VoiceLineRequest> _requests = new();

    private static readonly Telemetry Telemetry = new Telemetry();

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
        Telemetry.LogProcessedRequest(request);

        if (string.IsNullOrEmpty(request.Input.VoiceId))
        {
            await ContactRequester(request, null, $"Invalid ID: '{request.Input.VoiceId}'", false);
            return;
        }
        
        if (!voices.TryGetValue(request.Input.VoiceId, out var generator))
        {
            await ContactRequester(request, null, $"Failed to find voice by ID '{request.Input.VoiceId}'", false);
            return;
        }

        try
        {
            var result = await generator.GenerateVoiceLine(request.Input, request.JobId);
            if (result.Success)
            {
                await ContactRequester(request, result, "Success!", true);
                return;
            }
            
            await ContactRequester(request, null, "An internal exception occurred", false);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await ContactRequester(request, null, "An internal exception occurred", false);
        }
    }

    private async Task ContactRequester(VoiceLineRequest request, GenerationResult? result, string responseMessage, bool success)
    {
        Console.WriteLine($"[Contacting requester] request: '{request}', message: '{responseMessage}', success: '{success}'");
        Telemetry.LogRequestCompletionStatus(request, success);

        if (success && result != null)
        {
            await _notifier.NotifyFileReadyAsync(result);
        }
        else
        {
            await _notifier.NotifyFileReadyAsync(new GenerationResult(request.JobId, "ERROR", false));
        }
    }
}