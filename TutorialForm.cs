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

namespace XPilot.PilotClient
{
    public partial class TutorialForm : Form, IHost
    {
        private IEventBroker mEventBroker;
        private IAppConfig mConfig;
        public View CurrentView { get; private set; }

        public TutorialForm(IEventBroker eventBroker, IAppConfig config)
        {
            InitializeComponent();
            mEventBroker = eventBroker;
            mEventBroker.Register(this);
            mConfig = config;
            SwitchView("Welcome");
        }

        public void SwitchView(string name)
        {
            Controls.Remove(CurrentView);
            CurrentView = CreateViewFromName(name);
            Controls.Add(CurrentView);
            CurrentView.Show();
        }

        private View CreateViewFromName(string name)
        {
            switch (name)
            {
                default:
                case "Welcome":
                    return new WelcomeView(this, mConfig);
                case "XplanePath":
                    return new SetXplanePath(this, mConfig);
                case "ConflictingPlugins":
                    return new ConflictingPlugins(this);
                case "CslConfiguration":
                    return new CslConfiguration(this, mConfig);
                case "NetworkCredentials":
                    return new NetworkCredentials(this);
            }
        }

        public void CloseTutorial()
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to cancel the setup?", "Confirm", MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
            {
                Environment.Exit(0);
            }
        }

        public void SetTitle(string title)
        {
            Text = "xPilot Guided Setup: " + title;
        }

        public void ManualSetup()
        {
            Close();
        }
    }
}
