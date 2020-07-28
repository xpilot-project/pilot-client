using MessagePack;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class TransceiverDto : IMsgPack
    {
        [Key(0)]
        public ushort ID { get; set; }
        [Key(1)]
        public uint Frequency { get; set; }     //from 0 to 4.29GHz
        [Key(2)]
        public double LatDeg { get; set; }
        [Key(3)]
        public double LonDeg { get; set; }
        [Key(4)]
        public double HeightMslM { get; set; }
        [Key(5)]
        public double HeightAglM { get; set; }
    }
}
