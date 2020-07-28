using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class VoiceRoomRxCallsignsDto : IMsgPack      //This is a computed object
    {
        [Key(0)]
        public VoiceRoomDto VoiceRoom { get; set; }
        [Key(1)]
        public string[] RxCallsigns { get; set; }
    }
}