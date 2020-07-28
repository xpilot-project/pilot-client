using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class CrossCoupleGroupDto : IMsgPack
    {
        [Key(0)]
        public ushort ID { get; set; }
        [Key(1)]
        public List<ushort> TransceiverIDs { get; set; }
    }
}
