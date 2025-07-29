using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

public class ChorusEffect : SampleSourceEffect
{
    private readonly float[] _delayBufferLeft;
    private readonly float[] _delayBufferRight;
    private int _delayIndex;
    private readonly int _sampleRate;

    private float _phaseL;
    private float _phaseR;

    private readonly ChorusSettings _settings;

    private readonly float _crossCutoffCoeff;
    private float _crossFilteredL;
    private float _crossFilteredR;

    public ChorusEffect(ISampleSource source, ChorusSettings settings) : base(source)
    {
        _settings = settings;
        _sampleRate = source.WaveFormat.SampleRate;
        int bufferLength = _sampleRate; // 1 second max delay buffer
        _delayBufferLeft = new float[bufferLength];
        _delayBufferRight = new float[bufferLength];
        _delayIndex = 0;

        _phaseL = 0f;
        _phaseR = _settings.StereoPhaseDeg / 360f; // convert degrees to 0-1 phase

        _crossCutoffCoeff = CalculateLowpassCoefficient(_settings.CrossCutoffHz, _sampleRate);
        _crossFilteredL = 0f;
        _crossFilteredR = 0f;
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = base.Read(buffer, offset, count);
        bool isStereo = WaveFormat.Channels == 2;

        for (int i = 0; i < samplesRead; i += WaveFormat.Channels)
        {
            // Compute modulated delay in samples
            float lfoValueL = GetLfo(_phaseL);
            float lfoValueR = GetLfo(_phaseR);

            float delayMsL = _settings.DelayMs + _settings.DepthMs * lfoValueL;
            float delayMsR = _settings.DelayMs + _settings.DepthMs * lfoValueR;

            int delaySamplesL = (int)(_sampleRate * (delayMsL / 1000f));
            int delaySamplesR = (int)(_sampleRate * (delayMsR / 1000f));

            int readIdxL = (_delayIndex - delaySamplesL + _delayBufferLeft.Length) % _delayBufferLeft.Length;
            int readIdxR = (_delayIndex - delaySamplesR + _delayBufferRight.Length) % _delayBufferRight.Length;

            float dryL = buffer[offset + i];
            float dryR = isStereo ? buffer[offset + i + 1] : dryL;

            float delayedL = _delayBufferLeft[readIdxL];
            float delayedR = _delayBufferRight[readIdxR];

            // Cross feedback with lowpass filter
            _crossFilteredL += _crossCutoffCoeff * (delayedR - _crossFilteredL);
            _crossFilteredR += _crossCutoffCoeff * (delayedL - _crossFilteredR);

            // Write new samples to delay buffer with cross feedback
            _delayBufferLeft[_delayIndex] = dryL + _crossFilteredL * 0.3f;
            _delayBufferRight[_delayIndex] = dryR + _crossFilteredR * 0.3f;

            // Mix dry and wet signals
            float outL = _settings.DryMix * dryL + _settings.WetMix * delayedL;
            float outR = isStereo ? (_settings.DryMix * dryR + _settings.WetMix * delayedR) : outL;

            buffer[offset + i] = outL;
            if (isStereo)
                buffer[offset + i + 1] = outR;

            // Advance delay buffer index
            _delayIndex = (_delayIndex + 1) % _delayBufferLeft.Length;

            // Advance LFO phases with wrap-around
            _phaseL += _settings.LfoFrequencyHz / _sampleRate;
            _phaseR += _settings.LfoFrequencyHz / _sampleRate;
            if (_phaseL > 1f) _phaseL -= 1f;
            if (_phaseR > 1f) _phaseR -= 1f;
        }

        return samplesRead;
    }

    private static float GetLfo(float phase)
    {
        return (float)Math.Sin(2 * Math.PI * phase);
    }

    private static float CalculateLowpassCoefficient(float cutoffHz, int sampleRate)
    {
        float rc = 1.0f / (2 * MathF.PI * cutoffHz);
        float dt = 1.0f / sampleRate;
        return dt / (rc + dt);
    }
}