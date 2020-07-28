using MessagePack;
using MessagePack.CryptoDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    public enum CallResponseEvent
    {
        Undefined = 0,
        //Server response
        Routed = 1,
        NoRoute = 2,
        //Remote party response
        Busy = 3,
        Accept = 4,
        Reject = 5,
    }

    [MessagePackObject]
    [CryptoDto(ShortDtoNames.CallRequest)]
    public class CallRequestDto : IMsgPackTypeName, ICallDto
    {
        [IgnoreMember]
        public const string TypeNameConst = ShortDtoNames.CallRequest;
        [IgnoreMember]
        public string TypeName { get { return TypeNameConst; } }
        [Key(0)]
        public string FromCallsign { get; set; }
        [Key(1)]
        public string ToCallsign { get; set; }      //The callsign being rung
    }

    [MessagePackObject]
    [CryptoDto(ShortDtoNames.CallResponse)]
    public class CallResponseDto : IMsgPackTypeName, ICallDto
    {
        [IgnoreMember]
        public const string TypeNameConst = ShortDtoNames.CallResponse;
        [IgnoreMember]
        public string TypeName { get { return TypeNameConst; } }
        [Key(0)]
        public CallRequestDto Request { get; set; }
        [Key(1)]
        public CallResponseEvent Event { get; set; }
    }
}