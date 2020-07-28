using GeoVR.Shared;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoVR.Client
{
    public class SoundcardSampleProvider : ISampleProvider
    {
        public WaveFormat WaveFormat { get; private set; }
        public bool BypassEffects
        {
            set
            {
                foreach (var receiverInput in receiverInputs)
                {
                    receiverInput.BypassEffects = value;
                }
            }
        }

        public float Com1Volume
        {
            set
            {
                var receiver = receiverInputs.FirstOrDefault(r => r.ID == 1);
                if (receiver != null)
                {
                    if (value > 2.0f) value = 2.0f;
                    if (value < 0.0f) value = 0.0f;
                    receiver.Volume = value;
                }
            }
        }

        public float Com2Volume
        {
            set
            {
                var receiver = receiverInputs.FirstOrDefault(r => r.ID == 2);
                if (receiver != null)
                {
                    if (value > 2.0f) value = 2.0f;
                    if (value < 0.0f) value = 0.0f;
                    receiver.Volume = value;
                }
            }
        }

        private readonly MixingSampleProvider mixer;
        private readonly List<ReceiverSampleProvider> receiverInputs;
        private readonly List<ushort> receiverIDs;
        private readonly ResourceSoundSampleProvider landLineRing;

        public SoundcardSampleProvider(int sampleRate, List<ushort> receiverIDs, EventHandler<TransceiverReceivingCallsignsChangedEventArgs> eventHandler)
        {
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);

            mixer = new MixingSampleProvider(WaveFormat)
            {
                ReadFully = true
            };

            receiverInputs = new List<ReceiverSampleProvider>();
            this.receiverIDs = new List<ushort>();
            foreach (var receiverID in receiverIDs)
            {
                var receiverInput = new ReceiverSampleProvider(WaveFormat, receiverID, 4);
                receiverInput.ReceivingCallsignsChanged += eventHandler;
                receiverInputs.Add(receiverInput);
                this.receiverIDs.Add(receiverID);
                mixer.AddMixerInput(receiverInput);
            }

            landLineRing = new ResourceSoundSampleProvider(Samples.Instance.LandLineRing) { Looping = true, Gain = 0 };
            mixer.AddMixerInput(landLineRing);

        }

        public void PTT(bool active, TxTransceiverDto[] txTransceivers)
        {
            if (active)
            {
                if (txTransceivers != null && txTransceivers.Length > 0)
                {
                    var txTransceiversFiltered = txTransceivers.Where(y => receiverIDs.Contains(y.ID)).ToList();
                    foreach (var txTransceiver in txTransceiversFiltered)
                    {
                        var receiverInput = receiverInputs.First(x => x.ID == txTransceiver.ID);
                        receiverInput.Mute = true;
                    }
                }
            }
            else
            {
                foreach (var receiverInput in receiverInputs)
                {
                    receiverInput.Mute = false;
                }
            }
        }

        public void LandLineRing(bool active)
        {
            if (active)
            {
                landLineRing.ResetToStart(1.0f);
            }
            else
            {
                landLineRing.Gain = 0.0f;
            }
        }

        public int Read(float[] buffer, int offset, int count)
        {
            return mixer.Read(buffer, offset, count);
        }

        public void AddOpusSamples(IAudioDto audioDto, List<RxTransceiverDto> rxTransceivers)
        {
            var rxTransceiversFilteredAndSorted = rxTransceivers.Where(y => receiverIDs.Contains(y.ID)).OrderByDescending(x => x.DistanceRatio).ToList();

            // The issue in a nutshell - you can have multiple transmitters so the audio DTO contains multiple transceivers with the same ID or
            // you can have multiple receivers so the audio DTO contains multiple transceivers with different IDs, but you only want to play the audio once in either scenario.

            if (rxTransceiversFilteredAndSorted.Count > 0)
            {
                bool audioPlayed = false;
                List<ushort> handledTransceiverIDs = new List<ushort>();
                for (int i = 0; i < rxTransceiversFilteredAndSorted.Count; i++)
                {
                    var rxTransceiver = rxTransceiversFilteredAndSorted[i];
                    if (!handledTransceiverIDs.Contains(rxTransceiver.ID))
                    {
                        handledTransceiverIDs.Add(rxTransceiver.ID);

                        var receiverInput = receiverInputs.First(x => x.ID == rxTransceiver.ID);
                        if (receiverInput.Mute)
                            continue;
                        if (!audioPlayed)
                        {
                            receiverInput.AddOpusSamples(audioDto, rxTransceiver.Frequency, rxTransceiver.DistanceRatio);
                            audioPlayed = true;
                        }
                        else
                        {
                            receiverInput.AddSilentSamples(audioDto, rxTransceiver.Frequency, rxTransceiver.DistanceRatio);
                        }
                    }
                }
            }
        }

        public void UpdateTransceivers(List<TransceiverDto> transceivers)
        {
            foreach (var transceiver in transceivers)
            {
                if (receiverInputs.Any(x => x.ID == transceiver.ID))
                {
                    receiverInputs.First(x => x.ID == transceiver.ID).Frequency = transceiver.Frequency;
                }
            }
            foreach (var transceiverID in receiverInputs.Select(x => x.ID))
            {
                if (!transceivers.Select(x => x.ID).Contains(transceiverID))
                {
                    receiverInputs.First(x => x.ID == transceiverID).Frequency = 0;
                }
            }
        }
    }
}
