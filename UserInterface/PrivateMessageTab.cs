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
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Events.Arguments;
using Vatsim.Xpilot.Networking;

namespace Vatsim.Xpilot
{
    public class PrivateMessageTab : TabPage
    {
        [EventPublication(EventTopics.PrivateMessageSent)]
        public event EventHandler<PrivateMessageSentEventArgs> PrivateMessageSent;

        [EventPublication(EventTopics.PlayNotificationSound)]
        public event EventHandler<PlayNotifictionSoundEventArgs> PlayNotificationSound;

        [EventPublication(EventTopics.RealNameRequested)]
        public event EventHandler<RealNameRequestedEventArgs> RealNameRequested;

        private readonly MessageConsoleControl mChatBox;
        private readonly INetworkManager mNetworkManager;
        private readonly IEventBroker mEventBroker;
        private string tabName;
        private string realName;

        private TextCommandLine TextCommandLine => mChatBox.TextCommandLine;
        private RichTextBox RichTextBox => mChatBox.RichTextBox;

        public PrivateMessageTab(string tabName, string initialMessage, bool isOurMessage, string ourCallsign, IEventBroker eventBroker, INetworkManager networkManager)
        {
            this.tabName = tabName;
            Name = tabName;
            Text = tabName;

            BackColor = Color.FromArgb(20, 22, 24);
            ForeColor = Color.Cyan;

            mChatBox = new MessageConsoleControl
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(mChatBox);
            mChatBox.TextCommandLine.TextCommandReceived += TextCommandLine_TextCommandReceived;

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
                    WriteMessage(Color.White, $"{ this.tabName }: { initialMessage }");
                    PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.NewMessage));
                    ForeColor = Color.Yellow;
                }
            }
            RealNameRequested?.Invoke(this, new RealNameRequestedEventArgs(tabName));
        }

        private void TextCommandLine_TextCommandReceived(object sender, TextCommandReceivedEventArgs e)
        {
            try
            {
                if (e.Command.StartsWith("."))
                {
                    switch (e.Command.ToLower())
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
                            RealNameRequested?.Invoke(this, new RealNameRequestedEventArgs(tabName));
                            break;
                        case ".atis":
                            WriteMessage(Color.Yellow, $"Requesting ATIS for { tabName }");
                            if (!string.IsNullOrEmpty(realName))
                            {
                                mNetworkManager.SendRealNameRequest(tabName);
                            }
                            mNetworkManager.SendControllerInfoRequest(tabName);
                            break;
                        default:
                            throw new ApplicationException($"Unknown text command: { e.Command.ToLower() }");
                    }
                }
                else
                {
                    PrivateMessageSent?.Invoke(this, new PrivateMessageSentEventArgs(mNetworkManager.OurCallsign, tabName, e.Command));
                }
            }
            catch (Exception ex)
            {
                WriteMessage(Color.Red, $"Error processing text command: { ex.Message }");
                PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Error));
            }
        }

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnUserInterfaceAsync))]
        public void OnRealNameReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (e.From == tabName)
            {
                WriteName(e.Data);
            }
        }

        [EventSubscription(EventTopics.PrivateMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            if (e.From == tabName)
            {
                WriteIncomingMessage(e.Data);
                PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.PrivateMessage));
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
            if (tabName == e.To)
            {
                WriteOutgoingMessage(mNetworkManager.OurCallsign, e.Message);
            }
        }

        public void WriteOutgoingMessage(string ourCallsign, string msg)
        {
            WriteMessage(Color.Cyan, $"{ ourCallsign }: { msg }");
        }

        public void WriteIncomingMessage(string msg)
        {
            WriteMessage(Color.White, $"{ tabName }: { msg }");
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