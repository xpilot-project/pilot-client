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
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Events.Arguments;
using Vatsim.Xpilot.Networking;

namespace Vatsim.Xpilot.Controllers
{
    public class ControllerAtisManager : EventBus, IControllerAtisManager
    {
        [EventPublication(EventTopics.RequestedAtisReceived)]
        public event EventHandler<RequestedAtisReceivedEventArgs> RequestAtisReceived;

        private readonly INetworkManager mFsdManager;
        private readonly Dictionary<string, List<string>> mAtisReceived = new Dictionary<string, List<string>>();

        public ControllerAtisManager(IEventBroker broker, INetworkManager fsdManager) : base(broker)
        {
            mFsdManager = fsdManager;
        }

        [EventSubscription(EventTopics.AtisLineReceived, typeof(OnUserInterfaceAsync))]
        public void OnAtisLineReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (mAtisReceived.ContainsKey(e.From))
            {
                mAtisReceived[e.From].Add(e.Data);
            }
        }

        [EventSubscription(EventTopics.AtisEndReceived, typeof(OnUserInterfaceAsync))]
        public void OnAtisEndReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (mAtisReceived.ContainsKey(e.From))
            {
                RequestAtisReceived?.Invoke(this, new RequestedAtisReceivedEventArgs(e.From, mAtisReceived[e.From]));
                mAtisReceived.Remove(e.From);
            }
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            mAtisReceived.Clear();
        }

        public void RequestControllerAtis(string callsign)
        {
            if (!mAtisReceived.ContainsKey(callsign))
            {
                mAtisReceived.Add(callsign, new List<string>());
                mFsdManager.SendControlerInfoRequest(callsign);
            }
        }
    }
}
