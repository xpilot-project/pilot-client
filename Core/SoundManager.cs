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
using System.IO;
using System.Media;
using Vatsim.Xpilot.Config;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Xpilot.Events.Arguments;

namespace Vatsim.Xpilot.Core
{
    public enum SoundEvent
    {
        None,
        Alert,
        Broadcast,
        Buzzer,
        DirectRadioMessage,
        Error,
        NewMessage,
        PrivateMessage,
        RadioMessage,
        SelCal
    }

    public class SoundManager : EventBus, ISoundManager, IDisposable
    {
        private readonly IAppConfig mConfig;
        private Dictionary<SoundEvent, SoundPlayer> mSounds;

        public SoundManager(IEventBroker broker, IAppConfig config) : base(broker)
        {
            mConfig = config;
            LoadSoundFiles();
        }

        private void LoadSoundFiles()
        {
            mSounds = new Dictionary<SoundEvent, SoundPlayer>();
            foreach (SoundEvent sound in Enum.GetValues(typeof(SoundEvent)))
            {
                if (sound != SoundEvent.None)
                {
                    string path = Path.Combine(mConfig.AppPath, Path.Combine("Sounds", $"{sound}.wav"));
                    if (File.Exists(path))
                    {
                        mSounds.Add(sound, new SoundPlayer(path));
                        try
                        {
                            mSounds[sound].Load();
                        }
                        catch { }
                    }
                }
            }
        }

        private void Play(SoundEvent sound)
        {
            if (sound != SoundEvent.None)
            {
                try
                {
                    mSounds[sound].Play();
                }
                catch { }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (SoundPlayer player in mSounds.Values)
            {
                player.Dispose();
            }
        }

        [EventSubscription(EventTopics.PlayNotificationSound, typeof(OnUserInterfaceAsync))]
        public void OnPlaySoundRequested(object sender, PlayNotifictionSoundEventArgs e)
        {
            Play(e.Sound);
        }
    }
}
