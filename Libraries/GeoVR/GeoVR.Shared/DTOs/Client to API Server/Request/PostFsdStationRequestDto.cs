using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class PostFsdStationUpdateRequestDto
    {
        public uint Frequency { get; set; }
        public double LatDeg { get; set; }
        public double LonDeg { get; set; }
    }
}
