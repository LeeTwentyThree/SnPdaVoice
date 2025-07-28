using System.Text.Json.Serialization;
using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

public class FlangerEffect : SampleSourceEffect
{
    private readonly float[] _delayBuffer;
    private int _delayIndex;
    private readonly int _sampleRate;
    private float _phase;
    private FlangerSettings _settings;
    
    public FlangerEffect(ISampleSource source, FlangerSettings settings) : base(source)
    {
        _sampleRate = WaveFormat.SampleRate;
        _delayBuffer = new float[_sampleRate];
        _delayIndex = 0;
        _phase = 0;
        _settings = settings;
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = base.Read(buffer, offset, count);

        for (int i = 0; i < samplesRead; i++)
        {
            float lfo = GetLFO(_phase);
            float currentDelayMs = _settings.Delay + (_settings.Depth * lfo);
            int delaySamples = (int)(_sampleRate * (currentDelayMs / 1000f));
            int readIndex = (_delayIndex - delaySamples + _delayBuffer.Length) % _delayBuffer.Length;

            float dry = buffer[offset + i];
            float delayed = _delayBuffer[readIndex];

            // Apply feedback
            float input = dry + delayed * _settings.Feedback;

            // Mix wet and dry
            buffer[offset + i] = dry * (1f - _settings.Mix) + delayed * _settings.Mix;

            // Store in delay buffer
            _delayBuffer[_delayIndex] = input;

            // Advance pointers
            _delayIndex = (_delayIndex + 1) % _delayBuffer.Length;
            _phase += _settings.Rate / _sampleRate;
            if (_phase > 1f) _phase -= 1f;
        }

        return samplesRead;
    }

    private float GetLFO(float phase)
    {
        return _settings.Waveform.ToLower() switch
        {
            "sine" => (float)Math.Sin(2 * Math.PI * phase),
            "triangle" => 2f * Math.Abs(2f * (phase - MathF.Floor(phase + 0.5f))) - 1f,
            _ => 0f
        };
    }
}