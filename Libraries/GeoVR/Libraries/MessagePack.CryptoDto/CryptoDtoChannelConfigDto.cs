namespace MessagePack.CryptoDto
{
    //This should be exchanged from server to client [using serverChannel.GetRemoteEndpointChannelConfig()] over a secure transport like a HTTPS REST API
    [MessagePackObject]
    public class CryptoDtoChannelConfigDto
    {
        [Key(0)]
        public string ChannelTag { get; set; }
        [Key(1)]
        public byte[] AeadReceiveKey { get; set; }
        [Key(2)]
        public byte[] AeadTransmitKey { get; set; }
        [Key(3)]
        public byte[] HmacKey { get; set; }
    }
}
