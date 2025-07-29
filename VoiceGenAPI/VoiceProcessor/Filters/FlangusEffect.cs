using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

// I hardly know what is going on in this file, another one by ChatGPT
public class FlangusEffect : SampleSourceEffect
{
    private readonly float[] _delayBufferLeft;
    private readonly float[] _delayBufferRight;
    private int _delayIndex;
    private float _phase;
    private readonly int _sampleRate;
    private readonly FlangusSettings _settings;

    private readonly float _wetGain;
    private readonly float _dryGain;
    private readonly float _crossGain;

    public FlangusEffect(ISampleSource source, FlangusSettings settings) : base(source)
    {
        _settings = settings;
        _sampleRate = WaveFormat.SampleRate;
        int bufferLength = _sampleRate; // 1 second max delay buffer
        _delayBufferLeft = new float[bufferLength];
        _delayBufferRight = new float[bufferLength];
        _delayIndex = 0;
        _phase = 0;

        _wetGain = DbToGain(_settings.Wet);
        _dryGain = DbToGain(_settings.Dry);
        _crossGain = DbToGain(_settings.Cross);
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = base.Read(buffer, offset, count);
        bool isStereo = WaveFormat.Channels == 2;

        for (int i = 0; i < samplesRead; i += WaveFormat.Channels)
        {
            float lfoL = GetLFO(_phase);
            float lfoR = GetLFO((_phase + _settings.Spread) % 1f);

            float delayMsL = _settings.Delay + _settings.Depth * lfoL;
            float delayMsR = _settings.Delay + _settings.Depth * lfoR;

            int delaySamplesL = (int)(_sampleRate * (delayMsL / 1000f));
            int delaySamplesR = (int)(_sampleRate * (delayMsR / 1000f));

            int readIdxL = (_delayIndex - delaySamplesL + _delayBufferLeft.Length) % _delayBufferLeft.Length;
            int readIdxR = (_delayIndex - delaySamplesR + _delayBufferRight.Length) % _delayBufferRight.Length;

            float dryL = buffer[offset + i];
            float dryR = isStereo ? buffer[offset + i + 1] : dryL;

            float delayedL = _delayBufferLeft[readIdxL];
            float delayedR = _delayBufferRight[readIdxR];

            float outL = dryL * _dryGain + delayedL * _wetGain;
            float outR = dryR * _dryGain + delayedR * _wetGain;

            buffer[offset + i] = outL;
            if (isStereo)
                buffer[offset + i + 1] = outR;

            // Apply cross-feedback
            float feedbackL = dryL + delayedR * _settings.Cross * _settings.Feedback;
            float feedbackR = dryR + delayedL * _settings.Cross * _settings.Feedback;

            _delayBufferLeft[_delayIndex] = feedbackL;
            _delayBufferRight[_delayIndex] = feedbackR;

            _delayIndex = (_delayIndex + 1) % _delayBufferLeft.Length;
        }

        _phase += _settings.Speed / _sampleRate;
        if (_phase > 1f) _phase -= 1f;

        return samplesRead;
    }

    private float GetLFO(float phase)
    {
        return MathF.Sin(2.0f * MathF.PI * phase);
    }

    private static float DbToGain(float db)
    {
        return MathF.Pow(10f, db / 20.0f);
    }
}