using MessagePack;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class TxTelephoneDto : IMsgPack
    {
        [Key(0)]
        public ushort ID { get; set; }
    }
}
