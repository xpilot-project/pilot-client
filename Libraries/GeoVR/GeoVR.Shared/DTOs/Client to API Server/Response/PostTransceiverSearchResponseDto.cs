using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class PostTransceiverSearchResponseDto
    {
        public IEnumerable<StationTransceiverDto> Transceivers { get; set; }
        public int Total { get; set; }
    }
}