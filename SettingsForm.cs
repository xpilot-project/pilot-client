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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using XPilot.PilotClient.AudioForVatsim;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Core;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using GeoVR.Client;
using SharpDX.DirectInput;

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
        private ToggleDisplayConfiguration mToggleDisplayConfig;
        private bool mRecordDisplayKey;
        private readonly DirectInput mDirectInput;
        private readonly List<Joystick> mPttDevices = new List<Joystick>();
        private readonly Timer mPttDevicePollTimer;
        private readonly List<PTTConfiguration> mIgnoreList = new List<PTTConfiguration>();
        private PTTConfiguration mNewPttConfiguration;
        private bool mScanning;

        [DllImport("user32.dll")]
        private static extern ushort GetAsyncKeyState(int value);

        public SettingsForm(IAppConfig appConfig, IAfvManager audioForVatsim, IEventBroker eventBroker, IFsdManger networkManager, IUserInterface userInterface)
        {
            InitializeComponent();

            mConfig = appConfig;
            mAfv = audioForVatsim;
            mNetworkManager = networkManager;
            mUserInterface = userInterface;
            mEventBroker = eventBroker;
            mEventBroker.Register(this);

            mDirectInput = new DirectInput();
            mPttDevicePollTimer = new Timer
            {
                Interval = 50
            };
            mPttDevicePollTimer.Tick += PttDevicePollTimer_Tick;

            cbUpdateChannel.BindEnumToCombobox(UpdateChannel.Stable);
            vhfEqualizer.BindEnumToCombobox(EqualizerPresets.VHFEmulation2);

            if (mConfig.InputVolumeDb > 18) mConfig.InputVolumeDb = 18;
            if (mConfig.InputVolumeDb < -18) mConfig.InputVolumeDb = -18;

            if (mConfig.Com1Volume > 1) mConfig.Com1Volume = 1;
            if (mConfig.Com1Volume < 0) mConfig.Com1Volume = 0;

            if (mConfig.Com2Volume > 1) mConfig.Com2Volume = 1;
            if (mConfig.Com2Volume < 0) mConfig.Com2Volume = 0;

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
            levelMeterInput.Value = e.Value;
        }

        [EventSubscription(EventTopics.Com1Volume, typeof(OnUserInterfaceAsync))]
        public void OnCom1VolumeChanged(object sender, RadioVolumeChangedEventArgs e)
        {
            if (mConfig.VolumeKnobsControlVolume)
            {
                mConfig.Com1Volume = e.Volume;
                TrackCom1Volume.Value = (int)AudioUtils.ScaleVolumeDb(e.Volume, 0, 100, 0, 1);
                lblVolumeCom1.Text = mConfig.Com1Volume.ToString("0%");
                mAfv.UpdateVolumes();
                mConfig.SaveConfig();
            }
        }

        [EventSubscription(EventTopics.Com2Volume, typeof(OnUserInterfaceAsync))]
        public void OnCom2VolumeChanged(object sender, RadioVolumeChangedEventArgs e)
        {
            if (mConfig.VolumeKnobsControlVolume)
            {
                mConfig.Com2Volume = e.Volume;
                TrackCom2Volume.Value = (int)mConfig.Com2Volume;
                lblVolumeCom2.Text = mConfig.Com2Volume.ToString("0%");
                mAfv.UpdateVolumes();
                mConfig.SaveConfig();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            SettingsFormShown(this, EventArgs.Empty);
            txtNetworkLogin.Text = mConfig.VatsimId;
            txtNetworkPassword.Text = mConfig.VatsimPassword;
            txtFullName.Text = mConfig.Name;
            txtHomeAirport.Text = mConfig.HomeAirport;
            ddlServerName.SelectedIndex = ddlServerName.FindStringExact(mConfig.ServerName);
            lblPTTValue.Text = mConfig.PTTConfiguration.ToString();
            mNewPttConfiguration = mConfig.PTTConfiguration;
            mToggleDisplayConfig = mConfig.ToggleDisplayConfiguration;
            spinPluginPort.Value = mConfig.TcpPort;
            audioInputDevice.SelectedItem = mConfig.InputDeviceName;
            audioOutputDevice.SelectedItem = mConfig.OutputDeviceName;
            TrackCom1Volume.Value = (int)AudioUtils.ScaleVolumeDb(mConfig.Com1Volume, 0, 100, 0, 1);
            TrackCom2Volume.Value = (int)AudioUtils.ScaleVolumeDb(mConfig.Com2Volume, 0, 100, 0, 1);
            lblVolumeCom1.Text = mConfig.Com1Volume.ToString("0%");
            lblVolumeCom2.Text = mConfig.Com2Volume.ToString("0%");
            TrackInputVolumeDb.Value = (int)mConfig.InputVolumeDb;
            inputVolumeLabel.Text = mConfig.InputVolumeDb.ToString("+#;-#;0");
            chkDisableRadioEffects.Checked = mConfig.DisableAudioEffects;
            chkFlashPrivateMessage.Checked = mConfig.FlashTaskbarPrivateMessage;
            chkFlashRadioMessage.Checked = mConfig.FlashTaskbarRadioMessage;
            chkFlashSelcal.Checked = mConfig.FlashTaskbarSelCal;
            chkFlashDisconnect.Checked = mConfig.FlashTaskbarDisconnect;
            chkAutoSquawkModeC.Checked = mConfig.AutoSquawkModeC;
            chkKeepVisible.Checked = mConfig.KeepClientWindowVisible;
            chkUpdates.Checked = mConfig.CheckForUpdates;
            chkSelcalSound.Checked = mConfig.PlayGenericSelCalAlert;
            chkRadioMessageSound.Checked = mConfig.PlayRadioMessageAlert;
            cbUpdateChannel.SelectedValue = mConfig.UpdateChannel;
            vhfEqualizer.SelectedValue = mConfig.VhfEqualizer;
            chkVolumeKnobVolume.Checked = mConfig.VolumeKnobsControlVolume;
            lblDisplayShortcut.Text = mConfig.ToggleDisplayConfiguration.ToString();
            TogglePTTButtons();
            ToggleDisplayKeyButtons();
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

        private void btnSetNewPTT_Click(object sender, EventArgs e)
        {
            if (mScanning)
            {
                StopScanning();
            }
            else
            {
                if (mNewPttConfiguration != null)
                {
                    mIgnoreList.Add(mNewPttConfiguration);
                }
                StartScanning();
            }
        }

        private void StartScanning()
        {
            mScanning = true;
            UpdateControls();
            AcquireDevices();
            IdentifyPressedButtons();
            mPttDevicePollTimer.Start();
        }

        private void StopScanning()
        {
            mPttDevicePollTimer.Stop();
            mScanning = false;
            UpdateControls();
            DisposeDevices();
        }

        private void DisposeDevices()
        {
            if (mPttDevices != null)
            {
                foreach (Joystick device in mPttDevices)
                {
                    device.Dispose();
                }
                mPttDevices.Clear();
            }
        }
        private void UpdateControls()
        {
            if (mScanning)
            {
                btnSetNewPTT.Text = "Cancel";
                lblPTTValue.Text = "Waiting for button or key press...";
                btnClearPTT.Enabled = false;
            }
            else
            {
                btnClearPTT.Enabled = true;
                if (mNewPttConfiguration != null)
                {
                    btnSetNewPTT.Text = "Set a Different PTT Key or Button";
                    btnClearPTT.Enabled = mNewPttConfiguration.DeviceType != PTTDeviceType.None;
                    lblPTTValue.Text = mNewPttConfiguration.ToString();
                }
                else
                {
                    btnSetNewPTT.Text = "Set New PTT Key or Button";
                    btnClearPTT.Enabled = mConfig.PTTConfiguration.DeviceType != PTTDeviceType.None;
                    lblPTTValue.Text = mConfig.PTTConfiguration.ToString();
                }
            }
            Application.DoEvents();
        }

        private void TogglePTTButtons()
        {
            if (mPttDevicePollTimer.Enabled)
            {
                btnClearPTT.Enabled = false;
            }
            else if (mNewPttConfiguration != null)
            {
                btnClearPTT.Enabled = (mNewPttConfiguration.DeviceType > PTTDeviceType.None);
            }
            else
            {
                btnClearPTT.Enabled = (mConfig.PTTConfiguration.DeviceType > PTTDeviceType.None);
            }
        }

        private void PttDevicePollTimer_Tick(object sender, EventArgs e)
        {
            ScanDevices();
        }

        private void AcquireDevices()
        {
            mPttDevices.Clear();
            foreach (var deviceInstance in mDirectInput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices))
            {
                if (mDirectInput.IsDeviceAttached(deviceInstance.InstanceGuid))
                {
                    Joystick device = new Joystick(mDirectInput, deviceInstance.InstanceGuid);
                    try
                    {
                        device.Acquire();
                        mPttDevices.Add(device);
                    }
                    catch { }
                }
            }
        }

        private void IdentifyPressedButtons()
        {
            foreach (Joystick device in mPttDevices)
            {
                try
                {
                    JoystickState state = device.GetCurrentState();
                    bool[] buttons = state.Buttons;
                    for (int i = 0; i < buttons.Length; i++)
                    {
                        if (buttons[i])
                        {
                            mIgnoreList.Add(new PTTConfiguration()
                            {
                                DeviceType = PTTDeviceType.Joystick,
                                JoystickGuidString = device.Information.InstanceGuid.ToString(),
                                ButtonOrKey = i,
                                Name = device.Information.InstanceName
                            });
                        }
                    }
                }
                catch (SharpDX.SharpDXException) { }
            }
        }

        private void ScanDevices()
        {
            foreach (Joystick device in mPttDevices)
            {
                ScanDevice(device, out bool buttonPressDetected);
                if (buttonPressDetected)
                {
                    break;
                }
            }
        }

        private void ScanDevice(Joystick device, out bool buttonPressDetected)
        {
            buttonPressDetected = false;
            try
            {
                JoystickState state = device.GetCurrentState();
                bool[] buttons = state.Buttons;
                for (int i = 0; i < buttons.Length; i++)
                {
                    if (buttons[i])
                    {
                        PTTConfiguration config = new PTTConfiguration()
                        {
                            DeviceType = PTTDeviceType.Joystick,
                            JoystickGuidString = device.Information.InstanceGuid.ToString(),
                            ButtonOrKey = i,
                            Name = device.Information.InstanceName
                        };
                        if (mIgnoreList.Contains(config))
                        {
                            continue;
                        }
                        mNewPttConfiguration = config;
                        buttonPressDetected = true;
                        StopScanning();
                        break;
                    }
                }
            }
            catch (SharpDX.SharpDXException) { }
        }

        private void SettingsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (mRecordDisplayKey)
            {
                mToggleDisplayConfig = new ToggleDisplayConfiguration();
                mToggleDisplayConfig.ShortcutType = ShortcutType.Keyboard;
                mToggleDisplayConfig.KeyCode = (int)GetVirtualKey(e.KeyCode);
                btnSetDisplayKey.Text = "Set New Display Key";
                lblDisplayShortcut.Text = mToggleDisplayConfig.ToString();
                e.Handled = true;
                e.SuppressKeyPress = true;
                mRecordDisplayKey = false;
                ToggleDisplayKeyButtons();
            }
            else
            {
                if (!mScanning)
                    return;

                var config = new PTTConfiguration
                {
                    DeviceType = PTTDeviceType.Keyboard,
                    ButtonOrKey = (int)GetVirtualKey(e.KeyCode),
                    Name = "Keyboard"
                };

                if (mIgnoreList.Contains(config))
                    return;

                mNewPttConfiguration = config;
                StopScanning();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private Keys GetVirtualKey(Keys keyCode)
        {
            Keys key = Keys.Add;
            switch (keyCode)
            {
                case Keys.ControlKey:
                    if (0 != (GetAsyncKeyState((int)Keys.RControlKey) & 0x8000))
                    {
                        key = Keys.RControlKey;
                    }
                    else if (0 != (GetAsyncKeyState((int)Keys.LControlKey) & 0x8000))
                    {
                        key = Keys.LControlKey;
                    }
                    break;
                case Keys.Menu:
                    if (0 != (GetAsyncKeyState((int)Keys.RMenu) & 0x8000))
                    {
                        key = Keys.RMenu;
                    }
                    else if (0 != (GetAsyncKeyState((int)Keys.LMenu) & 0x8000))
                    {
                        key = Keys.LMenu;
                    }
                    break;
                case Keys.ShiftKey:
                    if (0 != (GetAsyncKeyState((int)Keys.RShiftKey) & 0x8000))
                    {
                        key = Keys.RShiftKey;
                    }
                    else if (0 != (GetAsyncKeyState((int)Keys.LShiftKey) & 0x8000))
                    {
                        key = Keys.LShiftKey;
                    }
                    break;
                default:
                    key = keyCode;
                    break;
            }
            return key;
        }

        private void BtnClearPTT_Click(object sender, EventArgs e)
        {
            mNewPttConfiguration = new PTTConfiguration();
            UpdateControls();
        }

        private void ShowError(string message)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        private void ddlInputDeviceName_SelectedIndexChanged(object sender, EventArgs e)
        {
            var device = audioInputDevice.SelectedItem.ToString();
            if (mConfig.InputDeviceName != device)
            {
                RestartAfvUserClient(this, EventArgs.Empty);
            }
            mConfig.InputDeviceName = device;
            mConfig.SaveConfig();
        }

        private void ddlOutputDeviceName_SelectedIndexChanged(object sender, EventArgs e)
        {
            var device = audioOutputDevice.SelectedItem.ToString();
            if (mConfig.OutputDeviceName != device)
            {
                RestartAfvUserClient(this, EventArgs.Empty);
            }
            mConfig.OutputDeviceName = device;
            mConfig.SaveConfig();
        }

        private void TrackInputVolumeDb_Scroll(object sender, EventArgs e)
        {
            mConfig.InputVolumeDb = TrackInputVolumeDb.Value;
            inputVolumeLabel.Text = mConfig.InputVolumeDb.ToString("+#;-#;0");
            mConfig.SaveConfig();
            mAfv.UpdateVolumes();
        }

        private void Com1Volume_Scroll(object sender, EventArgs e)
        {
            mConfig.Com1Volume = AudioUtils.ScaleVolumeDb(TrackCom1Volume.Value, 0, 1, 0, 100);
            lblVolumeCom1.Text = mConfig.Com1Volume.ToString("0%");
            mConfig.SaveConfig();
            mAfv.UpdateVolumes();
            if (mConfig.VolumeKnobsControlVolume)
            {
                RadioVolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(1, mConfig.Com1Volume));
            }
        }

        private void Com2Volume_Scroll(object sender, EventArgs e)
        {
            mConfig.Com2Volume = AudioUtils.ScaleVolumeDb(TrackCom2Volume.Value, 0, 1, 0, 100);
            lblVolumeCom2.Text = mConfig.Com2Volume.ToString("0%");
            mConfig.SaveConfig();
            mAfv.UpdateVolumes();
            if (mConfig.VolumeKnobsControlVolume)
            {
                RadioVolumeChanged?.Invoke(this, new RadioVolumeChangedEventArgs(2, mConfig.Com2Volume));
            }
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
            else if (ddlServerName.SelectedIndex == -1)
            {
                ShowError("Please select a VATSIM server.");
                ddlServerName.Select();
            }
            else
            {
                mConfig.DisableAudioEffects = chkDisableRadioEffects.Checked;
                mConfig.VatsimId = txtNetworkLogin.Text.Trim();
                mConfig.VatsimPassword = txtNetworkPassword.Text.Trim();
                mConfig.Name = txtFullName.Text;
                mConfig.HomeAirport = txtHomeAirport.Text;
                mConfig.ServerName = (ddlServerName.SelectedItem as NetworkServerItem).Text;
                mConfig.FlashTaskbarPrivateMessage = chkFlashPrivateMessage.Checked;
                mConfig.FlashTaskbarRadioMessage = chkFlashRadioMessage.Checked;
                mConfig.FlashTaskbarSelCal = chkFlashSelcal.Checked;
                mConfig.FlashTaskbarDisconnect = chkFlashDisconnect.Checked;
                mConfig.AutoSquawkModeC = chkAutoSquawkModeC.Checked;
                mConfig.KeepClientWindowVisible = chkKeepVisible.Checked;
                mConfig.CheckForUpdates = chkUpdates.Checked;
                mConfig.PlayGenericSelCalAlert = chkSelcalSound.Checked;
                mConfig.PlayRadioMessageAlert = chkRadioMessageSound.Checked;
                mConfig.UpdateChannel = (UpdateChannel)cbUpdateChannel.SelectedValue;
                mConfig.VhfEqualizer = (EqualizerPresets)vhfEqualizer.SelectedValue;
                mConfig.VolumeKnobsControlVolume = chkVolumeKnobVolume.Checked;
                mConfig.TcpPort = (int)spinPluginPort.Value;
                if (!mNewPttConfiguration.Equals(mConfig.PTTConfiguration))
                {
                    mConfig.PTTConfiguration = mNewPttConfiguration;
                }
                if (!mToggleDisplayConfig.Equals(mConfig.ToggleDisplayConfiguration))
                {
                    mConfig.ToggleDisplayConfiguration = mToggleDisplayConfig;
                }
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

        private void BtnSetToggleKey_Click(object sender, EventArgs e)
        {
            if (mRecordDisplayKey)
            {
                btnSetDisplayKey.Text = "Set New Display Key";
                ToggleDisplayKeyButtons();
                mRecordDisplayKey = false;
            }
            else
            {
                btnSetDisplayKey.Text = "Cancel";
                ToggleDisplayKeyButtons();
                mRecordDisplayKey = true;
            }
        }

        private void ToggleDisplayKeyButtons()
        {
            if (mRecordDisplayKey)
            {
                btnClearDisplayKey.Enabled = false;
            }
            else if (mToggleDisplayConfig != null)
            {
                btnClearDisplayKey.Enabled = (mToggleDisplayConfig.ShortcutType == ShortcutType.Keyboard);
            }
            else
            {
                btnClearDisplayKey.Enabled = (mConfig.ToggleDisplayConfiguration.ShortcutType == ShortcutType.Keyboard);
            }
        }

        private void BtnClearToggle_Click(object sender, EventArgs e)
        {
            mRecordDisplayKey = false;
            mToggleDisplayConfig = new ToggleDisplayConfiguration();
            lblDisplayShortcut.Text = mToggleDisplayConfig.ToString();
            ToggleDisplayKeyButtons();
        }

        private void vhfEqualizer_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (vhfEqualizer.Items.Count > 0)
            {
                if (mConfig.VhfEqualizer != (EqualizerPresets)vhfEqualizer.SelectedValue)
                {
                    RestartAfvUserClient(this, EventArgs.Empty);
                    mConfig.VhfEqualizer = (EqualizerPresets)vhfEqualizer.SelectedValue;
                    mConfig.SaveConfig();
                }
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

        private void spinPluginPort_ValueChanged(object sender, EventArgs e)
        {
            mConfig.TcpPort = (int)spinPluginPort.Value;
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
