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
using XPilot.PilotClient.AudioForVatsim;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.Core;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using GeoVR.Client;

namespace XPilot.PilotClient
{
    public partial class SettingsForm : Form
    {
        [EventPublication(EventTopics.ClientConfigChanged)]
        public event EventHandler<EventArgs> ClientConfigChanged;

        [EventPublication(EventTopics.RestartAfvUserClient)]
        public event EventHandler<EventArgs> RestartAfvUserClient;

        [EventPublication(EventTopics.SettingsFormShown)]
        public event EventHandler<EventArgs> SettingsFormShown;

        [EventPublication(EventTopics.RadioVolumeChanged)]
        public event EventHandler<RadioVolumeChangedEventArgs> RadioVolumeChanged;

        private readonly IAfvManager mAfv;
        private readonly IAppConfig mConfig;
        private readonly IEventBroker mEventBroker;
        private readonly IFsdManger mNetworkManager;
        private readonly IUserInterface mUserInterface;

        public SettingsForm(IAppConfig appConfig, IAfvManager audioForVatsim, IEventBroker eventBroker, IFsdManger networkManager, IUserInterface userInterface)
        {
            InitializeComponent();

            mConfig = appConfig;
            mAfv = audioForVatsim;
            mNetworkManager = networkManager;
            mUserInterface = userInterface;
            mEventBroker = eventBroker;
            mEventBroker.Register(this);

            //if (mConfig.Com1Volume > 1) mConfig.Com1Volume = 1;
            //if (mConfig.Com1Volume < 0) mConfig.Com1Volume = 0;
            //if (mConfig.Com2Volume > 1) mConfig.Com2Volume = 1;
            //if (mConfig.Com2Volume < 0) mConfig.Com2Volume = 0;

            GetAudioDevices();
            LoadNetworkServers();
        }

        [EventSubscription(EventTopics.NetworkServerListUpdated, typeof(OnUserInterfaceAsync))]
        public void NetworkServerListUpdated(object sender, EventArgs e)
        {
            LoadNetworkServers();
        }

        [EventSubscription(EventTopics.MicrophoneInputLevelChanged, typeof(OnUserInterfaceAsync))]
        public void MicrophoneInputLevelChanged(object sender, ClientEventArgs<float> e)
        {
            vuMeter.Value = e.Value;
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SettingsFormShown(this, EventArgs.Empty);
            txtNetworkLogin.Text = mConfig.VatsimId;
            txtNetworkPassword.Text = mConfig.VatsimPasswordDecrypted;
            txtFullName.Text = mConfig.Name;
            txtHomeAirport.Text = mConfig.HomeAirport;
            lstServerName.SelectedIndex = lstServerName.FindStringExact(mConfig.ServerName);
            lstAudioDriver.SelectedItem = mConfig.AudioDriver;
            lstInputDevice.SelectedItem = mConfig.InputDeviceName;
            lstListenDevice.SelectedItem = mConfig.ListenDeviceName;
            trackCom1.Value = mConfig.Com1Volume;
            trackCom2.Value = mConfig.Com2Volume;
            volCom1.Text = mConfig.Com1Volume.ToString("0%");
            volCom2.Text = mConfig.Com2Volume.ToString("0%");
            chkRadioEffects.Checked = mConfig.DisableAudioEffects;
            chkHfSquelch.Checked = mConfig.EnableHfSquelch;
            chkFlashPrivateMessage.Checked = mConfig.FlashTaskbarPrivateMessage;
            chkFlashRadioMessage.Checked = mConfig.FlashTaskbarRadioMessage;
            chkFlashSelcal.Checked = mConfig.FlashTaskbarSelCal;
            chkFlashDisconnect.Checked = mConfig.FlashTaskbarDisconnect;
            chkAutoSquawkModeC.Checked = mConfig.AutoSquawkModeC;
            chkKeepVisible.Checked = mConfig.KeepClientWindowVisible;
            chkEnableNotificationSounds.Checked = mConfig.EnableNotificationSounds;
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
                    lstServerName.Items.Add(item);
                }
            }
            else
            {
                mNetworkManager.DownloadNetworkServers();
            }
        }

        private void GetAudioDevices()
        {
            foreach (var inputDevice in ClientAudioUtilities.GetInputDevices())
            {
                audioInputDevice.Items.Add(inputDevice);
            }

            foreach (var outputDevice in ClientAudioUtilities.GetOutputDevices())
            {
                audioOutputDevice.Items.Add(outputDevice);
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtNetworkLogin.Text))
            {
                ShowError("VATSIM ID is required.");
                txtNetworkLogin.Select();
            }
            else if (string.IsNullOrEmpty(txtNetworkPassword.Text))
            {
                ShowError("VATSIM password is required.");
                txtNetworkPassword.Select();
            }
            else if (string.IsNullOrEmpty(txtFullName.Text))
            {
                ShowError("Name is required.");
                txtFullName.Select();
            }
            else if (lstServerName.SelectedIndex == -1)
            {
                ShowError("Please select a VATSIM server.");
                lstServerName.Select();
            }
            else
            {
                mConfig.DisableAudioEffects = chkDisableRadioEffects.Checked;
                mConfig.VatsimId = txtNetworkLogin.Text.Trim();
                mConfig.VatsimPasswordDecrypted = txtNetworkPassword.Text.Trim();
                mConfig.Name = txtFullName.Text;
                mConfig.HomeAirport = txtHomeAirport.Text;
                mConfig.ServerName = (lstServerName.SelectedItem as NetworkServerItem).Text;
                //mConfig.FlashTaskbarPrivateMessage = chkFlashPrivateMessage.Checked;
                //mConfig.FlashTaskbarRadioMessage = chkFlashRadioMessage.Checked;
                //mConfig.FlashTaskbarSelCal = chkFlashSelcal.Checked;
                //mConfig.FlashTaskbarDisconnect = chkFlashDisconnect.Checked;
                mConfig.AutoSquawkModeC = chkAutoSquawkModeC.Checked;
                //mConfig.KeepClientWindowVisible = chkKeepVisible.Checked;
                mConfig.PlayGenericSelCalAlert = chkSelcalSound.Checked;
                mConfig.PlayRadioMessageAlert = chkEnableNotificationSounds.Checked;
                mConfig.VolumeKnobsControlVolume = chkVolumeKnobVolume.Checked;
                mConfig.SimulatorPort = txtSimulatorPort.Text;
                mConfig.SaveConfig();
                ClientConfigChanged?.Invoke(this, EventArgs.Empty);
                Close();
            }
        }

        private void txtHomeAirport_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                (sender as TextBox).SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right && e.KeyCode != Keys.Up && e.KeyCode != Keys.Down && e.KeyCode != Keys.Home && e.KeyCode != Keys.End && (e.KeyCode < Keys.A || e.KeyCode > Keys.Z) && (e.Modifiers != Keys.None || e.KeyValue < 48 || e.KeyValue > 57) && (e.Modifiers != Keys.None || e.KeyValue < 96 || e.KeyValue > 105))
            {
                e.SuppressKeyPress = true;
            }
        }

        private void chkDisableRadioEffects_CheckedChanged(object sender, EventArgs e)
        {
            mAfv.SetAudioEffectsDisabled(chkDisableRadioEffects.Checked);
            mConfig.DisableAudioEffects = chkDisableRadioEffects.Checked;
            mConfig.SaveConfig();
        }

        private void btnGuidedSetup_Click(object sender, EventArgs e)
        {
            using (var dlg = mUserInterface.CreateSetupGuideForm())
            {
                dlg.ShowDialog(this);
            }
        }

        private void CheckboxCheckedChanged(object sender, EventArgs e)
        {
            var checkbox = sender as CheckBox;
            mConfig[checkbox.Tag.ToString()] = checkbox.Checked;
            mConfig.SaveConfig();
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
