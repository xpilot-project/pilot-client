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
using System.Drawing;
using System.Windows.Forms;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using XPilot.PilotClient.Core;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network;

namespace XPilot.PilotClient
{
    public class PrivateMessageTab : TabPage
    {
        [EventPublication(EventTopics.PrivateMessageSent)]
        public event EventHandler<PrivateMessageSentEventArgs> PrivateMessageSent;

        [EventPublication(EventTopics.PlaySoundRequested)]
        public event EventHandler<PlaySoundEventArgs> PlaySoundRequested;

        [EventPublication(EventTopics.RealNameRequested)]
        public event EventHandler<RealNameRequestedEventArgs> RequestRealName;

        private readonly ChatBox mChatBox;
        private readonly IFsdManger mNetworkManager;
        private readonly IEventBroker mEventBroker;
        private string tabIdentifier;
        private string realName = null;

        private TextCommandLine TextCommandLine
        {
            get
            {
                return mChatBox.TextCommandLine;
            }
        }

        private RichTextBox RichTextBox
        {
            get
            {
                return mChatBox.RichTextBox;
            }
        }

        public PrivateMessageTab(string tabName, string initialMessage, bool isOurMessage, string ourCallsign, IEventBroker eventBroker, IFsdManger networkManager)
        {
            tabIdentifier = tabName;
            Name = tabName;
            Text = tabName;

            BackColor = Color.FromArgb(20, 22, 24);
            ForeColor = Color.Cyan;

            mChatBox = new ChatBox
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(mChatBox);
            mChatBox.TextCommandLine.TextCommandReceived += PrivateMessageTab_TextCommandReceived;

            mNetworkManager = networkManager;
            mEventBroker = eventBroker;
            mEventBroker.Register(this);

            if (!string.IsNullOrEmpty(initialMessage))
            {
                if (isOurMessage)
                {
                    WriteMessage(Color.Cyan, $"{ ourCallsign }: { initialMessage }");
                }
                else
                {
                    WriteMessage(Color.White, $"{ tabIdentifier }: { initialMessage }");
                    PlaySoundRequested(this, new PlaySoundEventArgs(SoundEvent.NewMessage));
                    ForeColor = Color.Yellow;
                }
            }
            RequestRealName(this, new RealNameRequestedEventArgs(tabIdentifier));
        }

        private void PrivateMessageTab_TextCommandReceived(object sender, ClientEventArgs<string> e)
        {
            try
            {
                if (e.Value.StartsWith("."))
                {
                    switch (e.Value.ToLower())
                    {
                        case ".close":
                            Dispose();
                            break;
                        case ".clear":
                            RichTextBox.Clear();
                            break;
                        case ".copy":
                            if (!string.IsNullOrEmpty(RichTextBox.Text))
                            {
                                Clipboard.SetText(RichTextBox.Text);
                                WriteMessage(Color.Yellow, "Messages have been copied to your clipboard.");
                            }
                            break;
                        case ".showname":
                            RequestRealName(this, new RealNameRequestedEventArgs(tabIdentifier));
                            break;
                        case ".atis":
                            WriteMessage(Color.Yellow, $"Requesting ATIS for { tabIdentifier }");
                            if (!string.IsNullOrEmpty(realName))
                            {
                                mNetworkManager.RequestRealName(tabIdentifier);
                            }
                            mNetworkManager.RequestControllerInfo(tabIdentifier);
                            break;
                        default:
                            throw new ApplicationException($"Unknown text command: { e.Value.ToLower() }");
                    }
                }
                else
                {
                    PrivateMessageSent(this, new PrivateMessageSentEventArgs(mNetworkManager.OurCallsign, tabIdentifier, e.Value));
                }
            }
            catch (Exception ex)
            {
                WriteMessage(Color.Red, $"Error processing text command: { ex.Message }");
                PlaySoundRequested(this, new PlaySoundEventArgs(SoundEvent.Error));
            }
        }

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnUserInterfaceAsync))]
        public void OnRealNameReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (e.From == tabIdentifier)
            {
                WriteName(e.Data);
            }
        }

        [EventSubscription(EventTopics.PrivateMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (e.From == tabIdentifier)
            {
                WriteIncomingMessage(e.Data);
                PlaySoundRequested(this, new PlaySoundEventArgs(SoundEvent.PrivateMessage));
                if ((Parent as TabControl).SelectedTab != this)
                {
                    ForeColor = Color.Yellow;
                    (Parent as TabControl).Refresh();
                }
            }
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSentEventArgs e)
        {
            if (tabIdentifier == e.To)
            {
                WriteOutgoingMessage(mNetworkManager.OurCallsign, e.Message);
            }
        }

        [EventSubscription(EventTopics.RequestedAtisReceived, typeof(OnUserInterfaceAsync))]
        public void OnRequestedAtisReceived(object sender, RequestedAtisReceivedEventArgs e)
        {
            if (e.From == tabIdentifier)
            {
                foreach (var line in e.Lines)
                {
                    WriteIncomingMessage(line);
                }
            }
        }

        public void WriteOutgoingMessage(string ourCallsign, string msg)
        {
            WriteMessage(Color.Cyan, $"{ ourCallsign }: { msg }");
        }

        public void WriteIncomingMessage(string msg)
        {
            WriteMessage(Color.White, $"{ tabIdentifier }: { msg }");
        }

        private void WriteName(string name)
        {
            if (!string.IsNullOrEmpty(realName) && realName != name)
            {
                WriteMessage(Color.Yellow, $"Name: { name }");
            }
            else if (string.IsNullOrEmpty(realName))
            {
                bool firstMessage = RichTextBox.TextLength > 0;
                RichTextBox.SelectionStart = 0;
                RichTextBox.SelectionLength = 0;
                RichTextBox.SelectionColor = Color.Yellow;
                RichTextBox.SelectedText = string.Format("[{0}] Name: {1}{2}", DateTime.Now.ToString("HH:mm:ss"), name, firstMessage ? "\r\n" : "");
                RichTextBox.SelectionFont = RichTextBox.Font;
                RichTextBox.SelectionStart = RichTextBox.TextLength;
                RichTextBox.ScrollToCaret();
            }
            realName = name;
        }

        private void WriteMessage(Color color, string text)
        {
            if (!mChatBox.RichTextBox.IsDisposed && !mChatBox.RichTextBox.Disposing)
            {
                bool firstMessage = RichTextBox.TextLength > 0;
                RichTextBox.SelectionStart = RichTextBox.TextLength;
                RichTextBox.SelectionColor = color;
                RichTextBox.SelectedText = string.Format("{0}[{1}] {2}", firstMessage ? "\r\n" : "", DateTime.Now.ToString("HH:mm:ss"), text);
                RichTextBox.SelectionFont = RichTextBox.Font;
                RichTextBox.SelectionStart = RichTextBox.TextLength;
                RichTextBox.SelectionColor = Color.FromArgb(230, 230, 230);
                RichTextBox.ScrollToCaret();
            }
        }

        public void FocusTextCommandLine()
        {
            TextCommandLine.Focus();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                mEventBroker.Unregister(this);
            }
            base.Dispose(disposing);
        }
    }
}
