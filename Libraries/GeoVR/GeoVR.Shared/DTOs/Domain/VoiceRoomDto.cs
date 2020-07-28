using MessagePack;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class VoiceRoomDto : IMsgPack
    {
        [Key(0)]
        public string Name { get; set; }
    }
}