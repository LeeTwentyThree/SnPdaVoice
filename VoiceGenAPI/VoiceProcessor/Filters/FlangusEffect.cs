using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

public class FlangusEffect : SampleSourceEffect
{
    private readonly int _sampleRate;
    private readonly FlangusSettings _settings;
    private readonly float[][] _delayLinesLeft;
    private readonly float[][] _delayLinesRight;
    private readonly int[] _delayIndices;

    private readonly float[] _lfoPhases;
    private readonly Random _rand = new();

    private readonly int _maxDelaySamples;
    
    private const float MaxDelayMs = 50f;

    public FlangusEffect(ISampleSource source, FlangusSettings settings)
        : base(source)
    {
        _settings = settings;
        _sampleRate = WaveFormat.SampleRate;

        int order = Math.Max(1, settings.Order);
        _delayLinesLeft = new float[order][];
        _delayLinesRight = new float[order][];
        _delayIndices = new int[order];
        _lfoPhases = new float[order];

        _maxDelaySamples = (int)(_sampleRate * MaxDelayMs / 1000f);

        for (int i = 0; i < order; i++)
        {
            _delayLinesLeft[i] = new float[_maxDelaySamples];
            _delayLinesRight[i] = new float[_maxDelaySamples];
            _delayIndices[i] = 0;
            _lfoPhases[i] = (float)_rand.NextDouble(); // slight random phase offset per voice
        }
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int read = base.Read(buffer, offset, count);
        bool isStereo = WaveFormat.Channels == 2;
        int order = _settings.Order;

        for (int i = 0; i < read; i += WaveFormat.Channels)
        {
            float dryL = buffer[offset + i];
            float dryR = isStereo ? buffer[offset + i + 1] : dryL;

            float sumWetL = 0f, sumWetR = 0f;

            for (int v = 0; v < order; v++)
            {
                // Spread modulation
                float spreadFactor = v / (float)Math.Max(order - 1, 1);
                float variation = (_settings.Spread * (spreadFactor - 0.5f) * 2f);

                // Modulated depth/speed per voice, centered around base value
                float depth = Math.Clamp(_settings.Depth * (1f + variation), 0.001f, 0.02f);
                float speed = Math.Clamp(_settings.Speed * (1f + variation), 0.05f, 2f);
                

                float lfoPhase = (_lfoPhases[v] + speed / _sampleRate) % 1f;
                _lfoPhases[v] = lfoPhase;

                float lfo = MathF.Sin(2f * MathF.PI * lfoPhase);
                const float minDelayMs = 1f;
                float depthMs = Math.Clamp(_settings.Depth, 0.1f, MaxDelayMs);
                float baseDelayMs = Math.Clamp(_settings.Delay, minDelayMs, MaxDelayMs);

                float modulatedDelayMs = baseDelayMs + depthMs * lfo;
                modulatedDelayMs = Math.Clamp(modulatedDelayMs, minDelayMs, MaxDelayMs);

                int delaySamples = (int)(_sampleRate * (modulatedDelayMs / 1000f));
                delaySamples = Math.Clamp(delaySamples, 0, _maxDelaySamples - 1);

                int delayIdx = _delayIndices[v];
                int readIdx = (delayIdx - delaySamples + _maxDelaySamples) % _maxDelaySamples;

                float wetL = _delayLinesLeft[v][readIdx];
                float wetR = _delayLinesRight[v][readIdx];

                // Cross-feedback with stereo inversion if needed
                float cross = _settings.StereoCross;
                float feedbackL = dryL + wetR * cross * _settings.Feedback;
                float feedbackR = dryR + wetL * cross * _settings.Feedback;

                _delayLinesLeft[v][delayIdx] = feedbackL;
                _delayLinesRight[v][delayIdx] = feedbackR;

                _delayIndices[v] = (delayIdx + 1) % _maxDelaySamples;

                sumWetL += wetL;
                sumWetR += wetR;
            }

            float avgWetL = sumWetL / order;
            float avgWetR = sumWetR / order;

            float dryGain = _settings.Dry;
            float wetGain = _settings.Wet;

            // Final output with dry/wet and cross support
            float outL = Clamp(dryL * dryGain + avgWetL * wetGain);
            float outR = Clamp(dryR * dryGain + avgWetR * wetGain);

            buffer[offset + i] = outL;
            if (isStereo)
                buffer[offset + i + 1] = outR;
        }

        return read;
    }
    
    float Clamp(float x) => Math.Clamp(x, -1f, 1f);
}
