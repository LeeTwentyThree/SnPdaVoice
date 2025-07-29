using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

public class CompressorEffect : SampleSourceEffect
{
    private readonly float _threshold;
    private readonly float _makeupGain;
    private readonly float _kneeWidth;
    private readonly float _ratio;
    private readonly float _smoothing;

    private float _envelope;

    public CompressorEffect(ISampleSource source, CompressorSettings settings) : base(source)
    {
        _threshold = DbToLinear(settings.ThresholdDb);
        _makeupGain = DbToLinear(settings.MakeupGainDb);
        _kneeWidth = settings.KneeWidthDb;
        _ratio = settings.Ratio;
        _smoothing = settings.Smoothing;
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = base.Read(buffer, offset, count);
        for (int i = 0; i < samplesRead; i++)
        {
            float input = buffer[offset + i];
            float inputLevel = Math.Abs(input);

            // Envelope follower with smoothing
            _envelope += (_smoothing * (inputLevel - _envelope));

            float gain = ComputeGain(_envelope);
            buffer[offset + i] = input * gain * _makeupGain;
        }

        return samplesRead;
    }

    private float ComputeGain(float level)
    {
        if (level <= _threshold)
            return 1.0f;

        float dbAbove = LinearToDb(level) - LinearToDb(_threshold);

        // Apply soft knee
        float gainReductionDb = 0;
        if (_kneeWidth > 0 && dbAbove < _kneeWidth)
        {
            float x = dbAbove / _kneeWidth;
            gainReductionDb = (1 - 1 / _ratio) * x * x * _kneeWidth / 2;
        }
        else
        {
            gainReductionDb = dbAbove * (1 - 1 / _ratio);
        }

        return DbToLinear(-gainReductionDb);
    }

    private static float DbToLinear(float db)
    {
        return (float)Math.Pow(10.0, db / 20.0);
    }

    private static float LinearToDb(float linear)
    {
        return 20.0f * (float)Math.Log10(Math.Max(linear, 1e-8f));
    }
}