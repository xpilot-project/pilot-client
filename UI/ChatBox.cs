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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using XPilot.PilotClient.Core.Events;

namespace XPilot.PilotClient
{
    public partial class ChatBox : UserControl
    {
        public ChatBox()
        {
            InitializeComponent();
        }

        public RichTextBox RichTextBox
        {
            get
            {
                return rtfMessages;
            }
        }

        public TextCommandLine TextCommandLine
        {
            get
            {
                return txtCommandLine;
            }
        }

        public void WriteMessage(Color color, string message)
        {
            bool firstMessage = rtfMessages.TextLength > 0;
            rtfMessages.SelectionStart = rtfMessages.TextLength;
            rtfMessages.SelectionColor = color;
            rtfMessages.SelectedText = string.Format("{0}[{1}] {2}", firstMessage ? "\r\n" : "", DateTime.Now.ToString("HH:mm:ss"), message);
            rtfMessages.SelectionFont = rtfMessages.Font;
            rtfMessages.SelectionStart = rtfMessages.TextLength;
            rtfMessages.ScrollToCaret();
        }

        public void WriteMessage(NotificationType type, string message, Color color)
        {
            if (!rtfMessages.IsDisposed && !rtfMessages.Disposing)
            {
                bool firstMessage = rtfMessages.TextLength > 0;
                rtfMessages.SelectionStart = rtfMessages.TextLength;
                rtfMessages.SelectionColor = color;
                if (firstMessage)
                {
                    rtfMessages.SelectedText = $"\r\n[{ DateTime.UtcNow:HH:mm:ss}] { message }";
                }
                else
                {
                    rtfMessages.SelectedText = $"[{ DateTime.UtcNow:HH:mm:ss}] { message }";
                }
                rtfMessages.SelectionFont = rtfMessages.Font;
                rtfMessages.SelectionStart = rtfMessages.TextLength;
                rtfMessages.ScrollToCaret();
            }
        }

        private void rtfMessages_MouseUp(object sender, MouseEventArgs e)
        {
            if (rtfMessages.SelectionLength > 0)
            {
                Clipboard.SetText(this.rtfMessages.SelectedText);
                rtfMessages.SelectionLength = 0;
            }
            txtCommandLine.Select();
        }

        private void rtfMessages_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            if (MessageBox.Show(this, "Open link in browser?", "Confirm Open Link", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                Process.Start(e.LinkText);
            }
        }
    }
}
