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
using XPilot.PilotClient.AudioForVatsim;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Config;

namespace XPilot.PilotClient.Tutorial
{
    public partial class AudioConfiguration : SetupScreen, ISetupScreen
    {
        private IAFVManaged mAfv;
        private IEventBroker mEventBroker;

        public AudioConfiguration(ISetup host, IAppConfig config, IAFVManaged afv, IEventBroker eventBroker) : base(host, config)
        {
            InitializeComponent();
            mAfv = afv;
            mConfig = config;
            mEventBroker = eventBroker;
            mEventBroker.Register(this);
            Host.SetTitle("Audio Configuration");

            GetAudioDevices();
            audioInputDevice.SelectedItem = mConfig.InputDeviceName;
            audioOutputDevice.SelectedItem = mConfig.ListenDeviceName;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            mConfig.SaveConfig();
            Host.SwitchScreen("PushToTalk");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Host.EndSetup();
        }

        private void GetAudioDevices()
        {
            audioInputDevice.Items.Clear();
            audioOutputDevice.Items.Clear();

            //foreach (var inputDevice in ClientAudioUtilities.GetInputDevices())
            //{
            //    audioInputDevice.Items.Add(inputDevice);
            //}

            //foreach (var outputDevice in ClientAudioUtilities.GetOutputDevices())
            //{
            //    audioOutputDevice.Items.Add(outputDevice);
            //}
        }

        private void audioInputDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            var device = audioInputDevice.SelectedItem.ToString();
            if (mConfig.InputDeviceName != device)
            {
                //RestartAfvUserClient(this, EventArgs.Empty);
            }
            mConfig.InputDeviceName = device;
            mConfig.SaveConfig();
        }

        private void audioOutputDevice_SelectedIndexChanged(object sender, EventArgs e)
        {
            var device = audioOutputDevice.SelectedItem.ToString();
            if (mConfig.ListenDeviceName != device)
            {
                //RestartAfvUserClient(this, EventArgs.Empty);
            }
            mConfig.ListenDeviceName = device;
            mConfig.SaveConfig();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            GetAudioDevices();
        }
    }
}
