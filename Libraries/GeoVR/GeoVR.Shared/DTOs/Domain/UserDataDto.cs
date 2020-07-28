using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class CallsignDataDto : IMsgPack     //Basically a copy of GeoVR.Shared.Server.CallsignTransceiversDto
    {
        [Key(0)]
        public string Callsign { get; set; }
        [Key(1)]
        public TransceiverDto[] Transceivers { get; set; }

        public CallsignDataDto() { }
        public CallsignDataDto(string callsign)
        {
            Callsign = callsign;
            Transceivers = new TransceiverDto[0];
        }
    }

    [MessagePackObject]
    public class UserDataDto : IMsgPack
    {
        [Key(0)]
        public string User { get; set; }
        [Key(1)]
        public List<CallsignDataDto> Callsigns { get; set; }
    }
}
