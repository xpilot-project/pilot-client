using MoreLinq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoVR.Shared
{
    public class StationTreeDto : StationDto
    {
        public List<StationTransceiverDto> Transceivers { get; set; }
        public List<StationTreeDto> ChildStations { get; set; }
    }

    public static class StationDtoExtensions
    {
        public static List<StationTransceiverDto> GetStationTransceiversFull(this StationTreeDto station)
        {
            var transceivers = new List<StationTransceiverDto>();
            transceivers.AddRange(station.Transceivers);
            foreach (var childStation in station.ChildStations)
            {
                transceivers.AddRange(childStation.GetStationTransceiversFull());
            }
            return transceivers.OrderBy(x => x.Name).ToList();
        }

        public static List<StationTransceiverDto> GetStationTransceiversFullDistinct(this StationTreeDto station)
        {
            return GetStationTransceiversFull(station).DistinctBy(x => x.Name).ToList();
        }

        //public static void AddTransceivers(this StationTreeDto station, List<TransceiverDto> transceivers)
        //{
        //    ushort startingID = 0;
        //    if (transceivers == null)
        //    {
        //        transceivers = new List<TransceiverDto>();
        //    }
        //    if (transceivers.Count > 0)
        //    {
        //        startingID = transceivers.Max(x => x.ID);
        //    }
        //    if (station.Frequency != null)
        //    {
        //        var stationTransceivers = station.GetStationTransceiversFullDistinct();
        //        foreach (var selectedStationTransceiver in stationTransceivers)
        //        {
        //            transceivers.Add(new TransceiverDto()
        //            {
        //                ID = startingID++,
        //                Frequency = (uint)station.Frequency,
        //                LatDeg = selectedStationTransceiver.LatDeg,
        //                LonDeg = selectedStationTransceiver.LonDeg,
        //                HeightMslM = selectedStationTransceiver.HeightMslM,
        //                HeightAglM = selectedStationTransceiver.HeightAglM
        //            });
        //        }
        //    }
        //}

        //public static void AddTopDownTransceivers(this StationTreeDto station, List<TransceiverDto> transceivers, List<string> stationNames)
        //{
        //    foreach (var stationName in stationNames)
        //    {
        //        if (station.Name == stationName)
        //            continue;       //This is the parent station which shouldn't be added to the transceiver list. Only children allowed otherwise you get overlapping transceivers on different frequencies.
        //        var selectedStation = station.GetStation(stationName);
        //        if (selectedStation.Frequency == null)
        //            continue;       //The code calling this method should not have passed in this station name

        //        selectedStation.AddTransceivers(transceivers);
        //    }
        //}



        //private static StationTreeDto GetStation(this StationTreeDto station, string stationName)       //This will return top level or child station
        //{
        //    if (station.Name == stationName)
        //    {
        //        return station;
        //    }
        //    else
        //    {
        //        foreach (var childStation in station.ChildStations)
        //        {
        //            var asdf = childStation.GetStation(stationName);
        //            if (asdf != null)
        //                return asdf;
        //        }
        //    }
        //    return null;
        //}
    }
}
