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
using System.Windows.Forms;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core.Events;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;

namespace XPilot.PilotClient
{
    public partial class FlightPlanForm : Form
    {
        [EventPublication(EventTopics.SendFlightPlan)]
        public event EventHandler<FlightPlanSent> RaiseFlightPlanSent;

        [EventPublication(EventTopics.FetchFlightPlan)]
        public event EventHandler<EventArgs> RaiseFetchFlightPlan;

        private readonly Timer mClockTimer;
        private readonly IEventBroker mEventBroker;
        private readonly IAppConfig mConfig;

        public FlightPlanForm(IEventBroker eventBroker, IAppConfig appConfig)
        {
            InitializeComponent();

            mConfig = appConfig;

            mClockTimer = new Timer
            {
                Interval = 1000
            };
            mClockTimer.Tick += ClockTimer_Tick;

            foreach (FlightPlanType flightPlanType in Enum.GetValues(typeof(FlightPlanType)))
            {
                ddlFlightType.Items.Add(flightPlanType);
            }
            ddlFlightType.SelectedItem = FlightPlanType.IFR;

            if (mConfig.LastFlightPlan != null)
            {
                NewFlightPlan(mConfig.LastFlightPlan);
            }

            mEventBroker = eventBroker;
            mEventBroker.Register(this);
        }

        private void ClockTimer_Tick(object sender, EventArgs e)
        {
            Text = $"File Flight Plan - Current Time { DateTime.UtcNow.ToString("HHmm") }z";
        }

        [EventSubscription(EventTopics.FlightPlanReceived, typeof(OnUserInterfaceAsync))]
        public void OnFlightPlanReceived(object sender, FlightPlanReceived e)
        {
            NewFlightPlan(e.FlightPlan);
            mConfig.LastFlightPlan = e.FlightPlan;
            MessageBox.Show(this, "Pre-filed flight plan successfully retrieved from the server.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
        }

        [EventSubscription(EventTopics.NoFlightPlanReceived, typeof(OnUserInterfaceAsync))]
        public void OnNoFlightPlanReceived(object sender, EventArgs e)
        {
            MessageBox.Show(this, "No flight plan was found on the server.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
        }

        protected override void OnShown(EventArgs e)
        {
            mClockTimer.Start();
            base.OnShown(e);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            OnFormClosing();
            Hide();
        }

        private void BtnSend_Click(object sender, EventArgs e)
        {
            if (!rdoVoiceTypeFull.Checked && !rdoVoiceTypeReceiveOnly.Checked && !rdoVoiceTypeTextOnly.Checked)
            {
                MessageBox.Show(this, "Please indiciate your voice capability.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else if (ddlEquipmentSuffix.SelectedIndex == 0 || ddlEquipmentSuffix.SelectedIndex == -1) // -1 = initial, 0 = changed
            {
                MessageBox.Show(this, "Please indicate your equipment suffix.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            else
            {
                mConfig.LastFlightPlan = CreateFlightPlan();
                RaiseFlightPlanSent?.Invoke(this, new FlightPlanSent(CreateFlightPlan()));
                Close();
            }
        }

        private void NumbersOnly(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                (sender as TextBox).SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right && e.KeyCode != Keys.Up && e.KeyCode != Keys.Down && e.KeyCode != Keys.Home && e.KeyCode != Keys.End && (e.Modifiers != Keys.None || e.KeyValue < 48 || e.KeyValue > 57) && (e.Modifiers != Keys.None || e.KeyValue < 96 || e.KeyValue > 105))
            {
                e.SuppressKeyPress = true;
            }
        }

        private void AlphaNumeric(object sender, KeyEventArgs e)
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

        private void AlphaNumericSpaces(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                (sender as TextBox).SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete && e.KeyCode != Keys.Left && e.KeyCode != Keys.Right && e.KeyCode != Keys.Up && e.KeyCode != Keys.Down && e.KeyCode != Keys.Home && e.KeyCode != Keys.End && (e.KeyCode < Keys.A || e.KeyCode > Keys.Z) && (e.Modifiers != Keys.None || e.KeyValue < 48 || e.KeyValue > 57) && (e.Modifiers != Keys.None || e.KeyValue < 96 || e.KeyValue > 105) && (e.Modifiers != Keys.None || (e.KeyValue != 190 && e.KeyCode != Keys.Decimal)) && (e.Modifiers != Keys.None || (e.KeyValue != 191 && e.KeyValue != 111)) && e.KeyCode != Keys.Space)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void txtRemarks_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.A && e.Modifiers == Keys.Control)
            {
                (sender as TextBox).SelectAll();
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
            }
        }

        private void spinCruiseAltitude_Leave(object sender, EventArgs e)
        {
            if (spinCruiseAltitude.Value < 1000.0m)
            {
                spinCruiseAltitude.Value *= 100.0m;
            }
        }

        private void OnFormClosing()
        {
            mClockTimer.Stop();
            if (mEventBroker != null)
            {
                mEventBroker.Unregister(this);
            }
        }

        private void FlightPlanForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            OnFormClosing();
        }

        private void NewFlightPlan(FlightPlan flightPlan)
        {
            ddlFlightType.SelectedItem = flightPlan.FlightType;
            txtDepartureAirport.Text = flightPlan.DepartureAirport;
            txtDestinationAirport.Text = flightPlan.DestinationAirport;
            txtAlternateAirport.Text = flightPlan.AlternateAirport;
            txtDepartureTime.Text = flightPlan.DepartureTime.ToString("0000");
            txtEnrouteHours.Text = flightPlan.EnrouteHours.ToString();
            txtEnrouteMinutes.Text = flightPlan.EnrouteMinutes.ToString();
            txtFuelHours.Text = flightPlan.FuelHours.ToString();
            txtFuelMinutes.Text = flightPlan.FuelMinutes.ToString();
            spinCruiseSpeed.Value = flightPlan.CruiseSpeed;
            spinCruiseAltitude.Value = flightPlan.CruiseAltitude;
            chkHeavy.Checked = flightPlan.IsHeavy;
            ddlEquipmentSuffix.SelectedItem = flightPlan.EquipmentSuffix;
            txtRoute.Text = flightPlan.Route;
            txtRemarks.Text = flightPlan.Remarks.Replace("+VFPS+", "");
            rdoVoiceTypeFull.Checked = false;
            rdoVoiceTypeReceiveOnly.Checked = false;
            rdoVoiceTypeTextOnly.Checked = false;
            switch (flightPlan.VoiceType)
            {
                case VoiceType.Full:
                    rdoVoiceTypeFull.Checked = true;
                    break;
                case VoiceType.ReceiveOnly:
                    rdoVoiceTypeReceiveOnly.Checked = true;
                    break;
                case VoiceType.TextOnly:
                    rdoVoiceTypeTextOnly.Checked = true;
                    break;
            }
        }

        private FlightPlan CreateFlightPlan()
        {
            FlightPlan flightPlan = new FlightPlan
            {
                FlightType = (FlightPlanType)ddlFlightType.SelectedItem,
                DepartureAirport = txtDepartureAirport.Text,
                DestinationAirport = txtDestinationAirport.Text,
                AlternateAirport = txtAlternateAirport.Text
            };
            int.TryParse(txtDepartureTime.Text, out int result);
            flightPlan.DepartureTime = result;
            int.TryParse(txtEnrouteHours.Text, out result);
            flightPlan.EnrouteHours = result;
            int.TryParse(txtEnrouteMinutes.Text, out result);
            flightPlan.EnrouteMinutes = result;
            int.TryParse(txtFuelHours.Text, out result);
            flightPlan.FuelHours = result;
            int.TryParse(txtFuelMinutes.Text, out result);
            flightPlan.FuelMinutes = result;
            flightPlan.CruiseSpeed = (int)spinCruiseSpeed.Value;
            flightPlan.CruiseAltitude = (int)spinCruiseAltitude.Value;
            flightPlan.IsHeavy = chkHeavy.Checked;
            flightPlan.EquipmentSuffix = ((ddlEquipmentSuffix.SelectedIndex == -1) ? "" : ddlEquipmentSuffix.SelectedItem.ToString());
            flightPlan.Route = txtRoute.Text.Replace("\t", "").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ");
            flightPlan.Remarks = txtRemarks.Text.Replace("\t", "").Replace("\r\n", " ").Replace("\r", " ").Replace("\n", " ").Replace("+VFPS+", "");
            if (rdoVoiceTypeFull.Checked)
            {
                flightPlan.VoiceType = VoiceType.Full;
            }
            else if (rdoVoiceTypeReceiveOnly.Checked)
            {
                flightPlan.VoiceType = VoiceType.ReceiveOnly;
            }
            else if (rdoVoiceTypeTextOnly.Checked)
            {
                flightPlan.VoiceType = VoiceType.TextOnly;
            }
            else
            {
                flightPlan.VoiceType = VoiceType.Unknown;
            }
            return flightPlan;
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to clear the flight plan?", "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            {
                NewFlightPlan(new FlightPlan());
            }
        }

        private void BtnFetch_Click(object sender, EventArgs e)
        {
            RaiseFetchFlightPlan(this, EventArgs.Empty);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!rdoVoiceTypeFull.Checked && !rdoVoiceTypeReceiveOnly.Checked && !rdoVoiceTypeTextOnly.Checked)
            {
                MessageBox.Show(this, "Please indicate your voice capability.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            
            if (ddlEquipmentSuffix.SelectedIndex == 0 || ddlEquipmentSuffix.SelectedIndex == -1) // -1 = initial, 0 = changed
            {
                MessageBox.Show(this, "Please indicate your equipment suffix.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            FlightPlan flightPlan = CreateFlightPlan();
            saveFlightPlanDialog.Filter = "xPilot/vPilot Flight Plans (*.vfp)|*.vfp";
            if (!string.IsNullOrEmpty(flightPlan.DepartureAirport) && !string.IsNullOrEmpty(flightPlan.DestinationAirport))
            {
                saveFlightPlanDialog.FileName = $"{ flightPlan.DepartureAirport }-{ flightPlan.DestinationAirport }";
            }
            else
            {
                saveFlightPlanDialog.FileName = "";
            }
            if (saveFlightPlanDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SaveFlightPlan(flightPlan, saveFlightPlanDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error saving flight plan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void BtnLoad_Click(object sender, EventArgs e)
        {
            openFlightPlanDialog.Filter = "xPilot/vPilot Flight Plans (*.vfp)|*.vfp";
            if (openFlightPlanDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    FlightPlan flightPlan = LoadFlightPlan(openFlightPlanDialog.FileName);
                    NewFlightPlan(flightPlan);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Error loading flight plan: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                }
            }
        }

        private void SaveFlightPlan(FlightPlan flightPlan, string fileName)
        {
            SerializationUtils.SerializeObjectToFile(flightPlan, fileName);
        }

        private FlightPlan LoadFlightPlan(string fileName)
        {
            return SerializationUtils.DeserializeObjectFromFile<FlightPlan>(fileName);
        }

        private void BtnSwap_Click(object sender, EventArgs e)
        {
            string temp = txtDepartureAirport.Text;
            txtDepartureAirport.Text = txtDestinationAirport.Text;
            txtDestinationAirport.Text = temp;
        }
    }
}
