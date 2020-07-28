using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public class PostStationSearchResponseDto
    {
        public IEnumerable<StationDto> Stations { get; set; }
        public int Total { get; set; }
    }
}
