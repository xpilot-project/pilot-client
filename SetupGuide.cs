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
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Tutorial;
using Appccelerate.EventBroker;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.AudioForVatsim;

namespace XPilot.PilotClient
{
    public partial class SetupGuide : Form, ISetup
    {
        private IEventBroker mEventBroker;
        private IFsdManger mNetworkManager;
        private IAfvManager mAfv;
        private IAppConfig mConfig;
        public SetupScreen CurrentScreen { get; private set; }
        public bool XSquawkBox { get; set; }
        public bool XSwiftBus { get; set; }
        public string XplanePath { get; set; }

        public SetupGuide(IEventBroker eventBroker, IAppConfig config, IFsdManger network, IAfvManager afv)
        {
            InitializeComponent();
            mConfig = config;
            mNetworkManager = network;
            mAfv = afv;
            mEventBroker = eventBroker;
            mEventBroker.Register(this);
            SwitchScreen("Welcome");
        }

        public void SwitchScreen(string name)
        {
            Controls.Remove(CurrentScreen);
            CurrentScreen = CreateViewFromName(name);
            Controls.Add(CurrentScreen);
            CurrentScreen.Show();
        }

        private SetupScreen CreateViewFromName(string name)
        {
            switch (name)
            {
                default:
                case "Welcome":
                    return new WelcomeView(this, mConfig);
                case "XplanePath":
                    return new SetXplanePath(this, mConfig);
                case "ConflictingPlugins":
                    return new ConflictingPlugins(this, mConfig);
                case "CslConfiguration":
                    return new CslConfiguration(this, mConfig);
                case "NetworkCredentials":
                    return new NetworkCredentials(this, mConfig, mNetworkManager);
                case "AudioConfiguration":
                    return new AudioConfiguration(this, mConfig, mAfv, mEventBroker);
                case "PushToTalk":
                    return new PushToTalk(this, mConfig);
            }
        }

        public void SetTitle(string title)
        {
            Text = "xPilot Guided Setup: " + title;
        }

        public void ManualSetup()
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to cancel the guided setup?", "Confirm", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                DialogResult = DialogResult.No;
                Close();
            }
        }

        private void TutorialForm_KeyDown(object sender, KeyEventArgs e)
        {
            CurrentScreen.KeyDownHandler(e);
        }

        public void EndSetup()
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to cancel the guided setup?", "Confirm", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                Environment.Exit(0);
            }
        }

        public void SetupFinished()
        {
            DialogResult dr = MessageBox.Show("You have successfully configured xPilot. If you need to adjust any settings, click the \"Settings\" button in the xPilot client.", "Setup Complete", MessageBoxButtons.OK);
            if (dr == DialogResult.OK)
            {
                Close();
            }
        }
    }
}
