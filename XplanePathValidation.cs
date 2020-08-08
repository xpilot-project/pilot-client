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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using XPilot.PilotClient.Config;

namespace XPilot.PilotClient
{
    public partial class XplanePathValidation : Form
    {
        private IAppConfig mConfig;

        public XplanePathValidation(IAppConfig config)
        {
            InitializeComponent();
            mConfig = config;
            ValidateXplane();
        }

        private void ValidateXplane()
        {
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
                                btnSave.Enabled = true;
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
                btnSave.Enabled = false;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            var checkedPath = pathOptions.Controls.OfType<RadioButton>().FirstOrDefault(r => r.Checked);
            if (checkedPath != null)
            {
                mConfig.XplanePath = checkedPath.Tag.ToString();
                mConfig.SaveConfig();
                Close();
            }
        }
    }
}
