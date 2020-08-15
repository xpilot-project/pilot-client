using NAudio.Wave;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using GeoVR.Shared;
using NAudio.Wave.SampleProviders;
using NAudio.Utils;
using System.Net;
using System.Net.Sockets;
using RestSharp;
using Concentus.Structs;
using Concentus.Enums;
using System.Diagnostics;
using NLog;

namespace GeoVR.Client
{
    public class UserClient : BaseClient, IClient
    {
        Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private CancellationTokenSource playbackCancelTokenSource;
        private Task taskAudioPlayback;

        private Input input;
        private Output output;

        private SoundcardSampleProvider soundcardSampleProvider;
        private VolumeSampleProvider outputSampleProvider;

        public bool BypassEffects { set { if (soundcardSampleProvider != null) soundcardSampleProvider.BypassEffects = value; } }

        private float mCom1Volume = 1.0f;
        public float Com1Volume
        {
            get
            {
                return mCom1Volume;
            }
            set
            {
                if (soundcardSampleProvider != null)
                {
                    if (value > 18) { value = 18; }
                    if (value < -60) { value = -60; }
                    value = (float)Math.Pow(10, value / 20);
                    mCom1Volume = soundcardSampleProvider.Com1Volume = value;
                }
            }
        }

        private float mCom2Volume = 1.0f;
        public float Com2Volume
        {
            get
            {
                return mCom2Volume;
            }
            set
            {
                if (soundcardSampleProvider != null)
                {
                    if (value > 18) { value = 18; }
                    if (value < -60) { value = -60; }
                    value = (float)Math.Pow(10, value / 20);
                    mCom2Volume = soundcardSampleProvider.Com2Volume = value;
                }
            }
        }

        private bool transmit;
        private bool transmitHistory = false;
        private TxTransceiverDto[] transmittingTransceivers;

        public bool Started { get; private set; }
        public DateTime StartDateTimeUtc { get; private set; }

        private float inputVolumeDb;
        public float InputVolumeDb
        {
            set
            {
                if (value > 18) { value = 18; }
                if (value < -18) { value = -18; }
                inputVolumeDb = value;
                input.Volume = (float)System.Math.Pow(10, value / 20);
            }
            get
            {
                return inputVolumeDb;
            }
        }

        private double maxDbReadingInPTTInterval = -100;

        private CallRequestDto incomingCallRequest = null;

        public event EventHandler<InputVolumeStreamEventArgs> InputVolumeStream
        {
            add { input.InputVolumeStream += value; }
            remove { input.InputVolumeStream -= value; }
        }

        //public event EventHandler<VolumeStreamEventArgs> OutputVolumeStream
        //{

        //}

        public event EventHandler<CallRequestEventArgs> CallRequest;
        public event EventHandler<CallResponseEventArgs> CallResponse;

        private EventHandler<TransceiverReceivingCallsignsChangedEventArgs> eventHandler;

        public UserClient(string apiServer, EventHandler<TransceiverReceivingCallsignsChangedEventArgs> eventHandler) : base(apiServer)
        {
            this.eventHandler = eventHandler;
            input = new Input(sampleRate);
            input.OpusDataAvailable += Input_OpusDataAvailable;
            input.InputVolumeStream += Input_InputVolumeStream;
            output = new Output();
            logger.Debug(nameof(UserClient) + " instantiated");
        }

        private void Input_InputVolumeStream(object sender, InputVolumeStreamEventArgs e)
        {
            if (transmit)
            {
                //Gather AGC stats
                if (e.PeakDB > maxDbReadingInPTTInterval)
                    maxDbReadingInPTTInterval = e.PeakDB;
            }
        }

        private void Input_OpusDataAvailable(object sender, OpusDataAvailableEventArgs e)
        {
            if (transmittingTransceivers == null)
            {
                return;
            }

            if (transmittingTransceivers.Length > 0)
            {
                if (transmit)
                {
                    if (Connection.IsConnected)
                    {
                        Connection.VoiceServerTransmitQueue.Add(new AudioTxDto()
                        {
                            Callsign = Callsign,
                            SequenceCounter = e.SequenceCounter,
                            Audio = e.Audio,
                            LastPacket = false,
                            Transceivers = transmittingTransceivers
                        });

                        //Debug.WriteLine("Sending Audio:" + e.SequenceCounter);
                    }
                }
                if (!transmit && transmitHistory)
                {
                    if (Connection.IsConnected)
                    {
                        Connection.VoiceServerTransmitQueue.Add(new AudioTxDto()
                        {
                            Callsign = Callsign,
                            SequenceCounter = e.SequenceCounter,
                            Audio = e.Audio,
                            LastPacket = true,
                            Transceivers = transmittingTransceivers
                        });
                    }
                }
                transmitHistory = transmit;
            }
        }

        public void Start(string outputAudioDevice, List<ushort> transceiverIDs)
        {
            if (Started)
            {
                throw new Exception("Client already started");
            }

            soundcardSampleProvider = new SoundcardSampleProvider(sampleRate, transceiverIDs, eventHandler);
            outputSampleProvider = new VolumeSampleProvider(soundcardSampleProvider);

            output.Start(outputAudioDevice, outputSampleProvider);

            playbackCancelTokenSource = new CancellationTokenSource();
            taskAudioPlayback = new Task(() => TaskAudioPlayback(logger, playbackCancelTokenSource.Token, Connection.VoiceServerReceiveQueue), TaskCreationOptions.LongRunning);
            taskAudioPlayback.Start();

            StartDateTimeUtc = DateTime.UtcNow;

            Connection.ReceiveAudio = true;
            Started = true;
            logger.Debug("Started [Input: None] [Output: " + outputAudioDevice + "]");
        }

        public void Start(string inputAudioDevice, string outputAudioDevice, List<ushort> transceiverIDs)
        {
            if (Started)
                throw new Exception("Client already started");

            soundcardSampleProvider = new SoundcardSampleProvider(sampleRate, transceiverIDs, eventHandler);
            outputSampleProvider = new VolumeSampleProvider(soundcardSampleProvider);

            output.Start(outputAudioDevice, outputSampleProvider);

            playbackCancelTokenSource = new CancellationTokenSource();
            taskAudioPlayback = new Task(() => TaskAudioPlayback(logger, playbackCancelTokenSource.Token, Connection.VoiceServerReceiveQueue), TaskCreationOptions.LongRunning);
            taskAudioPlayback.Start();

            input.Start(inputAudioDevice);

            StartDateTimeUtc = DateTime.UtcNow;

            Connection.ReceiveAudio = true;
            Started = true;
            logger.Debug("Started [Input: " + inputAudioDevice + "] [Output: " + outputAudioDevice + "]");
        }

        public void Stop()
        {
            if (!Started)
                throw new Exception("Client not started");

            Started = false;
            Connection.ReceiveAudio = false;
            logger.Debug("Stopped");

            if (input.Started)
                input.Stop();

            playbackCancelTokenSource.Cancel();
            taskAudioPlayback.Wait();
            taskAudioPlayback = null;

            if (output.Started)
                output.Stop();

            while (Connection.VoiceServerReceiveQueue.TryTake(out _)) { }        //Clear the VoiceServerReceiveQueue.
        }

        public void TransmittingTransceivers(ushort transceiverID)
        {
            TransmittingTransceivers(new TxTransceiverDto[] { new TxTransceiverDto() { ID = transceiverID } });
        }

        public void TransmittingTransceivers(TxTransceiverDto[] transceivers)
        {
            transmittingTransceivers = transceivers;
        }

        public void PTT(bool active)
        {
            if (!Started)
                throw new Exception("Client not started");

            if (transmit == active)     //Ignore repeated keyboard events
                return;

            transmit = active;
            soundcardSampleProvider.PTT(active, transmittingTransceivers);

            if (!active)
            {
                //AGC
                //if (maxDbReadingInPTTInterval > -1)
                //    InputVolumeDb = InputVolumeDb - 1;
                //if(maxDbReadingInPTTInterval < -4)
                //    InputVolumeDb = InputVolumeDb + 1;
                maxDbReadingInPTTInterval = -100;
            }

            logger.Debug("PTT: " + active.ToString());
        }

        public void RequestCall(string callsign)
        {
            if (!Started)
                throw new Exception("Client not started");

            Connection.VoiceServerTransmitQueue.Add(new CallRequestDto() { FromCallsign = Callsign, ToCallsign = callsign });
            //logger.Debug("LandLineRing: " + active.ToString());
        }

        public void RejectCall()
        {
            if (incomingCallRequest != null)
            {
                soundcardSampleProvider.LandLineRing(false);
                Connection.VoiceServerTransmitQueue.Add(new CallResponseDto() { Request = incomingCallRequest, Event = CallResponseEvent.Reject });
                incomingCallRequest = null;
            }
        }

        public void AcceptCall()
        {
            if (incomingCallRequest != null)
            {
                soundcardSampleProvider.LandLineRing(false);
                Connection.VoiceServerTransmitQueue.Add(new CallResponseDto() { Request = incomingCallRequest, Event = CallResponseEvent.Accept });
                incomingCallRequest = null;
            }
        }

        public override void UpdateTransceivers(List<TransceiverDto> transceivers)
        {
            base.UpdateTransceivers(transceivers);
            if (soundcardSampleProvider != null)
                soundcardSampleProvider.UpdateTransceivers(transceivers);
        }

        private void TaskAudioPlayback(Logger logger, CancellationToken cancelToken, BlockingCollection<IMsgPackTypeName> queue)
        {
            while (!cancelToken.IsCancellationRequested)
            {
                if (queue.TryTake(out IMsgPackTypeName data, 500))
                {
                    switch (data.GetType().Name)
                    {
                        case nameof(AudioRxDto):
                            {
                                var dto = (AudioRxDto)data;
                                soundcardSampleProvider.AddOpusSamples(dto, dto.Transceivers);
                                if (logger.IsTraceEnabled)
                                    logger.Trace(dto.ToDebugString());
                                break;
                            }
                        case nameof(CallRequestDto):
                            {
                                var dto = (CallRequestDto)data;
                                if (incomingCallRequest != null)
                                {
                                    Connection.VoiceServerTransmitQueue.Add(new CallResponseDto() { Request = dto, Event = CallResponseEvent.Busy });
                                }
                                else
                                {
                                    incomingCallRequest = dto;
                                    soundcardSampleProvider.LandLineRing(true);
                                    CallRequest?.Invoke(this, new CallRequestEventArgs() { Callsign = dto.FromCallsign });
                                }
                                break;
                            }
                        case nameof(CallResponseDto):
                            {
                                var dto = (CallResponseDto)data;
                                switch (dto.Event)
                                {
                                    // Server events
                                    case CallResponseEvent.Routed:
                                        soundcardSampleProvider.LandLineRing(true);
                                        CallResponse?.Invoke(this, new CallResponseEventArgs() { Callsign = dto.Request.ToCallsign, Event = CallResponseEvent.Routed });        //Our request was routed
                                        break;
                                    case CallResponseEvent.NoRoute:
                                        CallResponse?.Invoke(this, new CallResponseEventArgs() { Callsign = dto.Request.ToCallsign, Event = CallResponseEvent.NoRoute });       //Our request was not routed
                                        break;
                                    //Remote party events
                                    case CallResponseEvent.Busy:
                                        soundcardSampleProvider.LandLineRing(false);
                                        //Play busy tone
                                        CallResponse?.Invoke(this, new CallResponseEventArgs() { Callsign = dto.Request.ToCallsign, Event = CallResponseEvent.Busy });
                                        break;
                                    case CallResponseEvent.Accept:
                                        soundcardSampleProvider.LandLineRing(false);
                                        CallResponse?.Invoke(this, new CallResponseEventArgs() { Callsign = dto.Request.ToCallsign, Event = CallResponseEvent.Accept });
                                        break;
                                    case CallResponseEvent.Reject:
                                        soundcardSampleProvider.LandLineRing(false);
                                        //Play reject tone
                                        CallResponse?.Invoke(this, new CallResponseEventArgs() { Callsign = dto.Request.ToCallsign, Event = CallResponseEvent.Reject });
                                        break;

                                }
                                break;
                            }

                    }
                }
            }
        }
    }
}
