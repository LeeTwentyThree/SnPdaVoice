using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

public class FlangerEffect : SampleSourceEffect
{
    private readonly float[] _delayBufferLeft;
    private readonly float[] _delayBufferRight;
    private int _delayIndex;
    private float _phase;
    private readonly int _sampleRate;
    private readonly FlangerSettings _settings;

    private readonly float _wetGain;
    private readonly float _dryGain;

    public FlangerEffect(ISampleSource source, FlangerSettings settings) : base(source)
    {
        _settings = settings;
        _sampleRate = WaveFormat.SampleRate;
        int bufferLength = _sampleRate / 2; // 0.5 second max delay buffer
        _delayBufferLeft = new float[bufferLength];
        _delayBufferRight = new float[bufferLength];
        _delayIndex = 0;
        _phase = 0;

        _wetGain = DbToGain(_settings.WetGainDb);
        _dryGain = DbToGain(_settings.DryGainDb);
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = base.Read(buffer, offset, count);
        bool isStereo = WaveFormat.Channels == 2;

        for (int i = 0; i < samplesRead; i += WaveFormat.Channels)
        {
            float lfoL = GetLFO(_phase);
            float lfoR = GetLFO((_phase + _settings.Spread) % 1f);

            float delayMsL = MathF.Max(1f, _settings.BaseDelayMs + _settings.Depth * lfoL);
            float delayMsR = MathF.Max(1f, _settings.BaseDelayMs + _settings.Depth * lfoR);
            
            float delaySamplesL = _sampleRate * (delayMsL / 1000f);
            float delaySamplesR = _sampleRate * (delayMsR / 1000f);
            
            float dryL = buffer[offset + i];
            float dryR = isStereo ? buffer[offset + i + 1] : dryL;

            float delayedL = ReadDelaySample(_delayBufferLeft, _delayIndex, delaySamplesL);
            float delayedR = ReadDelaySample(_delayBufferRight, _delayIndex, delaySamplesR);
            
            float outL = dryL * _dryGain + delayedL * _wetGain;
            float outR = dryR * _dryGain + delayedR * _wetGain;

            buffer[offset + i] = outL;
            if (isStereo)
                buffer[offset + i + 1] = outR;

            // Apply cross-feedback
            float feedbackGain = _settings.Feedback; // new parameter, e.g., 0.5

            float feedbackL = delayedR * feedbackGain * _settings.CrossMix;
            float feedbackR = delayedL * feedbackGain * _settings.CrossMix;

            _delayBufferLeft[_delayIndex] = dryL + feedbackL;
            _delayBufferRight[_delayIndex] = dryR + feedbackR;

            _delayIndex = (_delayIndex + 1) % _delayBufferLeft.Length;
        }

        _phase += _settings.LfoFrequency / _sampleRate;
        if (_phase > 1f) _phase -= 1f;

        return samplesRead;
    }

    private float GetLFO(float phase)
    {
        return (float)Math.Sin(2 * Math.PI * phase);
    }

    private static float DbToGain(float db)
    {
        return (float)Math.Pow(10, db / 20.0);
    }
    
    private float ReadDelaySample(float[] buffer, int writePos, float delaySamples)
    {
        int bufferLength = buffer.Length;
        float readPos = writePos - delaySamples;
        if (readPos < 0)
            readPos += bufferLength;

        int i1 = (int)readPos;
        int i2 = (i1 + 1) % bufferLength;
        float frac = readPos - i1;

        return buffer[i1] * (1 - frac) + buffer[i2] * frac;
    }

}