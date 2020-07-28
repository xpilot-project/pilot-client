using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    /// <summary>
    /// Basic example of a multi-band eq
    /// uses the same settings for both channels in stereo audio
    /// Call Update after you've updated the bands
    /// Potentially to be added to NAudio in a future version
    /// </summary>
    public class EqualizerSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider sourceProvider;
        private readonly BiQuadFilter[,] filters;
        private readonly int channels;
        private readonly int bandCount;
        private bool updated;

        public EqualizerSampleProvider(ISampleProvider sourceProvider, EqualizerPresets preset)
        {
            this.sourceProvider = sourceProvider;
            channels = this.sourceProvider.WaveFormat.Channels;
            filters = SetupPreset(preset, this.sourceProvider.WaveFormat.SampleRate);
            bandCount = filters.Length;
            Bypass = false;
            OutputGain = 1.0;
        }

        public EqualizerSampleProvider(ISampleProvider sourceProvider, BiQuadFilter[,] filters)
        {
            this.sourceProvider = sourceProvider;
            channels = this.sourceProvider.WaveFormat.Channels;
            this.filters = filters;
            bandCount = filters.Length;
            Bypass = false;
            OutputGain = 1.0;
        }

        public WaveFormat WaveFormat => sourceProvider.WaveFormat;
        public bool Bypass { get; set; }
        public double OutputGain { get; set; }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesRead = sourceProvider.Read(buffer, offset, count);
            if (Bypass)
                return samplesRead;

            if (updated)
            {
                // CreateFilters();
                updated = false;
            }

            for (int n = 0; n < samplesRead; n++)
            {
                int ch = n % channels;

                for (int band = 0; band < bandCount; band++)
                {
                    buffer[offset + n] = filters[ch, band].Transform(buffer[offset + n]);
                }

                buffer[offset + n] *= (float)OutputGain;
            }
            return samplesRead;
        }

        private BiQuadFilter[,] SetupPreset(EqualizerPresets preset, float sampleRate)
        {
            BiQuadFilter[,] filters;
            switch (preset)
            {
                case EqualizerPresets.VHFEmulation:
                    filters = new BiQuadFilter[channels, 5];

                    for (int i = 0; i < channels; i++)
                    {
                        filters[i, 0] = BiQuadFilter.HighPassFilter(sampleRate, 310, 0.25f);
                        filters[i, 1] = BiQuadFilter.PeakingEQ(sampleRate, 450, 0.75f, 17.0f);
                        filters[i, 2] = BiQuadFilter.PeakingEQ(sampleRate, 1450, 1.0f, 25.0f);
                        filters[i, 3] = BiQuadFilter.PeakingEQ(sampleRate, 2000, 1.0f, 25.0f);
                        filters[i, 4] = BiQuadFilter.LowPassFilter(sampleRate, 2500, 0.25f);
                    }
                    break;
                case EqualizerPresets.VHFEmulation2:
                    filters = new BiQuadFilter[channels, 7];

                    for (int i = 0; i < channels; i++)
                    {
                        filters[i, 0] = BiQuadFilterExt.Build(1.0, 0.0, 0.0, -0.01, 0.0, 0.0);
                        filters[i, 1] = BiQuadFilterExt.Build(1.0, -1.7152995098277, 0.761385315196423, 0.0, 1.0, 0.753162969638192);
                        filters[i, 2] = BiQuadFilterExt.Build(1.0, -1.71626681678914, 0.762433947105989, 1.0, -2.29278115712509, 1.00033663293577);
                        filters[i, 3] = BiQuadFilterExt.Build(1.0, -1.79384214686345, 0.909678364879526, 1.0, -2.05042803669041, 1.05048374237779);
                        filters[i, 4] = BiQuadFilterExt.Build(1.0, -1.79409285259567, 0.909822671281377, 1.0, -1.95188929743297, 0.951942325888074);
                        filters[i, 5] = BiQuadFilterExt.Build(1.0, -1.9390093095185, 0.9411847259142, 1.0, -1.82547932903698, 1.09157529229851);
                        filters[i, 6] = BiQuadFilterExt.Build(1.0, -1.94022767750807, 0.942630574503006, 1.0, -1.67241244173042, 0.916184578658119);
                    }
                    break;
                default:
                    throw new Exception("Preset not defined");
            }
            return filters;
        }
    }

    public enum EqualizerPresets
    {
        [Description("Narrow VHF EQ (Legacy)")]
        VHFEmulation = 1,
        [Description("Wide VHF EQ")]
        VHFEmulation2
    }
}