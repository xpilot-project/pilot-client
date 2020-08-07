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
using System.Windows.Forms;
using System.Runtime.InteropServices;
using SharpDX.DirectInput;
using XPilot.PilotClient.Config;

namespace XPilot.PilotClient.Tutorial
{
    public partial class PushToTalk : SetupScreen, ISetupScreen
    {
        private readonly DirectInput mDirectInput;
        private readonly List<Joystick> mPttDevices = new List<Joystick>();
        private readonly Timer mPttDevicePollTimer;
        private readonly List<PTTConfiguration> mIgnoreList = new List<PTTConfiguration>();
        private PTTConfiguration mNewPttConfiguration;
        private bool mScanning;

        [DllImport("user32.dll")]
        private static extern ushort GetAsyncKeyState(int value);

        public PushToTalk(ISetup host, IAppConfig config) : base(host, config)
        {
            InitializeComponent();
            Host.SetTitle("Push to Talk Configuration");

            mDirectInput = new DirectInput();
            mPttDevicePollTimer = new Timer
            {
                Interval = 50
            };
            mPttDevicePollTimer.Tick += PttDevicePollTimer_Tick;

            lblPTTValue.Text = mConfig.PTTConfiguration.ToString();
            mNewPttConfiguration = mConfig.PTTConfiguration;
            TogglePTTButtons();
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
                btnNext.Enabled = false;
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
                    btnNext.Enabled = mNewPttConfiguration.DeviceType != PTTDeviceType.None;
                    lblPTTValue.Text = mNewPttConfiguration.ToString();
                }
                else
                {
                    btnSetNewPTT.Text = "Set New PTT Key or Button";
                    btnClearPTT.Enabled = mConfig.PTTConfiguration.DeviceType != PTTDeviceType.None;
                    btnNext.Enabled = mConfig.PTTConfiguration.DeviceType != PTTDeviceType.None;
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
                btnNext.Enabled = (mNewPttConfiguration.DeviceType > PTTDeviceType.None);
                btnClearPTT.Enabled = (mNewPttConfiguration.DeviceType > PTTDeviceType.None);
            }
            else
            {
                btnNext.Enabled = (mNewPttConfiguration.DeviceType > PTTDeviceType.None);
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

        private void btnClearPTT_Click(object sender, EventArgs e)
        {
            mNewPttConfiguration = new PTTConfiguration();
            UpdateControls();
        }

        public override void KeyDownHandler(KeyEventArgs key)
        {
            base.KeyDownHandler(key);

            if (!mScanning)
                return;

            var config = new PTTConfiguration
            {
                DeviceType = PTTDeviceType.Keyboard,
                ButtonOrKey = (int)GetVirtualKey(key.KeyCode),
                Name = "Keyboard"
            };

            if (mIgnoreList.Contains(config))
                return;

            mNewPttConfiguration = config;
            StopScanning();
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

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (!mNewPttConfiguration.Equals(mConfig.PTTConfiguration))
            {
                mConfig.PTTConfiguration = mNewPttConfiguration;
            }
            mConfig.SaveConfig();
            Host.SetupFinished();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Host.EndSetup();
        }
    }
}
