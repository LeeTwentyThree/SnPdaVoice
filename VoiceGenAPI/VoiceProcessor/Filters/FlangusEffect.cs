using CSCore;
using VoiceProcessor.Data.FilterSettings;
using VoiceProcessor.Utilities;

namespace VoiceProcessor.Filters;

// I hardly know what is going on in this file
public class FlangusEffect : SampleSourceEffect
{
    private readonly ISampleSource _source;
    private readonly int _sampleRate;
    private readonly CircularBuffer _delayBufferL;
    private readonly CircularBuffer _delayBufferR;

    private int _frame;

    private readonly FlangusSettings _settings;

    public FlangusEffect(ISampleSource source, FlangusSettings settings) : base(source)
    {
        _source = source;
        _settings = settings;
        _sampleRate = source.WaveFormat.SampleRate;

        int maxDelaySamples = (int)(_sampleRate * settings.MaxDelayMs / 1000f) + 1;
        _delayBufferL = new CircularBuffer(maxDelaySamples);
        _delayBufferR = new CircularBuffer(maxDelaySamples);
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int read = _source.Read(buffer, offset, count);
        for (int i = 0; i < read; i += 2)
        {
            float time = _frame++ / (float)_sampleRate;

            float lfoL = (float)Math.Sin(2 * Math.PI * _settings.LfoFrequency * time);
            float lfoR = (float)Math.Sin(2 * Math.PI * _settings.LfoFrequency * time + Math.PI);

            float delaySamplesL = (_sampleRate * _settings.MaxDelayMs / 1000f) * ((lfoL + 1) / 2f);
            float delaySamplesR = (_sampleRate * _settings.MaxDelayMs / 1000f) * ((lfoR + 1) / 2f);

            delaySamplesL = Math.Min(delaySamplesL, _delayBufferL.Length - 2);
            delaySamplesR = Math.Min(delaySamplesR, _delayBufferR.Length - 2);

            float inL = buffer[offset + i];
            float inR = buffer[offset + i + 1];

            float delayedL = _delayBufferL.ReadInterpolated(delaySamplesL);
            float delayedR = _delayBufferR.ReadInterpolated(delaySamplesR);

            float mixedL = inL + delayedL * _settings.WetMix;
            float mixedR = inR + delayedR * _settings.WetMix;

            buffer[offset + i] = Math.Clamp(mixedL, -1f, 1f);
            buffer[offset + i + 1] = Math.Clamp(mixedR, -1f, 1f);

            _delayBufferL.Write(inL + delayedL * _settings.Feedback);
            _delayBufferR.Write(inR + delayedR * _settings.Feedback);
        }

        return read;
    }
}