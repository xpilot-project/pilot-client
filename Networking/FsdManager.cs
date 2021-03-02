/*
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
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Fsd.Connector;
using Vatsim.Fsd.Connector.PDU;
using Vatsim.Xpilot.Events.Arguments;
using Vatsim.Xpilot.Aircrafts;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Config;
using XPilot.PilotClient.Core.Events;
using Vatsim.Xpilot.Controllers;
using Vatsim.Xpilot.Simulator;
using Vatsim.Xpilot.Core;
using Xpilot.Events.Arguments;
using System.Threading;

namespace Vatsim.Xpilot.Networking
{
    public class FsdManager : EventBus, IFsdManager
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
        public event EventHandler<NetworkDataReceivedEventArgs> AircraftUpdateReceived;

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
        public event EventHandler<NetworkDataReceivedEventArgs> AircraftInfoReceived;

        [EventPublication(EventTopics.MetarReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> MetarReceived;

        [EventPublication(EventTopics.SelcalAlertReceived)]
        public event EventHandler<SelcalAlertReceivedEventArgs> SelcalAlertReceived;

        private const string STATUS_FILE_URL = "http://status.vatsim.net";
        private readonly IAppConfig mConfig;
        private readonly FSDSession mFsd;
        private readonly Version mFsdClientVersion;
        private readonly System.Timers.Timer mPositionUpdateTimer;
        private ConnectInfo mConnectInfo;
        private string mPublicIP;
        private string mSystemUID;
        private bool mForcedDisconnect;
        private bool mIntentionalDisconnect;
        private string mKillReason;
        private bool mIsIdenting;
        private bool mIsIdentPressed;
        private readonly ClientProperties mClientProperties;
        private UserAircraftData mUserAircraftData;
        private RadioStackState mRadioStackState;
        private List<NetworkServerInfo> mServerList = new List<NetworkServerInfo>();
        private readonly StreamWriter mRawDataStream;

        public FsdManager(IEventBroker broker, IAppConfig config) : base(broker)
        {
            mConfig = config;

            mFsdClientVersion = new Version("1.2");
            mPositionUpdateTimer = new System.Timers.Timer { Interval = 5000 };
            mPositionUpdateTimer.Elapsed += PositionUpdateTimer_Elapsed;

            mClientProperties = new ClientProperties("xPilot", mFsdClientVersion, "", "");
            mRawDataStream = new StreamWriter(Path.Combine(mConfig.AppPath, $"NetworkLogs/NetworkLog-{DateTime.UtcNow:yyyyMMddHHmmss}.log"), false);

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
        }

        private void Fsd_NetworkError(object sender, NetworkErrorEventArgs e)
        {
            if (e.Error.Contains("Parse error"))
            {
                // debug
            }
            else
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Network error: {0}", e.Error));
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

        private void Fsd_NetworkDisconnected(object sender, NetworkEventArgs e)
        {

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

        }

        private void Fsd_ClientQueryReceived(object sender, DataReceivedEventArgs<PDUClientQuery> e)
        {
            switch (e.PDU.QueryType)
            {
                case ClientQueryType.NewInfo:
                    break;
                case ClientQueryType.AircraftConfiguration:
                    break;
                case ClientQueryType.Capabilities:
                    break;
                case ClientQueryType.COM1Freq:
                    break;
                case ClientQueryType.RealName:
                    break;
                case ClientQueryType.INF:
                    break;
            }
        }

        private void Fsd_ClientQueryResponseReceived(object sender, DataReceivedEventArgs<PDUClientQueryResponse> e)
        {
            switch (e.PDU.QueryType)
            {
                case ClientQueryType.IsValidATC:
                    break;
                case ClientQueryType.Capabilities:
                    break;
                case ClientQueryType.RealName:
                    break;
                case ClientQueryType.COM1Freq:
                case ClientQueryType.Server:
                    break;
                case ClientQueryType.ATIS:
                    break;
                case ClientQueryType.PublicIP:
                    break;
            }
        }

        private void Fsd_PilotPositionReceived(object sender, DataReceivedEventArgs<PDUPilotPosition> e)
        {
            if (!mConnectInfo.ObserverMode || !Regex.IsMatch(mConnectInfo.Callsign, "^" + Regex.Escape(e.PDU.From) + "[A-Z]$"))
            {

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

        private void Fsd_BroadcastMessageReceived(object sender, DataReceivedEventArgs<PDUBroadcastMessage> e)
        {
            BroadcastMessageReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From.ToUpper(), e.PDU.Message));
        }

        private void Fsd_RadioMessageReceived(object sender, DataReceivedEventArgs<PDURadioMessage> e)
        {
            List<uint> freqs = new List<uint>();
            for (int i = 0; i < e.PDU.Frequencies.Length; i++)
            {
                uint freq = (uint)e.PDU.Frequencies[i];
            }
        }

        private void Fsd_VersionRequestReceived(object sender, DataReceivedEventArgs<PDUVersionRequest> e)
        {

        }

        private void Fsd_PlaneInfoRequestReceived(object sender, DataReceivedEventArgs<PDUPlaneInfoRequest> e)
        {
            Match m = Regex.Match(mConnectInfo.Callsign, "^([A-Z]{3})\\d+", RegexOptions.IgnoreCase);
            mFsd.SendPDU(new PDUPlaneInfoResponse(mConnectInfo.Callsign, e.PDU.From, mConnectInfo.TypeCode, m.Success ? m.Groups[1].Value : "", "", ""));
        }

        private void Fsd_PlaneInfoResponseReceived(object sender, DataReceivedEventArgs<PDUPlaneInfoResponse> e)
        {
            AircraftInfoReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Equipment));
        }

        private void Fsd_KillRequestReceived(object sender, DataReceivedEventArgs<PDUKillRequest> e)
        {

        }

        private void Fsd_FlightPlanReceived(object sender, DataReceivedEventArgs<PDUFlightPlan> e)
        {

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
            // send fast position
        }

        [EventSubscription(EventTopics.SessionEnded, typeof(OnUserInterface))]
        public void OnSessionEnded(object sender, EventArgs e)
        {

        }

        [EventSubscription(EventTopics.RealNameRequested, typeof(OnUserInterface))]
        public void OnRealNameRequested(object sender, RealNameRequestedEventArgs e)
        {

        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChangedEventArgs e)
        {
            mRadioStackState = e.RadioStackState;
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
    }
}
