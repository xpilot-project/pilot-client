using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class StationTransceiverDto
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public double LatDeg { get; set; }
        public double LonDeg { get; set; }
        public double HeightMslM { get; set; }
        public double HeightAglM { get; set; }
    }
}
