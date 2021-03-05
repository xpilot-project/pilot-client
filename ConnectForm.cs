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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Networking;

namespace Vatsim.Xpilot
{
    public partial class ConnectForm : Form
    {
        private readonly IAppConfig mConfig;
        private readonly List<TypeInfo> mTypeCodes = new List<TypeInfo>();

        public ConnectForm(IAppConfig clientConfig)
        {
            InitializeComponent();

            mConfig = clientConfig;

            try
            {
                if (File.Exists(Path.Combine(mConfig.AppPath, "TypeCodes.json")))
                {
                    string JsonData = File.ReadAllText(Path.Combine(mConfig.AppPath, "TypeCodes.json"));
                    var Codes = JsonConvert.DeserializeObject<List<TypeInfo>>(JsonData);
                    if (Codes != null)
                    {
                        mTypeCodes.AddRange(Codes.ToArray());
                    }
                }
            }
            catch { }

            foreach (ConnectInfo current in mConfig.RecentConnectionInfo)
            {
                ddlRecentAircraft.Items.Add(current);
            }
            if (ddlRecentAircraft.Items.Count > 0)
            {
                ddlRecentAircraft.SelectedIndex = 0;
            }
            btnConnect.Text = chkObserverMode.Checked ? "Connect as Observer" : "Connect";
            btnConnect.Select();
        }

        private void txtCallsign_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                (sender as TextBox).SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right && e.KeyCode != Keys.Up && e.KeyCode != Keys.Down && e.KeyCode != Keys.Home && e.KeyCode != Keys.End && (e.KeyCode < Keys.A || e.KeyCode > Keys.Z) && (e.Modifiers != Keys.None || e.KeyValue < 48 || e.KeyValue > 57) && (e.Modifiers != Keys.None || e.KeyValue < 96 || e.KeyValue > 105))
            {
                e.SuppressKeyPress = true;
            }
        }

        private void txtTypeCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                (sender as TextBox).Clear();
            }
            else if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                (sender as TextBox).SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right && e.KeyCode != Keys.Up && e.KeyCode != Keys.Down && e.KeyCode != Keys.Home && e.KeyCode != Keys.End && (e.KeyCode < Keys.A || e.KeyCode > Keys.Z) && (e.Modifiers != Keys.None || e.KeyValue < 48 || e.KeyValue > 57) && (e.Modifiers != Keys.None || e.KeyValue < 96 || e.KeyValue > 105))
            {
                e.SuppressKeyPress = true;
            }
        }

        private void txtSelcalCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                (sender as TextBox).SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right && e.KeyCode != Keys.Up && e.KeyCode != Keys.Down && e.KeyCode != Keys.Home && e.KeyCode != Keys.End && (e.KeyCode < Keys.A || e.KeyCode > Keys.S) && (e.Modifiers != Keys.None || (e.KeyCode != Keys.Subtract && e.KeyCode != Keys.OemMinus)))
            {
                e.SuppressKeyPress = true;
            }
        }

        private void ErrorFocus(string message, Control focusControl)
        {
            MessageBox.Show(this, message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            base.DialogResult = DialogResult.None;
            if (focusControl != null)
            {
                focusControl.Select();
            }
        }

        private void ddlRecentAircraft_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddlRecentAircraft.SelectedItem != null && ddlRecentAircraft.SelectedItem is ConnectInfo)
            {
                ConnectInfo info = ddlRecentAircraft.SelectedItem as ConnectInfo;
                txtCallsign.Text = info.Callsign;
                txtTypeCode.Text = info.TypeCode;
                txtSelcalCode.Text = info.SelCalCode;
                chkObserverMode.Checked = info.ObserverMode;
            }
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtCallsign.Text))
            {
                ErrorFocus("Please enter a callsign", txtCallsign);
                return;
            }
            else if (string.IsNullOrEmpty(txtTypeCode.Text))
            {
                ErrorFocus("Please enter the aircraft type code", txtTypeCode);
                return;
            }
            else if (!string.IsNullOrEmpty(txtSelcalCode.Text))
            {
                if (!ValidateSelcal(txtSelcalCode.Text))
                {
                    return;
                }
            }
            else if (mTypeCodes.Count(o => o.Identifier == txtTypeCode.Text.ToUpper()) == 0)
            {
                if (MessageBox.Show(this, string.Format("The aircraft type code {0} appears to be invalid. This can prevent other users from seeing your aircraft correctly. Are you sure you want to connect using this invalid type code?", txtTypeCode.Text.ToUpper()), "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Hand) == DialogResult.No)
                {
                    DialogResult = DialogResult.None;
                    txtTypeCode.Select();
                    return;
                }
            }
            ConnectInfo info = new ConnectInfo(txtCallsign.Text, txtTypeCode.Text, txtSelcalCode.Text, chkObserverMode.Checked);
            mConfig.RecentConnectionInfo.RemoveAll(o => o.Equals(info));
            mConfig.RecentConnectionInfo.Insert(0, info);
            if (mConfig.RecentConnectionInfo.Count > 20)
            {
                mConfig.RecentConnectionInfo.RemoveRange(20, mConfig.RecentConnectionInfo.Count - 20);
            }
            mConfig.SaveConfig();
        }

        private bool ValidateSelcal(string code)
        {
            char[] mProhibitedCharacters = new char[] { 'I', 'N', 'O' };
            code = code.Replace("-", "");
            code = code.ToUpper();
            char[] chars = code.ToCharArray();

            for (int i = 1; i < chars.Count(); i += 2)
            {
                if (chars[i] < chars[i - 1])
                {
                    ErrorFocus("SELCAL characters must be in ascending alphabetical order.", txtSelcalCode);
                    return false;
                }
                if (chars[i] > 'S')
                {
                    ErrorFocus("SELCAL may only contain characters A-S.", txtSelcalCode);
                    return false;
                }
                if (chars[i - 1] > 'R')
                {
                    ErrorFocus("SELCAL may only contain characters A-S.", txtSelcalCode);
                    return false;
                }
                if (mProhibitedCharacters.Contains(chars[i]))
                {
                    ErrorFocus("SELCAL cannot contain the characters I, N or O.", txtSelcalCode);
                    return false;
                }
            }

            if (code.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => new { name = x.Key, count = x.Count() }).ToArray().Count() > 0)
            {
                ErrorFocus("SELCAL cannot contain duplicate characters.", txtSelcalCode);
                return false;
            }

            return true;
        }

        private void txtTypeCode_KeyUp(object sender, KeyEventArgs e)
        {
            PopulateTypeCodes();
        }

        public ConnectInfo GetConnectInfo()
        {
            return new ConnectInfo(txtCallsign.Text, txtTypeCode.Text, txtSelcalCode.Text, chkObserverMode.Checked);
        }

        private void PopulateTypeCodes()
        {
            if (txtTypeCode.Text.Length > 0)
            {
                List<TypeInfo> Results = mTypeCodes.Where(o => o.Identifier.ToUpper().Contains(txtTypeCode.Text.ToUpper()) ||
                    o.Name.ToUpper().Contains(txtTypeCode.Text.ToString()) ||
                    o.Manufacturer.ToUpper().Contains(txtTypeCode.Text.ToString()))
                    .ToList();
                if (Results.Count > 0)
                {
                    comboTypeCodes.Items.Clear();
                    comboTypeCodes.Items.AddRange(Results.ToArray());
                    ShowTypeCodes();
                }
                else
                {
                    HideTypeCodes();
                }
            }
            else
            {
                HideTypeCodes();
            }
        }

        private void ShowTypeCodes()
        {
            comboTypeCodes.DroppedDown = true;
            Cursor.Current = Cursors.Default;
        }

        private void HideTypeCodes()
        {
            comboTypeCodes.DroppedDown = false;
            comboTypeCodes.Items.Clear();
        }

        private void comboTypeCodes_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboTypeCodes.SelectedIndex >= 0)
            {
                txtTypeCode.Text = (comboTypeCodes.SelectedItem as TypeInfo).Identifier;
                HideTypeCodes();
            }
        }

        private void ChkObserverMode_CheckedChanged(object sender, EventArgs e)
        {
            btnConnect.Text = (chkObserverMode.Checked) ? "Connect as Observer" : "Connect";
            txtCallsign.MaxLength = (chkObserverMode.Checked) ? 8 : 7;
            txtCallsign.Text = (chkObserverMode.Checked) ? txtCallsign.Text : txtCallsign.Text.Substring(0, Math.Min(7, txtCallsign.Text.Length));
        }
    }

    [Serializable]
    public class TypeInfo
    {
        [JsonProperty("TypeCode")]
        public string Identifier { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Manufacturer")]
        public string Manufacturer { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} ({2})", Manufacturer, Name, Identifier);
        }
    }
}
