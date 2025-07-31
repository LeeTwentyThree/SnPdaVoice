using System.Diagnostics;
using System.Runtime.Versioning;
using VoiceProcessor;
using VoiceProcessor.Data;

namespace VoiceProcessorServer;

[SupportedOSPlatform("windows")]
public class VoiceLineQueue(string queueName, Dictionary<string, VoiceLineGenerator> voices)
{
    private readonly CompletedFileNotifier _notifier = new(new HttpClient(), 8005, "api/notify-file-ready");
    private readonly Queue<VoiceLineRequest> _requests = new();

    private static TimeSpan QueueProcessingDelay { get; } = TimeSpan.FromMilliseconds(100);

    public int Count => _requests.Count;
    public int CountIncludingCurrentTask => _requests.Count + (_isProcessingEntry ? 1 : 0);

    private bool _stopRequested;
    private bool _isProcessingEntry;

    private readonly Stopwatch _generationTimeStopwatch = new();

    public void EnqueueVoiceLineRequest(VoiceLineRequest request)
    {
        _requests.Enqueue(request);
    }

    public void StartProcessingEntries()
    {
        _stopRequested = false;
        Task.Run(ProcessQueueLoop);
    }

    public void StopProcessingEntries()
    {
        _stopRequested = true;
    }
    
    private async Task ProcessQueueLoop()
    {
        Console.WriteLine("Starting queue process loop for queue: " + queueName);
        while (!_stopRequested)
        {
            await ProcessNextQueueElement();
            await Task.Delay(QueueProcessingDelay);
        }
    }
    
    internal async Task ProcessNextQueueElement()
    {
        if (_requests.Count == 0)
        {
            return;
        }

        var request = _requests.Dequeue();
        
        _isProcessingEntry = true;
        Console.WriteLine("Handling request: " + request);
        ServerProgram.Telemetry.LogProcessedRequest(request);
        _generationTimeStopwatch.Restart();

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

        _isProcessingEntry = false;
    }

    private async Task ContactRequester(VoiceLineRequest request, GenerationResult? result, string responseMessage, bool success)
    {
        Console.WriteLine($"[Contacting requester] request: '{request}', message: '{responseMessage}', success: '{success}'");
        var inputLength = request.Input.Message?.Length ?? 0;
        ServerProgram.Telemetry.LogRequestCompletionStatus(request, inputLength, _generationTimeStopwatch.Elapsed.TotalSeconds, success);

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