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
using System.Linq;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Core.Events;

namespace XPilot.PilotClient.Network.Controllers
{
    public class ControllerManager : EventBus, IControllerManager
    {
        [EventPublication(EventTopics.ControllerAdded)]
        public event EventHandler<ControllerEventArgs> ControllerAdded;

        [EventPublication(EventTopics.ControllerDeleted)]
        public event EventHandler<ControllerEventArgs> ControllerDeleted;

        [EventPublication(EventTopics.ControllerFrequencyChanged)]
        public event EventHandler<ControllerEventArgs> ControllerFrequencyChanged;

        [EventPublication(EventTopics.ControllerLocationChanged)]
        public event EventHandler<ControllerEventArgs> ControllerLocationChanged;

        [EventPublication(EventTopics.ControllerSupportsNewInfoChanged)]
        public event EventHandler<ControllerEventArgs> ControllerSupportsNewInfoChanged;

        private readonly Dictionary<string, Controller> mControllers = new Dictionary<string, Controller>();
        private readonly System.Windows.Forms.Timer mControllerUpdate;
        private readonly IFsdManger mFsdManager;

        public ControllerManager(IEventBroker broker, IFsdManger fsdManager) : base(broker)
        {
            mFsdManager = fsdManager;

            mControllerUpdate = new System.Windows.Forms.Timer
            {
                Interval = 45000
            };
            mControllerUpdate.Tick += ControllerUpdateTick;
        }

        private void ControllerUpdateTick(object sender, EventArgs e)
        {
            List<string> temp = mControllers.Values.Where(o => (DateTime.Now - o.LastUpdate).TotalMilliseconds > 45000.0).Select(o => o.Callsign).ToList();
            foreach (string value in temp)
            {
                Controller controller = mControllers[value];
                mControllers.Remove(value);
                if (controller.IsValid)
                {
                    controller.IsValid = false;
                    ControllerDeleted?.Invoke(this, new ControllerEventArgs(controller));
                }
            }
        }

        private void RemoveAll()
        {
            List<Controller> temp = new List<Controller>();
            temp = mControllers.Values.ToList();
            foreach (Controller controller in temp)
            {
                if (controller.IsValid)
                {
                    controller.IsValid = false;
                    ControllerDeleted?.Invoke(this, new ControllerEventArgs(controller));
                }
            }
            mControllers.Clear();
        }

        private void ValidateController(Controller controller)
        {
            bool isValid = controller.IsValid;
            controller.IsValid = (controller.IsValidATC && controller.Frequency.IsValidFrequency());
            if (isValid && !controller.IsValid)
            {

                ControllerDeleted?.Invoke(this, new ControllerEventArgs(controller));
            }
            else if (!isValid && controller.IsValid)
            {
                ControllerAdded?.Invoke(this, new ControllerEventArgs(controller));
            }
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            mControllerUpdate.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            mControllerUpdate.Stop();
            RemoveAll();
        }

        [EventSubscription(EventTopics.DeleteControllerReceived, typeof(OnUserInterfaceAsync))]
        public void OnDeleteControllerReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                Controller controller = mControllers[e.From];
                mControllers.Remove(controller.Callsign);
                if (controller.IsValid)
                {
                    ControllerDeleted?.Invoke(this, new ControllerEventArgs(controller));
                }
            }
        }

        [EventSubscription(EventTopics.ControllerUpdateReceived, typeof(OnUserInterfaceAsync))]
        public void OnControllerUpdated(object sender, ControllerUpdateReceivedEventArgs e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                Controller controller = mControllers[e.From];
                bool isValid = controller.IsValid;
                bool isFrequencyChanged = e.Frequency != controller.Frequency;
                bool isLocationChanged = !e.Location.Equals(controller.Location);
                controller.Frequency = e.Frequency;
                controller.NormalizedFrequency = e.Frequency.Normalize25KhzFsdFrequency();
                controller.Location = e.Location;
                controller.LastUpdate = DateTime.Now;
                ValidateController(controller);
                if (isValid && controller.IsValid)
                {
                    if (isFrequencyChanged)
                    {
                        ControllerFrequencyChanged?.Invoke(this, new ControllerEventArgs(controller));
                    }
                    if (isLocationChanged)
                    {
                        ControllerLocationChanged?.Invoke(this, new ControllerEventArgs(controller));
                    }
                }
            }
            else
            {
                Controller controller = new Controller
                {
                    Callsign = e.From,
                    Frequency = e.Frequency,
                    NormalizedFrequency = e.Frequency.Normalize25KhzFsdFrequency(),
                    Location = e.Location,
                    LastUpdate = DateTime.Now,
                    RealName = "Unknown"
                };

                mControllers.Add(controller.Callsign, controller);
                mFsdManager.CheckIfValidATC(controller.Callsign);
                mFsdManager.RequestClientCapabilities(controller.Callsign);
                mFsdManager.RequestRealName(controller.Callsign);
            }
        }

        [EventSubscription(EventTopics.IsValidAtcReceived, typeof(OnUserInterfaceAsync))]
        public void OnIsValidAtcReceived(object sender, IsValidAtcReceivedEventArgs e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                Controller controller = mControllers[e.From];
                if (controller.IsValidATC != e.IsValidAtc)
                {
                    controller.IsValidATC = e.IsValidAtc;
                    ValidateController(controller);
                }
            }
        }

        [EventSubscription(EventTopics.CapabilitiesResponseReceived, typeof(OnUserInterfaceAsync))]
        public void OnCapabilitiesResponseReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (mControllers.ContainsKey(e.From) && e.Data.ToUpper().Contains("NEWINFO=1"))
            {
                Controller controller = mControllers[e.From];
                if (controller.IsValid && !controller.SupportsNewInfo)
                {
                    controller.SupportsNewInfo = true;
                    ControllerSupportsNewInfoChanged?.Invoke(this, new ControllerEventArgs(controller));
                }
            }
        }

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnUserInterfaceAsync))]
        public void OnRealNameReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                mControllers[e.From].RealName = e.Data;
            }
        }
    }
}
