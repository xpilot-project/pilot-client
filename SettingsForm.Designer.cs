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
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.lblPTTValue = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.btnSetNewPTT = new System.Windows.Forms.Button();
            this.btnClearPTT = new System.Windows.Forms.Button();
            this.lblAudioHelp = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TrackInputVolumeDb = new System.Windows.Forms.TrackBar();
            this.lblVolumeCom1 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.TrackCom1Volume = new System.Windows.Forms.TrackBar();
            this.audioInputDevice = new System.Windows.Forms.ComboBox();
            this.label17 = new System.Windows.Forms.Label();
            this.audioOutputDevice = new System.Windows.Forms.ComboBox();
            this.label18 = new System.Windows.Forms.Label();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label15 = new System.Windows.Forms.Label();
            this.lblDisplayShortcut = new System.Windows.Forms.Label();
            this.btnClearDisplayKey = new System.Windows.Forms.Button();
            this.btnSetDisplayKey = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtNetworkPassword = new System.Windows.Forms.TextBox();
            this.txtNetworkLogin = new System.Windows.Forms.TextBox();
            this.txtHomeAirport = new System.Windows.Forms.TextBox();
            this.txtFullName = new System.Windows.Forms.TextBox();
            this.ddlServerName = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.spinPluginPort = new System.Windows.Forms.NumericUpDown();
            this.chkVolumeKnobVolume = new System.Windows.Forms.CheckBox();
            this.cbUpdateChannel = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkUpdates = new System.Windows.Forms.CheckBox();
            this.chkKeepVisible = new System.Windows.Forms.CheckBox();
            this.chkAutoSquawkModeC = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkRadioMessageSound = new System.Windows.Forms.CheckBox();
            this.chkSelcalSound = new System.Windows.Forms.CheckBox();
            this.chkFlashDisconnect = new System.Windows.Forms.CheckBox();
            this.chkFlashSelcal = new System.Windows.Forms.CheckBox();
            this.chkFlashRadioMessage = new System.Windows.Forms.CheckBox();
            this.chkFlashPrivateMessage = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.tabAudio = new System.Windows.Forms.TabPage();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.vhfEqualizer = new System.Windows.Forms.ComboBox();
            this.chkDisableRadioEffects = new System.Windows.Forms.CheckBox();
            this.lblVolumeCom2 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.TrackCom2Volume = new System.Windows.Forms.TrackBar();
            this.inputVolumeLabel = new System.Windows.Forms.Label();
            this.btnGuidedSetup = new System.Windows.Forms.Button();
            this.levelMeterInput = new XPilot.PilotClient.LevelMeter();
            this.groupBox8.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackInputVolumeDb)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackCom1Volume)).BeginInit();
            this.groupBox7.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinPluginPort)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabAudio.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackCom2Volume)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.lblPTTValue);
            this.groupBox8.Controls.Add(this.label20);
            this.groupBox8.Controls.Add(this.btnSetNewPTT);
            this.groupBox8.Controls.Add(this.btnClearPTT);
            this.groupBox8.Location = new System.Drawing.Point(15, 309);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(594, 101);
            this.groupBox8.TabIndex = 12;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Push to Talk (PTT) Device Configuration";
            // 
            // lblPTTValue
            // 
            this.lblPTTValue.AutoSize = true;
            this.lblPTTValue.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPTTValue.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(100)))), ((int)(((byte)(173)))));
            this.lblPTTValue.Location = new System.Drawing.Point(139, 68);
            this.lblPTTValue.Name = "lblPTTValue";
            this.lblPTTValue.Size = new System.Drawing.Size(37, 13);
            this.lblPTTValue.TabIndex = 6;
            this.lblPTTValue.Text = "None";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(17, 68);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(125, 13);
            this.label20.TabIndex = 5;
            this.label20.Text = "Current PTT Assignment:";
            // 
            // btnSetNewPTT
            // 
            this.btnSetNewPTT.Location = new System.Drawing.Point(17, 32);
            this.btnSetNewPTT.Name = "btnSetNewPTT";
            this.btnSetNewPTT.Size = new System.Drawing.Size(182, 23);
            this.btnSetNewPTT.TabIndex = 3;
            this.btnSetNewPTT.Text = "Set New PTT Key or Button";
            this.btnSetNewPTT.UseVisualStyleBackColor = true;
            this.btnSetNewPTT.Click += new System.EventHandler(this.btnSetNewPTT_Click);
            // 
            // btnClearPTT
            // 
            this.btnClearPTT.Enabled = false;
            this.btnClearPTT.Location = new System.Drawing.Point(205, 32);
            this.btnClearPTT.Name = "btnClearPTT";
            this.btnClearPTT.Size = new System.Drawing.Size(159, 23);
            this.btnClearPTT.TabIndex = 4;
            this.btnClearPTT.Text = "Clear Current PTT Assignment";
            this.btnClearPTT.UseVisualStyleBackColor = true;
            this.btnClearPTT.Click += new System.EventHandler(this.BtnClearPTT_Click);
            // 
            // lblAudioHelp
            // 
            this.lblAudioHelp.Location = new System.Drawing.Point(24, 232);
            this.lblAudioHelp.Name = "lblAudioHelp";
            this.lblAudioHelp.Size = new System.Drawing.Size(360, 26);
            this.lblAudioHelp.TabIndex = 16;
            this.lblAudioHelp.Text = "Adjust the mic volume slider so that the peak level indicator remains in the gree" +
    "n band while speaking normally.";
            this.lblAudioHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 163);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 26);
            this.label2.TabIndex = 14;
            this.label2.Text = "Mic\r\nVolume";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TrackInputVolumeDb
            // 
            this.TrackInputVolumeDb.AutoSize = false;
            this.TrackInputVolumeDb.BackColor = System.Drawing.SystemColors.Window;
            this.TrackInputVolumeDb.LargeChange = 1;
            this.TrackInputVolumeDb.Location = new System.Drawing.Point(72, 161);
            this.TrackInputVolumeDb.Maximum = 72;
            this.TrackInputVolumeDb.Minimum = -60;
            this.TrackInputVolumeDb.Name = "TrackInputVolumeDb";
            this.TrackInputVolumeDb.Size = new System.Drawing.Size(487, 30);
            this.TrackInputVolumeDb.TabIndex = 13;
            this.TrackInputVolumeDb.TickFrequency = 8;
            this.TrackInputVolumeDb.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.TrackInputVolumeDb.Scroll += new System.EventHandler(this.TrackInputVolumeDb_Scroll);
            // 
            // lblVolumeCom1
            // 
            this.lblVolumeCom1.AutoSize = true;
            this.lblVolumeCom1.Location = new System.Drawing.Point(561, 84);
            this.lblVolumeCom1.Name = "lblVolumeCom1";
            this.lblVolumeCom1.Size = new System.Drawing.Size(13, 13);
            this.lblVolumeCom1.TabIndex = 12;
            this.lblVolumeCom1.Text = "0";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(24, 77);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(42, 26);
            this.label16.TabIndex = 11;
            this.label16.Text = "COM1\r\nVolume";
            this.label16.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TrackCom1Volume
            // 
            this.TrackCom1Volume.AutoSize = false;
            this.TrackCom1Volume.BackColor = System.Drawing.SystemColors.Window;
            this.TrackCom1Volume.LargeChange = 1;
            this.TrackCom1Volume.Location = new System.Drawing.Point(72, 75);
            this.TrackCom1Volume.Maximum = 72;
            this.TrackCom1Volume.Minimum = -60;
            this.TrackCom1Volume.Name = "TrackCom1Volume";
            this.TrackCom1Volume.Size = new System.Drawing.Size(487, 30);
            this.TrackCom1Volume.TabIndex = 10;
            this.TrackCom1Volume.TickFrequency = 8;
            this.TrackCom1Volume.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.TrackCom1Volume.Scroll += new System.EventHandler(this.Com1Volume_Scroll);
            // 
            // audioInputDevice
            // 
            this.audioInputDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.audioInputDevice.FormattingEnabled = true;
            this.audioInputDevice.Location = new System.Drawing.Point(23, 41);
            this.audioInputDevice.Name = "audioInputDevice";
            this.audioInputDevice.Size = new System.Drawing.Size(271, 21);
            this.audioInputDevice.TabIndex = 7;
            this.audioInputDevice.SelectedIndexChanged += new System.EventHandler(this.ddlInputDeviceName_SelectedIndexChanged);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(22, 22);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(103, 13);
            this.label17.TabIndex = 0;
            this.label17.Text = "Microphone Device:";
            // 
            // audioOutputDevice
            // 
            this.audioOutputDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.audioOutputDevice.FormattingEnabled = true;
            this.audioOutputDevice.Location = new System.Drawing.Point(316, 41);
            this.audioOutputDevice.Name = "audioOutputDevice";
            this.audioOutputDevice.Size = new System.Drawing.Size(255, 21);
            this.audioOutputDevice.TabIndex = 8;
            this.audioOutputDevice.SelectedIndexChanged += new System.EventHandler(this.ddlOutputDeviceName_SelectedIndexChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(316, 25);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(79, 13);
            this.label18.TabIndex = 1;
            this.label18.Text = "Output Device:";
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label15);
            this.groupBox7.Controls.Add(this.lblDisplayShortcut);
            this.groupBox7.Controls.Add(this.btnClearDisplayKey);
            this.groupBox7.Controls.Add(this.btnSetDisplayKey);
            this.groupBox7.Controls.Add(this.label13);
            this.groupBox7.Location = new System.Drawing.Point(22, 324);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(582, 88);
            this.groupBox7.TabIndex = 4;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "xPilot Toggle";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(14, 60);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(144, 13);
            this.label15.TabIndex = 6;
            this.label15.Text = "Current Shortcut Assignment:";
            // 
            // lblDisplayShortcut
            // 
            this.lblDisplayShortcut.AutoSize = true;
            this.lblDisplayShortcut.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDisplayShortcut.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(100)))), ((int)(((byte)(173)))));
            this.lblDisplayShortcut.Location = new System.Drawing.Point(164, 60);
            this.lblDisplayShortcut.Name = "lblDisplayShortcut";
            this.lblDisplayShortcut.Size = new System.Drawing.Size(37, 13);
            this.lblDisplayShortcut.TabIndex = 16;
            this.lblDisplayShortcut.Text = "None";
            // 
            // btnClearDisplayKey
            // 
            this.btnClearDisplayKey.Enabled = false;
            this.btnClearDisplayKey.Location = new System.Drawing.Point(436, 51);
            this.btnClearDisplayKey.Name = "btnClearDisplayKey";
            this.btnClearDisplayKey.Size = new System.Drawing.Size(127, 23);
            this.btnClearDisplayKey.TabIndex = 15;
            this.btnClearDisplayKey.TabStop = false;
            this.btnClearDisplayKey.Text = "Clear Hokey";
            this.btnClearDisplayKey.UseVisualStyleBackColor = true;
            this.btnClearDisplayKey.Click += new System.EventHandler(this.BtnClearToggle_Click);
            // 
            // btnSetDisplayKey
            // 
            this.btnSetDisplayKey.Location = new System.Drawing.Point(436, 21);
            this.btnSetDisplayKey.Name = "btnSetDisplayKey";
            this.btnSetDisplayKey.Size = new System.Drawing.Size(127, 23);
            this.btnSetDisplayKey.TabIndex = 14;
            this.btnSetDisplayKey.TabStop = false;
            this.btnSetDisplayKey.Text = "Set Toggle Hotkey";
            this.btnSetDisplayKey.UseVisualStyleBackColor = true;
            this.btnSetDisplayKey.Click += new System.EventHandler(this.BtnSetToggleKey_Click);
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(14, 24);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(404, 29);
            this.label13.TabIndex = 12;
            this.label13.Text = "Set a shortcut key to toggle the visibility of xPilot. This is useful if you are " +
    "in fullscreen mode and need to bring xPilot forward.";
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtNetworkPassword);
            this.groupBox3.Controls.Add(this.txtNetworkLogin);
            this.groupBox3.Controls.Add(this.txtHomeAirport);
            this.groupBox3.Controls.Add(this.txtFullName);
            this.groupBox3.Controls.Add(this.ddlServerName);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label8);
            this.groupBox3.Controls.Add(this.label7);
            this.groupBox3.Controls.Add(this.label6);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Location = new System.Drawing.Point(313, 14);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(291, 304);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Network Connection";
            // 
            // txtNetworkPassword
            // 
            this.txtNetworkPassword.Location = new System.Drawing.Point(19, 100);
            this.txtNetworkPassword.Name = "txtNetworkPassword";
            this.txtNetworkPassword.PasswordChar = '●';
            this.txtNetworkPassword.Size = new System.Drawing.Size(253, 20);
            this.txtNetworkPassword.TabIndex = 9;
            // 
            // txtNetworkLogin
            // 
            this.txtNetworkLogin.Location = new System.Drawing.Point(19, 47);
            this.txtNetworkLogin.Name = "txtNetworkLogin";
            this.txtNetworkLogin.Size = new System.Drawing.Size(253, 20);
            this.txtNetworkLogin.TabIndex = 8;
            // 
            // txtHomeAirport
            // 
            this.txtHomeAirport.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtHomeAirport.Location = new System.Drawing.Point(19, 206);
            this.txtHomeAirport.MaxLength = 4;
            this.txtHomeAirport.Name = "txtHomeAirport";
            this.txtHomeAirport.Size = new System.Drawing.Size(250, 20);
            this.txtHomeAirport.TabIndex = 11;
            this.txtHomeAirport.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtHomeAirport_KeyDown);
            // 
            // txtFullName
            // 
            this.txtFullName.Location = new System.Drawing.Point(19, 153);
            this.txtFullName.Name = "txtFullName";
            this.txtFullName.Size = new System.Drawing.Size(253, 20);
            this.txtFullName.TabIndex = 10;
            // 
            // ddlServerName
            // 
            this.ddlServerName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlServerName.FormattingEnabled = true;
            this.ddlServerName.Location = new System.Drawing.Point(19, 259);
            this.ddlServerName.Name = "ddlServerName";
            this.ddlServerName.Size = new System.Drawing.Size(253, 21);
            this.ddlServerName.TabIndex = 5;
            this.ddlServerName.TabStop = false;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(19, 242);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(84, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "VATSIM Server:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 189);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 13);
            this.label8.TabIndex = 3;
            this.label8.Text = "Home Airport:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 136);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(35, 13);
            this.label7.TabIndex = 2;
            this.label7.Text = "Name";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 83);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(99, 13);
            this.label6.TabIndex = 1;
            this.label6.Text = "VATSIM Password:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(64, 13);
            this.label5.TabIndex = 0;
            this.label5.Text = "VATSIM ID:";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.spinPluginPort);
            this.groupBox2.Controls.Add(this.chkVolumeKnobVolume);
            this.groupBox2.Controls.Add(this.cbUpdateChannel);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.chkUpdates);
            this.groupBox2.Controls.Add(this.chkKeepVisible);
            this.groupBox2.Controls.Add(this.chkAutoSquawkModeC);
            this.groupBox2.Location = new System.Drawing.Point(22, 190);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(285, 128);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Miscellaneous";
            // 
            // spinPluginPort
            // 
            this.spinPluginPort.Location = new System.Drawing.Point(135, 100);
            this.spinPluginPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.spinPluginPort.Minimum = new decimal(new int[] {
            1024,
            0,
            0,
            0});
            this.spinPluginPort.Name = "spinPluginPort";
            this.spinPluginPort.Size = new System.Drawing.Size(66, 20);
            this.spinPluginPort.TabIndex = 9;
            this.spinPluginPort.Value = new decimal(new int[] {
            45001,
            0,
            0,
            0});
            this.spinPluginPort.ValueChanged += new System.EventHandler(this.spinPluginPort_ValueChanged);
            // 
            // chkVolumeKnobVolume
            // 
            this.chkVolumeKnobVolume.AutoSize = true;
            this.chkVolumeKnobVolume.Location = new System.Drawing.Point(10, 83);
            this.chkVolumeKnobVolume.Name = "chkVolumeKnobVolume";
            this.chkVolumeKnobVolume.Size = new System.Drawing.Size(200, 17);
            this.chkVolumeKnobVolume.TabIndex = 11;
            this.chkVolumeKnobVolume.TabStop = false;
            this.chkVolumeKnobVolume.Tag = "VolumeKnobsControlVolume";
            this.chkVolumeKnobVolume.Text = "Aircraft volume knobs control volume";
            this.chkVolumeKnobVolume.UseVisualStyleBackColor = true;
            this.chkVolumeKnobVolume.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // cbUpdateChannel
            // 
            this.cbUpdateChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUpdateChannel.FormattingEnabled = true;
            this.cbUpdateChannel.Location = new System.Drawing.Point(211, 38);
            this.cbUpdateChannel.Name = "cbUpdateChannel";
            this.cbUpdateChannel.Size = new System.Drawing.Size(68, 21);
            this.cbUpdateChannel.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(125, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "X-Plane Plugin TCP Port:";
            // 
            // chkUpdates
            // 
            this.chkUpdates.AutoSize = true;
            this.chkUpdates.Location = new System.Drawing.Point(10, 41);
            this.chkUpdates.Name = "chkUpdates";
            this.chkUpdates.Size = new System.Drawing.Size(205, 17);
            this.chkUpdates.TabIndex = 7;
            this.chkUpdates.TabStop = false;
            this.chkUpdates.Tag = "CheckForUpdates";
            this.chkUpdates.Text = "Automatically check for xPilot updates";
            this.chkUpdates.UseVisualStyleBackColor = true;
            this.chkUpdates.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // chkKeepVisible
            // 
            this.chkKeepVisible.AutoSize = true;
            this.chkKeepVisible.Location = new System.Drawing.Point(10, 62);
            this.chkKeepVisible.Name = "chkKeepVisible";
            this.chkKeepVisible.Size = new System.Drawing.Size(150, 17);
            this.chkKeepVisible.TabIndex = 6;
            this.chkKeepVisible.TabStop = false;
            this.chkKeepVisible.Tag = "KeepClientWindowVisible";
            this.chkKeepVisible.Text = "Keep xPilot window visible";
            this.chkKeepVisible.UseVisualStyleBackColor = true;
            this.chkKeepVisible.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // chkAutoSquawkModeC
            // 
            this.chkAutoSquawkModeC.AutoSize = true;
            this.chkAutoSquawkModeC.Location = new System.Drawing.Point(10, 20);
            this.chkAutoSquawkModeC.Name = "chkAutoSquawkModeC";
            this.chkAutoSquawkModeC.Size = new System.Drawing.Size(218, 17);
            this.chkAutoSquawkModeC.TabIndex = 5;
            this.chkAutoSquawkModeC.TabStop = false;
            this.chkAutoSquawkModeC.Tag = "AutoSquawkModeC";
            this.chkAutoSquawkModeC.Text = "Automatically squawk mode C on takeoff";
            this.chkAutoSquawkModeC.UseVisualStyleBackColor = true;
            this.chkAutoSquawkModeC.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkRadioMessageSound);
            this.groupBox1.Controls.Add(this.chkSelcalSound);
            this.groupBox1.Controls.Add(this.chkFlashDisconnect);
            this.groupBox1.Controls.Add(this.chkFlashSelcal);
            this.groupBox1.Controls.Add(this.chkFlashRadioMessage);
            this.groupBox1.Controls.Add(this.chkFlashPrivateMessage);
            this.groupBox1.Location = new System.Drawing.Point(22, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(285, 168);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Notifications";
            // 
            // chkRadioMessageSound
            // 
            this.chkRadioMessageSound.AutoSize = true;
            this.chkRadioMessageSound.Location = new System.Drawing.Point(10, 138);
            this.chkRadioMessageSound.Name = "chkRadioMessageSound";
            this.chkRadioMessageSound.Size = new System.Drawing.Size(275, 17);
            this.chkRadioMessageSound.TabIndex = 6;
            this.chkRadioMessageSound.TabStop = false;
            this.chkRadioMessageSound.Tag = "PlayRadioMessageAlert";
            this.chkRadioMessageSound.Text = "Play aural notification for all incoming radio messages";
            this.chkRadioMessageSound.UseVisualStyleBackColor = true;
            this.chkRadioMessageSound.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // chkSelcalSound
            // 
            this.chkSelcalSound.AutoSize = true;
            this.chkSelcalSound.Location = new System.Drawing.Point(10, 115);
            this.chkSelcalSound.Name = "chkSelcalSound";
            this.chkSelcalSound.Size = new System.Drawing.Size(236, 17);
            this.chkSelcalSound.TabIndex = 4;
            this.chkSelcalSound.TabStop = false;
            this.chkSelcalSound.Tag = "PlayGenericSelCalAlert";
            this.chkSelcalSound.Text = "Play generic SELCAL alert notification sound";
            this.chkSelcalSound.UseVisualStyleBackColor = true;
            this.chkSelcalSound.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // chkFlashDisconnect
            // 
            this.chkFlashDisconnect.AutoSize = true;
            this.chkFlashDisconnect.Location = new System.Drawing.Point(10, 92);
            this.chkFlashDisconnect.Name = "chkFlashDisconnect";
            this.chkFlashDisconnect.Size = new System.Drawing.Size(272, 17);
            this.chkFlashDisconnect.TabIndex = 3;
            this.chkFlashDisconnect.TabStop = false;
            this.chkFlashDisconnect.Tag = "FlashTaskbarDisconnect";
            this.chkFlashDisconnect.Text = "Flash taskbar icon when disconnected from network";
            this.chkFlashDisconnect.UseVisualStyleBackColor = true;
            this.chkFlashDisconnect.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // chkFlashSelcal
            // 
            this.chkFlashSelcal.AutoSize = true;
            this.chkFlashSelcal.Location = new System.Drawing.Point(10, 69);
            this.chkFlashSelcal.Name = "chkFlashSelcal";
            this.chkFlashSelcal.Size = new System.Drawing.Size(215, 17);
            this.chkFlashSelcal.TabIndex = 2;
            this.chkFlashSelcal.TabStop = false;
            this.chkFlashSelcal.Tag = "FlashTaskbarSelCal";
            this.chkFlashSelcal.Text = "Flash taskbar icon for SELCAL message";
            this.chkFlashSelcal.UseVisualStyleBackColor = true;
            this.chkFlashSelcal.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // chkFlashRadioMessage
            // 
            this.chkFlashRadioMessage.AutoSize = true;
            this.chkFlashRadioMessage.Location = new System.Drawing.Point(10, 46);
            this.chkFlashRadioMessage.Name = "chkFlashRadioMessage";
            this.chkFlashRadioMessage.Size = new System.Drawing.Size(218, 17);
            this.chkFlashRadioMessage.TabIndex = 1;
            this.chkFlashRadioMessage.TabStop = false;
            this.chkFlashRadioMessage.Tag = "FlashTaskbarRadioMessage";
            this.chkFlashRadioMessage.Text = "Flash taskbar icon for text radio message";
            this.chkFlashRadioMessage.UseVisualStyleBackColor = true;
            this.chkFlashRadioMessage.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // chkFlashPrivateMessage
            // 
            this.chkFlashPrivateMessage.AutoSize = true;
            this.chkFlashPrivateMessage.Location = new System.Drawing.Point(10, 23);
            this.chkFlashPrivateMessage.Name = "chkFlashPrivateMessage";
            this.chkFlashPrivateMessage.Size = new System.Drawing.Size(230, 17);
            this.chkFlashPrivateMessage.TabIndex = 0;
            this.chkFlashPrivateMessage.TabStop = false;
            this.chkFlashPrivateMessage.Tag = "FlashTaskbarPrivateMessage";
            this.chkFlashPrivateMessage.Text = "Flash taskbar icon for new private message";
            this.chkFlashPrivateMessage.UseVisualStyleBackColor = true;
            this.chkFlashPrivateMessage.CheckedChanged += new System.EventHandler(this.CheckboxCheckedChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(577, 489);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 23);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.TabStop = false;
            this.btnCancel.Text = "Close";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.BtnClose_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabGeneral);
            this.tabControl1.Controls.Add(this.tabAudio);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabControl1.Location = new System.Drawing.Point(30, 30);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(635, 453);
            this.tabControl1.TabIndex = 13;
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.groupBox2);
            this.tabGeneral.Controls.Add(this.groupBox7);
            this.tabGeneral.Controls.Add(this.groupBox3);
            this.tabGeneral.Controls.Add(this.groupBox1);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tabGeneral.Size = new System.Drawing.Size(627, 427);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // tabAudio
            // 
            this.tabAudio.Controls.Add(this.groupBox4);
            this.tabAudio.Controls.Add(this.groupBox8);
            this.tabAudio.Location = new System.Drawing.Point(4, 22);
            this.tabAudio.Name = "tabAudio";
            this.tabAudio.Padding = new System.Windows.Forms.Padding(3);
            this.tabAudio.Size = new System.Drawing.Size(627, 427);
            this.tabAudio.TabIndex = 1;
            this.tabAudio.Text = "Audio/Push to Talk (PTT)";
            this.tabAudio.UseVisualStyleBackColor = true;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.vhfEqualizer);
            this.groupBox4.Controls.Add(this.chkDisableRadioEffects);
            this.groupBox4.Controls.Add(this.lblVolumeCom2);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.TrackCom2Volume);
            this.groupBox4.Controls.Add(this.lblVolumeCom1);
            this.groupBox4.Controls.Add(this.inputVolumeLabel);
            this.groupBox4.Controls.Add(this.label16);
            this.groupBox4.Controls.Add(this.audioInputDevice);
            this.groupBox4.Controls.Add(this.TrackCom1Volume);
            this.groupBox4.Controls.Add(this.lblAudioHelp);
            this.groupBox4.Controls.Add(this.label18);
            this.groupBox4.Controls.Add(this.levelMeterInput);
            this.groupBox4.Controls.Add(this.audioOutputDevice);
            this.groupBox4.Controls.Add(this.label2);
            this.groupBox4.Controls.Add(this.label17);
            this.groupBox4.Controls.Add(this.TrackInputVolumeDb);
            this.groupBox4.Location = new System.Drawing.Point(15, 16);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(596, 287);
            this.groupBox4.TabIndex = 14;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Audio Devices";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(400, 229);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "VHF Equalizer";
            // 
            // vhfEqualizer
            // 
            this.vhfEqualizer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.vhfEqualizer.FormattingEnabled = true;
            this.vhfEqualizer.Location = new System.Drawing.Point(400, 244);
            this.vhfEqualizer.Name = "vhfEqualizer";
            this.vhfEqualizer.Size = new System.Drawing.Size(174, 21);
            this.vhfEqualizer.TabIndex = 23;
            this.vhfEqualizer.SelectionChangeCommitted += new System.EventHandler(this.vhfEqualizer_SelectionChangeCommitted);
            // 
            // chkDisableRadioEffects
            // 
            this.chkDisableRadioEffects.AutoSize = true;
            this.chkDisableRadioEffects.Location = new System.Drawing.Point(400, 205);
            this.chkDisableRadioEffects.Name = "chkDisableRadioEffects";
            this.chkDisableRadioEffects.Size = new System.Drawing.Size(160, 17);
            this.chkDisableRadioEffects.TabIndex = 22;
            this.chkDisableRadioEffects.Text = "Disable realistic radio effects";
            this.chkDisableRadioEffects.UseVisualStyleBackColor = true;
            this.chkDisableRadioEffects.CheckedChanged += new System.EventHandler(this.chkDisableRadioEffects_CheckedChanged);
            // 
            // lblVolumeCom2
            // 
            this.lblVolumeCom2.AutoSize = true;
            this.lblVolumeCom2.Location = new System.Drawing.Point(561, 127);
            this.lblVolumeCom2.Name = "lblVolumeCom2";
            this.lblVolumeCom2.Size = new System.Drawing.Size(13, 13);
            this.lblVolumeCom2.TabIndex = 21;
            this.lblVolumeCom2.Text = "0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 120);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(42, 26);
            this.label4.TabIndex = 20;
            this.label4.Text = "COM2\r\nVolume";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // TrackCom2Volume
            // 
            this.TrackCom2Volume.AutoSize = false;
            this.TrackCom2Volume.BackColor = System.Drawing.SystemColors.Window;
            this.TrackCom2Volume.LargeChange = 1;
            this.TrackCom2Volume.Location = new System.Drawing.Point(72, 118);
            this.TrackCom2Volume.Maximum = 72;
            this.TrackCom2Volume.Minimum = -60;
            this.TrackCom2Volume.Name = "TrackCom2Volume";
            this.TrackCom2Volume.Size = new System.Drawing.Size(487, 30);
            this.TrackCom2Volume.TabIndex = 19;
            this.TrackCom2Volume.TickFrequency = 8;
            this.TrackCom2Volume.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
            this.TrackCom2Volume.Scroll += new System.EventHandler(this.Com2Volume_Scroll);
            // 
            // inputVolumeLabel
            // 
            this.inputVolumeLabel.AutoSize = true;
            this.inputVolumeLabel.Location = new System.Drawing.Point(561, 170);
            this.inputVolumeLabel.Name = "inputVolumeLabel";
            this.inputVolumeLabel.Size = new System.Drawing.Size(13, 13);
            this.inputVolumeLabel.TabIndex = 18;
            this.inputVolumeLabel.Text = "0";
            // 
            // btnGuidedSetup
            // 
            this.btnGuidedSetup.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnGuidedSetup.Location = new System.Drawing.Point(30, 489);
            this.btnGuidedSetup.Name = "btnGuidedSetup";
            this.btnGuidedSetup.Size = new System.Drawing.Size(95, 23);
            this.btnGuidedSetup.TabIndex = 15;
            this.btnGuidedSetup.TabStop = false;
            this.btnGuidedSetup.Text = "Guided Setup";
            this.btnGuidedSetup.UseVisualStyleBackColor = true;
            this.btnGuidedSetup.Click += new System.EventHandler(this.btnGuidedSetup_Click);
            // 
            // levelMeterInput
            // 
            this.levelMeterInput.Location = new System.Drawing.Point(24, 214);
            this.levelMeterInput.Name = "levelMeterInput";
            this.levelMeterInput.Size = new System.Drawing.Size(357, 10);
            this.levelMeterInput.TabIndex = 17;
            this.levelMeterInput.Text = "levelMeter1";
            this.levelMeterInput.Value = 0F;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScrollMargin = new System.Drawing.Size(6, 13);
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(695, 538);
            this.ControlBox = false;
            this.Controls.Add(this.btnGuidedSetup);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnCancel);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Padding = new System.Windows.Forms.Padding(30);
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "xPilot Settings";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.SettingsForm_KeyDown);
            this.groupBox8.ResumeLayout(false);
            this.groupBox8.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackInputVolumeDb)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.TrackCom1Volume)).EndInit();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.spinPluginPort)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabAudio.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.TrackCom2Volume)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox txtNetworkPassword;
        private System.Windows.Forms.TextBox txtNetworkLogin;
        private System.Windows.Forms.TextBox txtHomeAirport;
        private System.Windows.Forms.TextBox txtFullName;
        private System.Windows.Forms.ComboBox ddlServerName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox chkUpdates;
        private System.Windows.Forms.CheckBox chkKeepVisible;
        private System.Windows.Forms.CheckBox chkAutoSquawkModeC;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkFlashDisconnect;
        private System.Windows.Forms.CheckBox chkFlashSelcal;
        private System.Windows.Forms.CheckBox chkFlashRadioMessage;
        private System.Windows.Forms.CheckBox chkFlashPrivateMessage;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Button btnSetDisplayKey;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button btnClearDisplayKey;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lblDisplayShortcut;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.Label lblPTTValue;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button btnSetNewPTT;
        private System.Windows.Forms.Button btnClearPTT;
        private System.Windows.Forms.Label lblVolumeCom1;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox audioInputDevice;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.ComboBox audioOutputDevice;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TrackBar TrackInputVolumeDb;
        private System.Windows.Forms.Label lblAudioHelp;
        private LevelMeter levelMeterInput;
        private System.Windows.Forms.CheckBox chkSelcalSound;
        private System.Windows.Forms.CheckBox chkRadioMessageSound;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabAudio;
        private ComboBox cbUpdateChannel;
        private Label inputVolumeLabel;
        private GroupBox groupBox4;
        private Label lblVolumeCom2;
        private Label label4;
        private TrackBar TrackCom2Volume;
        private CheckBox chkDisableRadioEffects;
        private Label label1;
        private ComboBox vhfEqualizer;
        private NumericUpDown spinPluginPort;
        private Label label3;
        private Button btnGuidedSetup;
        private TrackBar TrackCom1Volume;
        private CheckBox chkVolumeKnobVolume;
    }
}