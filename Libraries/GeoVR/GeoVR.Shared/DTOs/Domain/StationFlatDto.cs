using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class StationFlatDto : StationDto
    {
        public IEnumerable<Guid> TransceiverIDs { get; set; }
        public IEnumerable<Guid> ChildStationIDs { get; set; }
    }
}
