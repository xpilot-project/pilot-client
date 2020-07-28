using Concentus.Enums;
using Concentus.Structs;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoVR.Client
{
    public class Input
    {
        private readonly int frameSize = 960;
        private int sampleRate;

        private OpusEncoder encoder;
        private WaveInEvent waveIn;
        private InputVolumeStreamEventArgs inputVolumeStreamArgs;
        private OpusDataAvailableEventArgs opusDataAvailableArgs;

        public event EventHandler<OpusDataAvailableEventArgs> OpusDataAvailable;
        public event EventHandler<InputVolumeStreamEventArgs> InputVolumeStream;

        public bool Started { get; private set; }
        public long OpusBytesEncoded { get; set; }
        public float Volume { get; set; } = 1;

        public Input(int sampleRate)
        {
            this.sampleRate = sampleRate;
            encoder = OpusEncoder.Create(sampleRate, 1, OpusApplication.OPUS_APPLICATION_VOIP);
            encoder.Bitrate = 16 * 1024;
        }

        public void Start(string inputDevice)
        {
            if (Started)
                throw new Exception("Input already started");

            //waveIn = new WaveIn(WaveCallbackInfo.FunctionCallback());
            waveIn = new WaveInEvent();
            waveIn.BufferMilliseconds = 20;
            //Console.WriteLine("Input device: " + WaveIn.GetCapabilities(0).ProductName);
            waveIn.DeviceNumber = ClientAudioUtilities.MapInputDevice(inputDevice);
            waveIn.DataAvailable += _waveIn_DataAvailable;
            waveIn.WaveFormat = new WaveFormat(sampleRate, 16, 1);

            inputVolumeStreamArgs = new InputVolumeStreamEventArgs() { DeviceNumber = waveIn.DeviceNumber, PeakRaw = 0, PeakDB = float.NegativeInfinity, PeakVU = 0 };
            opusDataAvailableArgs = new OpusDataAvailableEventArgs();

            //inputControls = ClientAudioUtilities.GetWaveInMixerControls(waveIn.DeviceNumber);
            waveIn.StartRecording();

            Started = true;
        }

        public void Stop()
        {
            if (!Started)
                throw new Exception("Input not started");

            Started = false;

            waveIn.StopRecording();
            waveIn.Dispose();
            waveIn = null;
        }

        byte[] encodedDataBuffer = new byte[1275];
        int encodedDataLength;
        uint audioSequenceCounter = 0;
        //InputVolumeStream event
        float maxSampleInput = 0;
        float sampleInput = 0;
        private int sampleCount = 0;
        private int sampleCountPerEvent = 4800;
        float maxDb = 0;
        float minDb = -40;
        private void _waveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            var samples = ClientAudioUtilities.ConvertBytesTo16BitPCM(e.Buffer);
            if (samples.Length != frameSize)
                throw new Exception("Incorrect number of samples.");

            float value = 0;
            for (int i = 0; i < samples.Length; i++)
            {
                value = samples[i] * Volume;
                if (value > short.MaxValue)
                    value = short.MaxValue;
                if (value < short.MinValue)
                    value = short.MinValue;
                samples[i] = (short)value;
            }

            encodedDataLength = encoder.Encode(samples, 0, frameSize, encodedDataBuffer, 0, encodedDataBuffer.Length);
            OpusBytesEncoded += encodedDataLength;

            //Calculate max and raise event if needed
            if (InputVolumeStream != null)
            {
                for (int i = 0; i < samples.Length; i++)
                {
                    sampleInput = samples[i];
                    sampleInput = System.Math.Abs(sampleInput);
                    if (sampleInput > maxSampleInput)
                        maxSampleInput = sampleInput;
                }
                sampleCount += samples.Length;
                if (sampleCount >= sampleCountPerEvent)
                {
                    inputVolumeStreamArgs.PeakRaw = maxSampleInput / short.MaxValue;
                    inputVolumeStreamArgs.PeakDB = (float)(20 * Math.Log10(inputVolumeStreamArgs.PeakRaw));
                    float db = inputVolumeStreamArgs.PeakDB;
                    if (db < minDb)
                        db = minDb;
                    if (db > maxDb)
                        db = maxDb;
                    float ratio = (db - minDb) / (maxDb - minDb);
                    if (ratio < 0.30)
                        ratio = 0;
                    if (ratio > 1.0)
                        ratio = 1;
                    inputVolumeStreamArgs.PeakVU = ratio;
                    InputVolumeStream(this, inputVolumeStreamArgs);
                    sampleCount = 0;
                    maxSampleInput = 0;
                }
            }

            if (OpusDataAvailable != null)
            {
                byte[] trimmedBuff = new byte[encodedDataLength];
                Buffer.BlockCopy(encodedDataBuffer, 0, trimmedBuff, 0, encodedDataLength);
                opusDataAvailableArgs.Audio = trimmedBuff;
                opusDataAvailableArgs.SequenceCounter = audioSequenceCounter++;
                OpusDataAvailable(this, opusDataAvailableArgs);
            }
        }
    }

    public class OpusDataAvailableEventArgs
    {
        public uint SequenceCounter { get; set; }
        public byte[] Audio { get; set; }
    }

    public class InputVolumeStreamEventArgs
    {
        public int DeviceNumber { get; set; }
        public float PeakRaw { get; set; }
        public float PeakDB { get; set; }
        public float PeakVU { get; set; }
    }
}
