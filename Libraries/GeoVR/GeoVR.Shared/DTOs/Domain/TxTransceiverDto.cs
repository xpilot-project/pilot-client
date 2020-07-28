using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class TxTransceiverDto : IMsgPack
    {
        [Key(0)]
        public ushort ID { get; set; }
    }
}
