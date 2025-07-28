using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters;

public class TimePaddingEffect : SampleSourceEffect
{
    private readonly int _samplesBeforeTotal;
    private readonly int _samplesAfterTotal;

    private int _samplesBeforeEmitted;
    private int _samplesAfterEmitted;

    private bool _sourceEnded;

    public TimePaddingEffect(ISampleSource source, TimePaddingSettings settings) : base(source)
    {
        _samplesBeforeTotal = (int)(settings.MillisecondsBefore / 1000f * source.WaveFormat.SampleRate);
        _samplesAfterTotal = (int)(settings.MillisecondsAfter / 1000f * source.WaveFormat.SampleRate);

        _samplesBeforeEmitted = 0;
        _samplesAfterEmitted = 0;
        _sourceEnded = false;
    }

    public override int Read(float[] buffer, int offset, int count)
    {
        int samplesWritten = 0;

        // Silence
        while (_samplesBeforeEmitted < _samplesBeforeTotal && samplesWritten < count)
        {
            buffer[offset + samplesWritten] = 0f;
            samplesWritten++;
            _samplesBeforeEmitted++;
        }

        if (samplesWritten == count)
            return samplesWritten;

        // Read from source
        int readFromSource = 0;
        if (!_sourceEnded)
        {
            readFromSource = base.Read(buffer, offset + samplesWritten, count - samplesWritten);
            samplesWritten += readFromSource;

            if (readFromSource == 0)
                _sourceEnded = true;
        }

        if (samplesWritten == count)
            return samplesWritten;

        // Silence
        while (_samplesAfterEmitted < _samplesAfterTotal && samplesWritten < count)
        {
            buffer[offset + samplesWritten] = 0f;
            samplesWritten++;
            _samplesAfterEmitted++;
        }

        return samplesWritten;
    }
}