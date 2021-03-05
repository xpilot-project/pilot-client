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

namespace Vatsim.Xpilot
{
    public partial class FlatCheckbox : Control
    {
        public delegate void CheckedChangedEventHandler(object sender);
        public event CheckedChangedEventHandler CheckedChanged;

        private bool isHover = false;
        public Color CheckedColor { get; set; } = Color.FromArgb(0, 120, 206);
        public Color BorderColor { get; set; } = Color.FromArgb(60, 60, 60);

        public FlatCheckbox()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }

        private bool mChecked;
        public bool Checked
        {
            get { return mChecked; }
            set
            {
                mChecked = value;
                if (CheckedChanged != null)
                {
                    CheckedChanged(this);
                }
                Invalidate();
            }
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            OnClick(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            base.Invalidate();
        }

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            base.Invalidate();
        }

        protected override void OnMouseHover(EventArgs e)
        {
            isHover = true;
            Invalidate();
            base.OnMouseHover(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            isHover = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            isHover = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // paint background
            using (Brush brush = new SolidBrush(isHover && !Checked ? Color.FromArgb(0, 120, 206) : Checked ? CheckedColor : BackColor))
            {
                e.Graphics.FillRectangle(brush, 0, 0, ClientSize.Width, ClientSize.Height);
            }

            // draw border
            using (Pen pen = new Pen(BorderColor))
            {
                Rectangle clientRectangle = base.ClientRectangle;
                int num = clientRectangle.Width;
                clientRectangle.Width = num - 1;
                num = clientRectangle.Height;
                clientRectangle.Height = num - 1;
                e.Graphics.DrawRectangle(pen, clientRectangle);
            }
            // draw label
            if (Text != "")
            {
                using (Brush brush = new SolidBrush(Checked ? Color.White : ForeColor))
                {
                    Font font = new Font(this.Font.Name, this.Font.Size, FontStyle.Regular);
                    SizeF sizeF = e.Graphics.MeasureString(this.Text, font);
                    int num2 = (int)sizeF.Width / base.ClientRectangle.Width;
                    num2 += ((sizeF.Width % (float)base.ClientRectangle.Width != 0f) ? 1 : 0);
                    Rectangle r = new Rectangle(0, base.ClientRectangle.Height, base.ClientRectangle.Width, num2 * (int)sizeF.Height);
                    r.Y = (base.ClientRectangle.Height - r.Height) / 2;
                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    e.Graphics.DrawString(this.Text, font, brush, r, stringFormat);
                    stringFormat.Dispose();
                    font.Dispose();
                }
            }
        }
    }
}
