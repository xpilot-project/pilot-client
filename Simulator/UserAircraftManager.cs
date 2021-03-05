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
using System.Windows.Forms;
using Vatsim.Xpilot.Aircrafts;
using Vatsim.Xpilot.Networking;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Events.Arguments;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;

namespace Vatsim.Xpilot.Simulator
{
    public class UserAircraftManager : EventBus, IUserAircraftManager
    {
        private const int ACCONFIG_TOKEN_REFRESH_INTERVAL = 5000;
        private const int ACCONFIG_MAX_TOKENS = 10;

        private Timer mAcconfigTokenRefreshTimer;
        private int mTokensRemaining = ACCONFIG_MAX_TOKENS;
        private AircraftConfiguration mLastSentConfig;
        private UserAircraftConfigData mUserAircraftConfigData;
        private readonly INetworkManager mFsdManager;

        public UserAircraftManager(IEventBroker broker, INetworkManager fsdManager) : base(broker)
        {
            mFsdManager = fsdManager;
            mAcconfigTokenRefreshTimer = new Timer
            {
                Interval = ACCONFIG_TOKEN_REFRESH_INTERVAL
            };
            mAcconfigTokenRefreshTimer.Tick += AcconfigTokenRefreshTimer_Tick;
        }

        [EventSubscription(EventTopics.SessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnSessionStarted(object sender, EventArgs e)
        {
            mAcconfigTokenRefreshTimer.Start();
        }

        [EventSubscription(EventTopics.SessionEnded, typeof(OnUserInterfaceAsync))]
        public void OnSessionEnded(object sender, EventArgs e)
        {
            mAcconfigTokenRefreshTimer.Stop();
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
            if (info.Request.HasValue && info.Request.Value == AircraftConfigurationInfo.RequestType.Full)
            {
                AircraftConfiguration cfg = AircraftConfiguration.FromUserAircraftData(mUserAircraftConfigData);
                cfg.IsFullData = true;
                mFsdManager.SendAircraftConfigurationUpdate(e.From, cfg);
            }
        }

        [EventSubscription(EventTopics.UserAircraftConfigDataUpdated, typeof(OnUserInterfaceAsync))]
        public void OnUserAircraftConfigDataUpdated(object sender, UserAircraftConfigDataUpdatedEventArgs e)
        {
            mUserAircraftConfigData = e.UserAircraftConfigData;
            if (mUserAircraftConfigData != null && mFsdManager.IsConnected)
            {
                if (mTokensRemaining > 0)
                {
                    AircraftConfiguration aircraftConfiguration = AircraftConfiguration.FromUserAircraftData(mUserAircraftConfigData);
                    if (!aircraftConfiguration.Equals(mLastSentConfig))
                    {
                        AircraftConfiguration incrementalConfig = mLastSentConfig.CreateIncremental(aircraftConfiguration);
                        mFsdManager.SendAircraftConfigurationUpdate(incrementalConfig);
                        mLastSentConfig = aircraftConfiguration;
                        mTokensRemaining--;
                    }
                }
            }
            else
            {
                mLastSentConfig = AircraftConfiguration.FromUserAircraftData(mUserAircraftConfigData);
            }

            // auto squawk mode c
        }

        private void AcconfigTokenRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (mTokensRemaining < ACCONFIG_MAX_TOKENS)
            {
                mTokensRemaining++;
            }
        }
    }
}
