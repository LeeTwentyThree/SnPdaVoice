using System.Net;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using VoiceProcessor;
using VoiceProcessor.Utilities;

namespace VoiceProcessorServer;

[SupportedOSPlatform("windows")]
public static class ServerProgram
{
    private static VoiceLineQueue MainQueue { get; set; }

    private const int QueueProcessingDelay = 300;

    public const int Port = 8765;
    private static TcpListener? _listener;
    private static StreamReader? _reader;
    private static StreamWriter? _writer;
    
    public static bool IsReady { get; private set; }
    

    public static async Task Main(string[] args)
    {
        _listener = new TcpListener(IPAddress.Loopback, Port);
        _listener.Start();
        Console.WriteLine($"Listening on port {Port}...");
        
        var voices = VoiceLoadingUtils.LoadAllVoices();

        if (voices.Count == 0)
        {
            Console.WriteLine("No voices found! Press enter to exit...");
            Console.ReadLine();
        }


        MainQueue = new VoiceLineQueue(voices);
        Task.Run(ProcessQueue);
        
        Console.WriteLine($"Voice systems initialized. {voices.Count} voices found.");
        Console.WriteLine("Starting server...");

        IsReady = true;
        
        while (true)
        {
            Console.WriteLine("Waiting for a new client...");
            using TcpClient client = await _listener.AcceptTcpClientAsync();
            Console.WriteLine("Client connected!");

            await HandleClient(client);
            Console.WriteLine("Client disconnected.");
        }
    }

    private static async Task HandleClient(TcpClient client)
    {
        await using NetworkStream stream = client.GetStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        await using var writer = new StreamWriter(stream, Encoding.UTF8);
        writer.AutoFlush = true;

        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (line == "exit")
                break;

            Console.WriteLine($"Received: {line}");

            VoiceLineRequest? request;

            try
            {
                request = JsonSerializer.Deserialize<VoiceLineRequest>(line);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e);
                await writer.WriteLineAsync("Exception thrown!");
                continue;
            }

            GetBestQueueForInput(request).EnqueueVoiceLineRequest(request);
            await writer.WriteLineAsync("Enqueued request successfully!");
        }
    }

    private static async Task ProcessQueue()
    {
        Console.WriteLine("Starting queue process loop");
        while (true)
        {
            await MainQueue.ProcessNextQueueElement();
            await Task.Delay(QueueProcessingDelay);
        }
    }

    private static VoiceLineQueue GetBestQueueForInput(VoiceLineRequest request)
    {
        return MainQueue;
    }
}