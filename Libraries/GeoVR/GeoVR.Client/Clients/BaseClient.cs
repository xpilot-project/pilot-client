using GeoVR.Connection;
using GeoVR.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class BaseClient
    {
        protected static int sampleRate = 48000;
        protected static int frameSize = 960;     //20ms

        //Connection
        protected ClientConnection Connection { get; private set; }
        public ApiServerConnection ApiServerConnection { get { return Connection.ApiServerConnection; } }

        //Properties
        public string Callsign { get; private set; }
        //public bool Authenticated { get { return Connection.Authenticated; } }
        public bool IsConnected { get { return Connection.IsConnected; } }

        public event EventHandler<ConnectedEventArgs> Connected
        {
            add { Connection.Connected += value; }
            remove { Connection.Connected -= value; }
        }

        public event EventHandler<DisconnectedEventArgs> Disconnected
        {
            add { Connection.Disconnected += value; }
            remove { Connection.Disconnected -= value; }
        }

        //public long rttTime = 0;        //Replace this with a proper ping, maybe ApiServerPing, VoiceServerPing, DataServerPing

        public BaseClient(string apiServer)
        {
            Connection = new ClientConnection(apiServer);
            Connection.ReceiveAudio = false;
        }

        public async Task Connect(string username, string password, string callsign, string client)
        {
            Callsign = callsign;
            await Connection.Connect(username, password, callsign, client);
        }

        public async Task Disconnect(string reason)
        {
            await Connection.Disconnect(reason);
        }

        public virtual void UpdateTransceivers(List<TransceiverDto> transceivers)
        {
            if (!Connection.IsConnected)
                throw new Exception("Client not connected");

            Connection.ApiServerConnection.UpdateTransceivers(Callsign, transceivers);
        }

        public void UpdateCrossCoupleGroups(List<CrossCoupleGroupDto> groups)
        {
            if (!Connection.IsConnected)
                throw new Exception("Client not connected");

            Connection.ApiServerConnection.UpdateCrossCoupleGroups(Callsign, groups);
        }

        public void UpdateVoiceRooms(List<VoiceRoomDto> voiceRooms)
        {
            if (!Connection.IsConnected)
                throw new Exception("Client not connected");

            Connection.ApiServerConnection.UpdateVoiceRooms(Callsign, voiceRooms);
        }

        public void UpdateDirectConfiguration(DirectConfigurationDto directConfiguration)
        {
            if (!Connection.IsConnected)
                throw new Exception("Client not connected");

            Connection.ApiServerConnection.UpdateDirectConfiguration(Callsign, directConfiguration);
        }
    }
}
