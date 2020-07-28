using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public static class StationTransceiverDtoExtensions
    {
        public static TransceiverDto ToTransceiverDto(this StationTransceiverDto stationTransceiver, ushort id, uint frequency)
        {
            return new TransceiverDto()
            {
                ID = id,
                Frequency = frequency,
                LatDeg = stationTransceiver.LatDeg,
                LonDeg = stationTransceiver.LonDeg,
                HeightMslM = stationTransceiver.HeightMslM,
                HeightAglM = stationTransceiver.HeightAglM
            };
        }
    }
}
