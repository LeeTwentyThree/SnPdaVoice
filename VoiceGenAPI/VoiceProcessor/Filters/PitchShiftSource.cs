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
            float sample = _buffer[index] * (1 - frac) + _buffer[index + 1] * frac;

            buffer[offset + i] = sample;

            _position += _pitchFactor;
            samplesRead++;
        }

        return samplesRead;
    }
}