using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

public class FlangerEffect : SampleSourceEffect
{
    private readonly float[] _delayBufferLeft;
    private readonly float[] _delayBufferRight;
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
        int bufferSize = _sampleRate; // 1 second max delay
        _delayBufferLeft = new float[bufferSize];
        _delayBufferRight = new float[bufferSize];
        _delayIndex = 0;
        _phase = 0f;
        _settings = settings;

        _wetGain = DbToGain(_settings.WetGain);
        _dryGain = DbToGain(_settings.DryGain);
        _crossGain = DbToGain(_settings.CrossGain);
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = base.Read(buffer, offset, count);
        if (WaveFormat.Channels != 2)
            throw new InvalidOperationException("FlangerEffect requires stereo input.");

        float effectAmount = Math.Clamp(_settings.EffectAmount, 0f, 1f);
        float feedback = Math.Clamp(_settings.Feedback, 0f, 1f);

        for (int i = 0; i < samplesRead; i += 2)
        {
            float dryL = buffer[offset + i];
            float dryR = buffer[offset + i + 1];

            float lfoL = GetLFO(_phase);
            float lfoR = GetLFO((_phase + 0.1f) % 1f); // measured in degrees, this is like 36

            float delayMsL = Math.Max(_settings.Delay + _settings.Depth * lfoL, 0.1f);
            float delayMsR = Math.Max(_settings.Delay + _settings.Depth * lfoR, 0.1f);

            int delaySamplesL = (int)(_sampleRate * delayMsL / 1000f);
            int delaySamplesR = (int)(_sampleRate * delayMsR / 1000f);

            int readIdxL = (_delayIndex - delaySamplesL + _delayBufferLeft.Length) % _delayBufferLeft.Length;
            int readIdxR = (_delayIndex - delaySamplesR + _delayBufferRight.Length) % _delayBufferRight.Length;

            float delayedL = _delayBufferLeft[readIdxL];
            float delayedR = _delayBufferRight[readIdxR];

            float effectedL = dryL * _dryGain + delayedL * _wetGain;
            float effectedR = dryR * _dryGain + delayedR * _wetGain;

            buffer[offset + i] = dryL * (1f - effectAmount) + effectedL * effectAmount;
            buffer[offset + i + 1] = dryR * (1f - effectAmount) + effectedR * effectAmount;

            _delayBufferLeft[_delayIndex] = dryL + delayedR * feedback * _crossGain;
            _delayBufferRight[_delayIndex] = dryR + delayedL * feedback * _crossGain;

            _delayIndex = (_delayIndex + 1) % _delayBufferLeft.Length;
            _phase += _settings.Rate / _sampleRate;
            if (_phase > 1f) _phase -= 1f;
        }

        return samplesRead;
    }

    private float GetLFO(float phase) => _settings.Waveform.ToLower() switch
    {
        "sine" => MathF.Sin(2f * MathF.PI * phase),
        "triangle" => 2f * Math.Abs(2f * (phase - MathF.Floor(phase + 0.5f))) - 1f,
        _ => 0f
    };

    private static float DbToGain(float db) => MathF.Pow(10f, db / 20f);
}