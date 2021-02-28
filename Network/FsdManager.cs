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
using XPilot.PilotClient.Aircraft;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network.Controllers;
using XPilot.PilotClient.XplaneAdapter;

namespace XPilot.PilotClient.Network
{
    public class FsdManager : EventBus, IFsdManager
    {
        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPosted> RaiseNotificationPosted;

        [EventPublication(EventTopics.NetworkConnected)]
        public event EventHandler<NetworkConnected> RaiseNetworkConnected;

        [EventPublication(EventTopics.NetworkDisconnected)]
        public event EventHandler<NetworkDisconnected> RaiseNetworkDisconnected;

        [EventPublication(EventTopics.RadioMessageReceived)]
        public event EventHandler<RadioMessageReceived> RaiseRadioMessageReceived;

        [EventPublication(EventTopics.BroadcastMessageReceived)]
        public event EventHandler<NetworkDataReceived> RaiseBroadcastMessageReceived;

        [EventPublication(EventTopics.PrivateMessageReceived)]
        public event EventHandler<PrivateMessageReceived> RaisePrivateMessageReceived;

        [EventPublication(EventTopics.ServerMessageReceived)]
        public event EventHandler<NetworkDataReceived> RaiseServerMessageReceived;

        [EventPublication(EventTopics.DeletePilotReceived)]
        public event EventHandler<NetworkDataReceived> RaiseDeletePilotReceived;

        [EventPublication(EventTopics.PilotPositionReceived)]
        public event EventHandler<NetworkAircraftUpdateReceived> RaisePilotPositionReceived;

        [EventPublication(EventTopics.ControllerUpdateReceived)]
        public event EventHandler<ControllerUpdateReceived> RaiseControllerUpdateReceived;

        [EventPublication(EventTopics.ControllerDeleted)]
        public event EventHandler<NetworkDataReceived> RaiseControllerDeleted;

        [EventPublication(EventTopics.CapabilitiesRequestReceived)]
        public event EventHandler<NetworkDataReceived> RaiseCapabilitiesRequestReceived;

        [EventPublication(EventTopics.AircraftConfigurationInfoReceived)]
        public event EventHandler<NetworkDataReceived> RaiseAircraftConfigurationInfoReceived;

        [EventPublication(EventTopics.RealNameReceived)]
        public event EventHandler<NetworkDataReceived> RaiseRealNameReceived;

        [EventPublication(EventTopics.IsValidAtcReceived)]
        public event EventHandler<IsValidATCReceived> RaiseIsValidATCReceived;

        [EventPublication(EventTopics.AtisLinesReceived)]
        public event EventHandler<NetworkDataReceived> RaiseAtisLinesReceived;

        [EventPublication(EventTopics.AtisEndReceived)]
        public event EventHandler<NetworkDataReceived> RaiseAtisEndReceived;

        [EventPublication(EventTopics.CapabilitiesResponseReceived)]
        public event EventHandler<NetworkDataReceived> RaisCapabilitiesResponseReceived;

        [EventPublication(EventTopics.PlaneInfoReceived)]
        public event EventHandler<PlaneInfoReceived> RaisePlaneInfoReceived;

        [EventPublication(EventTopics.MetarReceived)]
        public event EventHandler<NetworkDataReceived> RaiseMetarReceived;

        [EventPublication(EventTopics.SelcalAlertReceived)]
        public event EventHandler<SelcalAlertReceived> RaiseSelcalAlertReceived;

        [EventPublication(EventTopics.RemoteFlightPlanReceived)]
        public event EventHandler<FlightPlanReceived> RaiseRemoteFlightPlanReceived;

        [EventPublication(EventTopics.TransponderIdentStateChanged)]
        public event EventHandler<TransponderIdentStateChanged> RaiseTransponderIdentChanged;

        [EventPublication(EventTopics.PlayNotificationSound)]
        public event EventHandler<PlayNotificationSound> RaisePlayNotificationSound;

        [EventPublication(EventTopics.ControllerInfoReceived)]
        public event EventHandler<NetworkDataReceived> RaiseControllerInfoReceived;

        [EventPublication(EventTopics.LegacyPlaneInfoReceived)]
        public event EventHandler<NetworkDataReceived> RaiseLegacyPlaneInfoReceived;

        [EventPublication(EventTopics.NetworkServerListUpdated)]
        public event EventHandler<EventArgs> RaiseNetworkServerListUpdated;

        private const string STATUS_FILE_URL = "http://status.vatsim.net";
        private readonly IAppConfig mConfig;
        private readonly FSDSession mFsd;
        private readonly Version mFsdClientVersion;
        private readonly Timer mPositionUpdateTimer;
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
        private UserAircraftRadioStack mRadioStackState;
        private List<NetworkServerInfo> mServerList = new List<NetworkServerInfo>();
        private readonly StreamWriter mRawDataStream;

        public FsdManager(IEventBroker broker, IAppConfig config) : base(broker)
        {
            mConfig = config;

            mFsdClientVersion = new Version("1.2");
            mPositionUpdateTimer = new Timer { Interval = 5000 };
            mPositionUpdateTimer.Elapsed += PositionUpdateTimer_Elapsed;

            mClientProperties = new ClientProperties("xPilot", mFsdClientVersion, "", "");
            mRawDataStream = new StreamWriter(Path.Combine(mConfig.AppPath, $"NetworkLogs/NetworkLog-{DateTime.UtcNow:yyyyMMddHHmmss}.log"), false);

            mFsd = new FSDSession(mClientProperties)
            {
                IgnoreUnknownPackets = true
            };
            mFsd.NetworkError += Fsd_NetworkError;
            mFsd.ProtocolErrorReceived += Fsd_ProtocolErrorReceived;
            mFsd.RawDataReceived += Fsd_RawDataReceived;
            mFsd.RawDataSent += Fsd_RawDataSent;
            mFsd.NetworkConnected += Fsd_NetworkConnected;
            mFsd.NetworkDisconnected += Fsd_NetworkDisconnected;
            mFsd.ServerIdentificationReceived += Fsd_ServerIdentificationReceived;
            mFsd.KillRequestReceived += Fsd_KillRequestReceived;
            mFsd.AcarsResponseReceived += Fsd_AcarsResponseReceived;
            mFsd.RadioMessageReceived += Fsd_RadioMessageReceived;
            mFsd.TextMessageReceived += Fsd_TextMessageReceived;
            mFsd.BroadcastMessageReceived += Fsd_BroadcastMessageReceived;
            mFsd.PilotPositionReceived += Fsd_PilotPositionReceived;
            mFsd.ATCPositionReceived += Fsd_ATCPositionReceived;
            mFsd.ClientQueryResponseReceived += Fsd_ClientQueryResponseReceived;
            mFsd.ClientQueryReceived += Fsd_ClientQueryReceived;
            mFsd.PlaneInfoRequestReceived += Fsd_PlaneInfoRequestReceived;
            mFsd.PlaneInfoResponseReceived += Fsd_PlaneInfoResponseReceived;
            mFsd.LegacyPlaneInfoResponseReceived += Fsd_LegacyPlaneInfoResponseReceived;
            mFsd.DeletePilotReceived += Fsd_DeletePilotReceived;
            mFsd.DeleteATCReceived += Fsd_DeleteATCReceived;
            mFsd.FlightPlanReceived += Fsd_FlightPlanReceived;
        }

        private void Fsd_FlightPlanReceived(object sender, DataReceivedEventArgs<PDUFlightPlan> e)
        {
            if (e.PDU.From != OurCallsign)
            {
                FlightPlan fp = ParseFlightPlan(e.PDU);
                RaiseRemoteFlightPlanReceived?.Invoke(this, new FlightPlanReceived(fp));
            }
        }

        private void Fsd_DeleteATCReceived(object sender, DataReceivedEventArgs<PDUDeleteATC> e)
        {
            RaiseControllerDeleted?.Invoke(this, new NetworkDataReceived(e.PDU.From));
        }

        private void Fsd_DeletePilotReceived(object sender, DataReceivedEventArgs<PDUDeletePilot> e)
        {
            RaiseDeletePilotReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From));
        }

        private void Fsd_LegacyPlaneInfoResponseReceived(object sender, DataReceivedEventArgs<PDULegacyPlaneInfoResponse> e)
        {
            RaiseLegacyPlaneInfoReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From, e.PDU.CSL.Replace("CSL=", "").Replace("~", "")));
        }

        private void Fsd_PlaneInfoResponseReceived(object sender, DataReceivedEventArgs<PDUPlaneInfoResponse> e)
        {
            RaisePlaneInfoReceived?.Invoke(this, new PlaneInfoReceived(e.PDU.From, e.PDU.Equipment, e.PDU.Airline, e.PDU.Livery, e.PDU.CSL)); ;
        }

        private void Fsd_PlaneInfoRequestReceived(object sender, DataReceivedEventArgs<PDUPlaneInfoRequest> e)
        {
            Match match = Regex.Match(OurCallsign, "^([A-Z]{3})\\d+", RegexOptions.IgnoreCase);
            mFsd.SendPDU(new PDUPlaneInfoResponse(OurCallsign, e.PDU.From, mConnectInfo.TypeCode, match.Success ? match.Groups[1].Value : "", "", ""));
        }

        private void Fsd_ClientQueryReceived(object sender, DataReceivedEventArgs<PDUClientQuery> e)
        {
            switch (e.PDU.QueryType)
            {
                case ClientQueryType.Capabilities:
                    if (e.PDU.From.ToUpper() != "SERVER")
                    {
                        RaiseCapabilitiesRequestReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From));
                    }
                    break;
                case ClientQueryType.COM1Freq:
                    if (mRadioStackState != null)
                    {
                        mFsd.SendPDU(new PDUClientQueryResponse(OurCallsign, e.PDU.From, ClientQueryType.COM1Freq, new List<string> { (mRadioStackState.Com1ActiveFreq.Normalize25KhzFrequency() / 1000000.0f).ToString("0.000") }));
                    }
                    break;
                case ClientQueryType.RealName:
                    mFsd.SendPDU(new PDUClientQueryResponse(OurCallsign, e.PDU.From, ClientQueryType.RealName, new List<string> { mConfig.NameWithAirport, "", "1" }));
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
                        mUserAircraftData.AltitudeMsl
                    });
                    mFsd.SendPDU(new PDUTextMessage(OurCallsign, e.PDU.From, info));
                    break;
                case ClientQueryType.AircraftConfiguration:
                    RaiseAircraftConfigurationInfoReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From, string.Join(":", e.PDU.Payload.ToArray())));
                    break;
            }
        }

        private void Fsd_ClientQueryResponseReceived(object sender, DataReceivedEventArgs<PDUClientQueryResponse> e)
        {
            switch (e.PDU.QueryType)
            {
                case ClientQueryType.IsValidATC:
                    RaiseIsValidATCReceived?.Invoke(this, new IsValidATCReceived(e.PDU.Payload[1].ToUpper(), e.PDU.Payload[0].ToUpper() == "Y"));
                    break;
                case ClientQueryType.Capabilities:
                    RaisCapabilitiesResponseReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From, string.Join(":", e.PDU.Payload.ToArray())));
                    break;
                case ClientQueryType.RealName:
                    RaiseRealNameReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From, e.PDU.Payload[0]));
                    break;
                case ClientQueryType.ATIS:
                    switch (e.PDU.Payload[0])
                    {
                        case "E":
                            RaiseAtisEndReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From));
                            break;
                        case "Z":
                            RaiseAtisLinesReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From, $"Estimated Logoff Time: {e.PDU.Payload[1]}"));
                            break;
                        case "T":
                            RaiseControllerInfoReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From, e.PDU.Payload[1]));
                            break;
                    }
                    break;
                case ClientQueryType.PublicIP:
                    mPublicIP = (e.PDU.Payload.Count > 0) ? e.PDU.Payload[0] : "";
                    break;
            }
        }

        private void Fsd_ATCPositionReceived(object sender, DataReceivedEventArgs<PDUATCPosition> e)
        {
            Controller c = new Controller
            {
                Callsign = e.PDU.From,
                Frequency = (uint)e.PDU.Frequency,
                Location = new WorldPoint(e.PDU.Lon, e.PDU.Lat)
            };
            RaiseControllerUpdateReceived?.Invoke(this, new ControllerUpdateReceived(c));
        }

        private void Fsd_PilotPositionReceived(object sender, DataReceivedEventArgs<PDUPilotPosition> e)
        {
            if (!IsObserver || !Regex.IsMatch(OurCallsign, "^" + Regex.Escape(e.PDU.From) + "[A-Z]$"))
            {
                NetworkAircraftPose pose = new NetworkAircraftPose();
                pose.Transponder.TransponderCode = e.PDU.SquawkCode;
                pose.Transponder.TransponderIdent = e.PDU.IsIdenting;
                pose.Transponder.TransponderModeC = e.PDU.IsSquawkingModeC;
                pose.Altitude = e.PDU.TrueAltitude;
                pose.Bank = e.PDU.Bank;
                pose.Pitch = e.PDU.Pitch;
                pose.Heading = e.PDU.Heading;
                pose.GroundSpeed = e.PDU.GroundSpeed;
                pose.Location = new WorldPoint(e.PDU.Lon, e.PDU.Lat);
                RaisePilotPositionReceived?.Invoke(this, new NetworkAircraftUpdateReceived(e.PDU.From, pose));
            }
        }

        private void Fsd_BroadcastMessageReceived(object sender, DataReceivedEventArgs<PDUBroadcastMessage> e)
        {
            RaiseBroadcastMessageReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From, e.PDU.Message));
        }

        private void Fsd_TextMessageReceived(object sender, DataReceivedEventArgs<PDUTextMessage> e)
        {
            if (e.PDU.From.ToUpper() == "SERVER")
            {
                RaiseServerMessageReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From.ToUpper(), e.PDU.Message));
            }
            else
            {
                RaisePrivateMessageReceived?.Invoke(this, new PrivateMessageReceived(e.PDU.From, e.PDU.Message));
            }
        }

        private void Fsd_RadioMessageReceived(object sender, DataReceivedEventArgs<PDURadioMessage> e)
        {
            List<int> tunedFrequencies = new List<int>();
            if (mRadioStackState != null)
            {
                foreach (int freq in e.PDU.Frequencies)
                {
                    if (mRadioStackState.IsCom1Receiving && freq.FsdFrequencyToHertz().Normalize25KhzFrequency() == mRadioStackState.Com1ActiveFreq.Normalize25KhzFrequency())
                    {
                        if (!tunedFrequencies.Contains(freq))
                        {
                            tunedFrequencies.Add(freq);
                        }
                    }
                    else if (mRadioStackState.IsCom2Receiving && freq.FsdFrequencyToHertz().Normalize25KhzFrequency() == mRadioStackState.Com2ActiveFreq.Normalize25KhzFrequency())
                    {
                        if (!tunedFrequencies.Contains(freq))
                        {
                            tunedFrequencies.Add(freq);
                        }
                    }
                }
            }

            if (tunedFrequencies.Count > 0)
            {
                var match = Regex.Match(e.PDU.Message, "^SELCAL ([A-Z][A-Z]\\-[A-Z][A-Z])$");
                if (match.Success)
                {
                    var selcal = "SELCAL " + match.Groups[1].Value.Replace("-", "");
                    if (!string.IsNullOrEmpty(mConnectInfo.SelCalCode)
                        && selcal == "SELCAL " + mConnectInfo.SelCalCode.Replace("-", ""))
                    {
                        RaiseSelcalAlertReceived?.Invoke(this, new SelcalAlertReceived(e.PDU.From, e.PDU.Frequencies));
                    }
                }
                else
                {
                    bool isDirectMessage = e.PDU.Message.ToUpper().StartsWith(OurCallsign.ToUpper());
                    RaiseRadioMessageReceived?.Invoke(this, new RadioMessageReceived(e.PDU.From.ToUpper(), tunedFrequencies.ToArray(), e.PDU.Message, isDirectMessage));
                }
            }
        }

        private void Fsd_AcarsResponseReceived(object sender, DataReceivedEventArgs<PDUMetarResponse> e)
        {
            RaiseMetarReceived?.Invoke(this, new NetworkDataReceived(e.PDU.From, e.PDU.Metar));
        }

        private void Fsd_KillRequestReceived(object sender, DataReceivedEventArgs<PDUKillRequest> e)
        {
            mKillReason = e.PDU.Reason;
            mForcedDisconnect = true;
        }

        private void Fsd_ServerIdentificationReceived(object sender, DataReceivedEventArgs<PDUServerIdentification> e)
        {
            mFsd.SendPDU(new PDUClientIdentification(OurCallsign, mFsd.GetClientKey(), "xPilot", mFsdClientVersion.Major, mFsdClientVersion.Minor, mConfig.VatsimId.Trim(), mSystemUID, null));

            if (IsObserver)
            {
                mFsd.SendPDU(new PDUAddATC(OurCallsign, mConfig.Name, mConfig.VatsimId.Trim(), mConfig.VatsimPasswordDecrypted.Trim(), NetworkRating.OBS, ProtocolRevision.VatsimAuth));
            }
            else
            {
                mFsd.SendPDU(new PDUAddPilot(OurCallsign, mConfig.VatsimId.Trim(), mConfig.VatsimPasswordDecrypted.Trim(), NetworkRating.OBS, ProtocolRevision.VatsimAuth, SimulatorType.XPlane, mConfig.Name));
            }

            RequestPublicIP();
            SendPositionUpdate();

            mPositionUpdateTimer.Interval = IsObserver ? 15000 : 5000;
            mPositionUpdateTimer.Start();
        }

        private void Fsd_NetworkDisconnected(object sender, NetworkEventArgs e)
        {
            mPositionUpdateTimer.Stop();
            DisconnectInfo di = new DisconnectInfo
            {
                Type = (mForcedDisconnect ? DisconnectType.Forcible : (mIntentionalDisconnect ? DisconnectType.Intentional : DisconnectType.Other)),
                Reason = (mForcedDisconnect ? mKillReason : "")
            };
            RaiseNetworkDisconnected?.Invoke(this, new NetworkDisconnected(di));
            mIntentionalDisconnect = false;
            mForcedDisconnect = false;
            mIsIdenting = false;
            mIsIdentPressed = false;
            switch (di.Type)
            {
                case DisconnectType.Forcible:
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Warning, !string.IsNullOrEmpty(mKillReason) ? $"Forcibly disconnected from the network: {mKillReason}" : "Forcibly disconnected from the network."));
                    break;
                default:
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, "Disconnected from network"));
                    break;
            }
        }

        private void Fsd_NetworkConnected(object sender, NetworkEventArgs e)
        {
            mForcedDisconnect = false;
            mIntentionalDisconnect = false;
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, IsObserver ? "Connected to network in oberver mode" : "Connected to network"));
            RaiseNetworkConnected?.Invoke(this, new NetworkConnected(ConnectInfo));
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

        private void Fsd_ProtocolErrorReceived(object sender, DataReceivedEventArgs<PDUProtocolError> e)
        {
            if (e.PDU.ErrorType != NetworkError.NoFlightPlan)
            {
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, $"Network Error: {e.PDU.Message}"));
            }
        }

        private void Fsd_NetworkError(object sender, NetworkErrorEventArgs e)
        {
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, e.Error));
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
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "Server list download cancelled. Using previously cached server list."));
                }
                else if (t.Exception != null)
                {
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, $"Server list download failed. Using previously cached server list. Error: {t.Exception.Message}"));
                }
                else
                {
                    mConfig.CachedServers.Clear();
                    foreach (NetworkServerInfo server in mServerList)
                    {
                        mConfig.CachedServers.Add(server);
                    }
                    mConfig.SaveConfig();
                    RaiseNetworkServerListUpdated?.Invoke(this, EventArgs.Empty);
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"Server list successfully downloaded. { mServerList.Count } servers found."));
                }
            });
        }

        private void PositionUpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            SendPositionUpdate();
        }

        private void SendPositionUpdate()
        {
            if (IsConnected && mUserAircraftData != null)
            {
                if (IsObserver)
                {
                    mFsd.SendPDU(new PDUATCPosition(OurCallsign, 99998, NetworkFacility.OBS, 40, NetworkRating.OBS, mUserAircraftData.Latitude, mUserAircraftData.Longitude));
                }
                else
                {
                    if (mIsIdentPressed)
                    {
                        mIsIdenting = true;
                        mIsIdentPressed = false;
                    }
                    else if (mIsIdenting)
                    {
                        mIsIdenting = false;
                        RaiseTransponderIdentChanged?.Invoke(this, new TransponderIdentStateChanged(false));
                    }
                    mFsd.SendPDU(new PDUPilotPosition(OurCallsign, mUserAircraftData.TransponderCode, mUserAircraftData.TransponderMode >= 2, mIsIdenting || mUserAircraftData.TransponderIdent, NetworkRating.OBS, mUserAircraftData.Latitude, mUserAircraftData.Longitude, Convert.ToInt32(mUserAircraftData.AltitudeMsl * 3.28084), Convert.ToInt32(mUserAircraftData.AltitudeAgl), Convert.ToInt32(mUserAircraftData.GroundSpeed), Convert.ToInt32(mUserAircraftData.Pitch), Convert.ToInt32(mUserAircraftData.Roll), Convert.ToInt32(mUserAircraftData.Yaw)));
                }
            }
        }

        private void RequestPublicIP()
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, "SERVER", ClientQueryType.PublicIP, null));
            }
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

        public void CheckIfValidATC(string callsign)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, "SERVER", ClientQueryType.IsValidATC, new List<string> { callsign }));
            }
        }

        public void Connect(ConnectInfo info, string address)
        {
            //address = "192.168.50.104";
            mConnectInfo = info;
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, "Connecting to network"));
            mFsd.Connect(address, 6809, !IsTowerView);
        }

        public void Disconnect(DisconnectInfo info)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUDeletePilot(OurCallsign, mConfig.VatsimId.Trim()));
                mFsd.Disconnect();
            }
        }

        public void ForceDisconnect(string reason = "")
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUDeletePilot(OurCallsign, mConfig.VatsimId.Trim()));
                mFsd.Disconnect();
            }
            DisconnectInfo di = new DisconnectInfo
            {
                Type = DisconnectType.Forcible,
                Reason = reason
            };
            RaiseNetworkDisconnected?.Invoke(this, new NetworkDisconnected(di));
        }

        public void RequestClientCapabilities(string callsign)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, callsign, ClientQueryType.Capabilities));
            }
        }

        public void RequestControllerInfo(string callsign)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, callsign, ClientQueryType.ATIS));
            }
        }

        public void RequestFullAircraftConfiguration(string callsign)
        {
            if (IsConnected)
            {
                AircraftConfigurationInfo acconfig = new AircraftConfigurationInfo
                {
                    Request = new AircraftConfigurationInfo.RequestType?(AircraftConfigurationInfo.RequestType.Full)
                };
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, callsign, ClientQueryType.AircraftConfiguration, new List<string>
                {
                    acconfig.ToJson()
                }));
            }
        }

        public void RequestInfoQuery(string to)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, to, ClientQueryType.INF));
            }
        }

        public void RequestPlaneInformation(string callsign)
        {
            if (IsConnected && !string.IsNullOrEmpty(callsign))
            {
                mFsd.SendPDU(new PDUPlaneInfoRequest(OurCallsign, callsign));
            }
        }

        public void RequestRealName(string callsign)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, callsign, ClientQueryType.RealName));
            }
        }

        public void RequestClientFlightPlan(string callsign)
        {
            if (IsConnected && !string.IsNullOrEmpty(callsign))
            {
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, "SERVER", ClientQueryType.FlightPlan, new List<string> { callsign.ToUpper() }));
            }
        }

        public List<NetworkServerInfo> ReturnServerList()
        {
            return mServerList;
        }

        public void SendAircraftConfiguration(string to, AircraftConfiguration config)
        {
            if (IsConnected)
            {
                AircraftConfigurationInfo acconfig = new AircraftConfigurationInfo
                {
                    Config = config
                };
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, to, ClientQueryType.AircraftConfiguration, new List<string>
                {
                    acconfig.ToJson()
                }));
            }
        }

        public void SendClientCaps(string from)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUClientQueryResponse(OurCallsign, from, ClientQueryType.Capabilities, new List<string>
                {
                    "VERSION=1",
                    "ATCINFO=1",
                    "MODELDESC=1",
                    "ACCONFIG=1"
                }));
            }
        }

        public void SendIncrementalAircraftConfigurationUpdate(AircraftConfiguration config)
        {
            if (IsConnected)
            {
                AircraftConfigurationInfo acconfig = new AircraftConfigurationInfo
                {
                    Config = config
                };
                mFsd.SendPDU(new PDUClientQuery(OurCallsign, "@94836", ClientQueryType.AircraftConfiguration, new List<string>
                {
                    acconfig.ToJson()
                }));
            }
        }

        public void SquawkIdent()
        {
            if (IsConnected)
            {
                mIsIdentPressed = true;
                RaiseTransponderIdentChanged?.Invoke(this, new TransponderIdentStateChanged(true));
            }
        }

        public bool IsConnected => mFsd.Connected;

        public bool IsObserver => mConnectInfo.ObserverMode;

        public bool IsTowerView => mConnectInfo.TowerViewMode;

        public string OurCallsign { get => mConnectInfo.Callsign; set => _ = mConnectInfo.Callsign; }

        public string SelcalCode => mConnectInfo.SelCalCode;

        public ClientProperties ClientProperties { get => mClientProperties; set => _ = mClientProperties; }

        public ConnectInfo ConnectInfo => mConnectInfo;

        [EventSubscription(EventTopics.UserAircraftDataChanged, typeof(OnPublisher))]
        public void OnUserAircraftDataUpdated(object sender, UserAircraftDataChanged e)
        {
            if (mUserAircraftData == null || !mUserAircraftData.Equals(e.AircraftData))
            {
                mUserAircraftData = e.AircraftData;
            }
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnPublisher))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChanged e)
        {
            if (mRadioStackState == null || !mRadioStackState.Equals(e.RadioStack))
            {
                mRadioStackState = e.RadioStack;
            }
        }

        [EventSubscription(EventTopics.MetarRequested, typeof(OnPublisher))]
        public void MetarRequested(object sender, MetarRequestSent e)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUMetarRequest(OurCallsign, e.Station));
            }
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnPublisher))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSent e)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUTextMessage(OurCallsign, e.To, e.Message));
            }
        }

        [EventSubscription(EventTopics.RadioMessageSent, typeof(OnPublisher))]
        public void OnRadioMessageSent(object sender, RadioMessageSent e)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDURadioMessage(OurCallsign, e.Frequencies, e.Message));
            }
        }

        [EventSubscription(EventTopics.WallopSent, typeof(OnPublisher))]
        public void OnWallopRequestSent(object sender, WallopSent e)
        {
            if (IsConnected)
            {
                mFsd.SendPDU(new PDUWallop(OurCallsign, e.Message));
            }
        }

        [EventSubscription(EventTopics.MainFormShown, typeof(OnPublisher))]
        public void OnMainFormShown(object sender, EventArgs e)
        {
            DownloadNetworkServersAsync();
        }
    }
}
