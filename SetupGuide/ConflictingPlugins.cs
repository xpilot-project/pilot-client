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
using XPilot.PilotClient.Config;
using System.IO;

namespace XPilot.PilotClient.Tutorial
{
    public partial class ConflictingPlugins : SetupScreen, ISetupScreen
    {
        public ConflictingPlugins(ISetup host, IAppConfig config) : base(host, config)
        {
            InitializeComponent();
            Host.SetTitle("Conflicting Plugins");

            if (Host.XSquawkBox)
            {
                Label l = new Label
                {
                    Text = "XSquawkBox",
                    Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold),
                    ForeColor = Color.Red,
                    AutoSize = true
                };
                pluginList.Controls.Add(l);
            }

            if (Host.XSquawkBox)
            {
                Label l = new Label
                {
                    Text = "XSwiftBus (swift)",
                    Font = new Font(this.Font.FontFamily, 14, FontStyle.Bold),
                    ForeColor = Color.Red,
                    AutoSize = true
                };
                pluginList.Controls.Add(l);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Host.XSquawkBox = false;
            Host.XSwiftBus = false;

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

            if (!Host.XSwiftBus && !Host.XSquawkBox)
            {
                Host.SwitchScreen("CslConfiguration");
            }
            else
            {
                MessageBox.Show("The conflicting plugins are still installed in the X-Plane Plugin folder.");
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Host.EndSetup();
        }
    }
}
