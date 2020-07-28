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
using System.Runtime.InteropServices;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using SharpDX;
using SharpDX.DirectInput;

namespace XPilot.PilotClient.Core
{
    public class PttManager : EventBus, IPttManager
    {
        [EventPublication(EventTopics.PushToTalkStateChanged)]
        public event EventHandler<PushToTalkStateChangedEventArgs> PushToTalkStateChanged;

        private readonly IAppConfig mConfig;
        private readonly DirectInput mDirectInput;
        private readonly System.Windows.Forms.Timer mPttTimer;
        private PTTConfiguration mPttConfiguration = new PTTConfiguration();
        private Joystick mJoystick;
        private bool mPttStatus;

        [DllImport("user32.dll")]
        private static extern ushort GetAsyncKeyState(int state);

        public PttManager(IEventBroker broker, IAppConfig config) : base(broker)
        {
            mConfig = config;
            mDirectInput = new DirectInput();
            mPttTimer = new System.Windows.Forms.Timer
            {
                Interval = 10
            };
            mPttTimer.Tick += PttTimer_Tick;
        }

        private void PttTimer_Tick(object sender, EventArgs e)
        {
            if (mPttConfiguration.DeviceType == PTTDeviceType.Joystick && !mPttConfiguration.JoystickAcquired)
            {
                AcquireJoystick();
            }
            CheckPttDevice();
        }

        private void CheckPttDevice()
        {
            bool isPressed = false;
            switch (mPttConfiguration.DeviceType)
            {
                case PTTDeviceType.None:
                    isPressed = false;
                    break;
                case PTTDeviceType.Keyboard:
                    isPressed = ((GetAsyncKeyState(mPttConfiguration.ButtonOrKey) & 32768) > 0);
                    break;
                case PTTDeviceType.Joystick:
                    if (mJoystick != null && mPttConfiguration.JoystickAcquired && mPttConfiguration.ButtonOrKey >= 0)
                    {
                        try
                        {
                            JoystickState currentState = mJoystick.GetCurrentState();
                            bool[] buttons = currentState.Buttons;
                            try
                            {
                                isPressed = buttons[mPttConfiguration.ButtonOrKey];
                            }
                            catch (IndexOutOfRangeException)
                            {
                            }
                        }
                        catch (SharpDXException)
                        {
                            mPttConfiguration.JoystickAcquired = false;
                            if (mJoystick != null)
                            {
                                mJoystick.Unacquire();
                                mJoystick.Dispose();
                            }
                            mJoystick = null;
                        }
                    }
                    break;
            }
            if (isPressed != mPttStatus)
            {
                mPttStatus = isPressed;
                PushToTalkStateChanged?.Invoke(this, new PushToTalkStateChangedEventArgs(mPttStatus));
            }
        }

        private void RemovePttDevice(PTTConfiguration ptt)
        {
            if (mJoystick != null)
            {
                mJoystick.Unacquire();
                mJoystick.Dispose();
            }
            mJoystick = null;
            mPttConfiguration = ptt;
            mPttConfiguration.JoystickAcquired = false;
        }

        private void AcquireJoystick()
        {
            if (mDirectInput.IsDeviceAttached(mPttConfiguration.JoystickGuid))
            {
                try
                {
                    mJoystick = new Joystick(mDirectInput, mPttConfiguration.JoystickGuid);
                    mJoystick.Acquire();
                    mPttConfiguration.JoystickAcquired = true;
                    return;
                }
                catch (SharpDXException)
                {
                    if (mJoystick != null)
                    {
                        mJoystick.Unacquire();
                        mJoystick.Dispose();
                    }
                    mJoystick = null;
                    return;
                }
            }
            mPttConfiguration.JoystickAcquired = false;
            if (mJoystick != null)
            {
                mJoystick.Unacquire();
                mJoystick.Dispose();
            }
            mJoystick = null;
        }

        [EventSubscription(EventTopics.SessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnSessionStarted(object sender, EventArgs e)
        {
            RemovePttDevice(mConfig.PTTConfiguration);
            mPttTimer.Start();
        }

        [EventSubscription(EventTopics.SessionEnded, typeof(OnUserInterfaceAsync))]
        public void OnSessionEnded(object sender, EventArgs e)
        {
            mPttTimer.Stop();
        }

        [EventSubscription(EventTopics.ClientConfigChanged, typeof(OnUserInterfaceAsync))]
        public void OnClientConfigChanged(object sender, EventArgs e)
        {
            RemovePttDevice(mConfig.PTTConfiguration);
        }
    }
}
