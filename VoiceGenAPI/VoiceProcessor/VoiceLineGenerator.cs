using System.Diagnostics;
using System.Net;
using System.Runtime.Versioning;
using System.Speech.AudioFormat;
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
    private readonly Stopwatch _timeOutStopWatch = new();

    private const int TimeOutInMilliseconds = 15000;
    private const int CheckCompletionIntervalMs = 200;
    
    private static GenerationResult GetErrorResult(string id) =>
        new("http://invalid.invalid/", id, false);
    
    private static readonly SpeechAudioFormatInfo Format = new(
        EncodingFormat.Pcm,
        44100,
        16,
        1,
        88200,
        2,
        null
    );

    
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

            var outputPath = Path.Combine(WorkingFolderUtils.GetTempFolder(), GetUniqueFileName() + ".wav");
            
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

    private async Task<RawSpeechGeneration> GenerateUnfilteredTextToSpeech(SpeechSynthesizer synthesizer, GenerationInput input)
    {
        var path = Path.Combine(WorkingFolderUtils.GetTempFolder(), GetUniqueFileName() + ".wav");
        synthesizer.SetOutputToWaveFile(path, Format);
        synthesizer.SpeakAsync(new Prompt(input.Message,
            input.UseSsml ? SynthesisTextFormat.Ssml : SynthesisTextFormat.Text));

        await Task.Delay(100);
            
        _timeOutStopWatch.Restart();
        var timedOut = false;
        while (true)
        {
            if (_timeOutStopWatch.ElapsedMilliseconds > TimeOutInMilliseconds)
            {
                timedOut = true;
                break;
            }

            if (synthesizer.State == SynthesizerState.Ready)
            {
                break;
            }

            await Task.Delay(CheckCompletionIntervalMs);
        }

        synthesizer.SetOutputToNull();

        if (timedOut)
        {
            Console.WriteLine("Timed out while accessing speech synthesizer!");
            return new RawSpeechGeneration("ERROR", false);
        }

        Console.WriteLine("Saving raw voice line to file at path: " + path);
        return new RawSpeechGeneration(path, true);
    }

    private void AddFiltersAndGenerateNewFile(string inputFilePath,
        string outputFilePath, VoiceFilterSettings filterSettings)
    {
        using var reader = new WaveFileReader(inputFilePath);
        var source = reader.ToSampleSource().ToStereo();

        if (Math.Abs(filterSettings.PitchShift) > 0.00001f)
        {
            source = PitchShift(source, filterSettings.PitchShift);
        }
        
        foreach (var filter in filterSettings.Filters)
        {
            source = filter.Apply(source);
        }
        
        using var final = source.ToWaveSource();
        using var outputStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write);
        final.WriteToWaveStream(outputStream);
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

    private static string GetUniqueFileName()
    {
        return Guid.NewGuid().ToString();
    }

    private class RawSpeechGeneration(string path, bool success)
    {
        public string Path { get; } = path;
        public bool Success { get; } = success;
    }
}