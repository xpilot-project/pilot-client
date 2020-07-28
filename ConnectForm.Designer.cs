namespace XPilot.PilotClient
{
    partial class ConnectForm
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
            this.ddlRecentAircraft = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtCallsign = new System.Windows.Forms.TextBox();
            this.txtTypeCode = new System.Windows.Forms.TextBox();
            this.txtSelcalCode = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.comboTypeCodes = new System.Windows.Forms.ComboBox();
            this.chkObserverMode = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(40, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Recent Aircraft:";
            // 
            // ddlRecentAircraft
            // 
            this.ddlRecentAircraft.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddlRecentAircraft.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ddlRecentAircraft.FormattingEnabled = true;
            this.ddlRecentAircraft.Location = new System.Drawing.Point(40, 48);
            this.ddlRecentAircraft.Name = "ddlRecentAircraft";
            this.ddlRecentAircraft.Size = new System.Drawing.Size(256, 21);
            this.ddlRecentAircraft.TabIndex = 1;
            this.ddlRecentAircraft.SelectedIndexChanged += new System.EventHandler(this.ddlRecentAircraft_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(40, 76);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Callsign:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(148, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "Type Code:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(234, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(50, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "SELCAL:";
            // 
            // txtCallsign
            // 
            this.txtCallsign.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtCallsign.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCallsign.Location = new System.Drawing.Point(40, 92);
            this.txtCallsign.MaxLength = 7;
            this.txtCallsign.Name = "txtCallsign";
            this.txtCallsign.Size = new System.Drawing.Size(100, 20);
            this.txtCallsign.TabIndex = 5;
            this.txtCallsign.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCallsign_KeyDown);
            // 
            // txtTypeCode
            // 
            this.txtTypeCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtTypeCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTypeCode.Location = new System.Drawing.Point(151, 92);
            this.txtTypeCode.Name = "txtTypeCode";
            this.txtTypeCode.Size = new System.Drawing.Size(75, 20);
            this.txtTypeCode.TabIndex = 6;
            this.txtTypeCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtTypeCode_KeyDown);
            this.txtTypeCode.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtTypeCode_KeyUp);
            // 
            // txtSelcalCode
            // 
            this.txtSelcalCode.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtSelcalCode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtSelcalCode.Location = new System.Drawing.Point(237, 92);
            this.txtSelcalCode.MaxLength = 5;
            this.txtSelcalCode.Name = "txtSelcalCode";
            this.txtSelcalCode.Size = new System.Drawing.Size(59, 20);
            this.txtSelcalCode.TabIndex = 7;
            this.txtSelcalCode.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSelcalCode_KeyDown);
            // 
            // btnConnect
            // 
            this.btnConnect.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnConnect.Location = new System.Drawing.Point(40, 142);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(178, 23);
            this.btnConnect.TabIndex = 8;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(224, 142);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(72, 23);
            this.btnCancel.TabIndex = 9;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // comboTypeCodes
            // 
            this.comboTypeCodes.DropDownWidth = 225;
            this.comboTypeCodes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboTypeCodes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboTypeCodes.FormattingEnabled = true;
            this.comboTypeCodes.IntegralHeight = false;
            this.comboTypeCodes.Location = new System.Drawing.Point(151, 92);
            this.comboTypeCodes.Name = "comboTypeCodes";
            this.comboTypeCodes.Size = new System.Drawing.Size(75, 21);
            this.comboTypeCodes.TabIndex = 10;
            this.comboTypeCodes.SelectedIndexChanged += new System.EventHandler(this.comboTypeCodes_SelectedIndexChanged);
            // 
            // chkObserverMode
            // 
            this.chkObserverMode.AutoSize = true;
            this.chkObserverMode.Location = new System.Drawing.Point(40, 119);
            this.chkObserverMode.Name = "chkObserverMode";
            this.chkObserverMode.Size = new System.Drawing.Size(258, 17);
            this.chkObserverMode.TabIndex = 11;
            this.chkObserverMode.Text = "Connect in shared cockpit mode (observer mode)";
            this.chkObserverMode.UseVisualStyleBackColor = true;
            this.chkObserverMode.CheckedChanged += new System.EventHandler(this.ChkObserverMode_CheckedChanged);
            // 
            // ConnectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 196);
            this.ControlBox = false;
            this.Controls.Add(this.chkObserverMode);
            this.Controls.Add(this.txtTypeCode);
            this.Controls.Add(this.comboTypeCodes);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.txtSelcalCode);
            this.Controls.Add(this.txtCallsign);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ddlRecentAircraft);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Connect to VATSIM";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddlRecentAircraft;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtCallsign;
        private System.Windows.Forms.TextBox txtTypeCode;
        private System.Windows.Forms.TextBox txtSelcalCode;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ComboBox comboTypeCodes;
        private System.Windows.Forms.CheckBox chkObserverMode;
    }
}