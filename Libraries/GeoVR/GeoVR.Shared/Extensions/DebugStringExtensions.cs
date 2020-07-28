using GeoVR.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Shared
{
    public static class DebugStringExtensions
    {
        public static string ToDebugString(this AudioRxDto dto)
        {
            return nameof(AudioRxDto) +
                " [Callsign: " + dto.Callsign +
                "] [SequenceCounter: " + dto.SequenceCounter.ToString() +
                "] [Last Packet: " + dto.LastPacket +
                "] [TransceiverIDs: " + string.Join(",", dto.Transceivers.Select(x => "{" + x.ID.ToString() + " " + x.DistanceRatio.ToString() + "}")) +
                "]";
        }

        public static string ToDebugString(this AudioTxDto dto)
        {
            return nameof(AudioTxDto) +
                " [Callsign: " + dto.Callsign +
                "] [SequenceCounter: " + dto.SequenceCounter.ToString() +
                "] [Last Packet: " + dto.LastPacket +
                "] [TransceiverIDs: " + string.Join(",", dto.Transceivers.Select(x => "{" + x.ID.ToString() + "}")) +
                "]";
        }
        
        //public static string ToDebugString(this AudioOnVoiceRoomDto dto)
        //{
        //    return nameof(AudioOnVoiceRoomDto) +
        //        " [Callsign: " + dto.Callsign +
        //        "] [SequenceCounter: " + dto.SequenceCounter.ToString() +
        //        "] [Last Packet: " + dto.LastPacket +
        //        "] [Voice Room: " + dto.VoiceRoom +
        //        "]";
        //}
    }
}
