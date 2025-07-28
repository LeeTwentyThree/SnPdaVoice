using CSCore;

namespace VoiceProcessor.Filters;

public abstract class SampleSourceEffect : ISampleSource
{
    protected readonly ISampleSource Source;

    protected SampleSourceEffect(ISampleSource source)
    {
        Source = source;
    }

    public virtual int Read(float[] buffer, int offset, int count)
    {
        return Source.Read(buffer, offset, count);
    }

    public virtual bool CanSeek => Source.CanSeek;
    public virtual WaveFormat WaveFormat => Source.WaveFormat;
    public virtual long Position
    {
        get => Source.Position;
        set => Source.Position = value;
    }

    public virtual long Length => Source.Length;

    public virtual void Dispose()
    {
        Source.Dispose();
    }
}