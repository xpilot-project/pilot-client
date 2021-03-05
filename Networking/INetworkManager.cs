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
using Vatsim.Xpilot.Aircrafts;

namespace Vatsim.Xpilot.Networking
{
    public interface INetworkManager
    {
        void RequestRealName(string callsign);
        void RequestIsValidATC(string callsign);
        void RequestControlerInfo(string callsign);
        void RequestClientCapabilities(string callsign);
        void RequestMetar(string station);
        void SendPrivateMessage(string to, string message);
        void SendRadioMessage(List<uint> frequencies, string message);
        void SendWallop(string message);
        void SendInfoRequest(string callsign);
        void RequestClientVersion(string callsign);
        void RequestAircraftInfo(string callsign);
        void SendClientCapabilities(string to);
        void SendAircraftConfigurationUpdate(string to, AircraftConfiguration config);
        void SendAircraftConfigurationUpdate(AircraftConfiguration config);
        void RequestAircraftConfiguration(string callsign);
        void SendCom1Frequency(string to, string frequency);
        void Connect(ConnectInfo connectInfo, string server);
        void Disconnect();
        void SetPluginHash(string hash);
        bool IsConnected { get; }
        string OurCallsign { get; }
    }
}