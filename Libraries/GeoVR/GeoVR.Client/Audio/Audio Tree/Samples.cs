using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class Samples
    {
        //Singleton Pattern
        private static readonly Lazy<Samples> lazy = new Lazy<Samples>(() => new Samples());

        public static Samples Instance { get { return lazy.Value; } }

        private Samples()
        {
            HFWhiteNoise = new ResourceSound("GeoVR.Client.Samples.HF_WhiteNoise_f32.wav");
            WhiteNoise = new ResourceSound("GeoVR.Client.Samples.WhiteNoise_f32.wav");
            Crackle = new ResourceSound("GeoVR.Client.Samples.Crackle_f32.wav");
            Click = new ResourceSound("GeoVR.Client.Samples.Click_f32.wav");
            AcBus = new ResourceSound("GeoVR.Client.Samples.AC_Bus_f32.wav");
            LandLineRing = new ResourceSound("GeoVR.Client.Samples.Land_Line_Ring_f32.wav");
        }
        //End of Singleton Pattern

        public ResourceSound Crackle { get; private set; }
        public ResourceSound Click { get; private set; }
        public ResourceSound WhiteNoise { get; private set; }
        public ResourceSound HFWhiteNoise { get; private set; }
        public ResourceSound AcBus { get; private set; }
        public ResourceSound LandLineRing { get; private set; }
    }
}
