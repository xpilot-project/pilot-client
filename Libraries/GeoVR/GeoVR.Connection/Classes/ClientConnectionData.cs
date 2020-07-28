using GeoVR.Shared;
using MessagePack.CryptoDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Connection
{
    public class ClientConnectionData
    {
        public ApiServerConnection ApiServerConnection { get; set; }

        public string Username { get; set; }
        public string Callsign { get; set; }

        public PostCallsignResponseDto Tokens { get; set; }
        public CryptoDtoChannel VoiceCryptoChannel { get; set; }
        //public CryptoDtoChannel DataCryptoChannel { get; set; }

        public DateTime VoiceConnectionDateTimeUtc { get; set; }
        public TimeSpan TimeSinceVoiceConnection { get { return DateTime.UtcNow.Subtract(VoiceConnectionDateTimeUtc); } }
        //public DateTime LastDataServerHeartbeatAckUtc { get; set; }
        public DateTime LastVoiceServerHeartbeatAckUtc { get; set; }
        public TimeSpan ServerTimeout { get; set; } = TimeSpan.FromSeconds(10);
        public bool VoiceServerAlive { get { return TimeSinceVoiceConnection < ServerTimeout || DateTime.UtcNow.Subtract(LastVoiceServerHeartbeatAckUtc) < ServerTimeout; } }
        //public bool DataServerAlive { get { return TimeSinceAuthentication < serverTimeout || DateTime.UtcNow.Subtract(LastDataServerHeartbeatAckUtc) < serverTimeout; } }

        public long VoiceServerBytesSent { get; set; }
        public long VoiceServerBytesReceived { get; set; }
        //public long DataServerBytesSent { get; set; }
        //public long DataServerBytesReceived { get; set; }

        public bool ReceiveAudio { get; set; }

        public bool IsConnected { get; set; }

        public Task TaskVoiceServerTransmit { get; set; }
        public Task TaskVoiceServerReceive { get; set; }
        public Task TaskVoiceServerHeartbeat { get; set; }

        public void CreateCryptoChannels()
        {
            if (Tokens == null)
                throw new Exception(nameof(Tokens) + " not set");

            VoiceCryptoChannel = new CryptoDtoChannel(Tokens.VoiceServer.ChannelConfig);
            //DataCryptoChannel = new CryptoDtoChannel(Tokens.DataServer.ChannelConfig);
        }
    }
}
