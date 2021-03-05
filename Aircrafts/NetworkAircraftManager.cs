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
using Vatsim.Xpilot.Aircrafts;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Simulator;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Events.Arguments;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;

namespace Vatsim.Xpilot.Networking.Aircraft
{
    public class NetworkAircraftManager : EventBus, INetworkAircraftManager
    {
        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        [EventPublication(EventTopics.AircraftDiscovered)]
        public event EventHandler<AircraftUpdatedEventArgs> AircraftDiscovered;

        [EventPublication(EventTopics.AircraftUpdated)]
        public event EventHandler<AircraftUpdatedEventArgs> AircraftUpdated;

        [EventPublication(EventTopics.AircraftDeleted)]
        public event EventHandler<AircraftUpdatedEventArgs> AircraftDeleted;

        private readonly IAppConfig mConfig;
        private readonly INetworkManager mFsd;
        private readonly IXplaneAdapter mXplane;
        private readonly Timer mCheckIdleAircraftTimer;
        private readonly List<NetworkAircraft> mNetworkAircraft = new List<NetworkAircraft>();
        private List<string> mIgnoreList = new List<string>();

        public NetworkAircraftManager(IEventBroker broker, IAppConfig config, INetworkManager fsdManager, IXplaneAdapter xplane) : base(broker)
        {
            mConfig = config;
            mFsd = fsdManager;
            mXplane = xplane;

            mCheckIdleAircraftTimer = new Timer
            {
                Interval = 1000
            };
            mCheckIdleAircraftTimer.Tick += CheckIdleAircraftTimer_Tick;
        }

        [EventSubscription(EventTopics.SessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnSessionStarted(object sender, EventArgs e)
        {
            mCheckIdleAircraftTimer.Start();
        }

        [EventSubscription(EventTopics.SessionEnded, typeof(OnUserInterface))]
        public void OnSessionEnded(object sender, EventArgs e)
        {
            mCheckIdleAircraftTimer.Stop();
        }

        [EventSubscription(EventTopics.CapabilitiesRequestReceived, typeof(OnUserInterfaceAsync))]
        public void OnCapabilitiesRequestReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            var aircraft = mNetworkAircraft.FirstOrDefault(a => a.Callsign == e.From);
            if (aircraft != null)
            {
                mFsd.RequestAircraftInfo(e.From);
            }
        }

        [EventSubscription(EventTopics.CapabilitiesResponseReceived, typeof(OnUserInterfaceAsync))]
        public void OnCapabilitiesResponseReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (e.Data.Contains("ACCONFIG=1"))
            {
                mFsd.RequestAircraftConfiguration(e.From);
            }
        }

        [EventSubscription(EventTopics.DeletePilotReceived, typeof(OnUserInterfaceAsync))]
        public void OnDeletePilotReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            DeletePilot(e.From);
        }

        [EventSubscription(EventTopics.AircraftUpdateReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftUpdateReceived(object sender, AircraftUpdateReceivedEventArgs e)
        {
            AircraftUpdateReceived(e.From, e.Position);
        }

        [EventSubscription(EventTopics.AircraftInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftInfoReceived(object sender, AircraftInfoReceivedEventArgs e)
        {
            var aircraft = mNetworkAircraft.FirstOrDefault(a => a.Callsign == e.From);
            if (aircraft != null)
            {
                if (aircraft.Equipment != e.Equipment)
                {

                }
            }
            else
            {

            }
        }

        [EventSubscription(EventTopics.AircraftConfigurationInfoReceived, typeof(OnUserInterfaceAsync))]
        public void OnAircraftConfigurationInfoReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            AircraftConfigurationInfo aircraftConfigurationInfo;
            try
            {
                aircraftConfigurationInfo = AircraftConfigurationInfo.FromJson(e.Data);
            }
            catch(Exception ex)
            {
                return;
            }
            var aircraft = mNetworkAircraft.FirstOrDefault(a => a.Callsign == e.From);
            if(aircraftConfigurationInfo.HasConfig && aircraft != null)
            {

            }
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            mCheckIdleAircraftTimer.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            mCheckIdleAircraftTimer.Stop();
        }

        [EventSubscription(EventTopics.UserAircraftDataUpdated, typeof(OnUserInterfaceAsync))]
        public void OnUserAircraftDataUpdated(object sender, UserAircraftDataUpdatedEventArgs e)
        {

        }

        private void CheckIdleAircraftTimer_Tick(object sender, EventArgs e)
        {
            List<NetworkAircraft> temp = new List<NetworkAircraft>();
            foreach (NetworkAircraft aircraft in mNetworkAircraft)
            {
                if ((DateTime.Now - aircraft.LastUpdated).TotalMilliseconds > 5000)
                {
                    temp.Add(aircraft);
                }
            }
            foreach (NetworkAircraft aircraft in temp)
            {

            }
        }

        private void DeletePilot(string from)
        {
            throw new NotImplementedException();
        }

        private void AircraftUpdateReceived(string from, NetworkAircraftState position)
        {
            var aircraft = mNetworkAircraft.FirstOrDefault(a => a.Callsign == from);
            if (aircraft == null)
            {
                CreateNewAircraft(from, position);
            }
            else
            {
                UpdateExistingAircraft(aircraft, position);
            }
        }

        private void CreateNewAircraft(string from, NetworkAircraftState position)
        {
            NetworkAircraft aircraft = new NetworkAircraft
            {
                Callsign = from,
                LastUpdated = DateTime.Now,
                UpdateCount = 1,
                GroundSpeed = position.GroundSpeed,
                Status = mIgnoreList.Contains(from) ? AircraftStatus.Ignored : AircraftStatus.New
            };
            aircraft.StateHistory.Add(position);
            mFsd.RequestClientCapabilities(aircraft.Callsign);
            mFsd.SendClientCapabilities(aircraft.Callsign);
            mFsd.RequestAircraftInfo(aircraft.Callsign);
        }

        private void UpdateExistingAircraft(NetworkAircraft aircraft, NetworkAircraftState position)
        {
            if (aircraft != null && aircraft.UpdateCount >= 2 && string.IsNullOrEmpty(aircraft.Equipment))
            {
                mFsd.RequestAircraftInfo(aircraft.Callsign);
            }
        }
    }
}