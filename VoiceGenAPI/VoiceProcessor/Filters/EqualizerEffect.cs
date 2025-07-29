using CSCore;
using VoiceProcessor.Data.FilterSettings;

namespace VoiceProcessor.Filters
{
    public class EqualizerEffect : SampleSourceEffect
    {
        private readonly List<BiQuadFilter> _filters;

        public EqualizerEffect(ISampleSource source, EqualizerSettings settings)
            : base(source)
        {
            
            _filters = new List<BiQuadFilter>();
            int sampleRate = source.WaveFormat.SampleRate;

            foreach (var band in settings.Bands)
            {
                var filter = BiQuadFilter.PeakingEq(
                    sampleRate,
                    band.FrequencyHz,
                    band.Q,
                    band.GainDb
                );
                _filters.Add(filter);
            }
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = base.Read(buffer, offset, count);

            for (int i = 0; i < samplesRead; i++)
            {
                float sample = buffer[offset + i];
                foreach (var filter in _filters)
                {
                    sample = filter.Process(sample);
                }

                buffer[offset + i] = sample;
            }

            return samplesRead;
        }
    }
}