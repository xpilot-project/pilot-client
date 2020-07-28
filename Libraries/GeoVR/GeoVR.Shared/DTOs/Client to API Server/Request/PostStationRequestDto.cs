using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class PostStationRequestDto
    {
        public string Name { get; set; }
        public uint? Frequency { get; set; }
        public uint? FrequencyAlias { get; set; }
        public List<Guid> TransceiverIDs { get; set; }
        public List<Guid> ChildStationIDs { get; set; }
    }
}
