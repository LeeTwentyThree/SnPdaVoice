using System.Diagnostics;
using System.Runtime.Versioning;
using System.Speech.Synthesis;
using CSCore;
using CSCore.Codecs.WAV;
using VoiceProcessor.Data;
using VoiceProcessor.Filters;
using VoiceProcessor.Utilities;

namespace VoiceProcessor;

[SupportedOSPlatform("windows")]
public class VoiceLineGenerator(VoiceGeneratorSettings generatorSettings)
{
    private static GenerationResult GetErrorResult(string id) =>
        new("http://invalid.invalid/", id, false);

    private const int ExportSampleRate = 44100;

    private SpeechSynthesizer? GetSynthesizer()
    {
        try
        {
            var synthesizer = new SpeechSynthesizer();
            synthesizer.SelectVoice(generatorSettings.VoiceName);
            synthesizer.Rate = generatorSettings.Rate;
            synthesizer.Volume = generatorSettings.Volume;
            return synthesizer;
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception while initializing voice synthesizer: " + e);
        }

        return null;
    }

    public async Task<GenerationResult> GenerateVoiceLine(GenerationInput input, string id)
    {
        using var synthesizer = GetSynthesizer();

        if (synthesizer == null)
        {
            Console.WriteLine("Synthesizer is not initialized! Check that the correct voice is installed.");
            return GetErrorResult(id);
        }

        try
        {
            var rawSpeech = await GenerateUnfilteredTextToSpeech(synthesizer, input);
            if (rawSpeech.Success == false)
            {
                return GetErrorResult(id);
            }

            var outputPath = Path.Combine(WorkingFolderUtils.GetTempFolder(), GetUniqueFileName(id, "wav"));

            AddFiltersAndGenerateNewFile(rawSpeech.Path, outputPath, generatorSettings.FilterSettings);

            File.Delete(rawSpeech.Path);

            var outputFileName = Path.GetFileName(outputPath);

            return new GenerationResult(id, outputFileName, true);
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception while generating voice line: " + e);
            return GetErrorResult(id);
        }
    }

    private async Task<RawSpeechGeneration> GenerateUnfilteredTextToSpeech(SpeechSynthesizer synthesizer,
        GenerationInput input)
    {
        var path = Path.Combine(WorkingFolderUtils.GetTempFolder(), GetUniqueString() + ".wav");
        synthesizer.SetOutputToWaveFile(path);

        var prompt = new Prompt(input.UseSsml ? FixSsml(input.Message) : input.Message,
            input.UseSsml ? SynthesisTextFormat.Ssml : SynthesisTextFormat.Text);
        bool success = await SpeakAsyncWithCompletion(synthesizer, prompt);

        if (!success)
        {
            Console.WriteLine("Synthesis timed out.");
            return new RawSpeechGeneration("ERROR", false);
        }

        synthesizer.SetOutputToNull();

        Console.WriteLine("Saving raw voice line to file at path: " + path);
        return new RawSpeechGeneration(path, true);
    }

    private void AddFiltersAndGenerateNewFile(string inputFilePath,
        string outputFilePath, VoiceFilterSettings filterSettings)
    {
        using var reader = new WaveFileReader(inputFilePath);
        var source = reader.ToSampleSource().ToStereo();

        // Change sample rate
        if (source.WaveFormat.SampleRate != ExportSampleRate)
        {
            source = source.ChangeSampleRate(ExportSampleRate);
        }

        if (Math.Abs(filterSettings.PitchShift) > 0.001f)
        {
            source = PitchShift(source, filterSettings.PitchShift);
        }

        foreach (var filter in filterSettings.Filters)
        {
            source = filter.Apply(source);
        }

        using var final = source.ToWaveSource(16);

        using var writer = new WaveWriter(outputFilePath, final.WaveFormat);
        byte[] buffer = new byte[final.WaveFormat.BytesPerSecond];
        int read;

        while ((read = final.Read(buffer, 0, buffer.Length)) > 0)
        {
            writer.Write(buffer, 0, read);
        }
    }

    private ISampleSource PitchShift(ISampleSource source, float semitones)
    {
        int sampleCount = (int)source.Length;
        float[] buffer = new float[sampleCount];
        int read = 0, totalRead = 0;
        while ((read = source.Read(buffer, totalRead, sampleCount - totalRead)) > 0)
        {
            totalRead += read;
        }

        return new PitchShiftSource(buffer, source.WaveFormat, semitones);
    }

    private async Task<bool> SpeakAsyncWithCompletion(SpeechSynthesizer synthesizer, Prompt prompt,
        int timeoutMs = 15000)
    {
        var tcs = new TaskCompletionSource<bool>();

        synthesizer.SpeakCompleted += Handler;
        synthesizer.SpeakAsync(prompt);

        var completedTask = await Task.WhenAny(tcs.Task, Task.Delay(timeoutMs));
        if (completedTask != tcs.Task)
        {
            // Timeout
            synthesizer.SpeakCompleted -= Handler;
            synthesizer.SpeakAsyncCancelAll(); // Cancel synthesis
            return false;
        }

        return true;

        void Handler(object? sender, SpeakCompletedEventArgs e)
        {
            synthesizer.SpeakCompleted -= Handler;
            tcs.TrySetResult(true);
        }
    }

    private static string GetUniqueString()
    {
        return Guid.NewGuid().ToString();
    }
    
    private static string GetUniqueFileName(string jobId, string extension)
    {
        var normalFileName = jobId + "." + extension;
        if (File.Exists(Path.Combine(WorkingFolderUtils.GetTempFolder(), normalFileName)))
        {
            return Guid.NewGuid().ToString();
        }
        return normalFileName;
    }

    private static string FixSsml(string original)
    {
        return original.Replace("<speak>",
            "<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"en-GB\">");
    }

    private class RawSpeechGeneration(string path, bool success)
    {
        public string Path { get; } = path;
        public bool Success { get; } = success;
    }
}