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
        public event EventHandler<ControllerUpdateReceived> RaiseControllerAdded;

        [EventPublication(EventTopics.ControllerDeleted)]
        public event EventHandler<NetworkDataReceived> RaiseControllerDeleted;

        [EventPublication(EventTopics.ControllerFrequencyChanged)]
        public event EventHandler<ControllerUpdateReceived> RaiseControllerFrequencyChanged;

        private readonly Dictionary<string, Controller> mControllers = new Dictionary<string, Controller>();
        private readonly System.Windows.Forms.Timer mControllerUpdate;
        private readonly IFsdManager mFsdManager;

        public ControllerManager(IEventBroker broker, IFsdManager fsdManager) : base(broker)
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
                    RaiseControllerDeleted?.Invoke(this, new NetworkDataReceived(controller.Callsign));
                }
            }
        }

        private void RemoveAll()
        {
            List<Controller> temp = mControllers.Values.ToList();
            foreach (Controller controller in temp)
            {
                if (controller.IsValid)
                {
                    controller.IsValid = false;
                    RaiseControllerDeleted?.Invoke(this, new NetworkDataReceived(controller.Callsign));
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

                RaiseControllerDeleted?.Invoke(this, new NetworkDataReceived(controller.Callsign));
            }
            else if (!isValid && controller.IsValid)
            {
                RaiseControllerAdded?.Invoke(this, new ControllerUpdateReceived(controller));
            }
        }

        public uint GetFrequencyByCallsign(string callsign)
        {
            if (mControllers.ContainsKey(callsign))
            {
                return mControllers[callsign].NormalizedFrequency;
            }
            return 0;
        }

        public Dictionary<string, Controller> GetControllers()
        {
            return mControllers;
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnected e)
        {
            mControllerUpdate.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnected e)
        {
            mControllerUpdate.Stop();
            RemoveAll();
        }

        [EventSubscription(EventTopics.ControllerDeleted, typeof(OnUserInterfaceAsync))]
        public void OnControllerDeleted(object sender, NetworkDataReceived e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                Controller controller = mControllers[e.From];
                mControllers.Remove(controller.Callsign);
                if (controller.IsValid)
                {
                    RaiseControllerDeleted?.Invoke(this, new NetworkDataReceived(e.From));
                }
            }
        }

        [EventSubscription(EventTopics.ControllerUpdateReceived, typeof(OnUserInterfaceAsync))]
        public void OnControllerUpdated(object sender, ControllerUpdateReceived e)
        {
            if (mControllers.ContainsKey(e.Controller.Callsign))
            {
                Controller controller = mControllers[e.Controller.Callsign];
                bool isValid = controller.IsValid;
                bool isFrequencyChanged = e.Controller.Frequency != controller.Frequency;
                controller.Frequency = e.Controller.Frequency;
                controller.NormalizedFrequency = e.Controller.Frequency.Normalize25KhzFsdFrequency();
                controller.Location = e.Controller.Location;
                controller.LastUpdate = DateTime.Now;
                ValidateController(controller);
                if (isValid && controller.IsValid)
                {
                    if (isFrequencyChanged)
                    {
                        RaiseControllerFrequencyChanged?.Invoke(this, new ControllerUpdateReceived(controller));
                    }
                }
            }
            else
            {
                Controller controller = new Controller
                {
                    Callsign = e.Controller.Callsign,
                    Frequency = e.Controller.Frequency,
                    NormalizedFrequency = e.Controller.Frequency.Normalize25KhzFsdFrequency(),
                    Location = e.Controller.Location,
                    LastUpdate = DateTime.Now,
                    RealName = "Unknown"
                };

                mControllers.Add(controller.Callsign, controller);
                mFsdManager.CheckIfValidATC(controller.Callsign);
                mFsdManager.RequestClientCapabilities(controller.Callsign);
                mFsdManager.RequestRealName(controller.Callsign);
            }
        }

        [EventSubscription(EventTopics.IsValidAtcReceived, typeof(OnPublisher))]
        public void OnIsValidAtcReceived(object sender, IsValidATCReceived e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                Controller controller = mControllers[e.From];
                if (controller.IsValidATC != e.IsValidATC)
                {
                    controller.IsValidATC = e.IsValidATC;
                    ValidateController(controller);
                }
            }
        }

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnPublisher))]
        public void OnRealNameReceived(object sender, NetworkDataReceived e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                mControllers[e.From].RealName = e.Data;
            }
        }
    }
}
