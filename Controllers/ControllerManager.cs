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
using System.Timers;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Events.Arguments;
using Vatsim.Xpilot.Networking;
using Vatsim.Xpilot.Protobuf;
using Vatsim.Xpilot.Simulator;

namespace Vatsim.Xpilot.Controllers
{
    public class ControllerManager : EventBus, IControllerManager
    {
        [EventPublication(EventTopics.ControllerAdded)]
        public event EventHandler<ControllerEventArgs> ControllerAdded;

        [EventPublication(EventTopics.ControllerDeleted)]
        public event EventHandler<ControllerEventArgs> ControllerDeleted;

        [EventPublication(EventTopics.ControllerFrequencyChanged)]
        public event EventHandler<ControllerEventArgs> ControllerFrequencyChanged;

        [EventPublication(EventTopics.ControllerSupportsNewInfoChanged)]
        public event EventHandler<ControllerEventArgs> ControllerSupportsNewInfoChanged;

        [EventPublication(EventTopics.ControllerLocationChanged)]
        public event EventHandler<ControllerEventArgs> ControllerLocationChanged;

        private readonly Dictionary<string, Controller> mControllers = new Dictionary<string, Controller>();
        private readonly Timer mControllerUpdate;
        private readonly Timer mControllerListRefreshTimer;
        private readonly INetworkManager mFsdManager;
        private readonly IXplaneAdapter mXplaneAdapter;

        public ControllerManager(IEventBroker broker, INetworkManager fsdManager, IXplaneAdapter xplaneAdapter) : base(broker)
        {
            mFsdManager = fsdManager;
            mXplaneAdapter = xplaneAdapter;

            mControllerUpdate = new Timer() { Interval = 45000 };
            mControllerUpdate.Elapsed += ControllerUpdate_Elapsed;

            mControllerListRefreshTimer = new Timer() { Interval = 5000 };
            mControllerListRefreshTimer.Elapsed += ControllerListRefreshTimer_Elapsed;
        }

        private void ControllerListRefreshTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<NearbyControllers.Types.Controller> list = new List<NearbyControllers.Types.Controller>();
            foreach (var controller in mControllers)
            {
                Controller c = mControllers[controller.Key];
                list.Add(new NearbyControllers.Types.Controller
                {
                    Callsign = c.Callsign,
                    Frequency = (c.NormalizedFrequency.FsdFrequencyToHertz() / 1000000.0).ToString("0.000"),
                    XplaneFrequency = (int)c.NormalizedFrequency + 100000,
                    RealName = c.RealName
                });
            }
            var msg = new Wrapper
            {
                NearbyControllers = new NearbyControllers()
            };
            msg.NearbyControllers.List.AddRange(list);
            mXplaneAdapter.SendProtobufArray(msg);
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            mControllerUpdate.Start();
            mControllerListRefreshTimer.Start();
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            mControllerUpdate.Stop();
            mControllerListRefreshTimer.Stop();
            DeleteAllControllers();
        }

        [EventSubscription(EventTopics.DeleteControllerReceived, typeof(OnUserInterfaceAsync))]
        public void OnControllerDeleted(object sender, NetworkDataReceivedEventArgs e)
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
                bool hasFrequencyChanged = e.Frequency != controller.Frequency;
                bool hasLocationChanged = !e.Location.Equals(controller.Location);
                controller.Frequency = e.Frequency;
                controller.NormalizedFrequency = e.Frequency.Normalize25KhzFsdFrequency();
                controller.Location = e.Location;
                controller.LastUpdate = DateTime.Now;
                ValidateController(controller);
                if (isValid && controller.IsValid)
                {
                    if (hasFrequencyChanged)
                    {
                        ControllerFrequencyChanged?.Invoke(this, new ControllerEventArgs(controller));
                    }
                    if (hasLocationChanged)
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
                mFsdManager.SendIsValidATCRequest(controller.Callsign);
                mFsdManager.SendCapabilitiesRequest(controller.Callsign);
                mFsdManager.SendRealNameRequest(controller.Callsign);
            }
        }

        [EventSubscription(EventTopics.IsValidATCReceived, typeof(OnUserInterfaceAsync))]
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

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnPublisher))]
        public void OnRealNameReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (mControllers.ContainsKey(e.From))
            {
                mControllers[e.From].RealName = e.Data;
            }
        }

        private void ControllerUpdate_Elapsed(object sender, ElapsedEventArgs e)
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

        private void DeleteAllControllers()
        {
            List<Controller> temp = mControllers.Values.ToList();
            foreach (Controller controller in temp)
            {
                if (controller.IsValid)
                {
                    controller.IsValid = false;
                    ControllerDeleted?.Invoke(this, new ControllerEventArgs(controller));
                }
            }
            mControllers.Clear();

            var msg = new Wrapper
            {
                NearbyControllers = new NearbyControllers()
            };
            msg.NearbyControllers.List.AddRange(new List<NearbyControllers.Types.Controller>());
            mXplaneAdapter.SendProtobufArray(msg);
        }

        private void ValidateController(Controller controller)
        {
            bool isValid = controller.IsValid;
            controller.IsValid = controller.IsValidATC && controller.NormalizedFrequency.IsValidFrequency();
            if (isValid && !controller.IsValid)
            {
                ControllerDeleted?.Invoke(this, new ControllerEventArgs(controller));
            }
            else if (!isValid && controller.IsValid)
            {
                ControllerAdded?.Invoke(this, new ControllerEventArgs(controller));
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
    }
}
