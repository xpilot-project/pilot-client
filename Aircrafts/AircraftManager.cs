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
using Abacus.DoublePrecision;
using Vatsim.Xpilot.Common;

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
        private readonly INetworkManager mNetworkManager;
        private readonly IXplaneAdapter mXplaneAdapter;

        public List<Aircraft> Aircraft { get; } = new List<Aircraft>();

        public AircraftManager(IEventBroker broker, INetworkManager fsdManager, IXplaneAdapter xplane) : base(broker)
        {
            mNetworkManager = fsdManager;
            mXplaneAdapter = xplane;

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
            HandleAircraftUpdate(e.From, e.VisualState, e.Speed);
        }

        [EventSubscription(EventTopics.FastPositionUpdateReceived, typeof(OnUserInterfaceAsync))]
        public void OnFastPositionUpdateReceived(object sender, FastPositionUpdateEventArgs e)
        {
            HandleFastPositionUpdate(e.From, e.VisualState, e.PositionalVelocityVector, e.RotationalVelocityVector);
        }

        [EventSubscription(EventTopics.AircraftInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftInfoReceived(object sender, AircraftInfoReceivedEventArgs e)
        {
            var aircraft = GetAircraft(e.From);
            if (aircraft == null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(aircraft.TypeCode) && aircraft.TypeCode != e.TypeCode)
            {
                mXplaneAdapter.ChangeModel(aircraft);
                return;
            }
            aircraft.TypeCode = e.TypeCode;
            if (IsEligibleToAddToSimulator(aircraft))
            {
                SyncSimulatorAircraft();
            }
        }

        [EventSubscription(EventTopics.AircraftConfigurationInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftConfigurationInfoReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            AircraftConfigurationInfo info;
            Console.WriteLine(e.Data);
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
                Console.WriteLine(info.Config.IsFullData);
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

        [EventSubscription(EventTopics.AircraftAddedToSimulator, typeof(OnUserInterfaceAsync))]
        public void OnAircraftAddedToSimulator(object sender, GenericEventArgs<string> e)
        {
            var aircraft = GetAircraft(e.Value);
            if (aircraft != null)
            {
                aircraft.Status = AircraftStatus.Active;
            }
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

        private void StaleAircraftCheckTimer_Tick(object sender, EventArgs e)
        {
            var currentTimeStamp = DateTime.UtcNow;

            List<Aircraft> deleteThese = new List<Aircraft>();
            foreach (Aircraft aircraft in Aircraft)
            {
                if ((currentTimeStamp - aircraft.LastSlowPositionUpdate).TotalMilliseconds > (sender as Timer).Interval)
                {
                    deleteThese.Add(aircraft);
                }
            }

            foreach (Aircraft aircraft in deleteThese)
            {
                DeleteAircraft(aircraft);
            }
        }

        private void SimulatorAircraftSyncTimer_Tick(object sender, EventArgs e)
        {
            SyncSimulatorAircraft();
        }

        private Aircraft GetAircraft(string callsign)
        {
            return Aircraft.FirstOrDefault(t => t.Callsign == callsign);
        }

        private void HandleAircraftDeletedByServer(string callsign)
        {
            Aircraft aircraft = GetAircraft(callsign);
            if (aircraft == null)
            {
                return;
            }
            DeleteAircraft(aircraft);
            SyncSimulatorAircraft();
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

        private void HandleFastPositionUpdate(string callsign, AircraftVisualState visualState, Vector3 positionalVelocityVector, Vector3 rotationalVelocityVector)
        {
            var aircraft = GetAircraft(callsign);
            if (aircraft == null)
            {
                return;
            }
            mXplaneAdapter.SendFastPositionUpdate(aircraft, visualState, positionalVelocityVector, rotationalVelocityVector);
        }

        private void HandleAircraftUpdate(string callsign, AircraftVisualState visualState, double speed)
        {
            var aircraft = GetAircraft(callsign);

            if (aircraft == null)
            {
                SetUpNewAircraft(callsign);
            }
            else
            {
                UpdateAircraft(aircraft, visualState, speed);
            }
        }

        private void UpdateAircraft(Aircraft aircraft, AircraftVisualState visualState, double speed)
        {
            mXplaneAdapter.SendSlowPositionUpdate(aircraft, visualState, speed);
        }

        private void SetUpNewAircraft(string callsign)
        {
            var aircraft = new Aircraft(callsign)
            {
                Status = mIgnoredAircraft.Contains(callsign) ? AircraftStatus.Ignored : AircraftStatus.New
            };
            Aircraft.Add(aircraft);
            mNetworkManager.SendCapabilitiesRequest(aircraft.Callsign);
            mNetworkManager.SendCapabilities(aircraft.Callsign);
            mNetworkManager.SendAircraftInfoRequest(aircraft.Callsign);
            AircraftDiscovered(this, new AircraftEventArgs(aircraft));
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

            if ((aircraft.Status == AircraftStatus.New) && IsEligibleToAddToSimulator(aircraft))
            {
                SyncSimulatorAircraft();
            }
            else if (aircraft.Status == AircraftStatus.Active)
            {
                mXplaneAdapter.PlaneConfigChanged(aircraft.Callsign, aircraft.Configuration);
            }
        }

        private void SyncSimulatorAircraft()
        {
            foreach (Aircraft aircraft in Aircraft)
            {
                if (aircraft.Status == AircraftStatus.Ignored)
                {
                    continue;
                }

                if (aircraft.Status == AircraftStatus.New)
                {
                    if (!IsEligibleToAddToSimulator(aircraft))
                    {
                        break;
                    }
                    mXplaneAdapter.AddPlane(aircraft);
                    Console.WriteLine(aircraft.Callsign);
                }
            }
        }

        private bool IsEligibleToAddToSimulator(Aircraft aircraft)
        {
            if (aircraft.Configuration == null)
            {
                Console.WriteLine("False, acconfig: " + aircraft.Callsign);
                return false;
            }

            if (string.IsNullOrEmpty(aircraft.TypeCode))
            {
                Console.WriteLine("False, typecode: " + aircraft.Callsign);
                return false;
            }

            return true;
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
                bool sync = false;
                if ((aircraft.Status == AircraftStatus.Active) || (aircraft.Status == AircraftStatus.Pending))
                {
                    mXplaneAdapter.RemovePlane(aircraft);
                    sync = true;
                }
                aircraft.Status = AircraftStatus.Ignored;
                if (sync)
                {
                    SyncSimulatorAircraft();
                }
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

            Aircraft aircraft = GetAircraft(callsign);
            if (aircraft != null)
            {
                if (aircraft.Status == AircraftStatus.Ignored)
                {
                    aircraft.Status = AircraftStatus.New;
                    SyncSimulatorAircraft();
                }
            }

            NotificationPosted(this, new NotificationPostedEventArgs(NotificationType.Info, "{0} has been removed from the ignore list.", callsign));
        }
    }
}