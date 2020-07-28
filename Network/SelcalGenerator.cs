/*
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
using System;
using System.Collections.Generic;
using System.Linq;
using XPilot.PilotClient.Core.Events;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace XPilot.PilotClient.Network
{
    public class SelcalGenerator : EventBus, ISelcalGenerator
    {
        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        private readonly Dictionary<string, float> mSelcalToneFrequencies = new Dictionary<string, float>();
        private readonly char[] mProhibitedCharacters = new char[] { 'I', 'N', 'O' };
        private readonly IFsdManger mFsdManager;
        private WaveOut mWaveOut;

        public SelcalGenerator(IEventBroker broker, IFsdManger fsdManager) : base(broker)
        {
            mFsdManager = fsdManager;

            mSelcalToneFrequencies = new Dictionary<string, float>()
            {
                { "A", 312.6f },
                { "B", 346.7f },
                { "C", 384.6f },
                { "D", 426.6f },
                { "E", 473.2f },
                { "F", 524.8f },
                { "G", 582.1f },
                { "H", 645.7f },
                { "J", 716.1f },
                { "K", 794.3f },
                { "L", 881.0f },
                { "M", 977.2f },
                { "P", 1083.9f },
                { "Q", 1202.3f },
                { "R", 1333.5f },
                { "S", 1479.1f },
            };
        }

        public bool ValidateSelcal(string code)
        {
            try
            {
                code = code.Replace("-", "");
                code = code.ToUpper();
                char[] chars = code.ToCharArray();

                for (int i = 1; i < chars.Count(); i += 2)
                {
                    if (chars[i] < chars[i - 1])
                    {
                        return false;
                    }
                    if (chars[i] > 'S')
                    {
                        return false;
                    }
                    if (chars[i - 1] > 'R')
                    {
                        return false;
                    }
                    if (mProhibitedCharacters.Contains(chars[i]))
                    {
                        return false;
                    }
                }

                if (code.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => new { name = x.Key, count = x.Count() }).ToArray().Count() > 0)
                {
                    return false;
                }

            }
            catch { }

            return true;
        }

        public void PlaySelcal(string code)
        {
            try
            {
                if (ValidateSelcal(code))
                {
                    code = code.Replace("-", "");
                    code = code.ToUpper();
                    char[] chars = code.ToCharArray();
                    float[] toneFrequencies = new float[4];

                    for (int i = 0; i < chars.Count(); i++)
                    {
                        toneFrequencies[i] = mSelcalToneFrequencies[chars[i].ToString()];
                    }

                    if (toneFrequencies.Count() > 0 && toneFrequencies.Count() < 5)
                    {
                        PlayTonePairs(toneFrequencies);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        private void PlayTonePairs(float[] frequencies)
        {
            float gain = 0.25f;

            try
            {
                ISampleProvider provider1 = new SignalGenerator
                {
                    Frequency = frequencies[0],
                    Gain = gain
                };

                ISampleProvider provider2 = new SignalGenerator
                {
                    Frequency = frequencies[1],
                    Gain = gain
                };

                ISampleProvider provider3 = new SignalGenerator
                {
                    Frequency = frequencies[2],
                    Gain = gain
                };

                ISampleProvider provider4 = new SignalGenerator
                {
                    Frequency = frequencies[3],
                    Gain = gain
                };

                ISampleProvider pause = new SilenceProvider(provider2.WaveFormat).ToSampleProvider().Take(TimeSpan.FromMilliseconds(200));

                var duration = TimeSpan.FromMilliseconds(1000);

                var group1 = new[] { provider1.Take(duration), provider2.Take(duration) };
                var group2 = new[] { provider3.Take(duration), provider4.Take(duration) };

                var mixer1 = new MixingSampleProvider(group1);
                var mixer2 = new MixingSampleProvider(group2);

                mWaveOut = new WaveOut();
                mWaveOut.Init(mixer1.FollowedBy(TimeSpan.FromMilliseconds(200), mixer2));
                mWaveOut.Play();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        [EventSubscription(EventTopics.PlaySelcalRequested, typeof(OnUserInterfaceAsync))]
        public void OnPlaySelcalRequested(object sender, EventArgs e)
        {
            try
            {
                PlaySelcal(mFsdManager.SelcalCode);
            }
            catch (Exception ex)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, $"Error playing SELCAL tone: {ex.Message}"));
            }
        }
    }
}
