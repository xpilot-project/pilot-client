using MessagePack;
using MessagePack.CryptoDto;
using System;
using System.Collections.Generic;
using System.Text;

namespace GeoVR.Shared
{
    [MessagePackObject]
    [CryptoDto(ShortDtoNames.HeartbeatAckDto)]       //If a client receives this response, it means they're still authenticated
    public class HeartbeatAckDto : IMsgPackTypeName
    {
        [IgnoreMember]
        public const string TypeNameConst = ShortDtoNames.HeartbeatAckDto;
        [IgnoreMember]
        public string TypeName { get { return TypeNameConst; } }
    }
}
