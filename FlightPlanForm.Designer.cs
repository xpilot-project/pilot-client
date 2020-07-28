namespace XPilot.PilotClient
{
    partial class FlightPlanForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.ddlFlightType = new System.Windows.Forms.ComboBox();
            this.txtDepartureAirport = new System.Windows.Forms.TextBox();
            this.txtDestinationAirport = new System.Windows.Forms.TextBox();
            this.txtAlternateAirport = new System.Windows.Forms.TextBox();
            this.txtDepartureTime = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtEnrouteHours = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.txtEnrouteMinutes = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtFuelMinutes = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.txtFuelHours = new System.Windows.Forms.TextBox();
            this.spinCruiseSpeed = new System.Windows.Forms.NumericUpDown();
            this.spinCruiseAltitude = new System.Windows.Forms.NumericUpDown();
            this.chkHeavy = new System.Windows.Forms.CheckBox();
            this.ddlEquipmentSuffix = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.txtRoute = new System.Windows.Forms.TextBox();
            this.txtRemarks = new System.Windows.Forms.TextBox();
            this.rdoVoiceTypeFull = new System.Windows.Forms.RadioButton();
            this.rdoVoiceTypeReceiveOnly = new System.Windows.Forms.RadioButton();
            this.rdoVoiceTypeTextOnly = new System.Windows.Forms.RadioButton();
            this.btnSend = new System.Windows.Forms.Button();
            this.btnFetch = new System.Windows.Forms.Button();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlVoiceType = new System.Windows.Forms.Panel();
            this.saveFlightPlanDialog = new System.Windows.Forms.SaveFileDialog();
            this.openFlightPlanDialog = new System.Windows.Forms.OpenFileDialog();
            this.btnSwap = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.spinCruiseSpeed)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinCruiseAltitude)).BeginInit();
            this.pnlVoiceType.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(82, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Flight Type:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(54, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(90, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Departure Airport:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(48, 101);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Destination Airport:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(59, 129);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(85, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Alternate Airport:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(61, 157);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 5;
            this.label5.Text = "Departure Time:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(71, 185);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 13);
            this.label6.TabIndex = 6;
            this.label6.Text = "Time Enroute:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(68, 213);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(76, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Fuel Available:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(71, 241);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(73, 13);
            this.label8.TabIndex = 8;
            this.label8.Text = "Cruise Speed:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(67, 269);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(77, 13);
            this.label9.TabIndex = 9;
            this.label9.Text = "Cruise Altitude:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(298, 70);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(39, 13);
            this.label10.TabIndex = 10;
            this.label10.Text = "Route:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(285, 175);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(52, 13);
            this.label11.TabIndex = 11;
            this.label11.Text = "Remarks:";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(287, 261);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(37, 13);
            this.label12.TabIndex = 12;
            this.label12.Text = "Voice:";
            // 
            // ddlFlightType
            // 
            this.ddlFlightType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlFlightType.FormattingEnabled = true;
            this.ddlFlightType.Location = new System.Drawing.Point(150, 39);
            this.ddlFlightType.Name = "ddlFlightType";
            this.ddlFlightType.Size = new System.Drawing.Size(68, 21);
            this.ddlFlightType.TabIndex = 13;
            // 
            // txtDepartureAirport
            // 
            this.txtDepartureAirport.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtDepartureAirport.Location = new System.Drawing.Point(150, 70);
            this.txtDepartureAirport.MaxLength = 4;
            this.txtDepartureAirport.Name = "txtDepartureAirport";
            this.txtDepartureAirport.Size = new System.Drawing.Size(50, 20);
            this.txtDepartureAirport.TabIndex = 14;
            this.txtDepartureAirport.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AlphaNumeric);
            // 
            // txtDestinationAirport
            // 
            this.txtDestinationAirport.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtDestinationAirport.Location = new System.Drawing.Point(150, 98);
            this.txtDestinationAirport.MaxLength = 4;
            this.txtDestinationAirport.Name = "txtDestinationAirport";
            this.txtDestinationAirport.Size = new System.Drawing.Size(50, 20);
            this.txtDestinationAirport.TabIndex = 15;
            this.txtDestinationAirport.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AlphaNumeric);
            // 
            // txtAlternateAirport
            // 
            this.txtAlternateAirport.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtAlternateAirport.Location = new System.Drawing.Point(150, 126);
            this.txtAlternateAirport.MaxLength = 4;
            this.txtAlternateAirport.Name = "txtAlternateAirport";
            this.txtAlternateAirport.Size = new System.Drawing.Size(50, 20);
            this.txtAlternateAirport.TabIndex = 16;
            this.txtAlternateAirport.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AlphaNumeric);
            // 
            // txtDepartureTime
            // 
            this.txtDepartureTime.Location = new System.Drawing.Point(150, 154);
            this.txtDepartureTime.MaxLength = 4;
            this.txtDepartureTime.Name = "txtDepartureTime";
            this.txtDepartureTime.Size = new System.Drawing.Size(50, 20);
            this.txtDepartureTime.TabIndex = 17;
            this.txtDepartureTime.Text = "0000";
            this.txtDepartureTime.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NumbersOnly);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(203, 155);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(45, 13);
            this.label13.TabIndex = 18;
            this.label13.Text = "hhmm Z";
            // 
            // txtEnrouteHours
            // 
            this.txtEnrouteHours.Location = new System.Drawing.Point(150, 182);
            this.txtEnrouteHours.MaxLength = 2;
            this.txtEnrouteHours.Name = "txtEnrouteHours";
            this.txtEnrouteHours.Size = new System.Drawing.Size(28, 20);
            this.txtEnrouteHours.TabIndex = 19;
            this.txtEnrouteHours.Text = "00";
            this.txtEnrouteHours.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NumbersOnly);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(182, 184);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(19, 13);
            this.label14.TabIndex = 20;
            this.label14.Text = "hh";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(237, 185);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(23, 13);
            this.label15.TabIndex = 22;
            this.label15.Text = "mm";
            // 
            // txtEnrouteMinutes
            // 
            this.txtEnrouteMinutes.Location = new System.Drawing.Point(205, 182);
            this.txtEnrouteMinutes.MaxLength = 2;
            this.txtEnrouteMinutes.Name = "txtEnrouteMinutes";
            this.txtEnrouteMinutes.Size = new System.Drawing.Size(28, 20);
            this.txtEnrouteMinutes.TabIndex = 21;
            this.txtEnrouteMinutes.Text = "00";
            this.txtEnrouteMinutes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NumbersOnly);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(237, 214);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(23, 13);
            this.label16.TabIndex = 26;
            this.label16.Text = "mm";
            // 
            // txtFuelMinutes
            // 
            this.txtFuelMinutes.Location = new System.Drawing.Point(205, 210);
            this.txtFuelMinutes.MaxLength = 2;
            this.txtFuelMinutes.Name = "txtFuelMinutes";
            this.txtFuelMinutes.Size = new System.Drawing.Size(28, 20);
            this.txtFuelMinutes.TabIndex = 25;
            this.txtFuelMinutes.Text = "00";
            this.txtFuelMinutes.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NumbersOnly);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(182, 214);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(19, 13);
            this.label17.TabIndex = 24;
            this.label17.Text = "hh";
            // 
            // txtFuelHours
            // 
            this.txtFuelHours.Location = new System.Drawing.Point(150, 210);
            this.txtFuelHours.MaxLength = 2;
            this.txtFuelHours.Name = "txtFuelHours";
            this.txtFuelHours.Size = new System.Drawing.Size(28, 20);
            this.txtFuelHours.TabIndex = 23;
            this.txtFuelHours.Text = "00";
            this.txtFuelHours.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NumbersOnly);
            // 
            // spinCruiseSpeed
            // 
            this.spinCruiseSpeed.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.spinCruiseSpeed.Location = new System.Drawing.Point(150, 238);
            this.spinCruiseSpeed.Maximum = new decimal(new int[] {
            99999,
            0,
            0,
            0});
            this.spinCruiseSpeed.Name = "spinCruiseSpeed";
            this.spinCruiseSpeed.Size = new System.Drawing.Size(68, 20);
            this.spinCruiseSpeed.TabIndex = 27;
            // 
            // spinCruiseAltitude
            // 
            this.spinCruiseAltitude.Increment = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.spinCruiseAltitude.Location = new System.Drawing.Point(150, 266);
            this.spinCruiseAltitude.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.spinCruiseAltitude.Name = "spinCruiseAltitude";
            this.spinCruiseAltitude.Size = new System.Drawing.Size(68, 20);
            this.spinCruiseAltitude.TabIndex = 28;
            this.spinCruiseAltitude.Leave += new System.EventHandler(this.spinCruiseAltitude_Leave);
            // 
            // chkHeavy
            // 
            this.chkHeavy.AutoSize = true;
            this.chkHeavy.Location = new System.Drawing.Point(339, 41);
            this.chkHeavy.Name = "chkHeavy";
            this.chkHeavy.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.chkHeavy.Size = new System.Drawing.Size(93, 17);
            this.chkHeavy.TabIndex = 29;
            this.chkHeavy.Text = "Heavy Aircraft";
            this.chkHeavy.UseVisualStyleBackColor = true;
            // 
            // ddlEquipmentSuffix
            // 
            this.ddlEquipmentSuffix.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlEquipmentSuffix.FormattingEnabled = true;
            this.ddlEquipmentSuffix.Items.AddRange(new object[] {
            "",
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"});
            this.ddlEquipmentSuffix.Location = new System.Drawing.Point(569, 39);
            this.ddlEquipmentSuffix.Name = "ddlEquipmentSuffix";
            this.ddlEquipmentSuffix.Size = new System.Drawing.Size(41, 21);
            this.ddlEquipmentSuffix.TabIndex = 31;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(474, 43);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(89, 13);
            this.label18.TabIndex = 30;
            this.label18.Text = "Equipment Suffix:";
            // 
            // txtRoute
            // 
            this.txtRoute.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtRoute.Location = new System.Drawing.Point(343, 70);
            this.txtRoute.Multiline = true;
            this.txtRoute.Name = "txtRoute";
            this.txtRoute.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRoute.Size = new System.Drawing.Size(267, 89);
            this.txtRoute.TabIndex = 32;
            this.txtRoute.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AlphaNumericSpaces);
            // 
            // txtRemarks
            // 
            this.txtRemarks.Location = new System.Drawing.Point(343, 175);
            this.txtRemarks.Multiline = true;
            this.txtRemarks.Name = "txtRemarks";
            this.txtRemarks.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtRemarks.Size = new System.Drawing.Size(267, 70);
            this.txtRemarks.TabIndex = 33;
            this.txtRemarks.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtRemarks_KeyDown);
            // 
            // rdoVoiceTypeFull
            // 
            this.rdoVoiceTypeFull.AutoSize = true;
            this.rdoVoiceTypeFull.Location = new System.Drawing.Point(4, 2);
            this.rdoVoiceTypeFull.Name = "rdoVoiceTypeFull";
            this.rdoVoiceTypeFull.Size = new System.Drawing.Size(102, 17);
            this.rdoVoiceTypeFull.TabIndex = 34;
            this.rdoVoiceTypeFull.TabStop = true;
            this.rdoVoiceTypeFull.Text = "Send + Receive";
            this.rdoVoiceTypeFull.UseVisualStyleBackColor = true;
            // 
            // rdoVoiceTypeReceiveOnly
            // 
            this.rdoVoiceTypeReceiveOnly.AutoSize = true;
            this.rdoVoiceTypeReceiveOnly.Location = new System.Drawing.Point(110, 2);
            this.rdoVoiceTypeReceiveOnly.Name = "rdoVoiceTypeReceiveOnly";
            this.rdoVoiceTypeReceiveOnly.Size = new System.Drawing.Size(89, 17);
            this.rdoVoiceTypeReceiveOnly.TabIndex = 35;
            this.rdoVoiceTypeReceiveOnly.TabStop = true;
            this.rdoVoiceTypeReceiveOnly.Text = "Receive Only";
            this.rdoVoiceTypeReceiveOnly.UseVisualStyleBackColor = true;
            // 
            // rdoVoiceTypeTextOnly
            // 
            this.rdoVoiceTypeTextOnly.AutoSize = true;
            this.rdoVoiceTypeTextOnly.Location = new System.Drawing.Point(205, 2);
            this.rdoVoiceTypeTextOnly.Name = "rdoVoiceTypeTextOnly";
            this.rdoVoiceTypeTextOnly.Size = new System.Drawing.Size(70, 17);
            this.rdoVoiceTypeTextOnly.TabIndex = 36;
            this.rdoVoiceTypeTextOnly.TabStop = true;
            this.rdoVoiceTypeTextOnly.Text = "Text Only";
            this.rdoVoiceTypeTextOnly.UseVisualStyleBackColor = true;
            // 
            // btnSend
            // 
            this.btnSend.Location = new System.Drawing.Point(52, 299);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(104, 23);
            this.btnSend.TabIndex = 37;
            this.btnSend.Text = "File Flight Plan";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.BtnSend_Click);
            // 
            // btnFetch
            // 
            this.btnFetch.Location = new System.Drawing.Point(161, 299);
            this.btnFetch.Name = "btnFetch";
            this.btnFetch.Size = new System.Drawing.Size(104, 23);
            this.btnFetch.TabIndex = 38;
            this.btnFetch.Text = "Fetch From Server";
            this.btnFetch.UseVisualStyleBackColor = true;
            this.btnFetch.Click += new System.EventHandler(this.BtnFetch_Click);
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(270, 299);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(103, 23);
            this.btnLoad.TabIndex = 39;
            this.btnLoad.Text = "Load From File";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(378, 299);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(93, 23);
            this.btnSave.TabIndex = 40;
            this.btnSave.Text = "Save To File";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.BtnSave_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(476, 299);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(63, 23);
            this.btnClear.TabIndex = 41;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.BtnClear_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(544, 299);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(63, 23);
            this.btnClose.TabIndex = 42;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // pnlVoiceType
            // 
            this.pnlVoiceType.Controls.Add(this.rdoVoiceTypeTextOnly);
            this.pnlVoiceType.Controls.Add(this.rdoVoiceTypeFull);
            this.pnlVoiceType.Controls.Add(this.rdoVoiceTypeReceiveOnly);
            this.pnlVoiceType.Location = new System.Drawing.Point(330, 257);
            this.pnlVoiceType.Name = "pnlVoiceType";
            this.pnlVoiceType.Size = new System.Drawing.Size(280, 21);
            this.pnlVoiceType.TabIndex = 43;
            // 
            // openFlightPlanDialog
            // 
            this.openFlightPlanDialog.FileName = "openFileDialog1";
            // 
            // btnSwap
            // 
            this.btnSwap.Location = new System.Drawing.Point(207, 82);
            this.btnSwap.Name = "btnSwap";
            this.btnSwap.Size = new System.Drawing.Size(58, 23);
            this.btnSwap.TabIndex = 44;
            this.btnSwap.Text = "Swap";
            this.btnSwap.UseVisualStyleBackColor = true;
            this.btnSwap.Click += new System.EventHandler(this.BtnSwap_Click);
            // 
            // FlightPlanForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(659, 361);
            this.ControlBox = false;
            this.Controls.Add(this.btnSwap);
            this.Controls.Add(this.pnlVoiceType);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.btnFetch);
            this.Controls.Add(this.btnSend);
            this.Controls.Add(this.txtRemarks);
            this.Controls.Add(this.txtRoute);
            this.Controls.Add(this.ddlEquipmentSuffix);
            this.Controls.Add(this.label18);
            this.Controls.Add(this.chkHeavy);
            this.Controls.Add(this.spinCruiseAltitude);
            this.Controls.Add(this.spinCruiseSpeed);
            this.Controls.Add(this.label16);
            this.Controls.Add(this.txtFuelMinutes);
            this.Controls.Add(this.label17);
            this.Controls.Add(this.txtFuelHours);
            this.Controls.Add(this.label15);
            this.Controls.Add(this.txtEnrouteMinutes);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.txtEnrouteHours);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.txtDepartureTime);
            this.Controls.Add(this.txtAlternateAirport);
            this.Controls.Add(this.txtDestinationAirport);
            this.Controls.Add(this.txtDepartureAirport);
            this.Controls.Add(this.ddlFlightType);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FlightPlanForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "File Flight Plan";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FlightPlanForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.spinCruiseSpeed)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.spinCruiseAltitude)).EndInit();
            this.pnlVoiceType.ResumeLayout(false);
            this.pnlVoiceType.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ComboBox ddlFlightType;
        private System.Windows.Forms.TextBox txtDepartureAirport;
        private System.Windows.Forms.TextBox txtDestinationAirport;
        private System.Windows.Forms.TextBox txtAlternateAirport;
        private System.Windows.Forms.TextBox txtDepartureTime;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtEnrouteHours;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.TextBox txtEnrouteMinutes;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox txtFuelMinutes;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtFuelHours;
        private System.Windows.Forms.NumericUpDown spinCruiseSpeed;
        private System.Windows.Forms.NumericUpDown spinCruiseAltitude;
        private System.Windows.Forms.CheckBox chkHeavy;
        private System.Windows.Forms.ComboBox ddlEquipmentSuffix;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TextBox txtRoute;
        private System.Windows.Forms.TextBox txtRemarks;
        private System.Windows.Forms.RadioButton rdoVoiceTypeFull;
        private System.Windows.Forms.RadioButton rdoVoiceTypeReceiveOnly;
        private System.Windows.Forms.RadioButton rdoVoiceTypeTextOnly;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.Button btnFetch;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel pnlVoiceType;
        private System.Windows.Forms.SaveFileDialog saveFlightPlanDialog;
        private System.Windows.Forms.OpenFileDialog openFlightPlanDialog;
        private System.Windows.Forms.Button btnSwap;
    }
}