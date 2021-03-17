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
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using Ninject;
using Vatsim.Xpilot.AudioForVatsim;
using Vatsim.Xpilot.Networking;
using Vatsim.Xpilot.Controllers;
using Vatsim.Xpilot.Simulator;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Aircrafts;
using Vatsim.Xpilot.Common;

namespace Vatsim.Xpilot
{
    static class Program
    {
        public static string ServerAddress;
        private static string AppPath;

        [STAThread]
        static void Main(string[] args)
        {
            if (!SingleInstance.Exists() || Debugger.IsAttached)
            {
                Application.CurrentCulture = new CultureInfo("en-US");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

                AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                if (!Directory.Exists(Path.Combine(AppPath, "NetworkLogs")))
                {
                    Directory.CreateDirectory(Path.Combine(AppPath, "NetworkLogs"));
                }

                if (args.Length > 0)
                {
                    ServerAddress = args[0];
                }

                IKernel kernel = new StandardKernel(new InjectionModules());

                var mainForm = kernel.Get<MainForm>();
                (kernel.Get<INetworkManager>() as IEventBus).Register();
                (kernel.Get<ISoundManager>() as IEventBus).Register();
                (kernel.Get<IAFVManaged>() as IEventBus).Register();
                (kernel.Get<IXplaneAdapter>() as IEventBus).Register();
                (kernel.Get<IVersionCheck>() as IEventBus).Register();
                (kernel.Get<IUserAircraftManager>() as IEventBus).Register();
                (kernel.Get<IAircraftManager>() as IEventBus).Register();
                (kernel.Get<IControllerManager>() as IEventBus).Register();
                (kernel.Get<IControllerAtisManager>() as IEventBus).Register();
                Application.Run(mainForm);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                FileStream fileStream = null;
                StreamWriter streamWriter = null;
                try
                {
                    fileStream = new FileStream(Path.Combine(AppPath, "GlobalExceptions.txt"), FileMode.Append, FileAccess.Write, FileShare.None, 1024, false);
                    streamWriter = new StreamWriter(fileStream);
                    streamWriter.WriteLine(Assembly.GetEntryAssembly().FullName);
                    streamWriter.WriteLine("============================================================");
                    streamWriter.WriteLine("Culture     : " + CultureInfo.CurrentCulture.Name);
                    streamWriter.WriteLine("OS          : " + Environment.OSVersion.ToString());
                    streamWriter.WriteLine("Framework   : " + Environment.Version);
                    streamWriter.WriteLine("Time        : " + DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
                    streamWriter.WriteLine("------------------------------------------------------------");
                    streamWriter.WriteLine("Details: " + ex.ToString());
                    streamWriter.WriteLine("============================================================");
                    streamWriter.Flush();
                    MessageBox.Show(null, "Unhandled exception: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
                }
                finally
                {
                    if (streamWriter != null)
                    {
                        streamWriter.Close();
                    }
                    if (fileStream != null)
                    {
                        fileStream.Close();
                    }
                }
            }
        }

        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            if (AFVBindings.IsClientInitialized())
            {
                AFVBindings.Destroy();
            }
        }
    }
}