﻿/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2020 Justin Shannon
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program. If not, see http://www.gnu.org/licenses/.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Threading;
using System.Reflection;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Fsd.Connector;
using Vatsim.Fsd.Connector.PDU;
using Vatsim.Xpilot.Events.Arguments;
using Vatsim.Xpilot.Aircrafts;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Simulator;
using System.Linq;
using Abacus.DoublePrecision;

namespace Vatsim.Xpilot.Networking
{
    public class NetworkManager : EventBus, INetworkManager
    {
        [EventPublication(EventTopics.NetworkConnectionInitiated)]
        public event EventHandler<EventArgs> NetworkConnectionInitiated;

        [EventPublication(EventTopics.NetworkConnectionFailed)]
        public event EventHandler<EventArgs> NetworkConnectionFailed;

        [EventPublication(EventTopics.NetworkConnected)]
        public event EventHandler<NetworkConnectedEventArgs> NetworkConnected;

        [EventPublication(EventTopics.NetworkDisconnectionInitiated)]
        public event EventHandler<EventArgs> NetworkDisconnectionInitiated;

        [EventPublication(EventTopics.NetworkDisconnected)]
        public event EventHandler<NetworkDisconnectedEventArgs> NetworkDisconnected;

        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        [EventPublication(EventTopics.NetworkServerListUpdated)]
        public event EventHandler<EventArgs> NetworkServerListUpdated;

        [EventPublication(EventTopics.FlightPlanReceived)]
        public event EventHandler<FlightPlanReceivedEventArgs> FlightPlanReceived;

        [EventPublication(EventTopics.RadioMessageReceived)]
        public event EventHandler<RadioMessageReceivedEventArgs> RadioMessageReceived;

        [EventPublication(EventTopics.BroadcastMessageReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> BroadcastMessageReceived;

        [EventPublication(EventTopics.PrivateMessageReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> PrivateMessageReceived;

        [EventPublication(EventTopics.ServerMessageReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> ServerMessageReceived;

        [EventPublication(EventTopics.DeleteControllerReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> DeleteControllerReceived;

        [EventPublication(EventTopics.DeletePilotReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> DeletePilotReceived;

        [EventPublication(EventTopics.AircraftUpdateReceived)]
        public event EventHandler<AircraftUpdateReceivedEventArgs> AircraftUpdateReceived;

        [EventPublication(EventTopics.FastPositionUpdateReceived)]
        public event EventHandler<FastPositionUpdateEventArgs> FastPositionUpdateReceived;

        [EventPublication(EventTopics.ControllerUpdateReceived)]
        public event EventHandler<ControllerUpdateReceivedEventArgs> ControllerUpdateReceived;

        [EventPublication(EventTopics.CapabilitiesRequestReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> CapabilitiesRequestReceived;

        [EventPublication(EventTopics.AircraftConfigurationInfoReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> AircraftConfigurationInfoReceived;

        [EventPublication(EventTopics.RealNameReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> RealNameReceived;

        [EventPublication(EventTopics.IsValidATCReceived)]
        public event EventHandler<IsValidAtcReceivedEventArgs> IsValidATCReceived;

        [EventPublication(EventTopics.AtisLineReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> AtisLineReceived;

        [EventPublication(EventTopics.AtisEndReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> AtisEndReceived;

        [EventPublication(EventTopics.CapabilitiesResponseReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> CapabilitiesResponseReceived;

        [EventPublication(EventTopics.AircraftInfoReceived)]
        public event EventHandler<AircraftInfoReceivedEventArgs> AircraftInfoReceived;

        [EventPublication(EventTopics.MetarReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> MetarReceived;

        [EventPublication(EventTopics.SelcalAlertReceived)]
        public event EventHandler<SelcalAlertReceivedEventArgs> SelcalAlertReceived;

        private const string STATUS_FILE_URL = "http://status.vatsim.net";
        private readonly IAppConfig mConfig;
        private readonly IXplaneAdapter mXplaneAdapter;
        private readonly FSDSession mFsd;
        private readonly Version mFsdClientVersion;
        private readonly System.Timers.Timer mPositionUpdateTimer;
        private ConnectInfo mConnectInfo;
        private readonly ClientProperties mClientProperties;
        private UserAircraftData mUserAircraftData;
        private RadioStackState mRadioStackState;
        private List<NetworkServerInfo> mServerList = new List<NetworkServerInfo>();
        private readonly StreamWriter mRawDataStream;
        private string mServerAddress;
        private string mPublicIP;
        private string mSystemUID;
        private bool mUserInitiatedDisconnect;
        private bool mForcedDiconnect;
        private string mForcedDisconnectReason;

        public NetworkManager(IEventBroker broker, IAppConfig config) : base(broker)
        {
            mConfig = config;
            mConnectInfo = new ConnectInfo();
            mFsdClientVersion = new Version("1.2");

            mClientProperties = new ClientProperties("xPilot", mFsdClientVersion, "", "");

            mFsd = new FSDSession(mClientProperties);
            mFsd.IgnoreUnknownPackets = true;
            mFsd.NetworkError += Fsd_NetworkError;
            mFsd.NetworkConnected += Fsd_NetworkConnected;
            mFsd.NetworkConnectionFailed += Fsd_NetworkConnectionFailed;
            mFsd.NetworkDisconnected += Fsd_NetworkDisconnected;
            mFsd.ProtocolErrorReceived += Fsd_ProtocolErrorReceived;
            mFsd.ServerIdentificationReceived += Fsd_ServerIdentificationReceived;
            mFsd.ClientQueryReceived += Fsd_ClientQueryReceived;
            mFsd.ClientQueryResponseReceived += Fsd_ClientQueryResponseReceived;
            mFsd.PilotPositionReceived += Fsd_PilotPositionReceived;
            mFsd.FastPilotPositionReceived += Fsd_FastPilotPositionReceived;
            mFsd.ATCPositionReceived += Fsd_ATCPositionReceived;
            mFsd.AcarsResponseReceived += Fsd_AcarsResponseReceived;
            mFsd.DeletePilotReceived += Fsd_DeletePilotReceived;
            mFsd.DeleteATCReceived += Fsd_DeleteATCReceived;
            mFsd.PingReceived += Fsd_PingReceived;
            mFsd.TextMessageReceived += Fsd_TextMessageReceived;
            mFsd.BroadcastMessageReceived += Fsd_BroadcastMessageReceived;
            mFsd.RadioMessageReceived += Fsd_RadioMessageReceived;
            mFsd.VersionRequestReceived += Fsd_VersionRequestReceived;
            mFsd.PlaneInfoRequestReceived += Fsd_PlaneInfoRequestReceived;
            mFsd.PlaneInfoResponseReceived += Fsd_PlaneInfoResponseReceived;
            mFsd.KillRequestReceived += Fsd_KillRequestReceived;
            mFsd.FlightPlanReceived += Fsd_FlightPlanReceived;
            mFsd.RawDataSent += Fsd_RawDataSent;
            mFsd.RawDataReceived += Fsd_RawDataReceived;

            mRawDataStream = new StreamWriter(Path.Combine(mConfig.AppPath, $"NetworkLogs/NetworkLog-{DateTime.UtcNow:yyyyMMddHHmmss}.log"), false);

            mPositionUpdateTimer = new System.Timers.Timer();
            mPositionUpdateTimer.Elapsed += PositionUpdateTimer_Elapsed;

            DownloadNetworkServersAsync();
        }

        [EventSubscription(EventTopics.MainFormShown, typeof(OnUserInterfaceAsync))]
        public void OnMainFormShown(object sender, EventArgs e)
        {
            mFsd.SetSyncContext(SynchronizationContext.Current);
        }

        [EventSubscription(EventTopics.UserAircraftDataUpdated, typeof(OnUserInterfaceAsync))]
        public void OnUserAircraftDataUpdated(object sender, UserAircraftDataUpdatedEventArgs e)
        {
            mUserAircraftData = e.UserAircraftData;
            if (mFsd.Connected && !mConnectInfo.ObserverMode)
            {
                SendFastPositionPacket();
            }
        }

        [EventSubscription(EventTopics.SessionEnded, typeof(OnUserInterface))]
        public void OnSessionEnded(object sender, EventArgs e)
        {
            if (mFsd.Connected)
            {
                Disconnect();
            }
        }

        [EventSubscription(EventTopics.RealNameRequested, typeof(OnUserInterface))]
        public void OnRealNameRequested(object sender, RealNameRequestedEventArgs e)
        {
            SendRealNameRequest(e.Callsign);
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChangedEventArgs e)
        {
            mRadioStackState = e.RadioStackState;
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSentEventArgs e)
        {
            SendPrivateMessage(e.To, e.Message);
        }

        private void Fsd_NetworkError(object sender, NetworkErrorEventArgs e)
        {
            if (!e.Error.Contains("Parse error"))
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Network error: {0}", e.Error));
            }
        }

        private void Fsd_NetworkDisconnected(object sender, NetworkEventArgs e)
        {
            mPositionUpdateTimer.Stop();
            DisconnectInfo disconnectInfo = new DisconnectInfo
            {
                Type = mForcedDiconnect ? DisconnectType.Forcible : (!mUserInitiatedDisconnect ? DisconnectType.Other : DisconnectType.Intentional),
                ForcibleReason = (mForcedDiconnect ? mForcedDisconnectReason : "")
            };
            NetworkDisconnected?.Invoke(this, new NetworkDisconnectedEventArgs(disconnectInfo));
            mUserInitiatedDisconnect = false;
            mForcedDiconnect = false;
            if (disconnectInfo.Type == DisconnectType.Forcible)
            {
                if (!string.IsNullOrEmpty(disconnectInfo.ForcibleReason))
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Forcibly disconnected from network: {0}", disconnectInfo.ForcibleReason));
                }
                else
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Forcibly disconnected from network."));
                }
            }
            else if (disconnectInfo.Type == DisconnectType.Intentional)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Disconnected from network."));
            }
            else
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Disconnected from network."));
            }
        }

        private void Fsd_NetworkConnected(object sender, NetworkEventArgs e)
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Connected to network{0}.", mConnectInfo.ObserverMode ? " in observer mode" : ""));
            NetworkConnected?.Invoke(this, new NetworkConnectedEventArgs(mConnectInfo));
        }

        private void Fsd_NetworkConnectionFailed(object sender, NetworkEventArgs e)
        {
            NetworkConnectionFailed?.Invoke(this, EventArgs.Empty);
        }

        private void Fsd_ProtocolErrorReceived(object sender, DataReceivedEventArgs<PDUProtocolError> e)
        {
            if (e.PDU.ErrorType != NetworkError.NoFlightPlan)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Network error: {0}", e.PDU.Message));
            }
        }

        private void Fsd_ServerIdentificationReceived(object sender, DataReceivedEventArgs<PDUServerIdentification> e)
        {
            SendClientIdentification();
            if (mConnectInfo.ObserverMode)
            {
                SendAddObserver();
            }
            else
            {
                SendAddPilot();
            }
            RequestPublicIP();
            SendPositionPacket();
            mPositionUpdateTimer.Interval = mConnectInfo.ObserverMode ? 15000 : 5000;
            mPositionUpdateTimer.Start();
        }

        private void RequestPublicIP()
        {
            mFsd.SendPDU(new PDUClientQuery(mConnectInfo.Callsign, "SERVER", ClientQueryType.PublicIP));
        }

        private void SendAddPilot()
        {
            mFsd.SendPDU(new PDUAddPilot(mConnectInfo.Callsign, mConfig.VatsimId.Trim(), mConfig.VatsimPasswordDecrypted.Trim(), NetworkRating.OBS, ProtocolRevision.VatsimAuth, SimulatorType.XPlane, mConfig.Name));
        }

        private void SendAddObserver()
        {
            mFsd.SendPDU(new PDUAddATC(mConnectInfo.Callsign, mConfig.Name, mConfig.VatsimId.Trim(), mConfig.VatsimPasswordDecrypted.Trim(), NetworkRating.OBS, ProtocolRevision.VatsimAuth));
        }

        private void SendClientIdentification()
        {
            mFsd.SendPDU(new PDUClientIdentification(mConnectInfo.Callsign, mFsd.GetClientKey(), "xPilot", mFsdClientVersion.Major, mFsdClientVersion.Minor, mConfig.VatsimId.Trim(), "", ""));
        }

        private void Fsd_ClientQueryReceived(object sender, DataReceivedEventArgs<PDUClientQuery> e)
        {
            switch (e.PDU.QueryType)
            {
                case ClientQueryType.NewInfo:
                    SendControllerInfoRequest(e.PDU.From);
                    break;
                case ClientQueryType.AircraftConfiguration:
                    AircraftConfigurationInfoReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From.ToUpper(), string.Join(":", e.PDU.Payload.ToArray())));
                    break;
                case ClientQueryType.Capabilities:
                    if (e.PDU.From.ToUpper() != "SERVER")
                    {
                        CapabilitiesRequestReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From));
                    }
                    SendCapabilities(e.PDU.From);
                    break;
                case ClientQueryType.COM1Freq:
                    SendCom1Frequency(e.PDU.From, mRadioStackState.Com1ActiveFrequency.FormatFromNetwork());
                    break;
                case ClientQueryType.RealName:
                    mFsd.SendPDU(new PDUClientQueryResponse(mConnectInfo.Callsign, e.PDU.From.ToUpper(), ClientQueryType.RealName, new List<string>
                    {
                        mConfig.Name,
                        "",
                        "1"
                    }));
                    break;
                case ClientQueryType.INF:
                    string info = string.Format("{0} {1} PID={2} ({3}) IP={4} SYS_UID={5} FSVER={6} LT={7} LO={8} AL={9}", new object[]
                    {
                        "xPilot",
                        Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion,
                        mConfig.VatsimId.Trim(),
                        mConfig.NameWithAirport,
                        mPublicIP,
                        mSystemUID,
                        SimulatorType.XPlane,
                        mUserAircraftData.Latitude,
                        mUserAircraftData.Longitude,
                        mUserAircraftData.AltitudeTrue
                    });
                    mFsd.SendPDU(new PDUTextMessage(mConnectInfo.Callsign, e.PDU.From, info));
                    break;
            }
        }

        private void Fsd_ClientQueryResponseReceived(object sender, DataReceivedEventArgs<PDUClientQueryResponse> e)
        {
            switch (e.PDU.QueryType)
            {
                case ClientQueryType.IsValidATC:
                    IsValidATCReceived?.Invoke(this, new IsValidAtcReceivedEventArgs(e.PDU.Payload[1].ToUpper(), e.PDU.Payload[0].ToUpper() == "Y"));
                    break;
                case ClientQueryType.Capabilities:
                    CapabilitiesResponseReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, string.Join(":", e.PDU.Payload.ToArray())));
                    break;
                case ClientQueryType.RealName:
                    RealNameReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Payload[0]));
                    break;
                case ClientQueryType.ATIS:
                    switch (e.PDU.Payload[0])
                    {
                        case "E":
                            AtisEndReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From));
                            break;
                        case "Z":
                            AtisLineReceived.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, $"Estimated Logoff Time: {e.PDU.Payload[1]}"));
                            break;
                        default:
                            AtisLineReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Payload[1]));
                            break;
                    }
                    break;
                case ClientQueryType.PublicIP:
                    mPublicIP = (e.PDU.Payload.Count > 0) ? e.PDU.Payload[0] : "";
                    break;
            }
        }

        private void Fsd_FastPilotPositionReceived(object sender, DataReceivedEventArgs<PDUFastPilotPosition> e)
        {
            AircraftVisualState visualState = new AircraftVisualState(new WorldPoint(e.PDU.Lon, e.PDU.Lat), e.PDU.Altitude, e.PDU.Pitch, e.PDU.Heading, e.PDU.Bank);
            Vector3 positionalVelocityVector = new Vector3(e.PDU.VelocityX, e.PDU.VelocityZ, e.PDU.VelocityY);
            Vector3 rotationalVeloctyVector = new Vector3(e.PDU.VelocityPitch, e.PDU.VelocityHeading, e.PDU.VelocityBank);
            FastPositionUpdateReceived(this, new FastPositionUpdateEventArgs(e.PDU.From, visualState, positionalVelocityVector, rotationalVeloctyVector));
        }

        private void Fsd_PilotPositionReceived(object sender, DataReceivedEventArgs<PDUPilotPosition> e)
        {
            if (!mConnectInfo.ObserverMode || !Regex.IsMatch(mConnectInfo.Callsign, "^" + Regex.Escape(e.PDU.From) + "[A-Z]$"))
            {
                AircraftVisualState visualState = new AircraftVisualState(new WorldPoint(e.PDU.Lon, e.PDU.Lat), e.PDU.TrueAltitude, e.PDU.Pitch, e.PDU.Heading, e.PDU.Bank);
                AircraftUpdateReceived?.Invoke(this, new AircraftUpdateReceivedEventArgs(e.PDU.From, visualState, e.PDU.GroundSpeed));
            }
        }

        private void Fsd_ATCPositionReceived(object sender, DataReceivedEventArgs<PDUATCPosition> e)
        {
            ControllerUpdateReceived?.Invoke(this, new ControllerUpdateReceivedEventArgs(e.PDU.From, (uint)e.PDU.Frequency, new WorldPoint(e.PDU.Lon, e.PDU.Lat)));
        }

        private void Fsd_AcarsResponseReceived(object sender, DataReceivedEventArgs<PDUMetarResponse> e)
        {
            MetarReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Metar));
        }

        private void Fsd_DeletePilotReceived(object sender, DataReceivedEventArgs<PDUDeletePilot> e)
        {
            DeletePilotReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From.ToUpper()));
        }

        private void Fsd_DeleteATCReceived(object sender, DataReceivedEventArgs<PDUDeleteATC> e)
        {
            DeleteControllerReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From.ToUpper()));
        }

        private void Fsd_PingReceived(object sender, DataReceivedEventArgs<PDUPing> e)
        {
            mFsd.SendPDU(new PDUPong(mConnectInfo.Callsign, e.PDU.From, e.PDU.TimeStamp));
        }

        private void Fsd_RadioMessageReceived(object sender, DataReceivedEventArgs<PDURadioMessage> e)
        {
            List<uint> frequencyList = new List<uint>();
            for (int i = 0; i < e.PDU.Frequencies.Length; i++)
            {
                uint frequency = (uint)e.PDU.Frequencies[i];
                if (mRadioStackState.Com1ReceiveEnabled && frequency.Normalize25KhzFsdFrequency() == mRadioStackState.Com1ActiveFrequency.MatchNetworkFormat())
                {
                    if (!frequencyList.Contains(frequency))
                    {
                        frequencyList.Add(frequency);
                    }
                }
                else if (mRadioStackState.Com2ReceiveEnabled && frequency.Normalize25KhzFrequency() == mRadioStackState.Com2ActiveFrequency.MatchNetworkFormat())
                {
                    if (!frequencyList.Contains(frequency))
                    {
                        frequencyList.Add(frequency);
                    }
                }
            }
            if (frequencyList.Count <= 0)
            {
                return;
            }
            var match = Regex.Match(e.PDU.Message, "^SELCAL ([A-Z][A-Z]\\-[A-Z][A-Z])$");
            if (match.Success)
            {
                var selcal = "SELCAL " + match.Groups[1].Value.Replace("-", "");
                if (!string.IsNullOrEmpty(mConnectInfo.SelCalCode)
                    && selcal == "SELCAL " + mConnectInfo.SelCalCode.Replace("-", ""))
                {
                    SelcalAlertReceived?.Invoke(this, new SelcalAlertReceivedEventArgs(e.PDU.From, e.PDU.Frequencies.Select(t => (uint)t).ToArray()));
                }
            }
            else
            {
                bool direct = e.PDU.Message.ToUpper().StartsWith(mConnectInfo.Callsign.ToUpper());
                RadioMessageReceived?.Invoke(this, new RadioMessageReceivedEventArgs(e.PDU.From.ToUpper(), frequencyList.ToArray(), e.PDU.Message, direct));
            }
        }

        private void Fsd_BroadcastMessageReceived(object sender, DataReceivedEventArgs<PDUBroadcastMessage> e)
        {
            BroadcastMessageReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From.ToUpper(), e.PDU.Message));
        }

        private void Fsd_TextMessageReceived(object sender, DataReceivedEventArgs<PDUTextMessage> e)
        {
            if (e.PDU.From.ToUpper() == "SERVER")
            {
                ServerMessageReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From.ToUpper(), e.PDU.Message));
            }
            else
            {
                PrivateMessageReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From.ToUpper(), e.PDU.Message));
            }
        }

        private void Fsd_VersionRequestReceived(object sender, DataReceivedEventArgs<PDUVersionRequest> e)
        {
            SendPrivateMessage(e.PDU.From, $"xPilot {mClientProperties.Version}");
        }

        private void Fsd_PlaneInfoRequestReceived(object sender, DataReceivedEventArgs<PDUPlaneInfoRequest> e)
        {
            Match m = Regex.Match(mConnectInfo.Callsign, "^([A-Z]{3})\\d+", RegexOptions.IgnoreCase);
            mFsd.SendPDU(new PDUPlaneInfoResponse(mConnectInfo.Callsign, e.PDU.From, mConnectInfo.TypeCode, m.Success ? m.Groups[1].Value : "", "", ""));
        }

        private void Fsd_PlaneInfoResponseReceived(object sender, DataReceivedEventArgs<PDUPlaneInfoResponse> e)
        {
            AircraftInfoReceived?.Invoke(this, new AircraftInfoReceivedEventArgs(e.PDU.From, e.PDU.Equipment, e.PDU.Airline));
        }

        private void Fsd_KillRequestReceived(object sender, DataReceivedEventArgs<PDUKillRequest> e)
        {
            mForcedDiconnect = true;
            mForcedDisconnectReason = e.PDU.Reason;
        }

        private void Fsd_FlightPlanReceived(object sender, DataReceivedEventArgs<PDUFlightPlan> e)
        {
            if (e.PDU.From != mConnectInfo.Callsign)
            {
                FlightPlan fp = ParseFlightPlan(e.PDU);
                FlightPlanReceived?.Invoke(this, new FlightPlanReceivedEventArgs(fp));
            }
        }

        private void DownloadNetworkServersAsync()
        {
            Task.Run(() =>
            {
                mServerList.Clear();
                mServerList = NetworkInfo.GetServerList(STATUS_FILE_URL);
            }).ContinueWith((t) =>
            {
                if (t.IsCanceled)
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Server list download cancelled. Using previously cached server list."));
                }
                else if (t.Exception != null)
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Server list download failed. Using previously cached server list. Error: {0}", t.Exception.Message));
                }
                else
                {
                    mConfig.CachedServers.Clear();
                    foreach (NetworkServerInfo server in mServerList)
                    {
                        mConfig.CachedServers.Add(server);
                    }
                    mConfig.SaveConfig();
                    NetworkServerListUpdated?.Invoke(this, EventArgs.Empty);
                }
            });
        }

        private void PositionUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendPositionPacket();
        }

        private void SendPositionPacket()
        {
            if (mConnectInfo.ObserverMode)
            {
                mFsd.SendPDU(new PDUATCPosition(mConnectInfo.Callsign, 99998, NetworkFacility.OBS, 40, NetworkRating.OBS, mUserAircraftData.Latitude, mUserAircraftData.Longitude));
            }
            else
            {
                mFsd.SendPDU(new PDUPilotPosition(mConnectInfo.Callsign, mRadioStackState.TransponderCode, false, mRadioStackState.SquawkingIdent, NetworkRating.OBS, mUserAircraftData.Latitude, mUserAircraftData.Longitude, Convert.ToInt32(mUserAircraftData.AltitudeTrue), Convert.ToInt32(mUserAircraftData.AltitudePressure), Convert.ToInt32(mUserAircraftData.SpeedGround), Convert.ToInt32(mUserAircraftData.Pitch), Convert.ToInt32(mUserAircraftData.Bank), Convert.ToInt32(mUserAircraftData.Heading)));
            }
        }

        private void SendFastPositionPacket()
        {
            mFsd.SendPDU(new PDUFastPilotPosition(mConnectInfo.Callsign, mUserAircraftData.Latitude, mUserAircraftData.Longitude, mUserAircraftData.AltitudeTrue, mUserAircraftData.Pitch, mUserAircraftData.Bank, mUserAircraftData.Heading, mUserAircraftData.LongitudeVelocity, mUserAircraftData.AltitudeVelocity, mUserAircraftData.LatitudeVelocity, mUserAircraftData.PitchVelocity, mUserAircraftData.BankVelocity, mUserAircraftData.HeadingVelocity));
        }

        public void SendRealNameRequest(string callsign)
        {
            mFsd.SendPDU(new PDUClientQuery(mConnectInfo.Callsign, callsign, ClientQueryType.RealName));
        }

        public void SendIsValidATCRequest(string callsign)
        {
            mFsd.SendPDU(new PDUClientQuery(mConnectInfo.Callsign, "SERVER", ClientQueryType.IsValidATC, new List<string> { callsign }));
        }

        public void SendControllerInfoRequest(string callsign)
        {
            mFsd.SendPDU(new PDUClientQuery(mConnectInfo.Callsign, callsign, ClientQueryType.ATIS));
        }

        public void SendCapabilitiesRequest(string callsign)
        {
            mFsd.SendPDU(new PDUClientQuery(mConnectInfo.Callsign, callsign, ClientQueryType.Capabilities));
        }

        public void SendMetarRequest(string station)
        {
            mFsd.SendPDU(new PDUMetarRequest(mConnectInfo.Callsign, station));
        }

        public void SendPrivateMessage(string to, string message)
        {
            mFsd.SendPDU(new PDUTextMessage(mConnectInfo.Callsign, to, message));
        }

        public void SendRadioMessage(List<int> freqs, string message)
        {
            mFsd.SendPDU(new PDURadioMessage(mConnectInfo.Callsign, freqs.ToArray(), message));
        }

        public void SendWallop(string message)
        {
            mFsd.SendPDU(new PDUWallop(mConnectInfo.Callsign, message));
        }

        public void SendInfoRequest(string callsign)
        {
            mFsd.SendPDU(new PDUClientQuery(mConnectInfo.Callsign, callsign, ClientQueryType.INF));
        }

        public void SendClientVersionRequest(string callsign)
        {
            mFsd.SendPDU(new PDUVersionRequest(mConnectInfo.Callsign, callsign));
        }

        public void SendAircraftInfoRequest(string callsign)
        {
            mFsd.SendPDU(new PDUPlaneInfoRequest(mConnectInfo.Callsign, callsign));
        }

        public void SendCapabilities(string to)
        {
            mFsd.SendPDU(new PDUClientQueryResponse(mConnectInfo.Callsign, to, ClientQueryType.Capabilities, new List<string>
            {
                "VERSION=1",
                "ATCINFO=1",
                "MODELDESC=1",
                "ACCONFIG=1",
                "VISUPDATE=1"
            }));
        }

        public void SendAircraftConfigurationUpdate(string to, AircraftConfiguration config)
        {
            AircraftConfigurationInfo acconfig = new AircraftConfigurationInfo
            {
                Config = config
            };
            mFsd.SendPDU(new PDUClientQueryResponse(mConnectInfo.Callsign, to, ClientQueryType.AircraftConfiguration, new List<string>
            {
                acconfig.ToJson()
            }));
        }

        public void SendAircraftConfigurationUpdate(AircraftConfiguration config)
        {
            SendAircraftConfigurationUpdate("@94836", config);
        }

        public void SendAircraftConfigurationRequest(string callsign)
        {
            AircraftConfigurationInfo acconfig = new AircraftConfigurationInfo
            {
                Request = AircraftConfigurationInfo.RequestType.Full
            };
            mFsd.SendPDU(new PDUClientQuery(mConnectInfo.Callsign, callsign, ClientQueryType.AircraftConfiguration, new List<string>
            {
                acconfig.ToJson()
            }));
        }

        public void SendCom1Frequency(string to, string frequency)
        {
            mFsd.SendPDU(new PDUClientQueryResponse(mConnectInfo.Callsign, to, ClientQueryType.COM1Freq, new List<string>
            {
                frequency
            }));
        }

        public void Connect(ConnectInfo connectInfo, string server)
        {
            mConnectInfo = connectInfo;
            mServerAddress = server;
            NetworkConnectionInitiated?.Invoke(this, EventArgs.Empty);
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Connecting to network..."));
            mFsd.Connect(mServerAddress, 6809, !mConnectInfo.TowerViewMode);
        }

        public void Disconnect()
        {
            mUserInitiatedDisconnect = true;
            NetworkDisconnectionInitiated?.Invoke(this, EventArgs.Empty);
            mFsd.SendPDU(new PDUDeletePilot(mConnectInfo.Callsign, mConfig.VatsimId.Trim()));
            mFsd.Disconnect();
        }

        private void Fsd_RawDataSent(object sender, RawDataEventArgs e)
        {
            mRawDataStream.Write("[{0}] >>>     {1}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), e.Data.Replace(mConfig.VatsimPasswordDecrypted, "XXXXXX"));
            mRawDataStream.Flush();
        }

        private void Fsd_RawDataReceived(object sender, RawDataEventArgs e)
        {
            mRawDataStream.Write("[{0}]     <<< {1}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), e.Data);
            mRawDataStream.Flush();
        }

        private static FlightPlan ParseFlightPlan(PDUFlightPlan fp)
        {
            if (fp.CruiseAlt.Contains("FL"))
            {
                fp.CruiseAlt = fp.CruiseAlt.Replace("FL", "");
                fp.CruiseAlt += "00";
            }
            int.TryParse(fp.CruiseAlt, out int cruiseAlt);
            if (cruiseAlt < 1000)
            {
                cruiseAlt *= 100;
            }
            int.TryParse(fp.TAS, out int cruiseSpeed);

            FlightPlan flightPlan = new FlightPlan();
            flightPlan.Callsign = fp.From;
            flightPlan.FlightType = (FlightPlanType)fp.Rules;
            flightPlan.ExtractEquipmentComponents(fp.Equipment);
            flightPlan.IsHeavy = fp.Equipment.StartsWith("H/");
            flightPlan.CruiseAltitude = cruiseAlt;
            flightPlan.CruiseSpeed = cruiseSpeed;
            flightPlan.DepartureAirport = fp.DepAirport.ToUpper();
            flightPlan.DestinationAirport = fp.DestAirport.ToUpper();
            flightPlan.AlternateAirport = fp.AltAirport.ToUpper();
            flightPlan.Route = fp.Route.ToUpper();
            if (flightPlan.Route.StartsWith("+"))
            {
                flightPlan.Route = flightPlan.Route.Substring(1);
            }
            flightPlan.Remarks = fp.Remarks;
            int.TryParse(fp.EstimatedDepTime, out int depTime);
            if (depTime > 2400)
            {
                depTime = 0;
            }
            int.TryParse(fp.HoursEnroute, out int enrouteHours);
            int.TryParse(fp.MinutesEnroute, out int enrouteMinutes);
            int.TryParse(fp.FuelAvailHours, out int fuelHours);
            int.TryParse(fp.FuelAvailMinutes, out int fuleMinutes);
            flightPlan.DepartureTime = depTime;
            flightPlan.EnrouteHours = enrouteHours;
            flightPlan.EnrouteMinutes = enrouteMinutes;
            flightPlan.FuelHours = fuelHours;
            flightPlan.FuelMinutes = fuleMinutes;
            flightPlan.VoiceType = flightPlan.VoiceType.FromFlightPlanRemarks(fp.Remarks);
            flightPlan.Remarks = flightPlan.Remarks.Replace("/V/", "");
            flightPlan.Remarks = flightPlan.Remarks.Replace("/v/", "");
            flightPlan.Remarks = flightPlan.Remarks.Replace("/T/", "");
            flightPlan.Remarks = flightPlan.Remarks.Replace("/t/", "");
            flightPlan.Remarks = flightPlan.Remarks.Replace("/R/", "");
            flightPlan.Remarks = flightPlan.Remarks.Replace("/r/", "");
            return flightPlan;
        }

        public void SetPluginHash(string hash)
        {
            mClientProperties.PluginHash = hash;
        }

        public void SendControlerInfoRequest(string callsign)
        {
            throw new NotImplementedException();
        }

        public bool IsConnected => mFsd.Connected;

        public string OurCallsign => mConnectInfo.Callsign;

        public List<NetworkServerInfo> ServerList => mServerList;
    }
}
