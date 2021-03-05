using Vatsim.Xpilot;

namespace Vatsim.Xpilot
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
            System.Windows.Forms.TreeNode treeNode7 = new System.Windows.Forms.TreeNode("Observers  ");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.controllerTreeContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.requestControllerInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.startPrivateChat = new System.Windows.Forms.ToolStripMenuItem();
            this.tuneCom1Frequency = new System.Windows.Forms.ToolStripMenuItem();
            this.bwVersionCheck = new System.ComponentModel.BackgroundWorker();
            this.hfTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.pnlTabs = new Vatsim.Xpilot.TransparentClickPanel();
            this.TabsMain = new Vatsim.Xpilot.CustomTabControl();
            this.TabPageMessages = new System.Windows.Forms.TabPage();
            this.RtfMessages = new Vatsim.Xpilot.MessageConsoleControl();
            this.pnlToolbar = new Vatsim.Xpilot.TransparentClickPanel();
            this.ChkIdent = new Vatsim.Xpilot.FlatButton();
            this.ChkModeC = new Vatsim.Xpilot.FlatButton();
            this.BtnMinimize = new Vatsim.Xpilot.FlatButton();
            this.BtnExit = new Vatsim.Xpilot.FlatButton();
            this.LblCallsign = new Vatsim.Xpilot.TransparentClickLabel();
            this.BtnSettings = new Vatsim.Xpilot.FlatButton();
            this.BtnFlightPlan = new Vatsim.Xpilot.FlatButton();
            this.BtnConnect = new Vatsim.Xpilot.FlatButton();
            this.pnlSidebar = new Vatsim.Xpilot.TransparentClickPanel();
            this.pnlTreeContainer = new Vatsim.Xpilot.TransparentClickPanel();
            this.TreeControllers = new System.Windows.Forms.TreeView();
            this.lblControllers = new Vatsim.Xpilot.TransparentClickLabel();
            this.pnlComRadios = new Vatsim.Xpilot.TransparentClickPanel();
            this.Com2RX = new Vatsim.Xpilot.TransparentClickLabel();
            this.Com2TX = new Vatsim.Xpilot.TransparentClickLabel();
            this.Com2Freq = new Vatsim.Xpilot.TransparentClickLabel();
            this.lblCom2 = new Vatsim.Xpilot.TransparentClickLabel();
            this.Com1RX = new Vatsim.Xpilot.TransparentClickLabel();
            this.Com1TX = new Vatsim.Xpilot.TransparentClickLabel();
            this.Com1Freq = new Vatsim.Xpilot.TransparentClickLabel();
            this.lblCom1 = new Vatsim.Xpilot.TransparentClickLabel();
            this.controllerTreeContextMenu.SuspendLayout();
            this.pnlTabs.SuspendLayout();
            this.TabsMain.SuspendLayout();
            this.TabPageMessages.SuspendLayout();
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
            this.startPrivateChat,
            this.tuneCom1Frequency});
            this.controllerTreeContextMenu.Name = "contextMenuStrip1";
            this.controllerTreeContextMenu.Size = new System.Drawing.Size(197, 70);
            // 
            // requestControllerInfo
            // 
            this.requestControllerInfo.Name = "requestControllerInfo";
            this.requestControllerInfo.Size = new System.Drawing.Size(196, 22);
            this.requestControllerInfo.Text = "Request Controller Info";
            // 
            // startPrivateChat
            // 
            this.startPrivateChat.Name = "startPrivateChat";
            this.startPrivateChat.Size = new System.Drawing.Size(196, 22);
            this.startPrivateChat.Text = "Start Private Chat";
            // 
            // tuneCom1Frequency
            // 
            this.tuneCom1Frequency.Name = "tuneCom1Frequency";
            this.tuneCom1Frequency.Size = new System.Drawing.Size(196, 22);
            this.tuneCom1Frequency.Text = "Tune COM1 Frequency";
            // 
            // pnlTabs
            // 
            this.pnlTabs.BackColor = System.Drawing.Color.Transparent;
            this.pnlTabs.BorderColor = System.Drawing.Color.Transparent;
            this.pnlTabs.Controls.Add(this.TabsMain);
            this.pnlTabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTabs.Location = new System.Drawing.Point(189, 61);
            this.pnlTabs.Name = "pnlTabs";
            this.pnlTabs.Padding = new System.Windows.Forms.Padding(10);
            this.pnlTabs.Size = new System.Drawing.Size(540, 148);
            this.pnlTabs.TabIndex = 2;
            // 
            // TabsMain
            // 
            this.TabsMain.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.TabsMain.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(92)))), ((int)(((byte)(92)))), ((int)(((byte)(92)))));
            this.TabsMain.Controls.Add(this.TabPageMessages);
            this.TabsMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TabsMain.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            this.TabsMain.ItemSize = new System.Drawing.Size(100, 21);
            this.TabsMain.Location = new System.Drawing.Point(10, 10);
            this.TabsMain.Name = "TabsMain";
            this.TabsMain.SelectedIndex = 0;
            this.TabsMain.Size = new System.Drawing.Size(520, 128);
            this.TabsMain.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.TabsMain.TabIndex = 1;
            // 
            // TabPageMessages
            // 
            this.TabPageMessages.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(20)))), ((int)(((byte)(22)))), ((int)(((byte)(24)))));
            this.TabPageMessages.Controls.Add(this.RtfMessages);
            this.TabPageMessages.ForeColor = System.Drawing.Color.Silver;
            this.TabPageMessages.Location = new System.Drawing.Point(4, 25);
            this.TabPageMessages.Name = "TabPageMessages";
            this.TabPageMessages.Size = new System.Drawing.Size(512, 99);
            this.TabPageMessages.TabIndex = 0;
            this.TabPageMessages.Text = "Messages";
            // 
            // RtfMessages
            // 
            this.RtfMessages.BackColor = System.Drawing.Color.Black;
            this.RtfMessages.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RtfMessages.Location = new System.Drawing.Point(0, 0);
            this.RtfMessages.Margin = new System.Windows.Forms.Padding(0);
            this.RtfMessages.Name = "RtfMessages";
            this.RtfMessages.Size = new System.Drawing.Size(512, 99);
            this.RtfMessages.TabIndex = 0;
            // 
            // pnlToolbar
            // 
            this.pnlToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.pnlToolbar.BorderColor = System.Drawing.Color.Transparent;
            this.pnlToolbar.Controls.Add(this.ChkIdent);
            this.pnlToolbar.Controls.Add(this.ChkModeC);
            this.pnlToolbar.Controls.Add(this.BtnMinimize);
            this.pnlToolbar.Controls.Add(this.BtnExit);
            this.pnlToolbar.Controls.Add(this.LblCallsign);
            this.pnlToolbar.Controls.Add(this.BtnSettings);
            this.pnlToolbar.Controls.Add(this.BtnFlightPlan);
            this.pnlToolbar.Controls.Add(this.BtnConnect);
            this.pnlToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlToolbar.Location = new System.Drawing.Point(189, 1);
            this.pnlToolbar.Name = "pnlToolbar";
            this.pnlToolbar.Size = new System.Drawing.Size(540, 60);
            this.pnlToolbar.TabIndex = 1;
            // 
            // ChkIdent
            // 
            this.ChkIdent.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.ChkIdent.Clicked = false;
            this.ChkIdent.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.ChkIdent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ChkIdent.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.ChkIdent.Enabled = false;
            this.ChkIdent.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChkIdent.ForeColor = System.Drawing.Color.White;
            this.ChkIdent.Location = new System.Drawing.Point(176, 19);
            this.ChkIdent.Name = "ChkIdent";
            this.ChkIdent.Pushed = false;
            this.ChkIdent.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.ChkIdent.Size = new System.Drawing.Size(75, 23);
            this.ChkIdent.TabIndex = 8;
            this.ChkIdent.Text = "Ident";
            this.ChkIdent.Click += new System.EventHandler(this.ChkIdent_Click);
            // 
            // ChkModeC
            // 
            this.ChkModeC.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.ChkModeC.Clicked = false;
            this.ChkModeC.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.ChkModeC.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ChkModeC.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.ChkModeC.Enabled = false;
            this.ChkModeC.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ChkModeC.ForeColor = System.Drawing.Color.White;
            this.ChkModeC.Location = new System.Drawing.Point(95, 19);
            this.ChkModeC.Name = "ChkModeC";
            this.ChkModeC.Pushed = false;
            this.ChkModeC.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.ChkModeC.Size = new System.Drawing.Size(75, 23);
            this.ChkModeC.TabIndex = 7;
            this.ChkModeC.Text = "Mode C";
            this.ChkModeC.Click += new System.EventHandler(this.ChkModeC_Click);
            // 
            // BtnMinimize
            // 
            this.BtnMinimize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnMinimize.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.BtnMinimize.Clicked = false;
            this.BtnMinimize.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.BtnMinimize.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnMinimize.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.BtnMinimize.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnMinimize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.BtnMinimize.Location = new System.Drawing.Point(484, 19);
            this.BtnMinimize.Name = "BtnMinimize";
            this.BtnMinimize.Pushed = false;
            this.BtnMinimize.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.BtnMinimize.Size = new System.Drawing.Size(20, 23);
            this.BtnMinimize.TabIndex = 6;
            this.BtnMinimize.Text = "–";
            this.BtnMinimize.Click += new System.EventHandler(this.BtnMinimize_Click);
            // 
            // BtnExit
            // 
            this.BtnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnExit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(94)))), ((int)(((byte)(57)))));
            this.BtnExit.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.BtnExit.Clicked = false;
            this.BtnExit.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.BtnExit.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnExit.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.BtnExit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnExit.ForeColor = System.Drawing.Color.White;
            this.BtnExit.Location = new System.Drawing.Point(510, 19);
            this.BtnExit.Name = "BtnExit";
            this.BtnExit.Pushed = false;
            this.BtnExit.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(76)))), ((int)(((byte)(60)))));
            this.BtnExit.Size = new System.Drawing.Size(20, 23);
            this.BtnExit.TabIndex = 2;
            this.BtnExit.Text = "X";
            this.BtnExit.Click += new System.EventHandler(this.BtnExit_Click);
            // 
            // LblCallsign
            // 
            this.LblCallsign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.LblCallsign.AutoSize = true;
            this.LblCallsign.BorderColor = System.Drawing.Color.Empty;
            this.LblCallsign.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LblCallsign.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(230)))), ((int)(((byte)(230)))));
            this.LblCallsign.HasBorder = false;
            this.LblCallsign.Location = new System.Drawing.Point(421, 23);
            this.LblCallsign.Name = "LblCallsign";
            this.LblCallsign.Size = new System.Drawing.Size(56, 15);
            this.LblCallsign.TabIndex = 5;
            this.LblCallsign.Text = "-------";
            this.LblCallsign.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BtnSettings
            // 
            this.BtnSettings.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.BtnSettings.Clicked = false;
            this.BtnSettings.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.BtnSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnSettings.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.BtnSettings.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnSettings.ForeColor = System.Drawing.Color.White;
            this.BtnSettings.Location = new System.Drawing.Point(338, 19);
            this.BtnSettings.Name = "BtnSettings";
            this.BtnSettings.Pushed = false;
            this.BtnSettings.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.BtnSettings.Size = new System.Drawing.Size(75, 23);
            this.BtnSettings.TabIndex = 4;
            this.BtnSettings.Text = "Settings";
            this.BtnSettings.Click += new System.EventHandler(this.BtnSettings_Click);
            // 
            // BtnFlightPlan
            // 
            this.BtnFlightPlan.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.BtnFlightPlan.Clicked = false;
            this.BtnFlightPlan.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.BtnFlightPlan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnFlightPlan.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.BtnFlightPlan.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnFlightPlan.ForeColor = System.Drawing.Color.White;
            this.BtnFlightPlan.Location = new System.Drawing.Point(257, 19);
            this.BtnFlightPlan.Name = "BtnFlightPlan";
            this.BtnFlightPlan.Pushed = false;
            this.BtnFlightPlan.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.BtnFlightPlan.Size = new System.Drawing.Size(75, 23);
            this.BtnFlightPlan.TabIndex = 3;
            this.BtnFlightPlan.Text = "Flight Plan";
            this.BtnFlightPlan.Click += new System.EventHandler(this.BtnFlightPlan_Click);
            // 
            // BtnConnect
            // 
            this.BtnConnect.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.BtnConnect.Clicked = false;
            this.BtnConnect.ClickedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.BtnConnect.Cursor = System.Windows.Forms.Cursors.Hand;
            this.BtnConnect.DisabledTextColor = System.Drawing.Color.DarkGray;
            this.BtnConnect.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BtnConnect.ForeColor = System.Drawing.Color.White;
            this.BtnConnect.Location = new System.Drawing.Point(14, 19);
            this.BtnConnect.Name = "BtnConnect";
            this.BtnConnect.Pushed = false;
            this.BtnConnect.PushedColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(120)))), ((int)(((byte)(206)))));
            this.BtnConnect.Size = new System.Drawing.Size(75, 23);
            this.BtnConnect.TabIndex = 0;
            this.BtnConnect.Text = "Connect";
            this.BtnConnect.Click += new System.EventHandler(this.BtnConnect_Click);
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.pnlSidebar.BorderColor = System.Drawing.Color.Transparent;
            this.pnlSidebar.Controls.Add(this.pnlTreeContainer);
            this.pnlSidebar.Controls.Add(this.lblControllers);
            this.pnlSidebar.Controls.Add(this.pnlComRadios);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(1, 1);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(188, 208);
            this.pnlSidebar.TabIndex = 0;
            // 
            // pnlTreeContainer
            // 
            this.pnlTreeContainer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.pnlTreeContainer.BorderColor = System.Drawing.Color.Transparent;
            this.pnlTreeContainer.Controls.Add(this.TreeControllers);
            this.pnlTreeContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlTreeContainer.Location = new System.Drawing.Point(0, 85);
            this.pnlTreeContainer.Name = "pnlTreeContainer";
            this.pnlTreeContainer.Padding = new System.Windows.Forms.Padding(5);
            this.pnlTreeContainer.Size = new System.Drawing.Size(188, 123);
            this.pnlTreeContainer.TabIndex = 3;
            // 
            // TreeControllers
            // 
            this.TreeControllers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(44)))), ((int)(((byte)(46)))));
            this.TreeControllers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TreeControllers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TreeControllers.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TreeControllers.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.TreeControllers.Indent = 19;
            this.TreeControllers.Location = new System.Drawing.Point(5, 5);
            this.TreeControllers.MinimumSize = new System.Drawing.Size(1, 1);
            this.TreeControllers.Name = "TreeControllers";
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
            treeNode7.Name = "Observers";
            treeNode7.NodeFont = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            treeNode7.Text = "Observers  ";
            this.TreeControllers.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4,
            treeNode5,
            treeNode6,
            treeNode7});
            this.TreeControllers.ShowNodeToolTips = true;
            this.TreeControllers.ShowPlusMinus = false;
            this.TreeControllers.ShowRootLines = false;
            this.TreeControllers.Size = new System.Drawing.Size(178, 113);
            this.TreeControllers.TabIndex = 5;
            this.TreeControllers.TabStop = false;
            this.TreeControllers.BeforeCollapse += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeControllers_BeforeCollapse);
            this.TreeControllers.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.TreeControllers_BeforeSelect);
            this.TreeControllers.MouseUp += new System.Windows.Forms.MouseEventHandler(this.TreeControllers_MouseUp);
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
            this.lblControllers.Size = new System.Drawing.Size(188, 25);
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
            this.pnlComRadios.Size = new System.Drawing.Size(188, 60);
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
            this.ClientSize = new System.Drawing.Size(730, 210);
            this.Controls.Add(this.pnlTabs);
            this.Controls.Add(this.pnlToolbar);
            this.Controls.Add(this.pnlSidebar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(730, 210);
            this.Name = "MainForm";
            this.Padding = new System.Windows.Forms.Padding(1);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "xPilot";
            this.controllerTreeContextMenu.ResumeLayout(false);
            this.pnlTabs.ResumeLayout(false);
            this.TabsMain.ResumeLayout(false);
            this.TabPageMessages.ResumeLayout(false);
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
        private System.Windows.Forms.TreeView TreeControllers;
        private TransparentClickLabel LblCallsign;
        private FlatButton BtnSettings;
        private FlatButton BtnFlightPlan;
        private FlatButton BtnConnect;
        private FlatButton BtnExit;
        private FlatButton BtnMinimize;
        private TransparentClickPanel pnlTabs;
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
        private CustomTabControl TabsMain;
        private System.Windows.Forms.TabPage TabPageMessages;
        private System.Windows.Forms.ToolTip hfTooltip;
        private FlatButton ChkIdent;
        private FlatButton ChkModeC;
        private System.Windows.Forms.ToolStripMenuItem tuneCom1Frequency;
        private MessageConsoleControl RtfMessages;
    }
}