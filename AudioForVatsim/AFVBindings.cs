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
using System.Runtime.InteropServices;

namespace Vatsim.Xpilot.AudioForVatsim
{
    public class AFVBindings
    {
        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsClientInitialized();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DictionaryAdd(int key, string value);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Initialize(string resourcePath, int numRadios, string clientName);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetBaseUrl(string url);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Destroy();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsAPIConnected();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsVoiceConnected();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool AfvConnect();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AfvDisconnect();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCredentials(string username, string password);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCallsign(string callsign);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetClientPosition(double lat, double lon, double amslm, double aglm);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetRadioState(uint radioNum, int freq);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetRadioGain(uint radioNum, float gain);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetTxRadio(uint radioNum);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetPtt(bool ptt);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetEnableInputFilters();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetEnableInputFilters(bool enabelInputFilters);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetEnableOutputEffects(bool enableOutputEffects);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetEnableHfSquelch(bool enableSquelch);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetAudioApis(DictionaryAdd callback);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetAudioApi(string api);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAudioDevice();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetOutputDevices(DictionaryAdd callback);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetInputDevices(DictionaryAdd callback);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAudioInputDevice(string inputDevice);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetAudioOutputDevice(string outputDevice);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetInputPeak();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern double GetInputVu();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void EventCallback(AFVEvents evt, int data);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RaiseClientEvent(EventCallback fn);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AfvStationCallback(string id, string callsign, uint frequency, uint frequencyAlias);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetStationAliases(AfvStationCallback fn);

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartAudio();

        [DllImport("afv_native", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StopAudio();

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
