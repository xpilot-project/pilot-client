using XPilot.PilotClient;

namespace XPilot.PilotClient
{
    partial class ChatBox
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

        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pnlMessages = new TransparentClickPanel();
            this.rtfMessages = new System.Windows.Forms.RichTextBox();
            this.pnlTextCommand = new TransparentClickPanel();
            this.txtCommandLine = new TextCommandLine();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlMessages.SuspendLayout();
            this.pnlTextCommand.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.pnlMessages, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pnlTextCommand, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(572, 302);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // pnlMessages
            // 
            this.pnlMessages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.pnlMessages.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
            this.pnlMessages.Controls.Add(this.rtfMessages);
            this.pnlMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMessages.Location = new System.Drawing.Point(3, 3);
            this.pnlMessages.Name = "pnlMessages";
            this.pnlMessages.Padding = new System.Windows.Forms.Padding(5);
            this.pnlMessages.Size = new System.Drawing.Size(566, 268);
            this.pnlMessages.TabIndex = 0;
            // 
            // rtfMessages
            // 
            this.rtfMessages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.rtfMessages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtfMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtfMessages.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtfMessages.Location = new System.Drawing.Point(5, 5);
            this.rtfMessages.Name = "rtfMessages";
            this.rtfMessages.ReadOnly = true;
            this.rtfMessages.Size = new System.Drawing.Size(556, 258);
            this.rtfMessages.TabIndex = 0;
            this.rtfMessages.Text = "";
            this.rtfMessages.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtfMessages_LinkClicked);
            this.rtfMessages.MouseUp += new System.Windows.Forms.MouseEventHandler(this.rtfMessages_MouseUp);
            // 
            // pnlTextCommand
            // 
            this.pnlTextCommand.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.pnlTextCommand.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
            this.pnlTextCommand.Controls.Add(this.txtCommandLine);
            this.pnlTextCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTextCommand.Location = new System.Drawing.Point(3, 277);
            this.pnlTextCommand.Name = "pnlTextCommand";
            this.pnlTextCommand.Padding = new System.Windows.Forms.Padding(3);
            this.pnlTextCommand.Size = new System.Drawing.Size(566, 22);
            this.pnlTextCommand.TabIndex = 1;
            // 
            // txtCommandLine
            // 
            this.txtCommandLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.txtCommandLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtCommandLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCommandLine.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtCommandLine.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.txtCommandLine.Location = new System.Drawing.Point(3, 3);
            this.txtCommandLine.Name = "txtCommandLine";
            this.txtCommandLine.Size = new System.Drawing.Size(560, 16);
            this.txtCommandLine.TabIndex = 0;
            // 
            // ChatBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "ChatBox";
            this.Size = new System.Drawing.Size(572, 302);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlMessages.ResumeLayout(false);
            this.pnlTextCommand.ResumeLayout(false);
            this.pnlTextCommand.PerformLayout();
            this.ResumeLayout(false);

        }

        private TransparentClickPanel pnlMessages;
        private TransparentClickPanel pnlTextCommand;
        private System.Windows.Forms.RichTextBox rtfMessages;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private TextCommandLine txtCommandLine;
    }
}
