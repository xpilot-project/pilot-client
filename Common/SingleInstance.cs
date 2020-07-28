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
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace XPilot.PilotClient.Common
{
    public class SingleInstance
    {
        private static Mutex mutex;

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern int SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern int IsIconic(IntPtr hWnd);

        private static IntPtr GetCurrentInstanceWindowHandle()
        {
            IntPtr result = IntPtr.Zero;
            Process currentProcess = Process.GetCurrentProcess();
            Process[] processesByName = Process.GetProcessesByName(currentProcess.ProcessName);
            Process[] array = processesByName;
            for (int i = 0; i < array.Length; i++)
            {
                Process process = array[i];
                bool flag = process.Id != currentProcess.Id && process.MainModule.FileName == currentProcess.MainModule.FileName && process.MainWindowHandle != IntPtr.Zero;
                if (flag)
                {
                    result = process.MainWindowHandle;
                    break;
                }
            }
            return result;
        }

        private static void SwitchToCurrentInstance()
        {
            IntPtr currentInstanceWindowHandle = GetCurrentInstanceWindowHandle();
            bool flag = currentInstanceWindowHandle != IntPtr.Zero;
            if (flag)
            {
                bool flag2 = IsIconic(currentInstanceWindowHandle) != 0;
                if (flag2)
                {
                    ShowWindow(currentInstanceWindowHandle, 9);
                }
                SetForegroundWindow(currentInstanceWindowHandle);
            }
        }

        public static bool Exists()
        {
            bool flag = IsAlreadyRunning();
            bool result;
            if (flag)
            {
                SwitchToCurrentInstance();
                result = true;
            }
            else
            {
                result = false;
            }
            return result;
        }

        private static bool IsAlreadyRunning()
        {
            string location = Assembly.GetExecutingAssembly().Location;
            FileSystemInfo fileSystemInfo = new FileInfo(location);
            string name = fileSystemInfo.Name;
            mutex = new Mutex(true, "Global\\" + name, out bool flag);
            if (flag)
            {
                mutex.ReleaseMutex();
            }
            return !flag;
        }
    }
}
