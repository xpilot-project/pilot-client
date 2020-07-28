using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class PutStationTransceiverExclusionsRequestDto
    {
        public List<Guid> TransceiverIDs { get; set; }
    }
}
