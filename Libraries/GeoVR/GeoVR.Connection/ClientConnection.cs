using GeoVR.Shared;
using MessagePack.CryptoDto;
using MessagePack.CryptoDto.Managed;
using NLog;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace GeoVR.Connection
{
    public class ClientConnection
    {
        private readonly Guid networkVersion = new Guid("3a5ddc6d-cf5d-4319-bd0e-d184f772db80");
        Logger logger = NLog.LogManager.GetCurrentClassLogger();

        //Data
        private readonly ClientConnectionData connection;

        //Voice server
        private UdpClient udpClient;
        private CancellationTokenSource voiceServerCancelTokenSource;
        public BlockingCollection<IMsgPackTypeName> VoiceServerTransmitQueue { get; private set; }
        public BlockingCollection<IMsgPackTypeName> VoiceServerReceiveQueue { get; private set; }

        //Connection checking
        private CancellationTokenSource connectionCheckCancelTokenSource;
        private Task taskServerConnectionCheck;

        //Properties
        //public bool Authenticated { get { return clientConnectionData.ApiServerConnection.Authenticated; } }
        public bool IsConnected { get { return connection.IsConnected; } }
        public bool ReceiveAudio { get { return connection.ReceiveAudio; } set { connection.ReceiveAudio = value; } }

        public ApiServerConnection ApiServerConnection { get { return connection.ApiServerConnection; } }

        public event EventHandler<ConnectedEventArgs> Connected;
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        public ClientConnection(string apiServer)
        {
            connection = new ClientConnectionData();
            connection.ApiServerConnection = new ApiServerConnection(apiServer);
            connection.ReceiveAudio = true;
            connection.Callsign = null;
            //client = new RestClient(apiServer);
            VoiceServerTransmitQueue = new BlockingCollection<IMsgPackTypeName>();
            VoiceServerReceiveQueue = new BlockingCollection<IMsgPackTypeName>();
            //DataServerTransmitQueue = new BlockingCollection<IMsgPack>();
            //DataServerReceiveQueue = new BlockingCollection<IMsgPack>();
            logger.Debug(nameof(ClientConnection) + " instantiated");
        }

        public async Task Connect(string username, string password, string callsign, string client)     //Client is something like "vPilot 2.2.3"
        {
            if (connection.IsConnected)
                throw new Exception("Client already connected");

            connection.Username = username;
            connection.Callsign = callsign;
            await connection.ApiServerConnection.Connect(username, password, networkVersion, client);

            await GetVoiceCredentials(callsign);
            await Task.Run(() => ConnectToVoiceServer());

            if (taskServerConnectionCheck != null && taskServerConnectionCheck.Status == TaskStatus.Running)        //Leftover from previous connection session.
            {
                connectionCheckCancelTokenSource.Cancel();
                taskServerConnectionCheck.Wait();
            }

            connectionCheckCancelTokenSource = new CancellationTokenSource();
            taskServerConnectionCheck = new Task(() => TaskServerConnectionCheck(logger, connectionCheckCancelTokenSource.Token, connection, InternalDisconnect), TaskCreationOptions.LongRunning);
            taskServerConnectionCheck.Start();

            connection.IsConnected = true;
            Connected?.Invoke(this, new ConnectedEventArgs());
            logger.Debug("Connected: " + callsign);
        }

        public async Task Disconnect(string reason)       //End-user initiated disconnect
        {
            await Disconnect(reason, false);
        }

        private async Task Disconnect(string reason, bool autoreconnect)
        {
            if (!connection.IsConnected)
                throw new Exception("Client not connected");

            connection.IsConnected = false;

            if (!string.IsNullOrWhiteSpace(connection.Callsign))
            {
                try     //Ignore exceptions from this API method, it's purely a best effort
                {
                    await connection.ApiServerConnection.RemoveCallsign(connection.Callsign);
                }
                catch { }
            }

            connectionCheckCancelTokenSource.Cancel();              //Stops connection check loop
            DisconnectFromVoiceServer();
            if (!autoreconnect)
                connection.ApiServerConnection.ForceDisconnect();   //Discard the JWT
            connection.Tokens = null;

            Disconnected?.Invoke(this, new DisconnectedEventArgs() { Reason = reason, AutoReconnect = autoreconnect });
            logger.Debug("Disconnected: " + reason);
        }

        private void InternalDisconnect(DisconnectReasons reason)
        {
            string disconnectReasonString;
            switch (reason)
            {
                case DisconnectReasons.LostConnection:
                    disconnectReasonString = "Lost server connection";
                    break;
                case DisconnectReasons.InternalLibraryError10:
                    disconnectReasonString = "Internal library error 10";
                    break;
                case DisconnectReasons.InternalLibraryError20:
                    disconnectReasonString = "Internal library error 20";
                    break;
                case DisconnectReasons.InternalLibraryError30:
                    disconnectReasonString = "Internal library error 30.";
                    break;
                default:
                    disconnectReasonString = "Unknown error";
                    break;
            }
            bool autoreconnect = reason == DisconnectReasons.LostConnection;

            Task.Run(() => Disconnect(disconnectReasonString, autoreconnect)).Wait();

            if (autoreconnect)
                Reconnect();
        }

        private async void Reconnect()
        {
            for (int i = 1; i <= 3; i++)
            {
                logger.Debug("Reconnection attempt " + i);

                try
                {
                    logger.Debug("Waiting for " + (i * i * i) + " seconds");
                    await Task.Delay(i * i * i * 1000);       //1 second, 8 seconds, 27 seconds.

                    await GetVoiceCredentials(connection.Callsign);
                    await Task.Run(() => ConnectToVoiceServer());

                    connectionCheckCancelTokenSource = new CancellationTokenSource();
                    taskServerConnectionCheck = new Task(() => TaskServerConnectionCheck(logger, connectionCheckCancelTokenSource.Token, connection, InternalDisconnect), TaskCreationOptions.LongRunning);
                    taskServerConnectionCheck.Start();

                    connection.IsConnected = true;
                    Connected?.Invoke(this, new ConnectedEventArgs());
                    logger.Debug("Reconnection success");
                    return;
                }
                catch (Exception ex)
                {
                    logger.Debug("Discarding the following exception");
                    logger.Debug(ex);
                    //Swallow the exception which is likely to be an API timeout             
                }
            }

            logger.Debug("Reconnection failed");
            Disconnected?.Invoke(this, new DisconnectedEventArgs() { Reason = "Reconnection failed", AutoReconnect = false });
        }

        private async Task GetVoiceCredentials(string callsign)
        {
            connection.Tokens = await connection.ApiServerConnection.AddCallsign(callsign);
            connection.VoiceConnectionDateTimeUtc = DateTime.UtcNow;
            connection.CreateCryptoChannels();
        }

        private void ConnectToVoiceServer()
        {
            DisconnectFromVoiceServer();

            string[] s = connection.Tokens.VoiceServer.AddressIpV4.Split(':');

            if (!IPAddress.TryParse(s[0], out IPAddress voiceServerIp))
            {
                logger.Error("IP address not in correct format");
                throw new Exception("IP address not in correct format");
            }

            if (!int.TryParse(s[1], out int voiceServerPort))
            {
                logger.Error("Port number not in correct format");
                throw new Exception("Port number not in correct format");
            }

            const int SIO_UDP_CONNRESET = -1744830452;
            udpClient = new UdpClient(0);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                udpClient.Client.IOControl(
                (IOControlCode)SIO_UDP_CONNRESET,
                new byte[] { 0, 0, 0, 0 },
                null);
            }

            var remoteEndpoint = new IPEndPoint(voiceServerIp, voiceServerPort);
            voiceServerCancelTokenSource = new CancellationTokenSource();
            connection.TaskVoiceServerTransmit = new Task(() => TaskVoiceServerTransmit(logger, voiceServerCancelTokenSource.Token, connection, udpClient, remoteEndpoint, VoiceServerTransmitQueue), TaskCreationOptions.LongRunning);
            connection.TaskVoiceServerTransmit.Start();
            connection.TaskVoiceServerReceive = new Task(() => TaskVoiceServerReceive(logger, voiceServerCancelTokenSource.Token, connection, udpClient, VoiceServerReceiveQueue), TaskCreationOptions.LongRunning);
            connection.TaskVoiceServerReceive.Start();
            connection.TaskVoiceServerHeartbeat = new Task(() => TaskVoiceServerHeartbeat(logger, voiceServerCancelTokenSource.Token, connection, VoiceServerTransmitQueue), TaskCreationOptions.LongRunning);
            connection.TaskVoiceServerHeartbeat.Start();

            logger.Debug("Connected to voice server (" + voiceServerIp.ToString() + ":" + voiceServerPort.ToString() + ")");
        }

        private void DisconnectFromVoiceServer()
        {
            if (voiceServerCancelTokenSource != null)
                voiceServerCancelTokenSource.Cancel();
            if (udpClient != null)
                udpClient.Close();

            if (connection.TaskVoiceServerTransmit?.Status == TaskStatus.Running)
                connection.TaskVoiceServerTransmit.Wait();
            connection.TaskVoiceServerTransmit = null;

            if (connection.TaskVoiceServerReceive?.Status == TaskStatus.Running)
                connection.TaskVoiceServerReceive.Wait();
            connection.TaskVoiceServerReceive = null;

            if (connection.TaskVoiceServerHeartbeat?.Status == TaskStatus.Running)
                connection.TaskVoiceServerHeartbeat.Wait();
            connection.TaskVoiceServerHeartbeat = null;

            logger.Debug("All TaskVoiceServer tasks stopped");
        }

        private static void TaskVoiceServerTransmit(
            Logger logger,
            CancellationToken cancelToken,
            ClientConnectionData connection,
            UdpClient udpClient,
            IPEndPoint server,
            BlockingCollection<IMsgPackTypeName> transmitQueue)
        {
            logger.Debug(nameof(TaskVoiceServerTransmit) + " started");

            try
            {
                byte[] dataBytes;
                while (!cancelToken.IsCancellationRequested)
                {
                    if (transmitQueue.TryTake(out IMsgPackTypeName obj, 250, cancelToken))
                    {
                        if (connection.IsConnected)
                        {
                            switch (obj.GetType().Name)
                            {
                                case nameof(AudioTxDto):
                                    if (logger.IsTraceEnabled)
                                        logger.Trace(((AudioTxDto)obj).ToDebugString());
                                    dataBytes = CryptoDtoSerializer.Serialize(connection.VoiceCryptoChannel, CryptoDtoMode.ChaCha20Poly1305, (AudioTxDto)obj);
                                    udpClient.Send(dataBytes, dataBytes.Length, server);
                                    connection.VoiceServerBytesSent += dataBytes.Length;
                                    break;
                                case nameof(CallRequestDto):
                                    if (logger.IsTraceEnabled)
                                        logger.Trace("Sending CallRequestDto");
                                    dataBytes = CryptoDtoSerializer.Serialize(connection.VoiceCryptoChannel, CryptoDtoMode.ChaCha20Poly1305, (CallRequestDto)obj);
                                    udpClient.Send(dataBytes, dataBytes.Length, server);
                                    connection.VoiceServerBytesSent += dataBytes.Length;
                                    break;
                                case nameof(CallResponseDto):
                                    if (logger.IsTraceEnabled)
                                        logger.Trace("Sending CallResponseDto");
                                    dataBytes = CryptoDtoSerializer.Serialize(connection.VoiceCryptoChannel, CryptoDtoMode.ChaCha20Poly1305, (CallResponseDto)obj);
                                    udpClient.Send(dataBytes, dataBytes.Length, server);
                                    connection.VoiceServerBytesSent += dataBytes.Length;
                                    break;
                                case nameof(HeartbeatDto):
                                    if (logger.IsTraceEnabled)
                                        logger.Trace("Sending voice server heartbeat");
                                    dataBytes = CryptoDtoSerializer.Serialize(connection.VoiceCryptoChannel, CryptoDtoMode.ChaCha20Poly1305, (HeartbeatDto)obj);
                                    udpClient.Send(dataBytes, dataBytes.Length, server);
                                    connection.VoiceServerBytesSent += dataBytes.Length;
                                    break;
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            logger.Debug(nameof(TaskVoiceServerTransmit) + " stopped");
        }

        private static void TaskVoiceServerReceive(
            Logger logger,
            CancellationToken cancelToken,
            ClientConnectionData connection,
            UdpClient udpClient,
            BlockingCollection<IMsgPackTypeName> receiveQueue)
        {
            logger.Debug(nameof(TaskVoiceServerReceive) + " started");

            try
            {
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 60005);
                byte[] data;

                while (!cancelToken.IsCancellationRequested)
                {
                    data = udpClient.Receive(ref sender);               //UDP is a datagram protocol, not a streaming protocol, so it's whole datagrams here. 
                    if (data.Length < 30 || data.Length > 1500)
                        continue;
                    //Could check that the sender has the right IP - but NAT-ing significantly reduces the attack surface here
                    connection.VoiceServerBytesReceived += data.Length;
                    var deserializer = CryptoDtoDeserializer.DeserializeIgnoreSequence(connection.VoiceCryptoChannel, data);
                    if (!deserializer.IsSequenceValid())
                    {
                        logger.Debug("Duplicate or old packet received");
                        continue;       //If a duplicate packet received (because it's UDP) - ignore it
                    }
                    //Crypto DTO stream is only concerned with duplicated or old packets, it doesn't discard out-of-order. Need to find out if Opus discards OOO packets.                    
                    switch (deserializer.GetDtoName())
                    {
                        case nameof(AudioRxDto):
                        case ShortDtoNames.AudioRxDto:
                            {
                                var dto = deserializer.GetDto<AudioRxDto>();
                                if (connection.ReceiveAudio && connection.IsConnected)
                                    receiveQueue.Add(dto);
                                break;
                            }
                        case nameof(CallRequestDto):
                        case ShortDtoNames.CallRequest:
                            {
                                var dto = deserializer.GetDto<CallRequestDto>();
                                if (connection.ReceiveAudio && connection.IsConnected)
                                    receiveQueue.Add(dto);
                                break;
                            }
                        case nameof(CallResponseDto):
                        case ShortDtoNames.CallResponse:
                            {
                                var dto = deserializer.GetDto<CallResponseDto>();
                                if (connection.ReceiveAudio && connection.IsConnected)
                                    receiveQueue.Add(dto);
                                break;
                            }
                        case nameof(HeartbeatAckDto):
                        case ShortDtoNames.HeartbeatAckDto:
                            connection.LastVoiceServerHeartbeatAckUtc = DateTime.UtcNow;
                            logger.Trace("Received voice server heartbeat");
                            break;
                    }
                }
            }
            catch (SocketException sex)
            {
                if (connection.IsConnected)     //If the socket exception occurs whilst disconnected, it's likely to just be a forced socket closure from doing disconnect().
                    logger.Error(sex);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
            }

            logger.Debug(nameof(TaskVoiceServerReceive) + " stopped");
        }

        private static void TaskVoiceServerHeartbeat(
            Logger logger,
            CancellationToken cancelToken,
            ClientConnectionData connection,
            BlockingCollection<IMsgPackTypeName> transmitQueue)
        {
            logger.Debug(nameof(TaskVoiceServerHeartbeat) + " started");

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            var keepAlive = new HeartbeatDto() { Callsign = connection.Callsign };
            while (!cancelToken.IsCancellationRequested)
            {
                if (stopWatch.ElapsedMilliseconds > 3000)
                {
                    transmitQueue.Add(keepAlive);
                    stopWatch.Restart();
                }
                Thread.Sleep(500);
            }

            logger.Debug(nameof(TaskVoiceServerHeartbeat) + " stopped");
        }

        private static void TaskServerConnectionCheck(
            Logger logger,
            CancellationToken cancelToken,
            ClientConnectionData connection,
            Action<DisconnectReasons> disconnectReason)
        {
            logger.Debug(nameof(TaskServerConnectionCheck) + " started");

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            while (!cancelToken.IsCancellationRequested)
            {
                if (stopWatch.ElapsedMilliseconds > 3000)
                {
                    if (connection.IsConnected && !connection.VoiceServerAlive)
                    {
                        logger.Error("Lost connection to Voice Server");
                        disconnectReason(DisconnectReasons.LostConnection);
                    }
                    if (connection.IsConnected && connection.TaskVoiceServerHeartbeat?.Status != TaskStatus.Running)
                    {
                        logger.Error("TaskVoiceServerHeartbeat not running");
                        disconnectReason(DisconnectReasons.InternalLibraryError10);
                    }
                    if (connection.IsConnected && connection.TaskVoiceServerReceive?.Status != TaskStatus.Running)
                    {
                        logger.Error("TaskVoiceServerReceive not running");
                        disconnectReason(DisconnectReasons.InternalLibraryError20);
                    }
                    if (connection.IsConnected && connection.TaskVoiceServerTransmit?.Status != TaskStatus.Running)
                    {
                        logger.Error("TaskVoiceServerTransmit not running");
                        disconnectReason(DisconnectReasons.InternalLibraryError30);
                    }
                    stopWatch.Restart();
                }
                Thread.Sleep(500);
            }

            logger.Debug(nameof(TaskServerConnectionCheck) + " stopped");
        }
    }
}