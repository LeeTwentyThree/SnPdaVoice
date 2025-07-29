using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

public class FlangerEffect : SampleSourceEffect
{
    private readonly float[] _delayBuffer;
    private int _delayIndex;
    private readonly int _sampleRate;
    private float _phase;
    private readonly FlangerSettings _settings;
    
    private readonly float _wetGain;
    private readonly float _dryGain;
    private readonly float _crossGain;
    
    public FlangerEffect(ISampleSource source, FlangerSettings settings) : base(source)
    {
        _sampleRate = WaveFormat.SampleRate;
        _delayBuffer = new float[_sampleRate];
        _delayIndex = 0;
        _phase = 0;
        _settings = settings;
        _wetGain = DbToGain(_settings.WetGain);;
        _dryGain = DbToGain(_settings.DryGain);;
        _crossGain = DbToGain(_settings.CrossGain);;
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = base.Read(buffer, offset, count);

        for (int i = 0; i < samplesRead; i++)
        {
            float lfo = GetLFO(_phase);

            // Interpolated params:
            float delayMs = _settings.Delay * _settings.EffectAmount;
            float depth = _settings.Depth * _settings.EffectAmount;

            // Feedback and gains in linear scale, scaled by EffectAmount
            float feedback = _settings.Feedback * _settings.EffectAmount;

            float wetGainLin = _wetGain * _settings.EffectAmount;
            float dryGainLin = 1.0f - _settings.EffectAmount + (_dryGain * _settings.EffectAmount);
            float crossGainLin = _crossGain * _settings.EffectAmount;

            float currentDelayMs = delayMs + (depth * lfo);
            currentDelayMs = Math.Max(currentDelayMs, 0.1f); // avoid zero delay

            int delaySamples = (int)(_sampleRate * (currentDelayMs / 1000f));
            int readIndex = (_delayIndex - delaySamples + _delayBuffer.Length) % _delayBuffer.Length;

            float dry = buffer[offset + i];
            float delayed = _delayBuffer[readIndex];

            // Feedback input includes cross feedback
            float input = dry + delayed * feedback;

            // Mix output
            buffer[offset + i] = dry * dryGainLin + delayed * wetGainLin;

            // Store feedback with cross gain applied
            _delayBuffer[_delayIndex] = input * crossGainLin;

            // Advance
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
    
    private static float DbToGain(float db) => (float)Math.Pow(10, db / 20.0);
}