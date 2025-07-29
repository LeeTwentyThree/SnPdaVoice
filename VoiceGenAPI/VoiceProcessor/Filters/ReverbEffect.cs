using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

public class ReverbEffect : SampleSourceEffect
{
    private readonly float[][] _delayLines;
    private readonly int[] _delayIndices;
    private readonly int[] _tapDelays;
    private readonly float _decay;
    private readonly float _mix;

    public ReverbEffect(ISampleSource source, ReverbSettings settings)
        : base(source)
    {
        var sampleRate = source.WaveFormat.SampleRate;

        // Multiple short taps for early reflections (in samples)
        _tapDelays = new[] {
            (int)(sampleRate * 0.015f),
            (int)(sampleRate * 0.030f),
            (int)(sampleRate * 0.045f),
            (int)(sampleRate * 0.060f)
        };

        _delayLines = new float[_tapDelays.Length][];
        _delayIndices = new int[_tapDelays.Length];

        for (int i = 0; i < _tapDelays.Length; i++)
        {
            _delayLines[i] = new float[_tapDelays[i]];
            _delayIndices[i] = 0;
        }

        _decay = settings.Decay;
        _mix = settings.Mix;
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int read = base.Read(buffer, offset, count);

        for (int i = 0; i < read; i++)
        {
            float dry = buffer[offset + i];
            float wet = 0f;

            for (int t = 0; t < _tapDelays.Length; t++)
            {
                float[] delayLine = _delayLines[t];
                int index = _delayIndices[t];

                wet += delayLine[index]; // collect echo
                delayLine[index] = dry + delayLine[index] * _decay; // feedback

                _delayIndices[t] = (index + 1) % delayLine.Length;
            }

            buffer[offset + i] = dry * (1 - _mix) + (wet / _tapDelays.Length) * _mix;
        }

        return read;
    }
}