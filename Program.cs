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
using Ninject;
using System;
using System.Globalization;
using System.IO;
using System.Management.Instrumentation;
using System.Reflection;
using System.Windows.Forms;
using XPilot.PilotClient.AudioForVatsim;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.Network.Aircraft;
using XPilot.PilotClient.Network.Controllers;
using XPilot.PilotClient.XplaneAdapter;

namespace XPilot.PilotClient
{
    static class Program
    {
        private static string AppPath;

        [STAThread]
        static void Main()
        {
            if (!SingleInstance.Exists())
            {
                Application.CurrentCulture = new CultureInfo("en-US");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

                AppPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string configFilePath = Path.Combine(AppPath, "AppConfig.json");

                IKernel kernel = new StandardKernel(new InjectionModules());
                IAppConfig config = kernel.Get<IAppConfig>();

                try
                {
                    config.LoadConfig(configFilePath);
                }
                catch (FileNotFoundException)
                {
                    config.SaveConfig();
                }
                catch (Exception ex)
                {
                    config.SaveConfig();
                    MessageBox.Show("Error loading configuration file. The configuration file has become corrupt and will be reset to the default settings.", "Error Loading Configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    config.AppPath = AppPath;
                }

                var mainForm = kernel.Get<MainForm>();
                (kernel.Get<IFsdManger>() as IEventBus).Register();
                (kernel.Get<ISoundManager>() as IEventBus).Register();
                (kernel.Get<IAfvManager>() as IEventBus).Register();
                (kernel.Get<IXplaneConnectionManager>() as IEventBus).Register();
                (kernel.Get<IVersionCheck>() as IEventBus).Register();
                (kernel.Get<IUserAircraftManager>() as IEventBus).Register();
                (kernel.Get<ISelcalGenerator>() as IEventBus).Register();
                (kernel.Get<INetworkAircraftManager>() as IEventBus).Register();
                (kernel.Get<IPttManager>() as IEventBus).Register();
                (kernel.Get<IControllerManager>() as IEventBus).Register();
                (kernel.Get<IControllerAtisManager>() as IEventBus).Register();
                Application.Run(mainForm);
                kernel.Dispose();
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
    }
}