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
using System.Collections.Generic;
using Vatsim.Fsd.Connector;
using Vatsim.Xpilot.Aircrafts;

namespace Vatsim.Xpilot.Networking
{
    public interface IFsdManager
    {
        void Connect(ConnectInfo info, string address);
        void Disconnect(DisconnectInfo info);
        void ForceDisconnect(string reason = "");
        List<NetworkServerInfo> ReturnServerList();
        void RequestClientCapabilities(string callsign);
        void RequestRealName(string callsign);
        void RequestControllerInfo(string callsign);
        void RequestClientFlightPlan(string callsign);
        void CheckIfValidATC(string callsign);
        void SquawkIdent();
        bool IsConnected { get; }
        bool IsObserver { get; }
        string OurCallsign { get; set; }
        void RequestPlaneInformation(string callsign);
        void SendAircraftConfiguration(string to, AircraftConfiguration config);
        void SendIncrementalAircraftConfigurationUpdate(AircraftConfiguration config);
        void RequestFullAircraftConfiguration(string to);
        void SendClientCaps(string from);
        void RequestInfoQuery(string to);
        string SelcalCode { get; }
        ClientProperties ClientProperties { get; set; }
        ConnectInfo ConnectInfo { get; }
    }
}