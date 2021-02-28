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
using XPilot.PilotClient.Aircraft;

namespace XPilot.PilotClient.XplaneAdapter
{
    public interface IXplaneConnectionManager
    {
        void SetTransponderCode(int code);
        void SetRadioFrequency(int radio, uint freq);
        void SetAudioComSelection(int radio);
        void SetAudioSelectionCom1(bool enabled);
        void SetAudioSelectionCom2(bool enabled);
        void SetLoginStatus(bool connected);
        void AddPlane(NetworkAircraft plane);
        void PlanePoseChanged(NetworkAircraft plane, NetworkAircraftPose pose);
        void PlaneConfigChanged(Xpilot.AirplaneConfig config);
        void RemovePlane(NetworkAircraft plane);
        void RemoveAllPlanes();
        void NetworkConnected(string callsign);
        void NetworkDisconnected();
    }
}