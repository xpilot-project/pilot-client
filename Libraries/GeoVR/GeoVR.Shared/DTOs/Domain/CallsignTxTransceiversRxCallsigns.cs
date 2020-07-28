using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class CallsignTxTransceiversRxCallsigns      //Used for Web API return
    {
        [Key(0)]
        public string Callsign { get; set; }
        [Key(1)]
        public TxTransceiverRxCallsignsDto[] TxTransceivers { get; set; }
    }
}
