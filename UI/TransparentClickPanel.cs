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

namespace Vatsim.Xpilot
{
    public class TransparentClickPanel : Panel
    {
        private const int WM_NCHITTEST = 0x84;
        private const int HTTRANSPARENT = -1;

        public Color BorderColor { get; set; } = Color.FromArgb(92, 92, 92);

        public TransparentClickPanel()
        {
            base.SetStyle(ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    m.Result = (IntPtr)HTTRANSPARENT;
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Rectangle rect = new Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            using (Pen pen = new Pen(BorderColor))
            {
                pevent.Graphics.DrawRectangle(pen, rect);
            }
        }
    }
}
