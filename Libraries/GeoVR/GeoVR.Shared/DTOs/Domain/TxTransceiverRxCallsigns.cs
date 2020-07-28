using MessagePack;
using System.Collections.Generic;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class TxTransceiverRxCallsignsDto : IMsgPack      //This is a computed object
    {
        [Key(0)]
        public ushort ID { get; set; }
        [Key(1)]
        public RxCallsignDto[] RxCallsigns { get; set; }
    }
}
