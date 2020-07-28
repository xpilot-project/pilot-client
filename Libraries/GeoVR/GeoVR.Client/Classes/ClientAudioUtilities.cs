using NAudio.CoreAudioApi;
using NAudio.Mixer;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class WaveInMixerControls
    {
        public UnsignedMixerControl VolumeControl { get; set; }
        public BooleanMixerControl MuteControl { get; set; }
        public bool Valid { get { return VolumeControl != null && MuteControl != null; } }
    }

    public static class ClientAudioUtilities
    {
        public static bool IsInputDevicePresent()
        {
            return WaveIn.DeviceCount > 0;
        }

        public static IEnumerable<string> GetInputDevices()
        {
            return GetWaveInInputDevices();
        }

        private static IEnumerable<string> GetWaveInInputDevices()
        {
            List<string> inputDevices = new List<string>();
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                //var controls = GetWaveInMixerControls(i);
                //if (controls.Valid)
                inputDevices.Add(WaveIn.GetCapabilities(i).ProductName);
            }
            return inputDevices;
        }

        private static IEnumerable<string> GetWasapiInputDevices()
        {
            MMDeviceEnumerator em = new MMDeviceEnumerator();
            var something = em.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            return something.Select(x => x.FriendlyName);
        }

        public static IEnumerable<string> GetOutputDevices()
        {
            return GetWasapiOutputDevices();
        }

        private static IEnumerable<string> GetWasapiOutputDevices()
        {
            MMDeviceEnumerator em = new MMDeviceEnumerator();
            var something = em.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            return something.Select(x => x.FriendlyName);
        }

        private static IEnumerable<string> GetWaveOutOutputDevices()
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                yield return WaveOut.GetCapabilities(i).ProductName;
            }
        }

        public static int MapInputDevice(string inputDevice)
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                if (WaveIn.GetCapabilities(i).ProductName == inputDevice)
                    return i;
            }
            return 0;       //Else use default
        }

        public static MMDevice MapWasapiInputDevice(string inputDevice)
        {
            MMDeviceEnumerator em = new MMDeviceEnumerator();
            var something = em.EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            return something.First(x => x.FriendlyName == inputDevice);
        }

        public static int MapOutputDevice(string outputDevice)
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                if (WaveOut.GetCapabilities(i).ProductName == outputDevice)
                    return i;
            }
            return 0;       //Else use default
        }

        public static MMDevice MapWasapiOutputDevice(string outputDevice)
        {
            MMDeviceEnumerator em = new MMDeviceEnumerator();
            var deviceCollection = em.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            if (deviceCollection.Any(x => x.FriendlyName == outputDevice))
                return deviceCollection.First(x => x.FriendlyName == outputDevice);
            else
                return deviceCollection[0];
        }

        public static WaveInMixerControls GetWaveInMixerControls(int waveInDevice)
        {
            WaveInMixerControls controls = new WaveInMixerControls();
            var waveInMixerID = MixerLine.GetMixerIdForWaveIn(waveInDevice);
            Mixer mixer = new Mixer(waveInMixerID);
            foreach (MixerLine destination in mixer.Destinations)
            {
                if (destination.ComponentType == MixerLineComponentType.DestinationWaveIn)
                {
                    foreach (MixerLine source in destination.Sources)
                    {
                        switch (source.ComponentType)
                        {
                            case MixerLineComponentType.SourceMicrophone:
                            case MixerLineComponentType.SourceLine:
                                foreach (MixerControl control in source.Controls)
                                {
                                    switch (control.ControlType)
                                    {
                                        case MixerControlType.Volume:
                                            controls.VolumeControl = (UnsignedMixerControl)control;
                                            break;
                                        case MixerControlType.Mute:
                                            controls.MuteControl = (BooleanMixerControl)control;
                                            break;
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            return controls;
        }

        //private static short GetShortFromLittleEndianBytes(byte[] data, int startIndex)
        //{
        //    return (short)((data[startIndex + 1] << 8)
        //         | data[startIndex]);
        //}

        //private static byte[] GetLittleEndianBytesFromShort(short data)
        //{
        //    byte[] b = new byte[2];
        //    b[0] = (byte)data;
        //    b[1] = (byte)(data >> 8 & 0xFF);
        //    return b;
        //}

        //This is very flawed - makes the sound weird
        //public static byte[] ToPcm16Bytes(float[] samples)
        //{
        //    byte[] output = new byte[samples.Length * 2];
        //    for (int i = 0; i < samples.Length; i+=2)
        //    {
        //        byte[] tmp = GetLittleEndianBytesFromShort(Convert.ToInt16(samples[i] * 32768f));
        //        output[i] = tmp[0];
        //        output[i + 1] = tmp[1];
        //    }
        //    return output;
        //}

        public static short[] ConvertBytesTo16BitPCM(byte[] input)
        {
            int inputSamples = input.Length / 2; // 16 bit input, so 2 bytes per sample
            short[] output = new short[inputSamples];
            int outputIndex = 0;
            for (int n = 0; n < inputSamples; n++)
            {
                output[outputIndex++] = BitConverter.ToInt16(input, n * 2);
            }
            return output;
        }
    }
}
