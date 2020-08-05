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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using XPilot.PilotClient.Config;
using Loamen.KeyMouseHook;

namespace XPilot.PilotClient
{
    public partial class WelcomeView : SetupScreen, ISetupScreen
    {
        public WelcomeView(ISetup host, IAppConfig config) : base(host, config)
        {
            InitializeComponent();
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            int instancesFound = 0;
            string usablePath = "";
            var installFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "x-plane_install_11.txt");
            if (File.Exists(installFile))
            {
                using (StreamReader sr = File.OpenText(installFile))
                {
                    string xpPath = string.Empty;
                    while ((xpPath = sr.ReadLine()) != null)
                    {
                        if (Directory.Exists(Path.Combine(xpPath, "Resources")))
                        {
                            if (Directory.EnumerateFiles(Path.Combine(xpPath, "Resources"), "*", SearchOption.AllDirectories).Count() > 0)
                            {
                                usablePath = xpPath;
                                ++instancesFound;
                            }
                        }
                    }
                }
            }
            if (instancesFound == 1)
            {
                mConfig.XplanePath = usablePath;
                mConfig.SaveConfig();

                // check for conflicting plugins (XSwiftBus, XSB)
                string pluginPath = Path.Combine(Path.GetDirectoryName(mConfig.XplanePath), "Resources/plugins");
                string[] dirs = Directory.GetDirectories(pluginPath);
                foreach (var dir in dirs)
                {
                    if (Path.GetFileName(dir).ToLower() == "xsquawkbox")
                    {
                        Host.XSquawkBox = true;
                    }
                    if (Path.GetFileName(dir).ToLower() == "xswiftbus")
                    {
                        Host.XSwiftBus = true;
                    }
                }

                if (Host.XSquawkBox || Host.XSwiftBus)
                {
                    Host.SwitchScreen("ConflictingPlugins");
                }
                else
                {
                    Host.SwitchScreen("CslConfiguration");
                }
            }
            else
            {
                Host.SwitchScreen("XplanePath");
            }
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            Host.ManualSetup();
        }
    }
}
