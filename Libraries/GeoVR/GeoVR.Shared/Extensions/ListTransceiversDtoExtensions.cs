using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeoVR.Shared
{
    public static class ListTransceiversDtoExtensions
    {
        public static void AddStationTransceivers(this List<TransceiverDto> transceivers, List<StationTransceiverDto> stationTransceivers, uint frequency)
        {
            ushort id = 0;
            if (transceivers.Count > 0)
            {
                id = transceivers.Max(x => x.ID);
            }

            foreach (var stationTransceiver in stationTransceivers)
            {
                transceivers.Add(stationTransceiver.ToTransceiverDto(id++, frequency));
            }
        }
    }
}
