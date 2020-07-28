using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class BlockingToneSampleProvider : ISampleProvider
    {
        // Const Math
        private const double TwoPi = 2 * Math.PI;

        // Generator variable
        private int nSample;

        public BlockingToneSampleProvider()
            : this(44100, 2)
        {
        }

        /// <summary>
        /// Initializes a new instance for the Generator (UserDef SampleRate &amp; Channels)
        /// </summary>
        /// <param name="sampleRate">Desired sample rate</param>
        /// <param name="channel">Number of channels</param>
        public BlockingToneSampleProvider(int sampleRate, int channel)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channel);

            // Default
            Frequency = 440.0;
            Gain = 1;
            PhaseReverse = new bool[channel];
        }

        /// <summary>
        /// The waveformat of this WaveProvider (same as the source)
        /// </summary>
        public WaveFormat WaveFormat { get; }

        /// <summary>
        /// Frequency for the Generator. (20.0 - 20000.0 Hz)
        /// Sin, Square, Triangle, SawTooth, Sweep (Start Frequency).
        /// </summary>
        public double Frequency { get; set; }

        /// <summary>
        /// Return Log of Frequency Start (Read only)
        /// </summary>
        public double FrequencyLog => Math.Log(Frequency);


        /// <summary>
        /// Gain for the Generator. (0.0 to 1.0)
        /// </summary>
        private double gain;
        public double Gain
        {
            get { return gain; }
            set
            {
                gain = value;
                if (gain == 0)
                    nSample = 0;        //Reset the phase accumulator
            }
        }

        /// <summary>
        /// Channel PhaseReverse
        /// </summary>
        public bool[] PhaseReverse { get; }

        /// <summary>
        /// Reads from this provider.
        /// </summary>
        public int Read(float[] buffer, int offset, int count)
        {
            int outIndex = offset;

            // Generator current value
            double multiple;
            double sampleValue = 0;

            // Complete Buffer
            for (int sampleCount = 0; sampleCount < count / WaveFormat.Channels; sampleCount++)
            {
                // Sinus Generator
                if (Gain > 0)
                {
                    multiple = TwoPi * Frequency / WaveFormat.SampleRate;
                    sampleValue = Gain * Math.Sin(nSample * multiple);
                }

                nSample++;

                // Phase Reverse Per Channel
                for (int i = 0; i < WaveFormat.Channels; i++)
                {
                    if (PhaseReverse[i])
                        buffer[outIndex++] = (float)-sampleValue;
                    else
                        buffer[outIndex++] = (float)sampleValue;
                }
            }
            return count;
        }
    }
}
