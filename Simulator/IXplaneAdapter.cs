﻿/*
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
using Abacus.DoublePrecision;
using System.Collections.Generic;
using Vatsim.Xpilot.Aircrafts;

namespace Vatsim.Xpilot.Simulator
{
    public interface IXplaneAdapter
    {
        void SetTransponderCode(int code);
        void SetRadioFrequency(int radio, uint freq);
        void SetAudioComSelection(int radio);
        void SetAudioSelectionCom1(bool enabled);
        void SetAudioSelectionCom2(bool enabled);
        void SetLoginStatus(bool connected);
        void AddPlane(Aircraft aircraft);
        void SendSlowPositionUpdate(Aircraft aircraft, AircraftVisualState visualState, double groundSpeed);
        void SendFastPositionUpdate(Aircraft aircraft, AircraftVisualState visualState, Vector3 positionalVelocityVector, Vector3 rotationalVelocityVector);
        void PlaneConfigChanged(string callsign, AircraftConfiguration config);
        void RemovePlane(Aircraft aircraft);
        void ChangeModel(Aircraft aircraft);
        void RemoveAllPlanes();
        void NetworkConnected(string callsign);
        void NetworkDisconnected();
        List<int> TunedFrequencies { get; }
    }
}