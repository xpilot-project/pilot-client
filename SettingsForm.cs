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
using System.Collections.Generic;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Xpilot.AudioForVatsim;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Core;

namespace Vatsim.Xpilot
{
    public partial class SettingsForm : Form
    {
        [EventPublication(EventTopics.SettingsModified)]
        public event EventHandler<EventArgs> SettingsModified;

        private readonly IAFVManaged mAudio;
        private readonly IAppConfig mConfig;
        private readonly IEventBroker mEventBroker;
        private readonly IUserInterface mUserInterface;
        private Timer mVuTimer;

        public SettingsForm(IAppConfig appConfig, IAFVManaged audio, IEventBroker eventBroker, IUserInterface userInterface)
        {
            InitializeComponent();

            mConfig = appConfig;
            mAudio = audio;
            mUserInterface = userInterface;
            mEventBroker = eventBroker;
            mEventBroker.Register(this);

            mVuTimer = new Timer() { Interval = 50 };
            mVuTimer.Tick += VuTimer_Tick;

            LoadNetworkServers();

            lstAudioDriver.Enabled = !mConfig.IsVoiceDisabled;
            lstInputDevice.Enabled = !mConfig.IsVoiceDisabled;
            lstListenDevice.Enabled = !mConfig.IsVoiceDisabled;
            chkHfSquelch.Enabled = !mConfig.IsVoiceDisabled;
            chkDisableRadioEffects.Enabled = !mConfig.IsVoiceDisabled;
            trackCom1.Enabled = !mConfig.IsVoiceDisabled;
            trackCom2.Enabled = !mConfig.IsVoiceDisabled;

            if (!mConfig.IsVoiceDisabled)
            {
                foreach (KeyValuePair<int, string> driver in mConfig.AudioDrivers)
                {
                    ComboboxItem item = new ComboboxItem
                    {
                        Text = driver.Value,
                        Value = driver.Value
                    };
                    lstAudioDriver.Items.Add(item);
                }
            }
        }

        private void VuTimer_Tick(object sender, EventArgs e)
        {
            if (!mConfig.IsVoiceDisabled && mConfig.IsAudioConfigured)
            {
                float v = (float)ScaleVu(AFVBindings.GetInputPeak());
                vuMeter.Value = v;
            }
        }

        [EventSubscription(EventTopics.NetworkServerListUpdated, typeof(OnUserInterfaceAsync))]
        public void NetworkServerListUpdated(object sender, EventArgs e)
        {
            LoadNetworkServers();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            mVuTimer.Stop();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            txtNetworkLogin.Text = mConfig.VatsimId;
            txtNetworkPassword.Text = mConfig.VatsimPasswordDecrypted;
            txtFullName.Text = mConfig.Name;
            txtHomeAirport.Text = mConfig.HomeAirport;
            lstServerName.SelectedIndex = lstServerName.FindStringExact(mConfig.ServerName);
            lstAudioDriver.SelectedIndex = lstAudioDriver.FindStringExact(mConfig.AudioDriver);
            lstInputDevice.SelectedIndex = lstInputDevice.FindStringExact(mConfig.InputDeviceName);
            lstListenDevice.SelectedIndex = lstListenDevice.FindStringExact(mConfig.ListenDeviceName);
            trackCom1.Value = mConfig.Com1Volume;
            trackCom2.Value = mConfig.Com2Volume;
            volCom1.Text = (mConfig.Com1Volume / 100.0).ToString("0%");
            volCom2.Text = (mConfig.Com2Volume / 100.0).ToString("0%");
            chkDisableRadioEffects.Checked = mConfig.DisableAudioEffects;
            chkHfSquelch.Checked = mConfig.EnableHfSquelch;
            chkAutoSquawkModeC.Checked = mConfig.AutoSquawkModeC;
            chkEnableNotificationSounds.Checked = mConfig.EnableNotificationSounds;
            chkFlashDisconnect.Checked = mConfig.FlashTaskbarDisconnect;
            chkFlashPrivateMessage.Checked = mConfig.FlashTaskbarPrivateMessage;
            chkFlashRadioMessage.Checked = mConfig.FlashTaskbarRadioMessage;
            chkFlashSelcal.Checked = mConfig.FlashTaskbarSelcal;
            chkKeepVisible.Checked = mConfig.KeepWindowVisible;
            mVuTimer.Start();
        }

        private void LoadNetworkServers()
        {
            if (mConfig.CachedServers.Count > 0)
            {
                foreach (var x in mConfig.CachedServers)
                {
                    ComboboxItem item = new ComboboxItem
                    {
                        Text = x.Name,
                        Value = x.Address
                    };
                    lstServerName.Items.Add(item);
                }
            }
        }

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void BtnOK_Click(object sender, EventArgs e)
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
                mConfig.ServerName = (lstServerName.SelectedItem as ComboboxItem).Text;
                mConfig.AutoSquawkModeC = chkAutoSquawkModeC.Checked;
                mConfig.EnableNotificationSounds = chkEnableNotificationSounds.Checked;
                mConfig.FlashTaskbarRadioMessage = chkFlashRadioMessage.Checked;
                mConfig.FlashTaskbarPrivateMessage = chkFlashPrivateMessage.Checked;
                mConfig.FlashTaskbarDisconnect = chkFlashDisconnect.Checked;
                mConfig.FlashTaskbarSelcal = chkFlashSelcal.Checked;
                mConfig.KeepWindowVisible = chkKeepVisible.Checked;
                mConfig.SaveConfig();
                SettingsModified?.Invoke(this, EventArgs.Empty);
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

        private void btnGuidedSetup_Click(object sender, EventArgs e)
        {
            using (var dlg = mUserInterface.CreateSetupGuideForm())
            {
                dlg.ShowDialog(this);
            }
        }

        private void lstAudioDriver_SelectedValueChanged(object sender, EventArgs e)
        {
            var driver = (lstAudioDriver.SelectedItem as ComboboxItem).Value.ToString();
            if (!string.IsNullOrEmpty(driver))
            {
                if (driver != mConfig.AudioDriver)
                {
                    lstListenDevice.Items.Clear();
                    lstInputDevice.Items.Clear();

                    mConfig.AudioDriver = driver;
                    mConfig.SaveConfig();
                }

                mAudio.SetAudioDriver(driver);
                SetAudioDevices();
            }
        }

        private void SetAudioDevices()
        {         
            foreach (KeyValuePair<int, string> driver in mConfig.OutputDevices)
            {
                ComboboxItem item = new ComboboxItem
                {
                    Text = driver.Value,
                    Value = driver.Value
                };
                lstListenDevice.Items.Add(item);
            }

            foreach (KeyValuePair<int, string> driver in mConfig.InputDevices)
            {
                ComboboxItem item = new ComboboxItem
                {
                    Text = driver.Value,
                    Value = driver.Value
                };
                lstInputDevice.Items.Add(item);
            }

            mAudio.ConfigureAudioDevices();
        }

        private void lstInputDevice_SelectedValueChanged(object sender, EventArgs e)
        {
            var device = (lstInputDevice.SelectedItem as ComboboxItem).Value.ToString();
            if (!string.IsNullOrEmpty(device))
            {
                if (device != mConfig.InputDeviceName)
                {
                    mConfig.InputDeviceName = device;
                    mConfig.SaveConfig();
                    mAudio.ConfigureAudioDevices();
                }
            }
        }

        private void lstListenDevice_SelectedValueChanged(object sender, EventArgs e)
        {
            var device = (lstListenDevice.SelectedItem as ComboboxItem).Value.ToString();
            if (!string.IsNullOrEmpty(device))
            {
                if (device != mConfig.ListenDeviceName)
                {
                    mConfig.ListenDeviceName = device;
                    mConfig.SaveConfig();
                    mAudio.ConfigureAudioDevices();
                }
            }
        }

        private double ScaleVu(double v)
        {
            int limitMin = 0;
            int limitMax = 1;
            int baseMin = -40;
            int baseMax = 0;
            return ((limitMax - limitMin) * (v - baseMin)) / (baseMax - baseMin) + limitMin;
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void trackCom1_Scroll(object sender, EventArgs e)
        {
            mConfig.Com1Volume = trackCom1.Value;
            volCom1.Text = (mConfig.Com1Volume / 100.0).ToString("0%");
            mConfig.SaveConfig();
            mAudio.UpdateRadioGains();
        }

        private void trackCom2_Scroll(object sender, EventArgs e)
        {
            mConfig.Com2Volume = trackCom2.Value;
            volCom2.Text = (mConfig.Com2Volume / 100.0).ToString("0%");
            mConfig.SaveConfig();
            mAudio.UpdateRadioGains();
        }

        private void chkHfSquelch_CheckedChanged(object sender, EventArgs e)
        {
            mConfig.EnableHfSquelch = chkHfSquelch.Checked;
            mAudio.EnableHFSquelch(chkHfSquelch.Checked);
        }

        private void chkDisableRadioEffects_CheckedChanged_1(object sender, EventArgs e)
        {
            mConfig.DisableAudioEffects = chkDisableRadioEffects.Checked;
            mAudio.DisbleRadioEffects(chkDisableRadioEffects.Checked);
        }
    }

    public class ComboboxItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public override string ToString()
        {
            return Text;
        }
    }
}
