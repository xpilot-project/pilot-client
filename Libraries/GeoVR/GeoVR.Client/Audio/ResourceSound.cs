using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class ResourceSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }
        public string FileName { get; private set; }
        public ResourceSound(string audioFileName)
        {
            FileName = audioFileName;
            using (var audioFileReader = new WaveFileReader(ExtractResourceStream(audioFileName)))
            {
                // TODO: could add resampling in here if required
                WaveFormat = audioFileReader.WaveFormat;
                var sampleProvider = audioFileReader.ToSampleProvider();
                AudioData = new float[audioFileReader.SampleCount];
                sampleProvider.Read(AudioData, 0, (int)audioFileReader.SampleCount);
            }
        }

        private static Stream ExtractResourceStream(string filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            return a.GetManifestResourceStream(filename);
        }
    }
}
