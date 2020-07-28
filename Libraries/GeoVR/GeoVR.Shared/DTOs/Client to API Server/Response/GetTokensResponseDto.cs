using MessagePack.CryptoDto;

namespace GeoVR.Shared
{
    public class VoiceServerConnectionDataDto
    {
        public string AddressIpV4 { get; set; }         // Example: 123.123.123.123:50000
        public string AddressIpV6 { get; set; }         // Example: 123.123.123.123:50000
        public CryptoDtoChannelConfigDto ChannelConfig { get; set; }
    }

    //public class DataServerConnectionDataDto
    //{
    //    public string AddressIpV4 { get; set; }         // Example: 123.123.123.123:50000
    //    public string AddressIpV6 { get; set; }         // Example: 123.123.123.123:50000
    //    public CryptoDtoChannelConfigDto ChannelConfig { get; set; }
    //}

    public class PostCallsignResponseDto
    {
        public VoiceServerConnectionDataDto VoiceServer { get; set; }
        //public DataServerConnectionDataDto DataServer { get; set; }
    }
}
