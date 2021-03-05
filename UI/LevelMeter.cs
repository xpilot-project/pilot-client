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
    public class LevelMeter : Control
    {
        private const int PEAK_HOLD_TIME = 1000;

        private readonly Color mLowRangeColor = Color.RoyalBlue;
        private readonly Color mLowRangeColorDark = ControlPaint.Dark(Color.RoyalBlue, 0.4f);
        private readonly Color mMidRangeColor = Color.LimeGreen;
        private readonly Color mMidRangeColorDark = ControlPaint.Dark(Color.LimeGreen, 0.4f);
        private readonly Color mHighRangeColor = Color.OrangeRed;
        private readonly Color mHighRangeColorDark = ControlPaint.Dark(Color.OrangeRed, 0.4f);
        private readonly float mMidRangeStart = 0.70f;
        private readonly float mHighRangeStart = 0.9f;
        private readonly Timer mPeakHoldTimer;
        private float mValue;
        private float mPeak;

        public float Value
        {
            get => mValue;
            set
            {
                if (value < 0.0f)
                {
                    mValue = 0.0f;
                }
                else if (value > 1.0f)
                {
                    mValue = 1.0f;
                }
                else
                {
                    mValue = value;
                }
                if (mValue > mPeak)
                {
                    mPeak = mValue;
                    mPeakHoldTimer.Start();
                }
                Invalidate();
            }
        }

        public LevelMeter()
        {
            mPeakHoldTimer = new Timer
            {
                Interval = PEAK_HOLD_TIME,
            };
            mPeakHoldTimer.Tick += PeakHoldTimer_Tick;
            mPeakHoldTimer.Enabled = true;
        }

        private void PeakHoldTimer_Tick(object sender, EventArgs e)
        {
            mPeak = 0.0f;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            DrawBackground(e.Graphics);

            if (Value > 0.0f)
            {
                DrawLevel(e.Graphics);
            }

            if (mPeak > 0.0f)
            {
                DrawPeak(e.Graphics);
            }
        }

        private void DrawBackground(Graphics g)
        {
            DrawRange(0.0f, mMidRangeStart, g, mLowRangeColorDark);
            DrawRange(mMidRangeStart, mHighRangeStart, g, mMidRangeColorDark);
            DrawRange(mHighRangeStart, 1.0f, g, mHighRangeColorDark);
        }

        private void DrawLevel(Graphics g)
        {
            float lowRangeEnd = Math.Min(mMidRangeStart, Value);
            DrawRange(0.0f, lowRangeEnd, g, mLowRangeColor);

            if (Value >= mMidRangeStart)
            {
                float midRangeEnd = Math.Min(mHighRangeStart, Value);
                DrawRange(mMidRangeStart, midRangeEnd, g, mMidRangeColor);
            }

            if (Value >= mHighRangeStart)
            {
                DrawRange(mHighRangeStart, Value, g, mHighRangeColor);
            }
        }

        private void DrawRange(float start, float end, Graphics g, Color color)
        {
            int startPixel = Math.Max(1, (int)(ClientRectangle.Width * start));
            int endPixel = Math.Min(ClientRectangle.Width - 1, (int)(ClientRectangle.Width * end));
            Rectangle rect = new Rectangle(startPixel, 1, endPixel - startPixel, ClientRectangle.Height - 2);
            using (SolidBrush brush = new SolidBrush(color))
            {
                g.FillRectangle(brush, rect);
            }
        }

        private void DrawPeak(Graphics g)
        {
            Color color = mLowRangeColor;
            if (mPeak >= mHighRangeStart)
            {
                color = mHighRangeColor;
            }
            else if (mPeak >= mMidRangeStart)
            {
                color = mMidRangeColor;
            }
            int pixel = Math.Max(2, (int)(ClientRectangle.Width * mPeak));
            if (pixel > ClientRectangle.Width)
            {
                pixel = ClientRectangle.Width - 1;
            }
            using (Pen pen = new Pen(color))
            {
                g.DrawLine(pen, pixel - 3, 1, pixel - 3, ClientRectangle.Height - 2);
                g.DrawLine(pen, pixel - 2, 1, pixel - 2, ClientRectangle.Height - 2);
                g.DrawLine(pen, pixel - 1, 1, pixel - 1, ClientRectangle.Height - 2);
            }
        }
    }
}
