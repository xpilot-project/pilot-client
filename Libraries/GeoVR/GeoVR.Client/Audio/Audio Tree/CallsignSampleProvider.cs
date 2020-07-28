using Concentus.Structs;
using GeoVR.Client.Audio;
using GeoVR.Shared;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Diagnostics;
using System.Timers;

namespace GeoVR.Client
{
    // This needs to:
    // - Apply some crackle based on the quality value.
    // - Apply some noise based on the quality value.
    // - Apply some hum based on the AC type.
    // - Apply a VHF radio filter

    public class CallsignSampleProvider : ISampleProvider
    {
        public WaveFormat WaveFormat { get; private set; }

        private const float whiteNoiseGainMin = 0.17f; //0.01;
        private const float hfWhiteNoiseGainMin = 0.10f; //0.01;
        private const float acBusGainMin = 0.0028f;    //0.002;
        private const int frameCount = 960;
        private const int idleTimeoutMs = 500;

        public string Callsign { get; private set; }
        public string Type { get; private set; }
        public bool InUse { get; private set; }

        private bool bypassEffects;
        public bool BypassEffects
        {
            set
            {
                bypassEffects = value;
                SetEffects();
            }
        }

        private float distanceRatio;
        public float DistanceRatio
        {
            set
            {
                distanceRatio = value;
                SetEffects();
            }
        }

        private readonly ReceiverSampleProvider receiver;        //To give us access to frequency
        private readonly MixingSampleProvider mixer;
        private readonly ResourceSoundSampleProvider crackleSoundProvider;
        //private readonly SignalGenerator whiteNoise;
        private readonly ResourceSoundSampleProvider whiteNoise;
        private readonly ResourceSoundSampleProvider hfWhiteNoise;
        private readonly ResourceSoundSampleProvider acBusNoise;

        private readonly SimpleCompressorEffect simpleCompressorEffect;
        private readonly LimiterEffect limiterEffect;
        //private readonly SoftLimiter softLimiter;
        private readonly EqualizerSampleProvider voiceEq;
        private readonly BufferedWaveProvider audioInput;
        private readonly short[] decoderShortBuffer;
        private readonly byte[] decoderByteBuffer;
        private readonly System.Timers.Timer timer;

        private OpusDecoder decoder;
        private bool lastPacketLatch;
        private DateTime lastSamplesAddedUtc;
        private bool underflow;
        private uint sequenceCounter;

        public CallsignSampleProvider(WaveFormat waveFormat, ReceiverSampleProvider receiver)
        {
            this.receiver = receiver;
            WaveFormat = waveFormat;
            if (waveFormat.Channels != 1)
                throw new Exception("Incorrect number of channels. A channel count of 1 is required.");

            decoderShortBuffer = new short[frameCount];
            decoderByteBuffer = new byte[frameCount * 2];

            mixer = new MixingSampleProvider(WaveFormat)
            {
                ReadFully = true
            };
            crackleSoundProvider = new ResourceSoundSampleProvider(Samples.Instance.Crackle) { Looping = true, Gain = 0 };
            whiteNoise = new ResourceSoundSampleProvider(Samples.Instance.WhiteNoise) { Looping = true, Gain = 0 };
            hfWhiteNoise = new ResourceSoundSampleProvider(Samples.Instance.HFWhiteNoise) { Looping = true, Gain = 0 };
            acBusNoise = new ResourceSoundSampleProvider(Samples.Instance.AcBus) { Looping = true, Gain = 0 };

            audioInput = new BufferedWaveProvider(new WaveFormat(WaveFormat.SampleRate, 16, 1)) { ReadFully = true, BufferLength = new WaveFormat(WaveFormat.SampleRate, 16, 1).AverageBytesPerSecond * 60 };

            //Create the compressor
            simpleCompressorEffect = new SimpleCompressorEffect(new Pcm16BitToSampleProvider(audioInput));
            simpleCompressorEffect.Enabled = true;
            simpleCompressorEffect.MakeUpGain = -5.5;

            limiterEffect = new LimiterEffect(simpleCompressorEffect);
            //softLimiter = new SoftLimiter(simpleCompressorEffect);

            // Create the voice EQ
            voiceEq = new EqualizerSampleProvider(limiterEffect, AudioConfig.Instance.VhfEqualizer);
            //voiceEq = new EqualizerSampleProvider(softLimiter, AudioConfig.Instance.VhfEqualizer);

            BypassEffects = false;
            DistanceRatio = 1;

            mixer.AddMixerInput(crackleSoundProvider);
            mixer.AddMixerInput(whiteNoise.ToMono());
            mixer.AddMixerInput(acBusNoise.ToMono());
            mixer.AddMixerInput(hfWhiteNoise.ToMono());
            mixer.AddMixerInput(voiceEq);

            timer = new System.Timers.Timer();
            timer.Interval = 100;
            timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object sender, ElapsedEventArgs e)      //In case the last packet is lost
        {
            if (InUse && audioInput.BufferedBytes == 0 && DateTime.UtcNow.Subtract(lastSamplesAddedUtc) > TimeSpan.FromMilliseconds(idleTimeoutMs))
            {
                Idle();
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            //if (audioInput.BufferedBytes < count && decoder != null)              //Packet loss concealment. Don't use as is - need to check the logic of any audioInput.BufferedBytes == 0 usage
            //{
            //    decoder.Decode(null, 0, 0, decoderShortBuffer, 0, frameCount, false);
            //    //Optimise the following at some point.
            //    for (int i = 0; i < 960; i++)
            //    {
            //        var bytes = BitConverter.GetBytes(decoderShortBuffer[i]);
            //        decoderByteBuffer[i * 2] = bytes[0];
            //        decoderByteBuffer[(i * 2) + 1] = bytes[1];
            //    }
            //    audioInput.AddSamples(decoderByteBuffer, 0, frameCount * 2);
            //}
            var samples = mixer.Read(buffer, offset, count);

            if (InUse && lastPacketLatch && audioInput.BufferedBytes == 0)
            {
                Idle();
                lastPacketLatch = false;
            }

            if (InUse && !underflow && audioInput.BufferedBytes == 0)
            {
                Debug.WriteLine("[" + Callsign + "] [Delay++]");
                CallsignDelayCache.Instance.Underflow(Callsign);
                underflow = true;
            }

            return samples;
        }

        public void Active(string callsign, string aircraftType)
        {
            Callsign = callsign;
            CallsignDelayCache.Instance.Initialise(callsign);
            Type = aircraftType;
            decoder = new OpusDecoder(WaveFormat.SampleRate, 1);
            InUse = true;
            SetEffects();
            underflow = false;

            var delayMs = CallsignDelayCache.Instance.Get(callsign);
            Debug.WriteLine("[" + Callsign + "] [Delay " + delayMs + "ms]");
            if (delayMs > 0)
            {
                var phaseDelayLength = (WaveFormat.SampleRate / 1000) * delayMs;
                var phaseDelay = new byte[phaseDelayLength * 2];
                audioInput.AddSamples(phaseDelay, 0, phaseDelayLength * 2);
            }
        }

        public void ActiveSilent(string callsign, string type)          //Used so that events are still raised but we don't to send it onto the soundcard
        {
            Callsign = callsign;
            CallsignDelayCache.Instance.Initialise(callsign);
            Type = type;
            decoder = new OpusDecoder(WaveFormat.SampleRate, 1);
            InUse = true;
            SetEffects(true);
            underflow = true;
        }

        private void Idle()
        {
            timer.Stop();
            InUse = false;
            SetEffects();
            Callsign = "";
            Type = "";
            sequenceCounter = 0;
        }

        public void Clear()
        {
            Idle();
            audioInput.ClearBuffer();
        }

        public void AddOpusSamples(IAudioDto audioDto, float distanceRatio)
        {
            if (audioDto.SequenceCounter > sequenceCounter)
            {
                sequenceCounter = audioDto.SequenceCounter;
                DistanceRatio = distanceRatio;
                DecodeOpus(audioDto.Audio);
                audioInput.AddSamples(decoderByteBuffer, 0, frameCount * 2);
                lastPacketLatch = audioDto.LastPacket;
                if (audioDto.LastPacket && !underflow)
                    CallsignDelayCache.Instance.Success(Callsign);

                lastSamplesAddedUtc = DateTime.UtcNow;
                if (!timer.Enabled)
                    timer.Start();
            }
        }

        public void AddSilentSamples(IAudioDto audioDto)
        {
            //Disable all audio effects
            SetEffects(true);

            Array.Clear(decoderByteBuffer, 0, frameCount * 2);
            audioInput.AddSamples(decoderByteBuffer, 0, frameCount * 2);
            lastPacketLatch = audioDto.LastPacket;

            lastSamplesAddedUtc = DateTime.UtcNow;
            if (!timer.Enabled)
                timer.Start();
        }

        private void DecodeOpus(byte[] opusData)
        {
            decoder.Decode(opusData, 0, opusData.Length, decoderShortBuffer, 0, frameCount, false);
            //Optimise the following at some point.
            for (int i = 0; i < 960; i++)
            {
                var bytes = BitConverter.GetBytes(decoderShortBuffer[i]);
                decoderByteBuffer[i * 2] = bytes[0];
                decoderByteBuffer[(i * 2) + 1] = bytes[1];
            }
        }

        private void SetEffects(bool noEffects = false)
        {
            if (noEffects || bypassEffects || !InUse)
            {
                crackleSoundProvider.Gain = 0;
                whiteNoise.Gain = 0;
                hfWhiteNoise.Gain = 0;
                acBusNoise.Gain = 0;
                simpleCompressorEffect.Enabled = false;
                voiceEq.Bypass = true;
            }
            else
            {
                if (receiver.Frequency < 30000000)
                {
                    float crackleFactor = (float)(((System.Math.Exp(distanceRatio) * System.Math.Pow(distanceRatio, -4.0)) / 350) - 0.00776652);

                    if (crackleFactor < 0.0f) { crackleFactor = 0.0f; }
                    if (crackleFactor > 0.20f) { crackleFactor = 0.20f; }
                    
                    hfWhiteNoise.Gain = hfWhiteNoiseGainMin;
                    acBusNoise.Gain = acBusGainMin + 0.001f;
                    simpleCompressorEffect.Enabled = true;
                    voiceEq.Bypass = false;
                    voiceEq.OutputGain = 0.38f;
                    whiteNoise.Gain = 0;
                }
                else
                {
                    float crackleFactor = (float)(((System.Math.Exp(distanceRatio) * System.Math.Pow(distanceRatio, -4.0)) / 350) - 0.00776652);

                    if (crackleFactor < 0.0f) { crackleFactor = 0.0f; }
                    if (crackleFactor > 0.20f) { crackleFactor = 0.20f; }

                    crackleSoundProvider.Gain = crackleFactor * 2;

                    whiteNoise.Gain = whiteNoiseGainMin;
                    acBusNoise.Gain = acBusGainMin;
                    simpleCompressorEffect.Enabled = true;
                    voiceEq.Bypass = false;
                    voiceEq.OutputGain = 1.0 - crackleFactor * 3.7;
                }

            }
        }
    }
}
