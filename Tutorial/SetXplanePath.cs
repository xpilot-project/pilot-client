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
using System.IO;
using XPilot.PilotClient.Config;
using System.Linq;

namespace XPilot.PilotClient
{
    public partial class SetXplanePath : View
    {
        private IAppConfig mConfig;

        public SetXplanePath(IHost host, IAppConfig config) : base(host)
        {
            InitializeComponent();

            mConfig = config;
            Host.SetTitle("Set X-Plane Path");

            bool foundXpilot = false;
            var installFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "x-plane_install_11.txt");
            if (File.Exists(installFile))
            {
                using (StreamReader sr = File.OpenText(installFile))
                {
                    string xpPath = string.Empty;
                    while ((xpPath = sr.ReadLine()) != null)
                    {
                        if (Directory.Exists(Path.Combine(xpPath, "Resources/plugins/xPilot")))
                        {
                            foundXpilot = true;
                            RadioButton btn = new RadioButton
                            {
                                Tag = xpPath,
                                Text = xpPath,
                                TextAlign = ContentAlignment.MiddleLeft,
                                AutoSize = true,
                                Font = new Font(this.Font.FontFamily, 12f, FontStyle.Bold)
                            };
                            btn.CheckedChanged += (s, e) =>
                            {
                                btnNext.Enabled = true;
                            };
                            pathOptions.Controls.Add(btn);
                        }
                    }
                }
            }

            if (!foundXpilot)
            {
                Label l = new Label
                {
                    AutoSize = true,
                    ForeColor = Color.Red,
                    Text = "The xPilot plugin could not be found in any of the X-Plane instances.\nPlease re-run the xPilot installer."
                };
                pathOptions.Controls.Add(l);
                btnNext.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Host.CloseTutorial();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            var checkedPath = pathOptions.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
            if(checkedPath != null)
            {
                bool xsb = false;
                bool xswiftbus = false;
                string pluginPath = Path.Combine(checkedPath.Tag.ToString(), "Resources/plugins");
                string[] dirs = Directory.GetDirectories(pluginPath);
                foreach (var dir in dirs)
                {
                    if (Path.GetFileName(dir).ToLower() == "xsquawkbox")
                    {
                        xsb = true;
                    }
                    if (Path.GetFileName(dir).ToLower() == "xswiftbus")
                    {
                        xswiftbus = true;
                    }
                }

                if (xsb || xswiftbus)
                {
                    Host.SwitchView("ConflictingPlugins");
                }
                else
                {
                    Host.SwitchView("CslConfiguration");
                }
            }
        }
    }
}