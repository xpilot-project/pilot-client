/*
 * xPilot: X-Plane pilot client for VATSIM
 * Copyright (C) 2019-2021 Justin Shannon
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
namespace Vatsim.Xpilot.AudioForVatsim
{
    public enum AFVEvents
    {
        APIServerConnected,
        APIServerDisconnected,
        APIServerError, // data is a pointer to the APISessionError
        VoiceServerConnected,
        VoiceServerDisconnected,
        VoiceServerChannelError, // data is a pointer to an int containing the errno
        VoiceServerError, // data is a pointer to the VoiceSessionError
        PttOpen,
        PttClosed,
        StationAliasesUpdated,
        AudioError,
        RxStarted,
        RxStopped
    }

    public enum APISessionError
    {
        NoError = 0,
        ConnectionError, // local socket or curl error - see data returned.
        BadRequestOrClientIncompatible, // APIServer 400
        RejectedCredentials, // APIServer 403
        BadPassword, // APIServer 401
        OtherRequestError,
        InvalidAuthToken,  // local parse error
        AuthTokenExpiryTimeInPast, // local parse error
    }

    public enum VoiceSessionError
    {
        NoError = 0,
        UDPChannelError,
        BadResponseFromAPIServer,
        Timeout,
    }

    public enum RadioState
    {
        RxStarted,
        RxStopped
    }
}
