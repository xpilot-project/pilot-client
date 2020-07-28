using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class PostStationTransceiverSearchRequestDto
    {
        public int Take { get; set; }
        public int Skip { get; set; }
        public string SearchText { get; set; }
    }
}
