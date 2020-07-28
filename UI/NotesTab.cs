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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using XPilot.PilotClient.Core.Events;

namespace XPilot.PilotClient
{
    public class NotesTab : TabPage
    {
        private readonly ChatBox mChatBox;
        private Color mMessageColor = Color.FromArgb(224, 224, 224);
        private Color mErrorColor = Color.Red;
        private Color mNotificationColor = Color.Yellow;

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

        public NotesTab()
        {
            BackColor = Color.FromArgb(20, 22, 24);
            ForeColor = Color.Silver;

            mChatBox = new ChatBox
            {
                Dock = DockStyle.Fill
            };
            Controls.Add(mChatBox);
            mChatBox.TextCommandLine.TextCommandReceived += NotesTab_TextCommandReceived;
        }

        private void NotesTab_TextCommandReceived(object sender, ClientEventArgs<string> e)
        {
            if (e.Value.ToLower().Equals(".close"))
            {
                Dispose();
            }
            else if (e.Value.ToLower().Equals(".clear"))
            {
                RichTextBox.Clear();
            }
            else if (e.Value.ToLower().Equals(".copy"))
            {
                if (!string.IsNullOrEmpty(RichTextBox.Text))
                {
                    Clipboard.SetText(RichTextBox.Text);
                    WriteMessage(mNotificationColor, "Messages have been copied to your clipboard.");
                }
            }
            else
            {
                WriteMessage(mMessageColor, e.Value);
            }
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
    }
}
