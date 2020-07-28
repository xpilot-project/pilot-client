using MessagePack;
using MessagePack.CryptoDto;

namespace GeoVR.Shared
{
    [MessagePackObject]
    [CryptoDto(ShortDtoNames.AudioTxDto)]
    public class AudioTxDto : IMsgPackTypeName, IAudioDto           //Tx from the perspective of the client
    {
        [IgnoreMember]
        public const string TypeNameConst = ShortDtoNames.AudioTxDto;
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
        public TxTransceiverDto[] Transceivers { get; set; }
        [Key(5)]
        public VoiceRoomDto[] VoiceRooms { get; set; }
        [Key(6)]
        public DirectDto[] Directs { get; set; }
        //[Key(5)]
        //public TxTelephoneDto[] Telephones { get; set; }
    }
}
