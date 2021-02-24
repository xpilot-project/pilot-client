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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace XPilot.PilotClient.AudioForVatsim
{
    public class AFVBindings
    {
        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool isClientInitialized();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DictionaryAdd(int key, string value);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void initialize(string resourcePath, int numRadios, string clientName);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setBaseUrl(string url);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void destroy();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool isAPIConnected();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool isVoiceConnected();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool afvConnect();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void afvDisconnect();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCredentials(string username, string password);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setCallsign(string callsign);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setClientPosition(double lat, double lon, double amslm, double aglm);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setRadioState(uint radioNum, int freq);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setRadioGain(uint radioNum, float gain);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setTxRadio(uint radioNum);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setPtt(bool ptt);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool getEnableInputFilters();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setEnableInputFilters(bool enabelInputFilters);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setEnableOutputEffects(bool enableOutputEffects);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setEnableHfSquelch(bool enableSquelch);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getAudioApis(DictionaryAdd callback);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setAudioApi(string api);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setAudioDevice();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getOutputDevices(DictionaryAdd callback);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getInputDevices(DictionaryAdd callback);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setAudioInputDevice(string inputDevice);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void setAudioOutputDevice(string outputDevice);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern double getInputPeak();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern double getInputVu();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EventCallback(AFVEvents evt, int data);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RaiseClientEvent(EventCallback fn);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AfvStationCallback(string id, string callsign, uint frequency, uint frequencyAlias);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void getStationAliases(AfvStationCallback fn);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void startAudio();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void stopAudio();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void RxCallsignCallback(uint id, uint count);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void ReceivingCallsignsCallback(RxCallsignCallback fn);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsCom1Rx();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsCom2Rx();
    }
}
