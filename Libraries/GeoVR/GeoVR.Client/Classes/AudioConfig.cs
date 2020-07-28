using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class AudioConfig
    {
        //Thread-safe singleton Pattern
        private static readonly Lazy<AudioConfig> lazy = new Lazy<AudioConfig>(() => new AudioConfig());

        public static AudioConfig Instance { get { return lazy.Value; } }

        private AudioConfig()
        {
            VhfEqualizer = EqualizerPresets.VHFEmulation;
        }
        //End of singleton pattern

        public EqualizerPresets VhfEqualizer { get; set; }
    }
}
