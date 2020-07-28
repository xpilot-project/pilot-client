using MessagePack;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class DirectDto : IMsgPack
    {
        [Key(0)]
        public string Callsign { get; set; }
    }
}
