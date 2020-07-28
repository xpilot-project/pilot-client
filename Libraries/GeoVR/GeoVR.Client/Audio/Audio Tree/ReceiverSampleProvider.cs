using GeoVR.Shared;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GeoVR.Client
{
    // This needs to:
    // - Apply the blocking tone when applicable
    // - Apply the click sound when applicable
    // - Mix the CallsignSampleProviders together

    public class ReceiverSampleProvider : ISampleProvider
    {
        public WaveFormat WaveFormat { get; private set; }

        public bool BypassEffects
        {
            set
            {
                foreach (var voiceInput in voiceInputs)
                {
                    voiceInput.BypassEffects = value;
                }
            }
        }

        private uint frequency;
        public uint Frequency
        {
            get
            {
                return frequency;
            }
            set
            {
                if (value != frequency)      //If there has been a change in frequency...
                {
                    foreach (var voiceInput in voiceInputs)
                    {
                        voiceInput.Clear();
                    }
                }
                frequency = value;
            }
        }

        public int ActiveCallsigns { get { return voiceInputs.Count(x => x.InUse); } }

        public float Volume { get { return volume.Volume; } set { volume.Volume = value; } }

        private bool mute;
        public bool Mute
        {
            get
            {
                return mute;
            }
            set
            {
                mute = value;
                if (value)
                {
                    foreach (var voiceInput in voiceInputs)
                    {
                        voiceInput.Clear();
                    }
                }
            }
        }

        private const float clickGain = 1.0f;
        private const double blockToneGain = 0.10f;

        public ushort ID { get; private set; }

        public event EventHandler<TransceiverReceivingCallsignsChangedEventArgs> ReceivingCallsignsChanged;

        private readonly VolumeSampleProvider volume;
        private readonly MixingSampleProvider mixer;
        private readonly BlockingToneSampleProvider blockTone;
        private readonly List<CallsignSampleProvider> voiceInputs;

        private bool doClickWhenAppropriate = false;
        private int lastNumberOfInUseInputs = 0;

        public ReceiverSampleProvider(WaveFormat waveFormat, ushort id, int voiceInputNumber)
        {
            WaveFormat = waveFormat;
            ID = id;

            mixer = new MixingSampleProvider(WaveFormat)
            {
                ReadFully = true
            };

            voiceInputs = new List<CallsignSampleProvider>();
            for (int i = 0; i < voiceInputNumber; i++)
            {
                var voiceInput = new CallsignSampleProvider(WaveFormat, this);
                voiceInputs.Add(voiceInput);
                mixer.AddMixerInput(voiceInput);
            };

            blockTone = new BlockingToneSampleProvider(WaveFormat.SampleRate, 1) { Gain = 0, Frequency = 180 };
            mixer.AddMixerInput(blockTone.ToMono());
            volume = new VolumeSampleProvider(mixer);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var numberOfInUseInputs = voiceInputs.Count(x => x.InUse);
            if (numberOfInUseInputs > 1)
            {
                blockTone.Frequency = 180;
                blockTone.Gain = blockToneGain;
            }
            else
            {
                blockTone.Gain = 0;
            }

            if (doClickWhenAppropriate && numberOfInUseInputs == 0)
            {
                mixer.AddMixerInput(new ResourceSoundSampleProvider(Samples.Instance.Click) { Gain = clickGain });
                doClickWhenAppropriate = false;
            }

            if (numberOfInUseInputs != lastNumberOfInUseInputs)
            {
                ReceivingCallsignsChanged?.Invoke(this, new TransceiverReceivingCallsignsChangedEventArgs(ID, voiceInputs.Select(x => x.Callsign).Where(x => !string.IsNullOrWhiteSpace(x)).ToList()));
            }
            lastNumberOfInUseInputs = numberOfInUseInputs;

            return volume.Read(buffer, offset, count);
        }

        public void AddOpusSamples(IAudioDto audioDto, uint frequency, float distanceRatio)
        {
            if (frequency != this.frequency)        //Lag in the backend means we get the tail end of a transmission if switching frequency in the middle of a transmission
                return;

            CallsignSampleProvider voiceInput = null;

            if (voiceInputs.Any(i => i.Callsign == audioDto.Callsign))
            {
                voiceInput = voiceInputs.First(b => b.Callsign == audioDto.Callsign);
            }
            else if (voiceInputs.Any(b => b.InUse == false))
            {
                voiceInput = voiceInputs.First(b => b.InUse == false);
                voiceInput.Active(audioDto.Callsign, "");
            }

            voiceInput?.AddOpusSamples(audioDto, distanceRatio);
            doClickWhenAppropriate = true;
        }

        public void AddSilentSamples(IAudioDto audioDto, uint frequency, float distanceRatio)
        {
            if (frequency != this.frequency)
                return;

            CallsignSampleProvider voiceInput = null;

            if (voiceInputs.Any(i => i.Callsign == audioDto.Callsign))
            {
                voiceInput = voiceInputs.First(b => b.Callsign == audioDto.Callsign);
            }
            else if (voiceInputs.Any(b => b.InUse == false))
            {
                voiceInput = voiceInputs.First(b => b.InUse == false);
                voiceInput.ActiveSilent(audioDto.Callsign, "");
            }

            voiceInput?.AddSilentSamples(audioDto);
            //doClickWhenAppropriate = true;
        }
    }
}
