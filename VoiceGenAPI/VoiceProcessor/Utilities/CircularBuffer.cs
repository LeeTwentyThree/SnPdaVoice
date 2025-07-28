namespace VoiceProcessor.Utilities;

public class CircularBuffer(int size)
{
    private readonly float[] _buffer = new float[size];
    private int _writeIndex;

    public int Length { get; } = size;

    public void Write(float value)
    {
        _buffer[_writeIndex] = value;
        _writeIndex = (_writeIndex + 1) % _buffer.Length;
    }

    public float Read(int delaySamples)
    {
        int index = (_writeIndex - delaySamples + _buffer.Length) % _buffer.Length;
        return _buffer[index];
    }

    public float ReadInterpolated(float delaySamples)
    {
        int baseDelay = (int)Math.Floor(delaySamples);
        float frac = delaySamples - baseDelay;

        int index1 = (_writeIndex - baseDelay + _buffer.Length) % _buffer.Length;
        int index2 = (_writeIndex - baseDelay - 1 + _buffer.Length) % _buffer.Length;

        float sample1 = _buffer[index1];
        float sample2 = _buffer[index2];

        return (1 - frac) * sample1 + frac * sample2;
    }

    public void Clear()
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _writeIndex = 0;
    }
}
