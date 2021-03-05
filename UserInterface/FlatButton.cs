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
	public class FlatButton : Control
	{
		private bool mPushed = false;
		private bool mClicked = false;
		private Color mPushedColor = Color.FromArgb(0, 120, 206);
		private Color mClickedColor = Color.FromArgb(0, 120, 206);
		private Color mBorderColor = Color.FromArgb(100, 100, 100);
		private Color mDisabledTextColor = Color.FromArgb(100, 100, 100);

		public FlatButton()
		{
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
		}

		public bool Pushed
		{
			get { return mPushed; }
			set { mPushed = value; Invalidate(); }
		}

		public Color PushedColor
		{
			get { return mPushedColor; }
			set { mPushedColor = value; Invalidate(); }
		}

		public bool Clicked
		{
			get { return mClicked; }
			set { mClicked = value; Invalidate(); }
		}

		public Color ClickedColor
		{
			get { return mClickedColor; }
			set { mClickedColor = value; Invalidate(); }
		}

		public Color BorderColor
		{
			get { return mBorderColor; }
			set { mBorderColor = value; Invalidate(); }
		}

		public Color DisabledTextColor
		{
			get { return mDisabledTextColor; }
			set { mDisabledTextColor = value; Invalidate(); }
		}

		protected override void OnDoubleClick(EventArgs e)
		{
			base.OnClick(e);
		}

        protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				mPushed = true;
				Focus();
			}
			base.OnMouseDown(e);
			Invalidate();
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				mPushed = false;
			}
			base.OnMouseUp(e);
			Invalidate();
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			Invalidate();
		}

		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
			Invalidate();
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			using (Brush backgroundBrush = new SolidBrush(mPushed ? PushedColor : (mClicked ? mClickedColor : BackColor)))
			{
				e.Graphics.FillRectangle(backgroundBrush, 0, 0, ClientSize.Width, ClientSize.Height);
			}

			Rectangle borderRect = new Rectangle(0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
			using (Pen borderPen = new Pen(BorderColor))
			{
				e.Graphics.DrawRectangle(borderPen, borderRect);
			}

			if (Text != "")
			{
				using (Brush textBrush = new SolidBrush(Enabled ? ForeColor : DisabledTextColor))
				{
					StringFormat fmt = new StringFormat
					{
						Alignment = StringAlignment.Center,
						LineAlignment = StringAlignment.Center
					};
					e.Graphics.DrawString(Text, Font, textBrush, ClientRectangle, fmt);
					fmt.Dispose();
				}
			}
		}
	}
}