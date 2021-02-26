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
using XPilot.PilotClient.Network;

namespace XPilot.PilotClient.Tutorial
{
    public partial class NetworkCredentials : SetupScreen,ISetupScreen
    {
        private IFsdManager mNetworkManager;

        public NetworkCredentials(ISetup host, IAppConfig config, IFsdManager network) : base(host, config)
        {
            InitializeComponent();
            Host.SetTitle("Network Credentials");
            mNetworkManager = network;
            LoadNetworkServers();
            LoadSettings();
        }

        private void LoadSettings()
        {
            txtNetworkLogin.Text = mConfig.VatsimId;
            txtNetworkPassword.Text = mConfig.VatsimPasswordDecrypted;
            txtName.Text = mConfig.Name;
            txtHomeAirport.Text = mConfig.HomeAirport;
            ddlServerName.SelectedIndex = ddlServerName.FindStringExact(mConfig.ServerName);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Host.EndSetup();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNetworkLogin.Text))
            {
                MessageBox.Show("VATSIM ID is required");
                txtNetworkLogin.Focus();
            }
            else if (string.IsNullOrEmpty(txtNetworkPassword.Text))
            {
                MessageBox.Show("VATSIM Password is required");
                txtNetworkPassword.Focus();
            }
            else if (string.IsNullOrEmpty(txtName.Text))
            {
                MessageBox.Show("Your Name is required");
                txtName.Focus();
            }
            else if (ddlServerName.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a VATSIM server.");
                ddlServerName.Select();
            }
            else
            {
                mConfig.VatsimId = txtNetworkLogin.Text.Trim();
                mConfig.VatsimPasswordDecrypted = txtNetworkPassword.Text.Trim();
                mConfig.Name = txtName.Text.Trim();
                mConfig.HomeAirport = txtHomeAirport.Text.ToUpper().Trim();
                mConfig.ServerName = (ddlServerName.SelectedItem as NetworkServerItem).Text;
                mConfig.SaveConfig();
                Host.SwitchScreen("AudioConfiguration");
            }
        }

        private void LoadNetworkServers()
        {
            if (mConfig.CachedServers.Count > 0)
            {
                foreach (var x in mConfig.CachedServers)
                {
                    NetworkServerItem item = new NetworkServerItem();
                    item.Text = x.Name;
                    item.Value = x.Address;
                    ddlServerName.Items.Add(item);
                }
            }
        }
    }

    public class NetworkServerItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}
