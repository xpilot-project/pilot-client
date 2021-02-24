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
using XPilot.PilotClient.Aircraft;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;

namespace XPilot.PilotClient.XplaneAdapter
{
    public class UserAircraftManager : EventBus, IUserAircraftManager
    {
        [EventPublication(EventTopics.SetXplaneDataRefValue)]
        public event EventHandler<DataRefEventArgs> SetXplaneDataRefValue;

        //[EventPublication(EventTopics.SendXplaneCommand)]
        //public event EventHandler<ClientEventArgs<XPlaneConnector.XPlaneCommand>> SendXplaneCommand;

        private const int ACCONFIG_TOKEN_REFRESH_INTERVAL = 5000;
        private const int ACCONFIG_MAX_TOKENS = 10;

        private Timer mAcconfigTokenRefreshTimer;
        private int mAcconfigAvailableTokens = ACCONFIG_MAX_TOKENS;
        private AircraftConfiguration mLastBroadcastConfig;
        private bool mInitialAircraftDataReceived = false;
        private bool mAirborne = false;
        private readonly IAppConfig mConfig;
        private readonly IFsdManger mFsdManager;

        public UserAircraftData UserAircraftData { get; private set; }

        public UserAircraftManager(IEventBroker broker, IAppConfig config, IFsdManger fsdManager) : base(broker)
        {
            mConfig = config;
            mFsdManager = fsdManager;
            InitializeTimers();
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

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnMainFormShown(object sender, EventArgs e)
        {
            mAcconfigTokenRefreshTimer.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnMainFormClosed(object sender, EventArgs e)
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
            if (info.Request.HasValue)
            {
                switch (info.Request.Value)
                {
                    case AircraftConfigurationInfo.RequestType.Full:
                        {
                            AircraftConfiguration cfg = AircraftConfiguration.FromUserAircraftData(UserAircraftData);
                            cfg.IsFullData = true;
                            mFsdManager.SendAircraftConfiguration(e.From, cfg);
                            break;
                        }
                }
            }
        }

        [EventSubscription(EventTopics.UserAircraftDataUpdated, typeof(OnUserInterfaceAsync))]
        public void OnUserAircraftDataUpdated(object sender, UserAircraftDataUpdatedEventArgs e)
        {
            if (UserAircraftData == null || !UserAircraftData.Equals(e.UserAircraftData))
            {
                UserAircraftData = e.UserAircraftData;
            }

            if ((mLastBroadcastConfig == null) || !mFsdManager.IsConnected)
            {
                mLastBroadcastConfig = AircraftConfiguration.FromUserAircraftData(UserAircraftData);
            }
            else if (mAcconfigAvailableTokens > 0)
            {
                AircraftConfiguration newCfg = AircraftConfiguration.FromUserAircraftData(UserAircraftData);
                if (!newCfg.Equals(mLastBroadcastConfig))
                {
                    AircraftConfiguration incremental = mLastBroadcastConfig.CreateIncremental(newCfg);
                    mFsdManager.SendIncrementalAircraftConfigurationUpdate(incremental);
                    mLastBroadcastConfig = newCfg;
                    mAcconfigAvailableTokens--;
                }
            }
            bool wasAirborne = mAirborne;
            mAirborne = !UserAircraftData.OnGround;
            //if (mInitialAircraftDataReceived && !wasAirborne && mAirborne && !mConfig.SquawkingModeC && mConfig.AutoSquawkModeC)
            //{
            //    var laminarB738 = new XPlaneConnector.DataRefElement
            //    {
            //        DataRef = "laminar/B738/knob/transpoder_pos"
            //    };

            //    var laminarB738_Dn_Cmd = new XPlaneConnector.XPlaneCommand("laminar/B738/knob/transponder_mode_up", "");
            //    var laminarB738_Up_Cmd = new XPlaneConnector.XPlaneCommand("laminar/B738/knob/transponder_mode_up", "");

            //    SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneConnector.XPlaneCommand>(laminarB738_Up_Cmd));
            //    SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(laminarB738, 3));
            //    SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneConnector.XPlaneCommand>(XPlaneConnector.Commands.TransponderTransponderAlt));
            //}
            mInitialAircraftDataReceived = true;
        }
    }
}
