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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Fsd.Connector;
using Vatsim.Fsd.Connector.PDU;
using XPilot.PilotClient.Aircraft;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.XplaneAdapter;

namespace XPilot.PilotClient.Network
{
    public class FsdManager : EventBus, IFsdManger
    {
        [EventPublication(EventTopics.NetworkConnected)]
        public event EventHandler<NetworkConnectedEventArgs> NetworkConnected;

        [EventPublication(EventTopics.NetworkDisconnected)]
        public event EventHandler<NetworkDisconnectedEventArgs> NetworkDisconnected;

        [EventPublication(EventTopics.NetworkServerListUpdated)]
        public event EventHandler<EventArgs> NetworkServerListUpdated;

        [EventPublication(EventTopics.RadioMessageReceived)]
        public event EventHandler<RadioMessageReceivedEventArgs> RadioMessageReceived;

        [EventPublication(EventTopics.BroadcastMessageReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> BroadcastMessageReceived;

        [EventPublication(EventTopics.PrivateMessageReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> PrivateMessageReceived;

        [EventPublication(EventTopics.ServerMessageReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> ServerMessageReceived;

        [EventPublication(EventTopics.DeletePilotReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> DeletePilotReceived;

        [EventPublication(EventTopics.NetworkAircraftUpdateReceived)]
        public event EventHandler<AircraftUpdateReceivedEventArgs> AircraftUpdateReceived;

        [EventPublication(EventTopics.ControllerUpdateReceived)]
        public event EventHandler<ControllerUpdateReceivedEventArgs> ControllerUpdateReceived;

        [EventPublication(EventTopics.DeleteControllerReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> DeleteControllerReceived;

        [EventPublication(EventTopics.CapabilitiesRequestReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> CapabilitiesRequestReceived;

        [EventPublication(EventTopics.AircraftConfigurationInfoReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> AircraftConfigurationInfoReceived;

        [EventPublication(EventTopics.RealNameReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> RealNameReceived;

        [EventPublication(EventTopics.IsValidAtcReceived)]
        public event EventHandler<IsValidAtcReceivedEventArgs> IsValidATCReceived;

        [EventPublication(EventTopics.AtisLinesReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> AtisLinesReceived;

        [EventPublication(EventTopics.AtisEndReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> AtisEndReceived;

        [EventPublication(EventTopics.CapabilitiesResponseReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> CapabilitiesResponseReceived;

        [EventPublication(EventTopics.NetworkAircraftInfoReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> AircraftInfoReceived;

        [EventPublication(EventTopics.MetarReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> MetarReceived;

        [EventPublication(EventTopics.SelcalAlertReceived)]
        public event EventHandler<SelcalAlertReceivedEventArgs> SelcalAlertReceived;

        [EventPublication(EventTopics.NoFlightPlanReceived)]
        public event EventHandler<EventArgs> NoFlightPlanReceived;

        [EventPublication(EventTopics.FlightPlanReceived)]
        public event EventHandler<FlightPlanReceivedEventArgs> FlightPlanReceived;

        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        [EventPublication(EventTopics.SquawkingIdentChanged)]
        public event EventHandler<SquawkingIdentChangedEventArgs> SquawkingIdentChanged;

        [EventPublication(EventTopics.PlaySoundRequested)]
        public event EventHandler<PlaySoundEventArgs> PlaySoundRequested;

        [EventPublication(EventTopics.ControllerInfoReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> ControllerInfoReceived;

        [EventPublication(EventTopics.LegacyPlaneInfoReceived)]
        public event EventHandler<NetworkDataReceivedEventArgs> LegacyPlaneInfoReceived;

        private const string VATSIM_STATUS_FILE_URL = "http://status.vatsim.net";
        private readonly FSDSession FSD;
        private readonly IAppConfig mConfig;
        private List<NetworkServerInfo> mServerList = new List<NetworkServerInfo>();
        private string mSystemUID;
        private string mPublicIP;
        private ConnectInfo mConnectInfo;
        private bool mForcedDisconnect;
        private bool mIntentialDisconnect;
        private string mKillReason;
        private bool mIsIdenting = false;
        private bool mIsIdentPressed = false;
        private UserAircraftData mUserAircraftData;
        private UserAircraftRadioStack mRadioStackData;
        private readonly System.Timers.Timer mPositionUpdateTimer;
        private readonly StreamWriter mRawDataStream;
        private readonly Version mVersion = new Version("1.2");

        public FsdManager(IEventBroker broker, IAppConfig config) : base(broker)
        {
            mConfig = config;
            mSystemUID = SystemIdentifier.GetSystemDriveVolumeId();

            mPositionUpdateTimer = new System.Timers.Timer();
            mPositionUpdateTimer.Elapsed += PositionUpdateTimer_Elapsed;

            if (!Directory.Exists(Path.Combine(mConfig.AppPath, "NetworkLogs")))
            {
                Directory.CreateDirectory(Path.Combine(mConfig.AppPath, "NetworkLogs"));
            }

            var directory = new DirectoryInfo(Path.Combine(mConfig.AppPath, "NetworkLogs"));
            var query = directory.GetFiles("*", SearchOption.AllDirectories);
            foreach (var file in query.OrderByDescending(file => file.CreationTime).Skip(10))
            {
                file.Delete();
            }

            mRawDataStream = new StreamWriter(Path.Combine(mConfig.AppPath, $"NetworkLogs/NetworkLog-{DateTime.UtcNow:yyyyMMddHHmmss}.log"), false);

            FSD = new FSDSession(new ClientProperties("xPilot", mVersion, Assembly.GetEntryAssembly().Location, mConfig.FullXplanePath));
            FSD.IgnoreUnknownPackets = true;
            FSD.NetworkError += FSD_NetworkError;
            FSD.ProtocolErrorReceived += FSD_ProtocolErrorReceived;
            FSD.RawDataReceived += FSD_RawDataReceived;
            FSD.RawDataSent += FSD_RawDataSent;
            FSD.NetworkConnected += FSD_NetworkConnected;
            FSD.NetworkDisconnected += FSD_NetworkDisconnected;
            FSD.ServerIdentificationReceived += FSD_ServerIdentificationReceived;
            FSD.KillRequestReceived += FSD_KillRequestReceived;
            FSD.AcarsResponseReceived += FSD_AcarsResponseReceived;
            FSD.RadioMessageReceived += FSD_RadioMessageReceived;
            FSD.TextMessageReceived += FSD_TextMessageReceived;
            FSD.BroadcastMessageReceived += FSD_BroadcastMessageReceived;
            FSD.PilotPositionReceived += FSD_PilotPositionReceived;
            FSD.ATCPositionReceived += FSD_ATCPositionReceived;
            FSD.ClientQueryResponseReceived += FSD_ClientQueryResponseReceived;
            FSD.ClientQueryReceived += FSD_ClientQueryReceived;
            FSD.PlaneInfoRequestReceived += FSD_PlaneInfoRequestReceived;
            FSD.PlaneInfoResponseReceived += FSD_PlaneInfoResponseReceived;
            FSD.LegacyPlaneInfoResponseReceived += FSD_LegacyPlaneInfoResponseReceived;
            FSD.DeletePilotReceived += FSD_DeletePilotReceived;
            FSD.DeleteATCReceived += FSD_DeleteATCReceived;
            FSD.FlightPlanReceived += FSD_FlightPlanReceived;
            FSD.PingReceived += FSD_PingReceived;
        }

        private void FSD_NetworkError(object sender, NetworkErrorEventArgs e)
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, e.Error));
        }

        private void FSD_ProtocolErrorReceived(object sender, DataReceivedEventArgs<PDUProtocolError> e)
        {
            if (e.PDU.ErrorType == NetworkError.NoFlightPlan)
            {
                NoFlightPlanReceived?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, $"Network error: { e.PDU.Message }"));
            }
        }

        private void FSD_RawDataSent(object sender, RawDataEventArgs e)
        {
            mRawDataStream.Write("[{0}] >>>     {1}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), e.Data.Replace(mConfig.VatsimPassword, "XXXXXX"));
            mRawDataStream.Flush();
        }

        private void FSD_RawDataReceived(object sender, RawDataEventArgs e)
        {
            mRawDataStream.Write("[{0}]     <<< {1}", DateTime.UtcNow.ToString("HH:mm:ss.fff"), e.Data);
            mRawDataStream.Flush();
        }

        private void FSD_NetworkConnected(object sender, NetworkEventArgs e)
        {
            mForcedDisconnect = false;
            mIntentialDisconnect = false;
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, mConnectInfo.ObserverMode ? "Connected to network in observer mode" : "Connected to network"));
            NetworkConnected?.Invoke(this, new NetworkConnectedEventArgs(mConnectInfo));
        }

        private void FSD_NetworkDisconnected(object sender, NetworkEventArgs e)
        {
            mPositionUpdateTimer.Stop();
            DisconnectInfo disconnectInfo = new DisconnectInfo
            {
                Type = (mForcedDisconnect ? DisconnectType.Forcible : (mIntentialDisconnect ? DisconnectType.Intentional : DisconnectType.Other)),
                Reason = (mForcedDisconnect ? mKillReason : string.Empty)
            };
            NetworkDisconnected?.Invoke(this, new NetworkDisconnectedEventArgs(disconnectInfo));
            mIntentialDisconnect = false;
            mForcedDisconnect = false;
            mIsIdenting = false;
            mIsIdentPressed = false;
            if (disconnectInfo.Type == DisconnectType.Forcible)
            {
                if (!string.IsNullOrEmpty(disconnectInfo.Reason))
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Warning, $"Forcibly disconnected from the network: { disconnectInfo.Reason }"));
                }
                else
                {
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Warning, "Forcibly disconnected from the network."));
                }
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
            }
            else
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Disconnected from the network."));
            }
        }

        private void FSD_ServerIdentificationReceived(object sender, DataReceivedEventArgs<PDUServerIdentification> e)
        {
            FSD.SendPDU(new PDUClientIdentification(OurCallsign, FSD.GetClientKey(), "xPilot", mVersion.Major, mVersion.Minor, mConfig.VatsimId.Trim(), mSystemUID, null));

            if (mConnectInfo.ObserverMode)
            {
                FSD.SendPDU(new PDUAddATC(mConnectInfo.Callsign, mConfig.Name, mConfig.VatsimId.Trim(), mConfig.VatsimPassword.Trim(), NetworkRating.OBS, ProtocolRevision.VatsimAuth));
            }
            else
            {
                FSD.SendPDU(new PDUAddPilot(OurCallsign, mConfig.VatsimId.Trim(), mConfig.VatsimPassword.Trim(), NetworkRating.OBS, ProtocolRevision.VatsimAuth, SimulatorType.XPlane, mConfig.NameWithAirport));
            }

            RequestPublicIP();
            SendPositionUpdate();

            mPositionUpdateTimer.Interval = mConnectInfo.ObserverMode ? 15000 : 5000;
            mPositionUpdateTimer.Start();
        }

        private void FSD_KillRequestReceived(object sender, DataReceivedEventArgs<PDUKillRequest> e)
        {
            mKillReason = e.PDU.Reason;
            mForcedDisconnect = true;
        }

        private void FSD_AcarsResponseReceived(object sender, DataReceivedEventArgs<PDUMetarResponse> e)
        {
            MetarReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Metar));
        }

        private void FSD_RadioMessageReceived(object sender, DataReceivedEventArgs<PDURadioMessage> e)
        {
            List<int> TunedFrequencies = new List<int>();

            if (mRadioStackData != null)
            {
                foreach (int freq in e.PDU.Frequencies)
                {
                    if (mRadioStackData.IsCom1Receiving && freq.FsdFrequencyToHertz().Normalize25KhzFrequency() == mRadioStackData.Com1ActiveFreq.Normalize25KhzFrequency())
                    {
                        if (!TunedFrequencies.Contains(freq))
                        {
                            TunedFrequencies.Add(freq);
                        }
                    }
                    else if (mRadioStackData.IsCom2Receiving && freq.FsdFrequencyToHertz().Normalize25KhzFrequency() == mRadioStackData.Com2ActiveFreq.Normalize25KhzFrequency())
                    {
                        if (!TunedFrequencies.Contains(freq))
                        {
                            TunedFrequencies.Add(freq);
                        }
                    }
                }
            }

            if (TunedFrequencies.Count > 0)
            {
                var match = Regex.Match(e.PDU.Message, "^SELCAL ([A-Z][A-Z]\\-[A-Z][A-Z])$");
                if (match.Success)
                {
                    var selcal = "SELCAL " + match.Groups[1].Value.Replace("-", "");
                    if (!string.IsNullOrEmpty(mConnectInfo.SelCalCode)
                        && selcal == "SELCAL " + mConnectInfo.SelCalCode.Replace("-", ""))
                    {
                        SelcalAlertReceived?.Invoke(this, new SelcalAlertReceivedEventArgs(e.PDU.From, e.PDU.Frequencies));
                    }
                }
                else
                {
                    bool isDirectMessage = e.PDU.Message.ToUpper().StartsWith(OurCallsign.ToUpper());
                    RadioMessageReceived?.Invoke(this, new RadioMessageReceivedEventArgs(e.PDU.From.ToUpper(), TunedFrequencies.ToArray(), e.PDU.Message, isDirectMessage));
                }
            }
        }

        private void FSD_TextMessageReceived(object sender, DataReceivedEventArgs<PDUTextMessage> e)
        {
            if (e.PDU.From.ToUpper() == "SERVER")
            {
                ServerMessageReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From.ToUpper(), e.PDU.Message));
            }
            else
            {
                PrivateMessageReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Message));
            }
        }

        private void FSD_BroadcastMessageReceived(object sender, DataReceivedEventArgs<PDUBroadcastMessage> e)
        {
            BroadcastMessageReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Message));
        }

        private void FSD_PilotPositionReceived(object sender, DataReceivedEventArgs<PDUPilotPosition> e)
        {
            if (!mConnectInfo.ObserverMode || !Regex.IsMatch(mConnectInfo.Callsign, "^" + Regex.Escape(e.PDU.From) + "[A-Z]$"))
            {
                NetworkAircraftState aircraftState = new NetworkAircraftState();
                aircraftState.Transponder.TransponderCode = e.PDU.SquawkCode;
                aircraftState.Transponder.TransponderIdent = e.PDU.IsIdenting;
                aircraftState.Transponder.TransponderModeC = e.PDU.IsSquawkingModeC;
                aircraftState.Altitude = e.PDU.TrueAltitude;
                aircraftState.Bank = e.PDU.Bank;
                aircraftState.Pitch = e.PDU.Pitch;
                aircraftState.Heading = e.PDU.Heading;
                aircraftState.GroundSpeed = e.PDU.GroundSpeed;
                aircraftState.Location = new WorldPoint(e.PDU.Lon, e.PDU.Lat);
                AircraftUpdateReceived?.Invoke(this, new AircraftUpdateReceivedEventArgs(e.PDU.From, aircraftState));
            }
        }

        private void FSD_ATCPositionReceived(object sender, DataReceivedEventArgs<PDUATCPosition> e)
        {
            ControllerUpdateReceived?.Invoke(this, new ControllerUpdateReceivedEventArgs(e.PDU.From, (uint)e.PDU.Frequency, new WorldPoint(e.PDU.Lon, e.PDU.Lat)));
        }

        private void FSD_ClientQueryResponseReceived(object sender, DataReceivedEventArgs<PDUClientQueryResponse> e)
        {
            switch (e.PDU.QueryType)
            {
                case ClientQueryType.IsValidATC:
                    IsValidATCReceived?.Invoke(this, new IsValidAtcReceivedEventArgs(e.PDU.Payload[1].ToUpper(), e.PDU.Payload[0].ToUpper() == "Y"));
                    break;
                case ClientQueryType.Capabilities:
                    CapabilitiesResponseReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From.ToUpper(), string.Join(":", e.PDU.Payload.ToArray())));
                    break;
                case ClientQueryType.RealName:
                    RealNameReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Payload[0]));
                    break;
                case ClientQueryType.ATIS:
                    switch (e.PDU.Payload[0])
                    {
                        // Controller information
                        case "E":
                            AtisEndReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From));
                            break;
                        // Estimated logoff time
                        case "Z":
                            AtisLinesReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, string.Format("Estimated Logoff Time: {0}", e.PDU.Payload[1])));
                            break;
                        // Controller Information
                        case "T":
                            ControllerInfoReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Payload[1]));
                            break;
                    }
                    break;
                case ClientQueryType.PublicIP:
                    mPublicIP = ((e.PDU.Payload.Count > 0) ? e.PDU.Payload[0] : "");
                    break;
            }
        }

        private void FSD_ClientQueryReceived(object sender, DataReceivedEventArgs<PDUClientQuery> e)
        {
            switch (e.PDU.QueryType)
            {
                case ClientQueryType.Capabilities:
                    if (e.PDU.From.ToUpper() != "SERVER")
                    {
                        CapabilitiesRequestReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From));
                    }
                    SendClientCaps(e.PDU.From);
                    break;
                case ClientQueryType.COM1Freq:
                    SendCom1Frequency(e.PDU.From, (mRadioStackData.Com1ActiveFreq.Normalize25KhzFrequency() / 1000000.0f).ToString("0.000"));
                    break;
                case ClientQueryType.RealName:
                    FSD.SendPDU(new PDUClientQueryResponse(OurCallsign, e.PDU.From, ClientQueryType.RealName, new List<string> { mConfig.NameWithAirport, "", "1" }));
                    break;
                case ClientQueryType.INF:
                    string info = string.Format("{0} {1} PID={2} ({3}) IP={4} SYS_UID={5} FSVER={6} LT={7} LO={8} AL={9}", new object[]
                    {
                        "xPilot",
                        Application.ProductVersion.ToString(),
                        mConfig.VatsimId.Trim(),
                        mConfig.NameWithAirport,
                        mPublicIP,
                        mSystemUID,
                        SimulatorType.XPlane,
                        mUserAircraftData.Latitude,
                        mUserAircraftData.Longitude,
                        mUserAircraftData.AltitudeMsl
                    });
                    FSD.SendPDU(new PDUTextMessage(OurCallsign, e.PDU.From, info));
                    break;
                case ClientQueryType.AircraftConfiguration:
                    AircraftConfigurationInfoReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, string.Join(":", e.PDU.Payload.ToArray())));
                    break;
                case ClientQueryType.NewInfo:
                    FSD.SendPDU(new PDUClientQuery(OurCallsign, e.PDU.From, ClientQueryType.ATIS, null));
                    break;
                default:
                    break;
            }
        }

        private void FSD_PlaneInfoRequestReceived(object sender, DataReceivedEventArgs<PDUPlaneInfoRequest> e)
        {
            Match match = Regex.Match(OurCallsign, "^([A-Z]{3})\\d+", RegexOptions.IgnoreCase);
            FSD.SendPDU(new PDUPlaneInfoResponse(OurCallsign, e.PDU.From, mConnectInfo.TypeCode, match.Success ? match.Groups[1].Value : "", "", ""));
        }

        private void FSD_PlaneInfoResponseReceived(object sender, DataReceivedEventArgs<PDUPlaneInfoResponse> e)
        {
            AircraftInfoReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.Equipment));
        }

        private void FSD_LegacyPlaneInfoResponseReceived(object sender, DataReceivedEventArgs<PDULegacyPlaneInfoResponse> e)
        {
            LegacyPlaneInfoReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From, e.PDU.CSL.Replace("CSL=", "").Replace("~", "")));
        }

        private void FSD_DeletePilotReceived(object sender, DataReceivedEventArgs<PDUDeletePilot> e)
        {
            DeletePilotReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From));
        }

        private void FSD_DeleteATCReceived(object sender, DataReceivedEventArgs<PDUDeleteATC> e)
        {
            DeleteControllerReceived?.Invoke(this, new NetworkDataReceivedEventArgs(e.PDU.From));
        }

        private void FSD_FlightPlanReceived(object sender, DataReceivedEventArgs<PDUFlightPlan> e)
        {
            if (e.PDU.From == OurCallsign)
            {
                FlightPlan flightPlan = ParseFlightPlan(e.PDU);
                if (string.IsNullOrEmpty(mConnectInfo.SelCalCode))
                {
                    string selcal = ExtractSelCal(flightPlan.Remarks);
                    if (!string.IsNullOrEmpty(selcal))
                    {
                        mConnectInfo.SelCalCode = selcal;
                    }
                }
                FlightPlanReceived?.Invoke(this, new FlightPlanReceivedEventArgs(flightPlan));
            }
        }

        private void FSD_PingReceived(object sender, DataReceivedEventArgs<PDUPing> e)
        {
            FSD.SendPDU(new PDUPong(OurCallsign, e.PDU.From, e.PDU.TimeStamp));
        }

        private void PositionUpdateTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            SendPositionUpdate();
        }

        private void SendPositionUpdate()
        {
            if (FSD.Connected)
            {
                if (mConnectInfo.ObserverMode)
                {
                    FSD.SendPDU(new PDUATCPosition(mConnectInfo.Callsign, 99998, NetworkFacility.OBS, 40, NetworkRating.OBS, mUserAircraftData.Latitude, mUserAircraftData.Longitude));
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
                        SquawkingIdentChanged?.Invoke(this, new SquawkingIdentChangedEventArgs(false));
                    }
                    if (mUserAircraftData != null)
                    {
                        FSD.SendPDU(new PDUPilotPosition(OurCallsign, mUserAircraftData.TransponderCode, mUserAircraftData.TransponderMode >= 2, mIsIdenting || mUserAircraftData.TransponderIdent, NetworkRating.OBS, mUserAircraftData.Latitude, mUserAircraftData.Longitude, Convert.ToInt32(mUserAircraftData.AltitudeMsl * 3.28084), Convert.ToInt32(mUserAircraftData.PressureAltitude), Convert.ToInt32(mUserAircraftData.GroundSpeed), Convert.ToInt32(mUserAircraftData.Pitch), Convert.ToInt32(mUserAircraftData.Bank), Convert.ToInt32(mUserAircraftData.Heading)));
                    }
                }
            }
        }

        private string ExtractSelCal(string remarks)
        {
            string result;
            Match match = Regex.Match(remarks, "SEL/([A-Z][A-Z])([A-Z][A-Z])");
            result = match.Success ? string.Format("{0}-{1}", match.Groups[1].Value, match.Groups[2].Value) : null;
            return result;
        }

        private FlightPlan ParseFlightPlan(PDUFlightPlan fp)
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
            flightPlan.FlightType = flightPlan.FlightType.FromString(fp.Rules.ToString());
            flightPlan.Equipment = fp.Equipment;
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

        private void RequestPublicIP()
        {
            if (FSD.Connected)
            {
                FSD.SendPDU(new PDUClientQuery(OurCallsign, "SERVER", ClientQueryType.PublicIP, null));
            }
        }

        public void FileFlightPlan(FlightPlan flightPlan)
        {
            if (FSD.Connected)
            {
                string acType = string.Format("{0}{1}{2}{3}", new object[]
                {
                flightPlan.IsHeavy ? "H/":"",
                mConnectInfo.TypeCode,
                string.IsNullOrEmpty(flightPlan.EquipmentSuffix) ? "" : "/",
                string.IsNullOrEmpty(flightPlan.EquipmentSuffix) ? "" : flightPlan.EquipmentSuffix
                });
                flightPlan.Remarks = Regex.Replace(flightPlan.Remarks, "/v/", "", RegexOptions.IgnoreCase);
                flightPlan.Remarks = Regex.Replace(flightPlan.Remarks, "/t/", "", RegexOptions.IgnoreCase);
                flightPlan.Remarks = Regex.Replace(flightPlan.Remarks, "/r/", "", RegexOptions.IgnoreCase);
                switch (flightPlan.VoiceType)
                {
                    case VoiceType.Full:
                        flightPlan.Remarks = string.Format("{0} /v/", flightPlan.Remarks);
                        break;
                    case VoiceType.ReceiveOnly:
                        flightPlan.Remarks = string.Format("{0} /r/", flightPlan.Remarks);
                        break;
                    case VoiceType.TextOnly:
                        flightPlan.Remarks = string.Format("{0} /t/", flightPlan.Remarks);
                        break;
                }
                // SELCAL
                string selcal = ExtractSelCal(flightPlan.Remarks);
                if (!string.IsNullOrEmpty(selcal))
                {
                    mConnectInfo.SelCalCode = selcal;
                }
                else if (!string.IsNullOrEmpty(mConnectInfo.SelCalCode))
                {
                    flightPlan.Remarks = string.Format("{0} SEL/{1}", flightPlan.Remarks, mConnectInfo.SelCalCode.Replace("-", ""));
                }
                FSD.SendPDU(new PDUFlightPlan(OurCallsign, "SERVER", FlightRules.IFR.FromString(flightPlan.FlightType.ToString()), acType, flightPlan.CruiseSpeed.ToString(), flightPlan.DepartureAirport, flightPlan.DepartureTime.ToString("0000"), "", flightPlan.CruiseAltitude.ToString(), flightPlan.DestinationAirport, flightPlan.EnrouteHours.ToString(), flightPlan.EnrouteMinutes.ToString(), flightPlan.FuelHours.ToString(), flightPlan.FuelMinutes.ToString(), flightPlan.AlternateAirport, flightPlan.Remarks, flightPlan.Route));
            }
        }

        public void CheckIfValidATC(string callsign)
        {
            if (FSD.Connected)
            {
                FSD.SendPDU(new PDUClientQuery(OurCallsign, "SERVER", ClientQueryType.IsValidATC, new List<string> { callsign }));
            }
        }

        private void RequestFlightPlan()
        {
            if (FSD.Connected)
            {
                FSD.SendPDU(new PDUClientQuery(OurCallsign, "SERVER", ClientQueryType.FlightPlan, new List<string> { OurCallsign }));
            }
        }

        public void Connect(ConnectInfo info, string address)
        {
            mConnectInfo = info;
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Connecting to network..."));
            FSD.Connect(address, 6809, !mConnectInfo.TowerViewMode);
        }

        public void Disconnect(DisconnectInfo info)
        {
            NetworkDisconnected?.Invoke(this, new NetworkDisconnectedEventArgs(info));
            if (FSD.Connected)
            {
                FSD.SendPDU(new PDUDeletePilot(OurCallsign, mConfig.VatsimId.Trim()));
                FSD.Disconnect();
            }
        }

        public void ForceDisconnect(string reason = "")
        {
            if (IsConnected)
            {
                FSD.SendPDU(new PDUDeletePilot(OurCallsign, mConfig.VatsimId.Trim()));
                FSD.Disconnect();
            }
            DisconnectInfo disconnectInfo = new DisconnectInfo
            {
                Type = DisconnectType.Quiet,
                Reason = reason
            };
            NetworkDisconnected?.Invoke(this, new NetworkDisconnectedEventArgs(disconnectInfo));
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Warning, disconnectInfo.Reason));
        }

        public void DownloadNetworkServers()
        {
            mServerList.Clear();

            try
            {
                foreach (var server in mConfig.CachedServers)
                {
                    mServerList.Add(server);
                }
            }
            catch { }

            BackgroundWorker serverListBackgroundWorker = new BackgroundWorker();
            serverListBackgroundWorker.DoWork += ServerListBackgroundWorker_DoWork;
            serverListBackgroundWorker.RunWorkerCompleted += ServerListBackgroundWorker_RunWorkerCompleted;
            serverListBackgroundWorker.RunWorkerAsync();
        }

        private void ServerListBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Server list download cancelled. Using previously cached server list."));
            }
            else if (e.Error != null)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, $"Server list download failed. Using previously cached server list. Error: { e.Error.Message }"));
            }
            else
            {
                mConfig.CachedServers.Clear();
                foreach (NetworkServerInfo server in mServerList)
                {
                    mConfig.CachedServers.Add(server);
                }
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"Server list successfully downloaded. { mServerList.Count } servers found."));
                NetworkServerListUpdated?.Invoke(this, EventArgs.Empty);
                mConfig.SaveConfig();
            }
        }

        private void ServerListBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            List<NetworkServerInfo> list = NetworkInfo.GetServerList(VATSIM_STATUS_FILE_URL);
            if (list.Count > 0)
            {
                mServerList = list;
            }
        }

        public void SendRadioMessage(List<int> frequencies, string message)
        {
            FSD.SendPDU(new PDURadioMessage(OurCallsign, frequencies.ToArray(), message));
        }

        public void RequestClientCapabilities(string callsign)
        {
            FSD.SendPDU(new PDUClientQuery(OurCallsign, callsign, ClientQueryType.Capabilities));
        }

        public void RequestControllerInfo(string callsign)
        {
            if (IsConnected)
            {
                FSD.SendPDU(new PDUClientQuery(OurCallsign, callsign, ClientQueryType.ATIS));
            }
        }

        public void RequestInfoQuery(string to)
        {
            if (!string.IsNullOrEmpty(to) && FSD.Connected)
            {
                FSD.SendPDU(new PDUClientQuery(OurCallsign, to, ClientQueryType.INF));
            }
        }

        public void RequestPlaneInformation(string callsign)
        {
            if (!string.IsNullOrEmpty(callsign) && FSD.Connected)
            {
                FSD.SendPDU(new PDUPlaneInfoRequest(OurCallsign, callsign));
            }
        }

        public void RequestRealName(string callsign)
        {
            if (IsConnected)
            {
                FSD.SendPDU(new PDUClientQuery(OurCallsign, callsign, ClientQueryType.RealName));
            }
        }

        public List<NetworkServerInfo> ReturnServerList()
        {
            return mServerList;
        }

        public void SendClientCaps(string from)
        {
            if (FSD.Connected)
            {
                FSD.SendPDU(new PDUClientQueryResponse(OurCallsign, from, ClientQueryType.Capabilities, new List<string>
                {
                    "VERSION=1",
                    "ATCINFO=1",
                    "MODELDESC=1",
                    "ACCONFIG=1"
                }));
            }
        }

        public void RequestFullAircraftConfiguration(string to)
        {
            if (FSD.Connected)
            {
                AircraftConfigurationInfo aircraftConfigurationInfo = new AircraftConfigurationInfo
                {
                    Request = new AircraftConfigurationInfo.RequestType?(AircraftConfigurationInfo.RequestType.Full)
                };
                FSD.SendPDU(new PDUClientQuery(OurCallsign, to, ClientQueryType.AircraftConfiguration, new List<string>
                {
                    aircraftConfigurationInfo.ToJson()
                }));
            }
        }

        public void SendAircraftConfiguration(string to, AircraftConfiguration config)
        {
            if (FSD.Connected)
            {
                AircraftConfigurationInfo aircraftConfigurationInfo = new AircraftConfigurationInfo
                {
                    Config = config
                };
                FSD.SendPDU(new PDUClientQuery(OurCallsign, to, ClientQueryType.AircraftConfiguration, new List<string>
                {
                    aircraftConfigurationInfo.ToJson()
                }));
            }
        }

        public void SendIncrementalAircraftConfigurationUpdate(AircraftConfiguration config)
        {
            if (FSD.Connected)
            {
                AircraftConfigurationInfo aircraftConfigurationInfo = new AircraftConfigurationInfo
                {
                    Config = config
                };
                FSD.SendPDU(new PDUClientQuery(OurCallsign, "@94836", ClientQueryType.AircraftConfiguration, new List<string>
                {
                    aircraftConfigurationInfo.ToJson()
                }));
            }
        }

        public void SquawkIdent()
        {
            if (FSD.Connected)
            {
                mIsIdentPressed = true;
                SquawkingIdentChanged?.Invoke(this, new SquawkingIdentChangedEventArgs(true));
            }
        }

        private void SendCom1Frequency(string from, string frequency)
        {
            if (FSD.Connected)
            {
                FSD.SendPDU(new PDUClientQueryResponse(OurCallsign, from, ClientQueryType.COM1Freq, new List<string>
                {
                    frequency
                }));
            }
        }

        public bool IsConnected
        {
            get
            {
                return FSD.Connected;
            }
        }

        public bool IsObserver
        {
            get
            {
                return mConnectInfo.ObserverMode;
            }
        }

        public string OurCallsign
        {
            get
            {
                return mConnectInfo.Callsign;
            }
            set => _ = mConnectInfo.Callsign;
        }

        public string SelcalCode
        {
            get
            {
                return mConnectInfo.SelCalCode;
            }
        }

        [EventSubscription(EventTopics.UserAircraftDataUpdated, typeof(OnUserInterfaceAsync))]
        public void OnUserAircraftDataUpdated(object sender, UserAircraftDataUpdatedEventArgs e)
        {
            if (mUserAircraftData == null || !mUserAircraftData.Equals(e.UserAircraftData))
            {
                mUserAircraftData = e.UserAircraftData;
            }
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChangedEventArgs e)
        {
            if (mRadioStackData == null || !mRadioStackData.Equals(e.RadioStackState))
            {
                mRadioStackData = e.RadioStackState;
            }
        }

        [EventSubscription(EventTopics.MainFormShown, typeof(OnUserInterfaceAsync))]
        public void OnMainFormShown(object sender, EventArgs e)
        {
            FSD.SetSyncContext(SynchronizationContext.Current);
        }

        [EventSubscription(EventTopics.RealNameRequested, typeof(OnUserInterfaceAsync))]
        public void OnRealNameRequested(object sender, RealNameRequestedEventArgs e)
        {
            RequestRealName(e.Callsign);
        }

        [EventSubscription(EventTopics.MetarRequested, typeof(OnUserInterfaceAsync))]
        public void MetarRequested(object sender, MetarRequestedEventArgs e)
        {
            if (FSD.Connected)
            {
                FSD.SendPDU(new PDUMetarRequest(OurCallsign, e.Station));
            }
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSentEventArgs e)
        {
            FSD.SendPDU(new PDUTextMessage(e.From, e.To, e.Message));
        }

        [EventSubscription(EventTopics.RadioMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnRadioMessageSent(object sender, RadioMessageSentEventArgs e)
        {
            FSD.SendPDU(new PDURadioMessage(e.Callsign, e.Frequencies, e.Message));
        }

        [EventSubscription(EventTopics.FileFlightPlan, typeof(OnUserInterfaceAsync))]
        public void OnFileFlightPlan(object sender, FlightPlanReceivedEventArgs e)
        {
            if (IsConnected)
            {
                FileFlightPlan(e.FlightPlan);
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Flight plan submitted."));
            }
            else
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "Unable to submit flight plan. Not connected to network."));
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
            }
        }

        [EventSubscription(EventTopics.RequestFlightPlan, typeof(OnUserInterfaceAsync))]
        public void OnRequestFlightPlan(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                RequestFlightPlan();
            }
        }

        [EventSubscription(EventTopics.WallopRequestSent, typeof(OnUserInterfaceAsync))]
        public void OnWallopRequestSent(object sender, WallopReceivedEventArgs e)
        {
            if (IsConnected)
            {
                FSD.SendPDU(new PDUWallop(OurCallsign, e.Message));
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Warning, string.Format("[WALLOP] {0}: {1}", OurCallsign, e.Message)));
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Broadcast));
            }
        }

        [EventSubscription(EventTopics.ClientConfigChanged, typeof(OnUserInterfaceAsync))]
        public void ClientConfigChanged(object sender, EventArgs e)
        {
            FSD.ClientProperties.PluginPath = mConfig.FullXplanePath;
        }
    }
}
