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
using Appccelerate.EventBroker;
using SimpleInjector;
using SimpleInjector.Diagnostics;
using Vatsim.Xpilot.AudioForVatsim;
using Vatsim.Xpilot.Networking;
using Vatsim.Xpilot.Controllers;
using Vatsim.Xpilot.Simulator;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Aircrafts;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Config;

namespace Vatsim.Xpilot
{
    static class Program
    {
        private static Container Container;
        private static string AppPath;
        private static IAppConfig Config;

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

                Container = new Container();

                AutoRegisterWindowsForms(Container);

                Container.RegisterSingleton<NotesTab>();
                Container.RegisterFactory<IUserInterface>();
                Container.RegisterFactory<ITabPages>();
                Container.RegisterSingleton<IEventBroker>(() => new EventBroker());
                Container.RegisterSingleton<IAppConfig, AppConfig>();
                Container.RegisterSingleton<INetworkManager, NetworkManager>();
                Container.RegisterSingleton<IAFVManaged, AFVManaged>();
                Container.RegisterSingleton<ISoundManager, SoundManager>();
                Container.RegisterSingleton<IXplaneAdapter, XplaneAdapter>();
                Container.RegisterSingleton<IVersionCheck, VersionCheck>();
                Container.RegisterSingleton<IUserAircraftManager, UserAircraftManager>();
                Container.RegisterSingleton<IAircraftManager, AircraftManager>();
                Container.RegisterSingleton<IControllerManager, ControllerManager>();
                Container.RegisterSingleton<IControllerAtisManager, ControllerAtisManager>();

                Container.Verify();

                Config = Container.GetInstance<IAppConfig>();
                (Container.GetInstance<INetworkManager>() as IEventBus).Register();
                if (!Config.IsVoiceDisabled)
                {
                    (Container.GetInstance<IAFVManaged>() as IEventBus).Register();
                }
                (Container.GetInstance<ISoundManager>() as IEventBus).Register();
                (Container.GetInstance<IXplaneAdapter>() as IEventBus).Register();
                (Container.GetInstance<IVersionCheck>() as IEventBus).Register();
                (Container.GetInstance<IUserAircraftManager>() as IEventBus).Register();
                (Container.GetInstance<IAircraftManager>() as IEventBus).Register();
                (Container.GetInstance<IControllerManager>() as IEventBus).Register();
                (Container.GetInstance<IControllerAtisManager>() as IEventBus).Register();

                Application.Run(Container.GetInstance<MainForm>());
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
            if (!Config.IsVoiceDisabled && AFVBindings.IsClientInitialized())
            {
                AFVBindings.Destroy();
            }
        }

        private static void AutoRegisterWindowsForms(Container container)
        {
            var types = container.GetTypesToRegister<Form>(typeof(Program).Assembly);

            foreach (var type in types)
            {
                var registration =
                    Lifestyle.Transient.CreateRegistration(type, container);

                registration.SuppressDiagnosticWarning(
                    DiagnosticType.DisposableTransientComponent,
                    "Forms should be disposed by app code; not by the container.");

                container.AddRegistration(type, registration);
            }
        }
    }
}