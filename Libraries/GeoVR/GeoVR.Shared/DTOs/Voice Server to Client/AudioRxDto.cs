using MessagePack;
using MessagePack.CryptoDto;
using System.Collections.Generic;

namespace GeoVR.Shared
{
    [MessagePackObject]
    [CryptoDto(ShortDtoNames.AudioRxDto)]
    public class AudioRxDto : IMsgPackTypeName, IAudioDto
    {
        [IgnoreMember]
        public const string TypeNameConst = ShortDtoNames.AudioRxDto;
        [IgnoreMember]
        public string TypeName { get { return TypeNameConst; } }
        [Key(0)]
        public string Callsign { get; set; }
        [Key(1)]
        public uint SequenceCounter { get; set; }
        [Key(2)]
        public byte[] Audio { get; set; }
        [Key(3)]
        public bool LastPacket { get; set; }
        
        [Key(4)]
        public List<RxTransceiverDto> Transceivers { get; set; }
        [Key(5)]
        public List<VoiceRoomDto> VoiceRooms { get; set; }
        [Key(6)]
        public bool Direct { get; set; }
    }
}
