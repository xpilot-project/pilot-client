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
using Vatsim.Xpilot.Protobuf;

namespace Vatsim.Xpilot.Simulator
{
    public interface IXplaneAdapter
    {
        void SendProtobufArray(Wrapper msg);
        void SetTransponderCode(int code);
        void EnableTransponderModeC(bool enabled);
        void TriggerTransponderIdent();
        void SetRadioFrequency(int radio, uint freq);
        void SetRadioTransmit(int radio);
        void SetRadioReceive(int radio, bool enabled);
        void SetLoginStatus(bool connected);
        void AddPlane(Aircraft aircraft);
        void SendSlowPositionUpdate(Aircraft aircraft, AircraftVisualState visualState, double groundSpeed);
        void SendFastPositionUpdate(Aircraft aircraft, AircraftVisualState visualState, VelocityVector positionalVelocityVector, VelocityVector rotationalVelocityVector);
        void PlaneConfigChanged(Aircraft aircraft);
        void DeleteAircraft(Aircraft aircraft);
        void ChangeModel(Aircraft aircraft);
        void NetworkConnected(string callsign);
        void NetworkDisconnected();
        List<int> TunedFrequencies { get; }
        bool ValidSimConnection { get; }
    }
}