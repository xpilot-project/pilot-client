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
using System.Linq;
using System.Windows.Forms;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Simulator;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Events.Arguments;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Xpilot.Networking;

namespace Vatsim.Xpilot.Aircrafts
{
    public class AircraftManager : EventBus, IAircraftManager
    {
        private const int STALE_AIRCRAFT_TIMEOUT = 10000;
        private const int SIMULATOR_AIRCRAFT_SYNC_INTERVAL = 5000;
        private const long FAST_POSITION_INTERVAL_TOLERANCE = 300;
        private const double ERROR_CORRECTION_INTERVAL = 2.0;

        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        [EventPublication(EventTopics.AircraftDiscovered)]
        public event EventHandler<AircraftEventArgs> AircraftDiscovered;

        [EventPublication(EventTopics.AircraftUpdated)]
        public event EventHandler<AircraftEventArgs> AircraftUpdated;

        [EventPublication(EventTopics.AircraftDeleted)]
        public event EventHandler<AircraftEventArgs> AircraftDeleted;

        private Timer mStaleAircraftCheckTimer;
        private Timer mSimulatorAircraftSyncTimer;
        private List<string> mIgnoredAircraft = new List<string>();
        private readonly IAppConfig mConfig;
        private readonly ITimeStampProvider mTimeStampProvider;
        private readonly INetworkManager mNetworkManager;
        private readonly IXplaneAdapter mXplaneAdapter;

        public List<Aircraft> Aircraft { get; } = new List<Aircraft>();

        public AircraftManager(IEventBroker broker, IAppConfig config, INetworkManager fsdManager, IXplaneAdapter xplane, ITimeStampProvider timeStampProvider) : base(broker)
        {
            mConfig = config;
            mNetworkManager = fsdManager;
            mXplaneAdapter = xplane;
            mTimeStampProvider = timeStampProvider;

            InitializeTimers();
        }

        [EventSubscription(EventTopics.SessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnSessionStarted(object sender, EventArgs e)
        {
            mStaleAircraftCheckTimer.Start();
        }

        [EventSubscription(EventTopics.SessionEnded, typeof(OnUserInterface))]
        public void OnSessionEnded(object sender, EventArgs e)
        {
            mStaleAircraftCheckTimer.Stop();
        }

        [EventSubscription(EventTopics.CapabilitiesRequestReceived, typeof(OnUserInterfaceAsync))]
        public void OnCapabilitiesRequestReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            var aircraft = GetAircraft(e.From);
            if (aircraft != null)
            {
                mNetworkManager.SendAircraftInfoRequest(e.From);
            }
        }

        [EventSubscription(EventTopics.CapabilitiesResponseReceived, typeof(OnUserInterfaceAsync))]
        public void OnCapabilitiesResponseReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (e.Data.Contains("ACCONFIG=1"))
            {
                mNetworkManager.SendAircraftConfigurationRequest(e.From);
            }
        }

        [EventSubscription(EventTopics.DeletePilotReceived, typeof(OnUserInterfaceAsync))]
        public void OnDeletePilotReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            HandleAircraftDeletedByServer(e.From);
        }

        [EventSubscription(EventTopics.AircraftUpdateReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftUpdateReceived(object sender, AircraftUpdateReceivedEventArgs e)
        {
            HandleAircraftUpdate(e.From, e.Position, e.Position.GroundSpeed);
        }

        [EventSubscription(EventTopics.FastPositionUpdateReceived, typeof(OnUserInterfaceAsync))]
        public void OnFastPositionUpdateReceived(object sender, FastPositionUpdateReceivedEventArgs e)
        {

        }

        [EventSubscription(EventTopics.AircraftInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftInfoReceived(object sender, AircraftInfoReceivedEventArgs e)
        {
            var aircraft = GetAircraft(e.From);
            if (aircraft != null)
            {

            }
        }

        [EventSubscription(EventTopics.AircraftConfigurationInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftConfigurationInfoReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            AircraftConfigurationInfo info;
            try
            {
                info = AircraftConfigurationInfo.FromJson(e.Data);
            }
            catch
            {
                return;
            }
            Aircraft aircraft = GetAircraft(e.From);
            if (info.HasConfig && aircraft != null)
            {
                HandleAircraftConfiguration(aircraft, info.Config);
            }
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            mStaleAircraftCheckTimer.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            DeleteAllAircraft();
            mStaleAircraftCheckTimer.Stop();
        }

        private void InitializeTimers()
        {
            mStaleAircraftCheckTimer = new Timer
            {
                Interval = STALE_AIRCRAFT_TIMEOUT
            };
            mStaleAircraftCheckTimer.Tick += StaleAircraftCheckTimer_Tick;

            mSimulatorAircraftSyncTimer = new Timer
            {
                Interval = SIMULATOR_AIRCRAFT_SYNC_INTERVAL
            };
            mSimulatorAircraftSyncTimer.Tick += SimulatorAircraftSyncTimer_Tick;
        }


        private void HandleAircraftConfiguration(Aircraft aircraft, AircraftConfiguration cfg)
        {
            var isFullData = cfg.IsFullData.HasValue && cfg.IsFullData.Value;

            // We can just ignore incremental config updates if we haven't received a full config yet.
            if (!isFullData && (aircraft.Configuration == null))
            {
                return;
            }

            if (isFullData)
            {
                cfg.EnsurePopulated();
                aircraft.Configuration = cfg;
            }
            else
            {
                aircraft.Configuration.ApplyIncremental(cfg);
            }
        }

        private void SimulatorAircraftSyncTimer_Tick(object sender, EventArgs e)
        {
            SyncSimulatorAircraft();
        }

        private void SyncSimulatorAircraft()
        {
            throw new NotImplementedException();
        }

        private void StaleAircraftCheckTimer_Tick(object sender, EventArgs e)
        {
            var currentTimeStamp = mTimeStampProvider.Precision;

            List<Aircraft> deleteThese = new List<Aircraft>();
            foreach (Aircraft aircraft in Aircraft)
            {
                if (!aircraft.LastSlowPositionTimeStamp.HasValue)
                {
                    continue;
                }

                if ((currentTimeStamp - aircraft.LastSlowPositionTimeStamp.Value) > (sender as Timer).Interval)
                {
                    deleteThese.Add(aircraft);
                }
            }

            foreach (Aircraft aircraft in deleteThese)
            {
                DeleteAircraft(aircraft);
            }
        }

        private void HandleAircraftDeletedByServer(string callsign)
        {
            Aircraft aircraft = GetAircraft(callsign);
            if (aircraft != null)
            {
                DeleteAircraft(aircraft);
                SyncSimulatorAircraft();
            }
        }

        private void DeleteAircraft(string callsign)
        {
            Aircraft aircraft = GetAircraft(callsign);
            if(aircraft != null)
            {
                DeleteAircraft(aircraft);
            }
        }

        private void DeleteAircraft(Aircraft aircraft)
        {
            Aircraft.Remove(aircraft);
            AircraftDeleted(this, new AircraftEventArgs(aircraft));
        }

        private void DeleteAllAircraft()
        {
            Aircraft[] allAircraft = Aircraft.ToArray();
            foreach (Aircraft aircraft in allAircraft)
            {
                DeleteAircraft(aircraft);
            }
        }

        private void HandleAircraftUpdate(string callsign, NetworkAircraftState position, double speed)
        {
            var aircraft = GetAircraft(callsign);

            if (aircraft == null)
            {
                SetUpNewAircraft(callsign, position, speed);
            }
            else
            {
                UpdateAircraft(aircraft, position, speed);
            }
        }

        private void SetUpNewAircraft(string callsign, NetworkAircraftState position, double speed)
        {
            var aircraft = new Aircraft(callsign)
            {
                Status = mIgnoredAircraft.Contains(callsign) ? AircraftStatus.Ignored : AircraftStatus.New,
                LastSlowPositionTimeStamp = mTimeStampProvider.Precision,
                Speed = speed
            };
            Aircraft.Add(aircraft);
            mNetworkManager.SendCapabilitiesRequest(aircraft.Callsign);
            mNetworkManager.SendCapabilities(aircraft.Callsign);
            mNetworkManager.SendAircraftInfoRequest(aircraft.Callsign);
            AircraftDiscovered(this, new AircraftEventArgs(aircraft));
        }

        private void UpdateAircraft(Aircraft aircraft, NetworkAircraftState position, double speed)
        {

        }

        private void CreateNewAircraft(string from, NetworkAircraftState position)
        {
            NetworkAircraft aircraft = new NetworkAircraft
            {
                Callsign = from,
                LastUpdated = DateTime.Now,
                UpdateCount = 1,
                GroundSpeed = position.GroundSpeed,
                Status = mIgnoredAircraft.Contains(from) ? AircraftStatus.Ignored : AircraftStatus.New
            };
            aircraft.StateHistory.Add(position);
            mNetworkManager.SendCapabilitiesRequest(aircraft.Callsign);
            mNetworkManager.SendCapabilities(aircraft.Callsign);
            mNetworkManager.SendAircraftInfoRequest(aircraft.Callsign);
        }

        private Aircraft GetAircraft(string callsign)
        {
            return Aircraft.FirstOrDefault(t => t.Callsign == callsign);
        }

        public void IgnoreAircraft(string callsign)
        {
            if (mIgnoredAircraft.Contains(callsign))
            {
                NotificationPosted(this, new NotificationPostedEventArgs(NotificationType.Error, "{0} is already in the ignore list.", callsign));
                return;
            }

            mIgnoredAircraft.Add(callsign);
            NotificationPosted(this, new NotificationPostedEventArgs(NotificationType.Info, "{0} has been added to the ignore list.", callsign));

            Aircraft aircraft = GetAircraft(callsign);
            if (aircraft != null)
            {

            }
        }

        public void UnignoreAircraft(string callsign)
        {
            if (!mIgnoredAircraft.Contains(callsign))
            {
                NotificationPosted(this, new NotificationPostedEventArgs(NotificationType.Error, "{0} was not found in the ignore list.", callsign));
                return;
            }

            mIgnoredAircraft.Remove(callsign);

            NotificationPosted(this, new NotificationPostedEventArgs(NotificationType.Info, "{0} has been removed from the ignore list.", callsign));
        }
    }
}