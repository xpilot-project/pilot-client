using XPilot.PilotClient;

namespace XPilot.PilotClient
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Center   ");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Approach/Departure     ");
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Tower   ");
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("Ground   ");
            System.Windows.Forms.TreeNode treeNode5 = new System.Windows.Forms.TreeNode("Clearance Delivery   ");
            System.Windows.Forms.TreeNode treeNode6 = new System.Windows.Forms.TreeNode("ATIS   ");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.controllerTreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.requestControllerInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.startPrivateChat = new System.Windows.Forms.ToolStripMenuItem();
            this.bwVersionCheck = new System.ComponentModel.BackgroundWorker();
            this.hfTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.pnlTabs = new XPilot.PilotClient.TransparentClickPanel();
            this.tabControl = new XPilot.PilotClient.CustomTabControl();
            this.tabPageMessages = new System.Windows.Forms.TabPage();
            this.ChatMessageBox = new XPilot.PilotClient.ChatBox();
            this.pnlToolbar = new XPilot.PilotClient.TransparentClickPanel();
            this.btnIdent = new XPilot.PilotClient.FlatButton();
            this.chkModeC = new XPilot.PilotClient.FlatButton();
            this.btnMinimize = new XPilot.PilotClient.FlatButton();
            this.btnClose = new XPilot.PilotClient.FlatButton();
            this.lblCallsign = new XPilot.PilotClient.TransparentClickLabel();
            this.btnSettings = new XPilot.PilotClient.FlatButton();
            this.btnFlightPlan = new XPilot.PilotClient.FlatButton();
            this.btnConnect = new XPilot.PilotClient.FlatButton();
            this.pnlSidebar = new XPilot.PilotClient.TransparentClickPanel();
            this.pnlTreeContainer = new XPilot.PilotClient.TransparentClickPanel();
            this.treeControllers = new System.Windows.Forms.TreeView();
            this.lblControllers = new XPilot.PilotClient.TransparentClickLabel();
            this.pnlComRadios = new XPilot.PilotClient.TransparentClickPanel();
            this.Com2RX = new XPilot.PilotClient.TransparentClickLabel();
            this.Com2TX = new XPilot.PilotClient.TransparentClickLabel();
            this.Com2Freq = new XPilot.PilotClient.TransparentClickLabel();
            this.lblCom2 = new XPilot.PilotClient.TransparentClickLabel();
            this.Com1RX = new XPilot.PilotClient.TransparentClickLabel();
            this.Com1TX = new XPilot.PilotClient.TransparentClickLabel();
            this.Com1Freq = new XPilot.PilotClient.TransparentClickLabel();
            this.lblCom1 = new XPilot.PilotClient.TransparentClickLabel();
            this.controllerTreeContextMenu.SuspendLayout();
            this.pnlTabs.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPageMessages.SuspendLayout();
            this.pnlToolbar.SuspendLayout();
            this.pnlSidebar.SuspendLayout();
            this.pnlTreeContainer.SuspendLayout();
            this.pnlComRadios.SuspendLayout();
            this.SuspendLayout();
            // 
            // controllerTreeContextMenu
            // 
            this.controllerTreeContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.requestControllerInfo,
            this.startPrivateChat});
            this.controllerTreeContextMenu.Name = "contextMenuStrip1";
            this.controllerTreeContextMenu.Size = new System.Drawing.Size(197, 48);
            // 
            // requestControllerInfo
            // 
            this.requestControllerInfo.Name = "requestControllerInfo";
            this.requestControllerInfo.Size = new System.Drawing.Size(196, 22);
            this.requestControllerInfo.Text = "Request Controller Info";
            this.requestControllerInfo.Click += new System.EventHandler(this.requestControllerInfo_Click);
            // 
            // startPrivateChat
            // 
            this.startPrivateChat.Name = "startPrivateChat";
            this.startPrivateChat.Size = new System.Drawing.Size(196, 22);
            this.startPrivateChat.Text = "Start Private Chat";
            this.startPrivateChat.Click += new System.EventHandler(this.startPrivateChat_Click);
            // 
            // pnlTabs
            // 
            this.pnlTabs.BackColor = System.Drawing.Color.Transparent;
            this.pnlTabs.BorderColor = System.Drawing.Color.Transparent;
            this.pnlTabs.Controls.Add(this.tabControl);
            this.pnlTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTabs.Location = new System.Drawing.Point(201, 61);
            this.pnlTabs.Name = "pnlTabs";
            this.pnlTabs.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTabs.Size = new System.Drawing.Size(598, 218);
            this.pnlTabs.TabIndex = 2;
            // 
            // tabControl
            // 
            this.tabControl.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.tabControl.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
            this.tabControl.Controls.Add(this.tabPageMessages);
            this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.tabControl.ItemSize = new System.Drawing.Size(100, 21);
            this.tabControl.Location = new System.Drawing.Point(10, 10);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(578, 198);
            this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl.TabIndex = 1;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
            // 
            // tabPageMessages
            // 
            this.tabPageMessages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.tabPageMessages.Controls.Add(this.ChatMessageBox);
            this.tabPageMessages.ForeColor = System.Drawing.Color.Silver;
            this.tabPageMessages.Location = new System.Drawing.Point(4, 25);
            this.tabPageMessages.Name = "tabPageMessages";
            this.tabPageMessages.Size = new System.Drawing.Size(570, 169);
            this.tabPageMessages.TabIndex = 0;
            this.tabPageMessages.Text = "Messages";
            // 
            // ChatMessageBox
            // 
            this.ChatMessageBox.BackColor = System.Drawing.Color.Black;
            this.ChatMessageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ChatMessageBox.Location = new System.Drawing.Point(0, 0);
            this.ChatMessageBox.Margin = new System.Windows.Forms.Padding(0);
            this.ChatMessageBox.Name = "ChatMessageBox";
            this.ChatMessageBox.Size = new System.Drawing.Size(570, 169);
            this.ChatMessageBox.TabIndex = 0;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.pnlToolbar.BorderColor = System.Drawing.Color.Transparent;
            this.pnlToolbar.Controls.Add(this.btnIdent);
            this.pnlToolbar.Controls.Add(this.chkModeC);
            this.pnlToolbar.Controls.Add(this.btnMinimize);
            this.pnlToolbar.Controls.Add(this.btnClose);
            this.pnlToolbar.Controls.Add(this.lblCallsign);
            this.pnlToolbar.Controls.Add(this.btnSettings);
            this.pnlToolbar.Controls.Add(this.btnFlightPlan);
            this.pnlToolbar.Controls.Add(this.btnConnect);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(201, 1);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(598, 60);
            this.pnlToolbar.TabIndex = 1;
            // 
            // btnIdent
            // 
            this.btnIdent.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnIdent.Clicked = false;
            this.btnIdent.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnIdent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnIdent.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.btnIdent.Enabled = false;
            this.btnIdent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnIdent.ForeColor = System.Drawing.Color.White;
            this.btnIdent.Location = new System.Drawing.Point(177, 19);
            this.btnIdent.Name = "btnIdent";
            this.btnIdent.Pushed = false;
            this.btnIdent.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnIdent.Size = new System.Drawing.Size(75, 23);
            this.btnIdent.TabIndex = 8;
            this.btnIdent.Text = "Ident";
            this.btnIdent.Click += new System.EventHandler(this.btnIdent_Click);
            // 
            // chkModeC
            // 
            this.chkModeC.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.chkModeC.Clicked = false;
            this.chkModeC.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.chkModeC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.chkModeC.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.chkModeC.Enabled = false;
            this.chkModeC.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkModeC.ForeColor = System.Drawing.Color.White;
            this.chkModeC.Location = new System.Drawing.Point(93, 19);
            this.chkModeC.Name = "chkModeC";
            this.chkModeC.Pushed = false;
            this.chkModeC.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.chkModeC.Size = new System.Drawing.Size(75, 23);
            this.chkModeC.TabIndex = 7;
            this.chkModeC.Text = "Mode C";
            this.chkModeC.Click += new System.EventHandler(this.chkModeC_Click);
            // 
            // btnMinimize
            // 
            this.btnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMinimize.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnMinimize.Clicked = false;
            this.btnMinimize.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnMinimize.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMinimize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.btnMinimize.Location = new System.Drawing.Point(539, 19);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Pushed = false;
            this.btnMinimize.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnMinimize.Size = new System.Drawing.Size(20, 23);
            this.btnMinimize.TabIndex = 6;
            this.btnMinimize.Text = "–";
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(94)))), ((int)(((byte)(57)))));
            this.btnClose.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.btnClose.Clicked = false;
            this.btnClose.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(564, 19);
            this.btnClose.Name = "btnClose";
            this.btnClose.Pushed = false;
            this.btnClose.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.btnClose.Size = new System.Drawing.Size(20, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "X";
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // lblCallsign
            // 
            this.lblCallsign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCallsign.AutoSize = true;
            this.lblCallsign.BorderColor = System.Drawing.Color.Empty;
            this.lblCallsign.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCallsign.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.lblCallsign.HasBorder = false;
            this.lblCallsign.Location = new System.Drawing.Point(421, 23);
            this.lblCallsign.Name = "lblCallsign";
            this.lblCallsign.Size = new System.Drawing.Size(56, 15);
            this.lblCallsign.TabIndex = 5;
            this.lblCallsign.Text = "-------";
            this.lblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSettings
            // 
            this.btnSettings.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnSettings.Clicked = false;
            this.btnSettings.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSettings.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.btnSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSettings.ForeColor = System.Drawing.Color.White;
            this.btnSettings.Location = new System.Drawing.Point(340, 19);
            this.btnSettings.Name = "btnSettings";
            this.btnSettings.Pushed = false;
            this.btnSettings.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnSettings.Size = new System.Drawing.Size(75, 23);
            this.btnSettings.TabIndex = 4;
            this.btnSettings.Text = "Settings";
            this.btnSettings.Click += new System.EventHandler(this.btnSettings_Click);
            // 
            // btnFlightPlan
            // 
            this.btnFlightPlan.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnFlightPlan.Clicked = false;
            this.btnFlightPlan.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnFlightPlan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFlightPlan.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.btnFlightPlan.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFlightPlan.ForeColor = System.Drawing.Color.White;
            this.btnFlightPlan.Location = new System.Drawing.Point(258, 19);
            this.btnFlightPlan.Name = "btnFlightPlan";
            this.btnFlightPlan.Pushed = false;
            this.btnFlightPlan.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnFlightPlan.Size = new System.Drawing.Size(75, 23);
            this.btnFlightPlan.TabIndex = 3;
            this.btnFlightPlan.Text = "Flight Plan";
            this.btnFlightPlan.Click += new System.EventHandler(this.btnFlightPlan_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.btnConnect.Clicked = false;
            this.btnConnect.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnConnect.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.btnConnect.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConnect.ForeColor = System.Drawing.Color.White;
            this.btnConnect.Location = new System.Drawing.Point(12, 19);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Pushed = false;
            this.btnConnect.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.btnConnect.Size = new System.Drawing.Size(75, 23);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.pnlSidebar.BorderColor = System.Drawing.Color.Transparent;
            this.pnlSidebar.Controls.Add(this.pnlTreeContainer);
            this.pnlSidebar.Controls.Add(this.lblControllers);
            this.pnlSidebar.Controls.Add(this.pnlComRadios);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(1, 1);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(200, 278);
            this.pnlSidebar.TabIndex = 0;
            // 
            // pnlTreeContainer
            // 
            this.pnlTreeContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.pnlTreeContainer.BorderColor = System.Drawing.Color.Transparent;
            this.pnlTreeContainer.Controls.Add(this.treeControllers);
            this.pnlTreeContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTreeContainer.Location = new System.Drawing.Point(0, 85);
            this.pnlTreeContainer.Name = "pnlTreeContainer";
            this.pnlTreeContainer.Padding = new System.Windows.Forms.Padding(5);
            this.pnlTreeContainer.Size = new System.Drawing.Size(200, 193);
            this.pnlTreeContainer.TabIndex = 3;
            // 
            // treeControllers
            // 
            this.treeControllers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.treeControllers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeControllers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeControllers.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeControllers.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.treeControllers.Location = new System.Drawing.Point(5, 5);
            this.treeControllers.Name = "treeControllers";
            treeNode1.Name = "Center";
            treeNode1.NodeFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            treeNode1.Text = "Center   ";
            treeNode2.Name = "Approach";
            treeNode2.NodeFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            treeNode2.Text = "Approach/Departure     ";
            treeNode3.Name = "Tower";
            treeNode3.NodeFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            treeNode3.Text = "Tower   ";
            treeNode4.Name = "Ground";
            treeNode4.NodeFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            treeNode4.Text = "Ground   ";
            treeNode5.Name = "Delivery";
            treeNode5.NodeFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            treeNode5.Text = "Clearance Delivery   ";
            treeNode6.Name = "ATIS";
            treeNode6.NodeFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            treeNode6.Text = "ATIS   ";
            this.treeControllers.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6});
            this.treeControllers.ShowNodeToolTips = true;
            this.treeControllers.ShowPlusMinus = false;
            this.treeControllers.ShowRootLines = false;
            this.treeControllers.Size = new System.Drawing.Size(190, 183);
            this.treeControllers.TabIndex = 5;
            this.treeControllers.TabStop = false;
            this.treeControllers.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeControllers_BeforeCollapse);
            this.treeControllers.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.treeControllers_AfterCollapse);
            this.treeControllers.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.treeControllers_AfterExpand);
            this.treeControllers.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.treeControllers_BeforeSelect);
            this.treeControllers.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeControllers_NodeMouseClick);
            this.treeControllers.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeControllers_NodeMouseDoubleClick);
            this.treeControllers.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeControllers_MouseUp);
            // 
            // lblControllers
            // 
            this.lblControllers.BorderColor = System.Drawing.Color.Empty;
            this.lblControllers.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblControllers.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblControllers.ForeColor = System.Drawing.Color.White;
            this.lblControllers.HasBorder = false;
            this.lblControllers.Location = new System.Drawing.Point(0, 60);
            this.lblControllers.Name = "lblControllers";
            this.lblControllers.Size = new System.Drawing.Size(200, 25);
            this.lblControllers.TabIndex = 2;
            this.lblControllers.Text = "Nearby Controllers:";
            this.lblControllers.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlComRadios
            // 
            this.pnlComRadios.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(1)))), ((int)(((byte)(100)))), ((int)(((byte)(173)))));
            this.pnlComRadios.BorderColor = System.Drawing.Color.Transparent;
            this.pnlComRadios.Controls.Add(this.Com2RX);
            this.pnlComRadios.Controls.Add(this.Com2TX);
            this.pnlComRadios.Controls.Add(this.Com2Freq);
            this.pnlComRadios.Controls.Add(this.lblCom2);
            this.pnlComRadios.Controls.Add(this.Com1RX);
            this.pnlComRadios.Controls.Add(this.Com1TX);
            this.pnlComRadios.Controls.Add(this.Com1Freq);
            this.pnlComRadios.Controls.Add(this.lblCom1);
            this.pnlComRadios.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlComRadios.Location = new System.Drawing.Point(0, 0);
            this.pnlComRadios.Name = "pnlComRadios";
            this.pnlComRadios.Size = new System.Drawing.Size(200, 60);
            this.pnlComRadios.TabIndex = 1;
            // 
            // Com2RX
            // 
            this.Com2RX.BackColor = System.Drawing.Color.Transparent;
            this.Com2RX.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.Com2RX.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Com2RX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.Com2RX.HasBorder = true;
            this.Com2RX.Location = new System.Drawing.Point(152, 34);
            this.Com2RX.Name = "Com2RX";
            this.Com2RX.Size = new System.Drawing.Size(28, 15);
            this.Com2RX.TabIndex = 16;
            this.Com2RX.Text = "RX";
            this.Com2RX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Com2TX
            // 
            this.Com2TX.BackColor = System.Drawing.Color.Transparent;
            this.Com2TX.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.Com2TX.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Com2TX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.Com2TX.HasBorder = true;
            this.Com2TX.Location = new System.Drawing.Point(122, 34);
            this.Com2TX.Name = "Com2TX";
            this.Com2TX.Size = new System.Drawing.Size(28, 15);
            this.Com2TX.TabIndex = 15;
            this.Com2TX.Text = "TX";
            this.Com2TX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Com2Freq
            // 
            this.Com2Freq.AutoSize = true;
            this.Com2Freq.BorderColor = System.Drawing.Color.Empty;
            this.Com2Freq.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Com2Freq.ForeColor = System.Drawing.Color.White;
            this.Com2Freq.HasBorder = false;
            this.Com2Freq.Location = new System.Drawing.Point(62, 34);
            this.Com2Freq.Name = "Com2Freq";
            this.Com2Freq.Size = new System.Drawing.Size(56, 15);
            this.Com2Freq.TabIndex = 14;
            this.Com2Freq.Text = "---.---";
            // 
            // lblCom2
            // 
            this.lblCom2.AutoSize = true;
            this.lblCom2.BorderColor = System.Drawing.Color.Empty;
            this.lblCom2.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCom2.ForeColor = System.Drawing.Color.White;
            this.lblCom2.HasBorder = false;
            this.lblCom2.Location = new System.Drawing.Point(20, 34);
            this.lblCom2.Name = "lblCom2";
            this.lblCom2.Size = new System.Drawing.Size(42, 15);
            this.lblCom2.TabIndex = 13;
            this.lblCom2.Text = "COM2:";
            // 
            // Com1RX
            // 
            this.Com1RX.BackColor = System.Drawing.Color.Transparent;
            this.Com1RX.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.Com1RX.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Com1RX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.Com1RX.HasBorder = true;
            this.Com1RX.Location = new System.Drawing.Point(152, 12);
            this.Com1RX.Name = "Com1RX";
            this.Com1RX.Size = new System.Drawing.Size(28, 15);
            this.Com1RX.TabIndex = 12;
            this.Com1RX.Text = "RX";
            this.Com1RX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Com1TX
            // 
            this.Com1TX.BackColor = System.Drawing.Color.Transparent;
            this.Com1TX.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.Com1TX.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Com1TX.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.Com1TX.HasBorder = true;
            this.Com1TX.Location = new System.Drawing.Point(122, 12);
            this.Com1TX.Name = "Com1TX";
            this.Com1TX.Size = new System.Drawing.Size(28, 15);
            this.Com1TX.TabIndex = 11;
            this.Com1TX.Text = "TX";
            this.Com1TX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Com1Freq
            // 
            this.Com1Freq.AutoSize = true;
            this.Com1Freq.BorderColor = System.Drawing.Color.Empty;
            this.Com1Freq.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Com1Freq.ForeColor = System.Drawing.Color.White;
            this.Com1Freq.HasBorder = false;
            this.Com1Freq.Location = new System.Drawing.Point(62, 12);
            this.Com1Freq.Name = "Com1Freq";
            this.Com1Freq.Size = new System.Drawing.Size(56, 15);
            this.Com1Freq.TabIndex = 10;
            this.Com1Freq.Text = "---.---";
            // 
            // lblCom1
            // 
            this.lblCom1.AutoSize = true;
            this.lblCom1.BorderColor = System.Drawing.Color.Empty;
            this.lblCom1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCom1.ForeColor = System.Drawing.Color.White;
            this.lblCom1.HasBorder = false;
            this.lblCom1.Location = new System.Drawing.Point(20, 12);
            this.lblCom1.Name = "lblCom1";
            this.lblCom1.Size = new System.Drawing.Size(42, 15);
            this.lblCom1.TabIndex = 9;
            this.lblCom1.Text = "COM1:";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.ClientSize = new System.Drawing.Size(800, 280);
            this.Controls.Add(this.pnlTabs);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlSidebar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(800, 280);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xPilot";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseDown);
            this.controllerTreeContextMenu.ResumeLayout(false);
            this.pnlTabs.ResumeLayout(false);
            this.tabControl.ResumeLayout(false);
            this.tabPageMessages.ResumeLayout(false);
            this.pnlToolbar.ResumeLayout(false);
            this.pnlToolbar.PerformLayout();
            this.pnlSidebar.ResumeLayout(false);
            this.pnlTreeContainer.ResumeLayout(false);
            this.pnlComRadios.ResumeLayout(false);
            this.pnlComRadios.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private TransparentClickPanel pnlSidebar;
        private TransparentClickPanel pnlComRadios;
        private TransparentClickPanel pnlToolbar;
        private TransparentClickLabel lblControllers;
        private TransparentClickPanel pnlTreeContainer;
        private System.Windows.Forms.TreeView treeControllers;
        private TransparentClickLabel lblCallsign;
        private FlatButton btnSettings;
        private FlatButton btnFlightPlan;
        private FlatButton btnConnect;
        private FlatButton btnClose;
        private FlatButton btnMinimize;
        private TransparentClickPanel pnlTabs;
        private ChatBox ChatMessageBox;
        private System.Windows.Forms.ContextMenuStrip controllerTreeContextMenu;
        private System.Windows.Forms.ToolStripMenuItem requestControllerInfo;
        private System.Windows.Forms.ToolStripMenuItem startPrivateChat;
        private System.ComponentModel.BackgroundWorker bwVersionCheck;
        private TransparentClickLabel Com2RX;
        private TransparentClickLabel Com2TX;
        private TransparentClickLabel Com2Freq;
        private TransparentClickLabel lblCom2;
        private TransparentClickLabel Com1RX;
        private TransparentClickLabel Com1TX;
        private TransparentClickLabel Com1Freq;
        private TransparentClickLabel lblCom1;
        private CustomTabControl tabControl;
        private System.Windows.Forms.TabPage tabPageMessages;
        private System.Windows.Forms.ToolTip hfTooltip;
        private FlatButton btnIdent;
        private FlatButton chkModeC;
    }
}