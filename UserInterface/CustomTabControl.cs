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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Vatsim.Xpilot
{
    public class CustomTabControl : TabControl
    {
        [Description("Background Color")]
        public Color BackgroundColor { get; set; } = Color.Transparent;

        [Description("Border Color")]
        public Color BorderColor { get; set; } = SystemColors.ControlDark;

        public CustomTabControl()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.DoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle TabControlArea = ClientRectangle;
            Rectangle TabArea = DisplayRectangle;

            using (SolidBrush brush = new SolidBrush(BackgroundColor))
            {
                e.Graphics.FillRectangle(brush, TabControlArea);
            }

            int borderWidth = SystemInformation.Border3DSize.Width;
            TabArea.Inflate(borderWidth, borderWidth);
            using (Pen pen = new Pen(BorderColor))
            {
                e.Graphics.DrawRectangle(pen, TabArea);
            }

            for (int i = 0; i < TabCount; i++)
            {
                DrawTab(e.Graphics, TabPages[i], i);
            }
        }

        private void DrawTab(Graphics g, TabPage tabPage, int index)
        {
            Pen borderPen = new Pen(BorderColor);
            Rectangle tabRect = GetTabRect(index);

            bool mSelected = SelectedIndex == index;
            Point[] pt = new Point[7];

            if (Alignment == TabAlignment.Top)
            {
                pt[0] = new Point(tabRect.Left, tabRect.Bottom);
                pt[1] = new Point(tabRect.Left, tabRect.Top + 3);
                pt[2] = new Point(tabRect.Left + 3, tabRect.Top);
                pt[3] = new Point(tabRect.Right - 3, tabRect.Top);
                pt[4] = new Point(tabRect.Right, tabRect.Top + 3);
                pt[5] = new Point(tabRect.Right, tabRect.Bottom);
                pt[6] = new Point(tabRect.Left, tabRect.Bottom);
            }
            else
            {
                pt[0] = new Point(tabRect.Left, tabRect.Top);
                pt[1] = new Point(tabRect.Right, tabRect.Top);
                pt[2] = new Point(tabRect.Right, tabRect.Bottom - 3);
                pt[3] = new Point(tabRect.Right - 3, tabRect.Bottom);
                pt[4] = new Point(tabRect.Left + 3, tabRect.Bottom);
                pt[5] = new Point(tabRect.Left, tabRect.Bottom - 3);
                pt[6] = new Point(tabRect.Left, tabRect.Top);
            }

            using (SolidBrush brush = new SolidBrush(tabPage.BackColor))
            {
                g.FillPolygon(brush, pt);
            }

            g.DrawPolygon(borderPen, pt);

            if (mSelected)
            {
                Pen pen = new Pen(tabPage.BackColor);
                switch (Alignment)
                {
                    case TabAlignment.Top:
                        g.DrawLine(pen, tabRect.Left + 1, tabRect.Bottom, tabRect.Right - 1, tabRect.Bottom);
                        g.DrawLine(pen, tabRect.Left + 1, tabRect.Bottom + 1, tabRect.Right - 1, tabRect.Bottom + 1);
                        break;
                    case TabAlignment.Bottom:
                        g.DrawLine(pen, tabRect.Left + 1, tabRect.Top, tabRect.Right - 1, tabRect.Top);
                        g.DrawLine(pen, tabRect.Left + 1, tabRect.Top - 1, tabRect.Right - 1, tabRect.Top - 1);
                        g.DrawLine(pen, tabRect.Left + 1, tabRect.Top - 2, tabRect.Right - 1, tabRect.Top - 2);
                        break;
                }
            }

            bool isPrivateMessageTab = TabPages[index] is PrivateMessageTab;
            int num = ((tabRect.Height - 3 - 11) / 2) + 2;
            if (isPrivateMessageTab)
            {
                Rectangle square = new Rectangle(tabRect.Right - num - 11, tabRect.Bottom - num - 11, 11, 11);
                g.DrawRectangle(borderPen, square);
                g.DrawLine(borderPen, tabRect.Right - num - 11 + 3, tabRect.Bottom - num - 11 + 3, tabRect.Right - num - 3, tabRect.Bottom - num - 3);
                g.DrawLine(borderPen, tabRect.Right - num - 11 + 3, tabRect.Bottom - num - 3, tabRect.Right - num - 3, tabRect.Bottom - num - 11 + 3);
            }

            StringFormat stringFormat = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };
            using (SolidBrush brush = new SolidBrush(TabPages[index].ForeColor))
            {
                RectangleF layoutRect = isPrivateMessageTab ? new RectangleF(tabRect.X, tabRect.Y, tabRect.Width - num - 11, tabRect.Height) : tabRect;
                g.DrawString(tabPage.Text, Font, brush, layoutRect, stringFormat);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            bool isPrivateMessageTab = SelectedTab is PrivateMessageTab;
            if (SelectedIndex != 0 && !DesignMode && isPrivateMessageTab)
            {
                Rectangle tabRect = GetTabRect(SelectedIndex);
                int num = (tabRect.Height - 3 - 11) / 2 + 2;
                if (new Rectangle(tabRect.Right - num - 11, tabRect.Bottom - num - 11, 11, 11).Contains(e.Location))
                {
                    SelectedTab.Dispose();
                }
            }
        }
    }
}
