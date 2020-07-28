using MessagePack;

namespace GeoVR.Shared
{
    [MessagePackObject]
    public class RxTransceiverDto : IMsgPack       //Changed to class to prevent copy-by-value (https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct)
    {
        [Key(0)]
        public ushort ID { get; set; }
        [Key(1)]
        public uint Frequency { get; set; }     //from 0 to 4.29GHz. Note: Not strictly necessary but fixes the situation where a user changes frequency but still receives audio on the old frequency due to the network update being a fraction of a second slower.
        [Key(2)]
        public float DistanceRatio { get; set; }           //This is the distance ratio from 0.0 to 1.0 (where 0.1 is on the very edge of reception range)
    }
}
