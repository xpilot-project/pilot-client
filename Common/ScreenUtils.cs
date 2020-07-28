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

namespace XPilot.PilotClient.Common
{
    public static class ScreenUtils
    {
        public static void EnsureOnScreen(Form form)
        {
            Point location = form.Location;
            location.Offset(10, 10);
            Screen[] allScreens = Screen.AllScreens;
            for (int i = 0; i < allScreens.Length; i++)
            {
                Screen screen = allScreens[i];
                bool flag = screen.WorkingArea.Contains(location);
                if (flag)
                {
                    return;
                }
            }
            Screen screen2 = Screen.FromPoint(form.Location);
            form.Location = new Point(screen2.WorkingArea.Left, screen2.WorkingArea.Top);
        }

        public static void ApplyWindowProperties(WindowProperties properties, Form form)
        {
            form.ClientSize = properties.Size;
            form.Location = properties.Location;
            if (properties.Maximized)
            {
                form.WindowState = FormWindowState.Maximized;
            }
            EnsureOnScreen(form);
        }

        public static void SaveWindowProperties(WindowProperties properties, Form form)
        {
            FormWindowState windowState = form.WindowState;
            if (windowState != FormWindowState.Normal)
            {
                if (windowState == FormWindowState.Maximized)
                {
                    properties.Maximized = true;
                }
            }
            else
            {
                properties.Size = form.ClientSize;
                properties.Location = form.Location;
                properties.Maximized = false;
            }
        }

        public static void SetVisibleState(WindowProperties properties, Form form)
        {
            bool minimized = properties.Minimized;
            if (minimized)
            {
                form.WindowState = FormWindowState.Normal;
            }
            else
            {
                form.WindowState = FormWindowState.Minimized;
            }
        }

        public static void ToggleVisibility(WindowProperties properties, Form form)
        {
            properties.Minimized = !properties.Minimized;
            ScreenUtils.SetVisibleState(properties, form);
        }
    }
}
