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
using System.Collections.Generic;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Controllers;

namespace Vatsim.Xpilot.Simulator
{
    public class UserAircraftManager : EventBus, IUserAircraftManager
    {
        private const int ACCONFIG_TOKEN_REFRESH_INTERVAL = 5000;
        private const int ACCONFIG_MAX_TOKENS = 10;

        private Timer mAcconfigTokenRefreshTimer;
        private int mAcconfigAvailableTokens = ACCONFIG_MAX_TOKENS;
        private AircraftConfiguration mLastBroadcastConfig;
        private bool mInitialAircraftDataReceived = false;
        private bool mAirborne = false;
        private RadioStackState mRadioStackState;
        private UserAircraftConfigData mUserAircraftConfigData;
        private readonly IAppConfig mConfig;
        private readonly IXplaneAdapter mXplaneAdapter;
        private readonly IControllerManager mControllerManager;
        private readonly INetworkManager mNetworkManager;

        public UserAircraftManager(
            IEventBroker broker,
            INetworkManager networkManager,
            IAppConfig appConfig,
            IXplaneAdapter xplaneAdapter,
            IControllerManager controllerManager) : base(broker)
        {
            mConfig = appConfig;
            mNetworkManager = networkManager;
            mXplaneAdapter = xplaneAdapter;
            mControllerManager = controllerManager;
            InitializeTimers();
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
                mNetworkManager.SendAircraftConfigurationUpdate(e.From, cfg);
            }
        }

        [EventSubscription(EventTopics.UserAircraftConfigDataUpdated, typeof(OnUserInterfaceAsync))]
        public void OnUserAircraftConfigDataUpdated(object sender, UserAircraftConfigDataUpdatedEventArgs e)
        {
            mUserAircraftConfigData = e.UserAircraftConfigData;
            if ((mLastBroadcastConfig == null) || !mNetworkManager.IsConnected)
            {
                mLastBroadcastConfig = AircraftConfiguration.FromUserAircraftData(mUserAircraftConfigData);
            }
            else if (mAcconfigAvailableTokens > 0)
            {
                AircraftConfiguration newCfg = AircraftConfiguration.FromUserAircraftData(mUserAircraftConfigData);
                if (!newCfg.Equals(mLastBroadcastConfig))
                {
                    AircraftConfiguration incremental = mLastBroadcastConfig.CreateIncremental(newCfg);
                    mNetworkManager.SendAircraftConfigurationUpdate(incremental);
                    mLastBroadcastConfig = newCfg;
                    mAcconfigAvailableTokens--;
                }
            }
            bool wasAirborne = mAirborne;
            mAirborne = !mUserAircraftConfigData.OnGround;
            if (mInitialAircraftDataReceived && !wasAirborne && mAirborne && !mConfig.SquawkingModeC && mConfig.AutoSquawkModeC)
            {
                mXplaneAdapter.SetModeC(true);
            }
            mInitialAircraftDataReceived = true;
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChangedEventArgs e)
        {
            mRadioStackState = e.RadioStackState;
        }

        private void InitializeTimers()
        {
            mAcconfigTokenRefreshTimer = new Timer
            {
                Interval = ACCONFIG_TOKEN_REFRESH_INTERVAL
            };
            mAcconfigTokenRefreshTimer.Tick += AcconfigTokenRefreshTimer_Tick;
        }

        private void AcconfigTokenRefreshTimer_Tick(object sender, EventArgs e)
        {
            if (mAcconfigAvailableTokens < ACCONFIG_MAX_TOKENS)
            {
                mAcconfigAvailableTokens++;
            }
        }
    }
}