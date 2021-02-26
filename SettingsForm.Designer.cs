using System.Windows.Forms;

namespace XPilot.PilotClient
{
    partial class SettingsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsForm));
            this.txtNetworkPassword = new System.Windows.Forms.TextBox();
            this.txtNetworkLogin = new System.Windows.Forms.TextBox();
            this.txtHomeAirport = new System.Windows.Forms.TextBox();
            this.txtFullName = new System.Windows.Forms.TextBox();
            this.lstServerName = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.chkKeepVisible = new System.Windows.Forms.CheckBox();
            this.chkAutoSquawkModeC = new System.Windows.Forms.CheckBox();
            this.chkEnableNotificationSounds = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnGuidedSetup = new System.Windows.Forms.Button();
            this.lstAudioDriver = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lstInputDevice = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.lstListenDevice = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.trackCom2 = new System.Windows.Forms.TrackBar();
            this.label13 = new System.Windows.Forms.Label();
            this.trackCom1 = new System.Windows.Forms.TrackBar();
            this.chkHfSquelch = new System.Windows.Forms.CheckBox();
            this.chkDisableRadioEffects = new System.Windows.Forms.CheckBox();
            this.chkFlashPrivateMessage = new System.Windows.Forms.CheckBox();
            this.chkFlashSelcal = new System.Windows.Forms.CheckBox();
            this.chkFlashDisconnect = new System.Windows.Forms.CheckBox();
            this.volCom1 = new System.Windows.Forms.Label();
            this.volCom2 = new System.Windows.Forms.Label();
            this.chkFlashRadioMessage = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.vuMeter = new XPilot.PilotClient.LevelMeter();
            ((System.ComponentModel.ISupportInitialize)(this.trackCom2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackCom1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtNetworkPassword
            // 
            this.txtNetworkPassword.Location = new System.Drawing.Point(7, 70);
            this.txtNetworkPassword.Name = "txtNetworkPassword";
            this.txtNetworkPassword.PasswordChar = '●';
            this.txtNetworkPassword.Size = new System.Drawing.Size(283, 20);
            this.txtNetworkPassword.TabIndex = 9;
            // 
            // txtNetworkLogin
            // 
            this.txtNetworkLogin.Location = new System.Drawing.Point(7, 21);
            this.txtNetworkLogin.Name = "txtNetworkLogin";
            this.txtNetworkLogin.Size = new System.Drawing.Size(283, 20);
            this.txtNetworkLogin.TabIndex = 8;
            // 
            // txtHomeAirport
            // 
            this.txtHomeAirport.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtHomeAirport.Location = new System.Drawing.Point(7, 168);
            this.txtHomeAirport.MaxLength = 4;
            this.txtHomeAirport.Name = "txtHomeAirport";
            this.txtHomeAirport.Size = new System.Drawing.Size(283, 20);
            this.txtHomeAirport.TabIndex = 11;
            this.txtHomeAirport.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtHomeAirport_KeyDown);
            // 
            // txtFullName
            // 
            this.txtFullName.Location = new System.Drawing.Point(7, 119);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Size = new System.Drawing.Size(283, 20);
            this.txtFullName.TabIndex = 10;
            // 
            // lstServerName
            // 
            this.lstServerName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstServerName.FormattingEnabled = true;
            this.lstServerName.Location = new System.Drawing.Point(7, 217);
            this.lstServerName.Name = "lstServerName";
            this.lstServerName.Size = new System.Drawing.Size(283, 21);
            this.lstServerName.TabIndex = 5;
            this.lstServerName.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(7, 201);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(84, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "VATSIM Server:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 152);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Home Airport:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 105);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(60, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Your Name";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "VATSIM Password:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 5);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "VATSIM ID:";
            // 
            // chkKeepVisible
            // 
            this.chkKeepVisible.AutoSize = true;
            this.chkKeepVisible.Location = new System.Drawing.Point(308, 54);
            this.chkKeepVisible.Name = "chkKeepVisible";
            this.chkKeepVisible.Size = new System.Drawing.Size(150, 17);
            this.chkKeepVisible.TabIndex = 6;
            this.chkKeepVisible.TabStop = false;
            this.chkKeepVisible.Tag = "KeepClientWindowVisible";
            this.chkKeepVisible.Text = "Keep xPilot window visible";
            this.chkKeepVisible.UseVisualStyleBackColor = true;
            // 
            // chkAutoSquawkModeC
            // 
            this.chkAutoSquawkModeC.AutoSize = true;
            this.chkAutoSquawkModeC.Location = new System.Drawing.Point(308, 21);
            this.chkAutoSquawkModeC.Name = "chkAutoSquawkModeC";
            this.chkAutoSquawkModeC.Size = new System.Drawing.Size(218, 17);
            this.chkAutoSquawkModeC.TabIndex = 5;
            this.chkAutoSquawkModeC.TabStop = false;
            this.chkAutoSquawkModeC.Tag = "AutoSquawkModeC";
            this.chkAutoSquawkModeC.Text = "Automatically squawk mode C on takeoff";
            this.chkAutoSquawkModeC.UseVisualStyleBackColor = true;
            // 
            // chkEnableNotificationSounds
            // 
            this.chkEnableNotificationSounds.AutoSize = true;
            this.chkEnableNotificationSounds.Location = new System.Drawing.Point(308, 87);
            this.chkEnableNotificationSounds.Name = "chkEnableNotificationSounds";
            this.chkEnableNotificationSounds.Size = new System.Drawing.Size(150, 17);
            this.chkEnableNotificationSounds.TabIndex = 6;
            this.chkEnableNotificationSounds.TabStop = false;
            this.chkEnableNotificationSounds.Tag = "PlayRadioMessageAlert";
            this.chkEnableNotificationSounds.Text = "Enable notification sounds";
            this.chkEnableNotificationSounds.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(513, 485);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.TabStop = false;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // btnGuidedSetup
            // 
            this.btnGuidedSetup.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnGuidedSetup.Location = new System.Drawing.Point(3, 485);
            this.btnGuidedSetup.Name = "btnGuidedSetup";
            this.btnGuidedSetup.Size = new System.Drawing.Size(95, 23);
            this.btnGuidedSetup.TabIndex = 15;
            this.btnGuidedSetup.TabStop = false;
            this.btnGuidedSetup.Text = "Guided Setup";
            this.btnGuidedSetup.UseVisualStyleBackColor = true;
            this.btnGuidedSetup.Click += new System.EventHandler(this.btnGuidedSetup_Click);
            // 
            // lstAudioDriver
            // 
            this.lstAudioDriver.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstAudioDriver.FormattingEnabled = true;
            this.lstAudioDriver.Location = new System.Drawing.Point(7, 278);
            this.lstAudioDriver.Name = "lstAudioDriver";
            this.lstAudioDriver.Size = new System.Drawing.Size(591, 21);
            this.lstAudioDriver.TabIndex = 14;
            this.lstAudioDriver.SelectedValueChanged += new System.EventHandler(this.lstAudioDriver_SelectedValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 259);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Audio Driver";
            // 
            // lstInputDevice
            // 
            this.lstInputDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstInputDevice.FormattingEnabled = true;
            this.lstInputDevice.Location = new System.Drawing.Point(7, 332);
            this.lstInputDevice.Name = "lstInputDevice";
            this.lstInputDevice.Size = new System.Drawing.Size(284, 21);
            this.lstInputDevice.TabIndex = 16;
            this.lstInputDevice.SelectedValueChanged += new System.EventHandler(this.lstInputDevice_SelectedValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 316);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Microphone Device:";
            // 
            // lstListenDevice
            // 
            this.lstListenDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.lstListenDevice.FormattingEnabled = true;
            this.lstListenDevice.Location = new System.Drawing.Point(318, 332);
            this.lstListenDevice.Name = "lstListenDevice";
            this.lstListenDevice.Size = new System.Drawing.Size(280, 21);
            this.lstListenDevice.TabIndex = 18;
            this.lstListenDevice.SelectedValueChanged += new System.EventHandler(this.lstListenDevice_SelectedValueChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(315, 313);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(75, 13);
            this.label10.TabIndex = 17;
            this.label10.Text = "Listen Device:";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(7, 384);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(283, 35);
            this.label11.TabIndex = 19;
            this.label11.Text = "Adjust your system\'s microphone level so the volume peak indicator remains green " +
    "when speaking normally.";
            this.label11.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(314, 429);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(37, 13);
            this.label12.TabIndex = 24;
            this.label12.Text = "COM2";
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackCom2
            // 
            this.trackCom2.AutoSize = false;
            this.trackCom2.BackColor = System.Drawing.SystemColors.Window;
            this.trackCom2.LargeChange = 10;
            this.trackCom2.Location = new System.Drawing.Point(358, 420);
            this.trackCom2.Maximum = 150;
            this.trackCom2.Name = "trackCom2";
            this.trackCom2.Size = new System.Drawing.Size(212, 30);
            this.trackCom2.TabIndex = 23;
            this.trackCom2.TickFrequency = 10;
            this.trackCom2.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(315, 384);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(37, 13);
            this.label13.TabIndex = 22;
            this.label13.Text = "COM1\r\n";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // trackCom1
            // 
            this.trackCom1.AutoSize = false;
            this.trackCom1.BackColor = System.Drawing.SystemColors.Window;
            this.trackCom1.Location = new System.Drawing.Point(358, 371);
            this.trackCom1.Maximum = 150;
            this.trackCom1.Name = "trackCom1";
            this.trackCom1.Size = new System.Drawing.Size(212, 30);
            this.trackCom1.TabIndex = 21;
            this.trackCom1.TickFrequency = 10;
            this.trackCom1.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            // 
            // chkHfSquelch
            // 
            this.chkHfSquelch.AutoSize = true;
            this.chkHfSquelch.Location = new System.Drawing.Point(7, 429);
            this.chkHfSquelch.Name = "chkHfSquelch";
            this.chkHfSquelch.Size = new System.Drawing.Size(118, 17);
            this.chkHfSquelch.TabIndex = 25;
            this.chkHfSquelch.TabStop = false;
            this.chkHfSquelch.Tag = "KeepClientWindowVisible";
            this.chkHfSquelch.Text = "Enable HF Squelch";
            this.chkHfSquelch.UseVisualStyleBackColor = true;
            // 
            // chkDisableRadioEffects
            // 
            this.chkDisableRadioEffects.AutoSize = true;
            this.chkDisableRadioEffects.Location = new System.Drawing.Point(7, 457);
            this.chkDisableRadioEffects.Name = "chkDisableRadioEffects";
            this.chkDisableRadioEffects.Size = new System.Drawing.Size(128, 17);
            this.chkDisableRadioEffects.TabIndex = 26;
            this.chkDisableRadioEffects.TabStop = false;
            this.chkDisableRadioEffects.Tag = "KeepClientWindowVisible";
            this.chkDisableRadioEffects.Text = "Disable Radio Effects";
            this.chkDisableRadioEffects.UseVisualStyleBackColor = true;
            // 
            // chkFlashPrivateMessage
            // 
            this.chkFlashPrivateMessage.AutoSize = true;
            this.chkFlashPrivateMessage.Location = new System.Drawing.Point(308, 120);
            this.chkFlashPrivateMessage.Name = "chkFlashPrivateMessage";
            this.chkFlashPrivateMessage.Size = new System.Drawing.Size(230, 17);
            this.chkFlashPrivateMessage.TabIndex = 27;
            this.chkFlashPrivateMessage.TabStop = false;
            this.chkFlashPrivateMessage.Tag = "KeepClientWindowVisible";
            this.chkFlashPrivateMessage.Text = "Flash taskbar icon for new private message";
            this.chkFlashPrivateMessage.UseVisualStyleBackColor = true;
            // 
            // chkFlashSelcal
            // 
            this.chkFlashSelcal.AutoSize = true;
            this.chkFlashSelcal.Location = new System.Drawing.Point(308, 186);
            this.chkFlashSelcal.Name = "chkFlashSelcal";
            this.chkFlashSelcal.Size = new System.Drawing.Size(193, 17);
            this.chkFlashSelcal.TabIndex = 28;
            this.chkFlashSelcal.TabStop = false;
            this.chkFlashSelcal.Tag = "KeepClientWindowVisible";
            this.chkFlashSelcal.Text = "Flash taskbar icon for SELCAL alert";
            this.chkFlashSelcal.UseVisualStyleBackColor = true;
            // 
            // chkFlashDisconnect
            // 
            this.chkFlashDisconnect.AutoSize = true;
            this.chkFlashDisconnect.Location = new System.Drawing.Point(308, 219);
            this.chkFlashDisconnect.Name = "chkFlashDisconnect";
            this.chkFlashDisconnect.Size = new System.Drawing.Size(290, 17);
            this.chkFlashDisconnect.TabIndex = 29;
            this.chkFlashDisconnect.TabStop = false;
            this.chkFlashDisconnect.Tag = "KeepClientWindowVisible";
            this.chkFlashDisconnect.Text = "Flash taskbar icon when disconnected from the network";
            this.chkFlashDisconnect.UseVisualStyleBackColor = true;
            // 
            // volCom1
            // 
            this.volCom1.AutoSize = true;
            this.volCom1.Location = new System.Drawing.Point(574, 384);
            this.volCom1.Name = "volCom1";
            this.volCom1.Size = new System.Drawing.Size(21, 13);
            this.volCom1.TabIndex = 30;
            this.volCom1.Text = "0%";
            this.volCom1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // volCom2
            // 
            this.volCom2.AutoSize = true;
            this.volCom2.Location = new System.Drawing.Point(574, 429);
            this.volCom2.Name = "volCom2";
            this.volCom2.Size = new System.Drawing.Size(21, 13);
            this.volCom2.TabIndex = 31;
            this.volCom2.Text = "0%";
            this.volCom2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // chkFlashRadioMessage
            // 
            this.chkFlashRadioMessage.AutoSize = true;
            this.chkFlashRadioMessage.Location = new System.Drawing.Point(308, 153);
            this.chkFlashRadioMessage.Name = "chkFlashRadioMessage";
            this.chkFlashRadioMessage.Size = new System.Drawing.Size(221, 17);
            this.chkFlashRadioMessage.TabIndex = 32;
            this.chkFlashRadioMessage.TabStop = false;
            this.chkFlashRadioMessage.Tag = "KeepClientWindowVisible";
            this.chkFlashRadioMessage.Text = "Flash taskbar icon for new radio message";
            this.chkFlashRadioMessage.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.chkFlashRadioMessage);
            this.panel1.Controls.Add(this.lstServerName);
            this.panel1.Controls.Add(this.volCom2);
            this.panel1.Controls.Add(this.chkEnableNotificationSounds);
            this.panel1.Controls.Add(this.volCom1);
            this.panel1.Controls.Add(this.txtHomeAirport);
            this.panel1.Controls.Add(this.chkFlashDisconnect);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.chkFlashSelcal);
            this.panel1.Controls.Add(this.chkKeepVisible);
            this.panel1.Controls.Add(this.chkFlashPrivateMessage);
            this.panel1.Controls.Add(this.lstAudioDriver);
            this.panel1.Controls.Add(this.chkDisableRadioEffects);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.btnGuidedSetup);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.chkHfSquelch);
            this.panel1.Controls.Add(this.txtNetworkPassword);
            this.panel1.Controls.Add(this.label12);
            this.panel1.Controls.Add(this.lstInputDevice);
            this.panel1.Controls.Add(this.btnCancel);
            this.panel1.Controls.Add(this.txtFullName);
            this.panel1.Controls.Add(this.trackCom2);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.chkAutoSquawkModeC);
            this.panel1.Controls.Add(this.label13);
            this.panel1.Controls.Add(this.lstListenDevice);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label8);
            this.panel1.Controls.Add(this.trackCom1);
            this.panel1.Controls.Add(this.vuMeter);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.txtNetworkLogin);
            this.panel1.Controls.Add(this.label11);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(15, 15);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(604, 511);
            this.panel1.TabIndex = 33;
            // 
            // vuMeter
            // 
            this.vuMeter.Location = new System.Drawing.Point(7, 371);
            this.vuMeter.Name = "vuMeter";
            this.vuMeter.Size = new System.Drawing.Size(283, 10);
            this.vuMeter.TabIndex = 20;
            this.vuMeter.Text = "levelMeter1";
            this.vuMeter.Value = 0F;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScrollMargin = new System.Drawing.Size(6, 13);
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(634, 541);
            this.ControlBox = false;
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Padding = new System.Windows.Forms.Padding(15);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            ((System.ComponentModel.ISupportInitialize)(this.trackCom2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackCom1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtNetworkPassword;
        private System.Windows.Forms.TextBox txtNetworkLogin;
        private System.Windows.Forms.TextBox txtHomeAirport;
        private System.Windows.Forms.TextBox txtFullName;
        private System.Windows.Forms.ComboBox lstServerName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox chkKeepVisible;
        private System.Windows.Forms.CheckBox chkAutoSquawkModeC;
        private System.Windows.Forms.CheckBox chkEnableNotificationSounds;
        private Button btnGuidedSetup;
        private ComboBox lstAudioDriver;
        private Label label1;
        private ComboBox lstInputDevice;
        private Label label2;
        private ComboBox lstListenDevice;
        private Label label10;
        private Label label11;
        private LevelMeter vuMeter;
        private Label label12;
        private TrackBar trackCom2;
        private Label label13;
        private TrackBar trackCom1;
        private CheckBox chkHfSquelch;
        private CheckBox chkDisableRadioEffects;
        private CheckBox chkFlashPrivateMessage;
        private CheckBox chkFlashSelcal;
        private CheckBox chkFlashDisconnect;
        private Label volCom1;
        private Label volCom2;
        private CheckBox chkFlashRadioMessage;
        private Panel panel1;
    }
}