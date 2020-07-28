using MessagePack;
using MessagePack.CryptoDto;

namespace GeoVR.Shared
{
    [MessagePackObject]
    [CryptoDto(ShortDtoNames.HeartbeatDto)]
    public class HeartbeatDto : IMsgPackTypeName
    {
        [IgnoreMember]
        public const string TypeNameConst = ShortDtoNames.HeartbeatDto;
        [IgnoreMember]
        public string TypeName { get { return TypeNameConst; } }
        [Key(0)]
        public string Callsign { get; set; }
    }
}
