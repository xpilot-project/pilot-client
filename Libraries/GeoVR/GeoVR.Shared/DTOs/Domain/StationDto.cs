using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class StationDto
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public uint? Frequency { get; set; }
        public uint? FrequencyAlias { get; set; }
    }
}
