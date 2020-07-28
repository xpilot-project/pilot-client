using MessagePack;
using System.Collections.Generic;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class RxCallsignDto : IMsgPack
    {
        [Key(0)]
        public string Callsign { get; set; }
        [Key(1)]
        public RxTransceiverDto[] RxTransceivers { get; set; }
    }
}
