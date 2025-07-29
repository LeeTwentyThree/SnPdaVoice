using CSCore;

namespace VoiceProcessor.Filters;

public class PitchShiftSource : ISampleSource
{
    private readonly float[] _buffer;
    private readonly float _pitchFactor;
    private float _position;

    public bool CanSeek => false;
    public WaveFormat WaveFormat { get; }
    public long Length => 0;

    public long Position
    {
        get => (long)_position;
        set => throw new NotSupportedException();
    }

    public void Dispose()
    {
    }

    public PitchShiftSource(float[] buffer, WaveFormat waveFormat, float semitones)
    {
        _buffer = buffer;
        WaveFormat = waveFormat;
        _pitchFactor = (float)Math.Pow(2, semitones / 12.0);
        _position = 0f;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesRead = 0;

        for (int i = 0; i < count; i++)
        {
            int index = (int)_position;
            if (index >= _buffer.Length - 1)
                break;

            float frac = _position - index;
            
            int i0 = (int)_position - 1;
            int i1 = i0 + 1;
            int i2 = i0 + 2;
            int i3 = i0 + 3;

            float sample = CubicInterpolate(
                _buffer[Math.Max(0, i0)],
                _buffer[Math.Max(0, i1)],
                _buffer[Math.Min(_buffer.Length - 1, i2)],
                _buffer[Math.Min(_buffer.Length - 1, i3)],
                frac
            );

            buffer[offset + i] = sample;

            _position += _pitchFactor;
            samplesRead++;
        }

        return samplesRead;
    }
    
    private float CubicInterpolate(float a, float b, float c, float d, float t)
    {
        float t2 = t * t;
        float a0 = d - c - a + b;
        float a1 = a - b - a0;
        float a2 = c - a;
        float a3 = b;

        return a0 * t * t2 + a1 * t2 + a2 * t + a3;
    }
}