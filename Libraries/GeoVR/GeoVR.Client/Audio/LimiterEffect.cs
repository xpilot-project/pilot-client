using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client.Audio
{
    public class LimiterEffect : ISampleProvider
    {
        private readonly ISampleProvider sourceStream;
        private readonly object lockObject = new object();

        public WaveFormat WaveFormat => sourceStream.WaveFormat;
        public bool Enabled { get; set; } = true;

        public LimiterEffect(ISampleProvider sourceStream)
        {
            this.sourceStream = sourceStream;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            lock (lockObject)
            {
                int samplesRead = sourceStream.Read(buffer, offset, count);
                if (Enabled)
                {
                    for (int sample = 0; sample < samplesRead; sample++)
                    {
                        if (buffer[offset + sample] > 1.0f)
                            buffer[offset + sample] = 1.0f;
                        if (buffer[offset + sample] < -1.0f)
                            buffer[offset + sample] = -1.0f;
                    }
                }
                return samplesRead;
            }
        }
    }
}