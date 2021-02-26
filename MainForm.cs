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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Fsd.Connector;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.Network.Controllers;
using XPilot.PilotClient.XplaneAdapter;

namespace XPilot.PilotClient
{
    public partial class MainForm : Form
    {
        [EventPublication(EventTopics.SessionStarted)]
        public event EventHandler<EventArgs> RaiseSessionStarted;

        [EventPublication(EventTopics.SessionEnded)]
        public event EventHandler<EventArgs> RaiseSessionEnded;

        [EventPublication(EventTopics.MainFormShown)]
        public event EventHandler<EventArgs> RaiseMainFormShown;

        [EventPublication(EventTopics.PlayNotificationSound)]
        public event EventHandler<PlayNotificationSound> RaisePlayNotificationSound;

        [EventPublication(EventTopics.RadioMessageSent)]
        public event EventHandler<RadioMessageSent> RaiseRadioMessageSent;

        [EventPublication(EventTopics.ChatSessionStarted)]
        public event EventHandler<ChatSessionStarted> RaiseChatSessionStarted;

        [EventPublication(EventTopics.WallopSent)]
        public event EventHandler<WallopSent> RaiseWallopSent;

        [EventPublication(EventTopics.SimulatorMessageSent)]
        public event EventHandler<SimulatorMessageSent> RaiseSimulatorMessageSent;

        [EventPublication(EventTopics.MetarRequested)]
        public event EventHandler<MetarRequestSent> RaiseMetarRequestedSent;

        [EventPublication(EventTopics.PrivateMessageSent)]
        public event EventHandler<PrivateMessageSent> RaisePrivateMessageSent;

        [EventPublication(EventTopics.OverrideRadioStackState)]
        public event EventHandler<OverrideRadioStackState> RaiseOverrideRadioStackState;

        [EventPublication(EventTopics.ValidateCslPaths)]
        public event EventHandler<EventArgs> RaiseValidateCslPaths;

        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPosted> RaiseNotificationPosted;

        [EventPublication(EventTopics.RadioVolumeChanged)]
        public event EventHandler<RadioVolumeChanged> RaiseRadioVolumeChanged;

        private readonly IEventBroker mEventBroker;
        private readonly IAppConfig mConfig;
        private readonly IFsdManager mNetworkManager;
        private readonly IUserInterface mUserInterface;
        private readonly ITabPages mTabPages;
        private readonly IControllerAtisManager mAtisManager;
        private readonly IControllerManager mControllerManager;
        private readonly IXplaneConnectionManager mXplane;

        [System.Runtime.InteropServices.DllImport("usser32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private bool mWaitingForConnection = true;
        private bool mInitializing = true;
        private bool mFlightLoaded = false;
        private bool mReplayMode = false;
        private bool mConnected = false;

        private bool mForceCom1Tx = false;
        private bool mForceCom1Rx = false;
        private bool mForceCom2Tx = false;
        private bool mForceCom2Rx = false;

        private readonly Timer mCheckSimConnection;
        private UserAircraftRadioStack mRadioStackData;
        private ConnectInfo mConnectInfo;
        private NotesTab tabNotes;

        private readonly List<TreeNode> mControllerNodes = new List<TreeNode>();

        private const string CONFIGURATION_REQUIRED = "xPilot hasn't been fully configured yet. You will not be able to connect to the network until it is configured. Open the settings and verify that your network login credentials are provided.";

        public MainForm(IEventBroker eventBroker, IAppConfig appConfig, IFsdManager networkManager, IUserInterface userInterface, ITabPages tabPages, IControllerAtisManager atisManager, IControllerManager controllerManager, IXplaneConnectionManager xplane)
        {
            InitializeComponent();

            mEventBroker = eventBroker;
            mConfig = appConfig;
            mNetworkManager = networkManager;
            mUserInterface = userInterface;
            mTabPages = tabPages;
            mAtisManager = atisManager;
            mControllerManager = controllerManager;
            mXplane = xplane;

            mCheckSimConnection = new Timer
            {
                Interval = 1000
            };
            mCheckSimConnection.Tick += CheckSimConnection_Tick;
            mCheckSimConnection.Start();

            ChatMessageBox.TextCommandLine.TextCommandReceived += MainForm_TextCommandReceived;
            ChatMessageBox.RichTextBox.MouseUp += rtfMessages_MouseUp;

            treeControllers.MouseUp += TreeControllers_MouseUp;
            treeControllers.BeforeSelect += TreeControllers_BeforeSelect;
            treeControllers.BeforeCollapse += TreeControllers_BeforeCollapse;
            treeControllers.ExpandAll();
            treeControllers.TreeViewNodeSorter = new TreeNodeSorter();

            tabNotes = mTabPages.CreateNotesTab();
            tabNotes.Text = "Notes";
            tabControl.TabPages.Add(tabNotes);

            mEventBroker.Register(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            RaiseSessionStarted?.Invoke(this, EventArgs.Empty);
            RaiseValidateCslPaths?.Invoke(this, EventArgs.Empty);
            RaiseMainFormShown?.Invoke(this, EventArgs.Empty);
            ScreenUtils.ApplyWindowProperties(mConfig.ClientWindowProperties, this);
            mInitializing = false;
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"xPilot Version {Application.ProductVersion}"));

            if (mConfig.ConfigurationRequired)
            {
                //using (var dlg = mUserInterface.CreateSetupGuideForm())
                //{
                //    if (dlg.ShowDialog(this) == DialogResult.No)
                //    {
                //        RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, CONFIGURATION_REQUIRED));
                //        RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.Error));
                //    }
                //}
            }

            if (!string.IsNullOrEmpty(mConfig.SimulatorIP))
            {
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Warning, $"Looking for simulator at IP {mConfig.SimulatorIP}."));
            }
            if (mConfig.VisualClientIPs.Count > 0)
            {
                string tempIPs = "";
                foreach (string ip in mConfig.VisualClientIPs)
                {
                    tempIPs += " " + ip;
                }
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Warning, $"Looking for Visuals machine at IP:{tempIPs}."));
            }
        }

        private void MainForm_TextCommandReceived(object sender, ClientEventArgs<string> e)
        {
            string[] split = e.Value.Split(new char[] { ' ' });

            try
            {
                if (e.Value.StartsWith("."))
                {
                    switch (split[0].ToLower())
                    {
                        case ".simip":
                            if (split.Length - 1 < 1)
                            {
                                mConfig.SimulatorIP = "";
                                mConfig.SaveConfig();
                                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"Simulator IP reset."));
                            }
                            else
                            {
                                mConfig.SimulatorIP = split[1];
                                mConfig.SaveConfig();
                                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"Simulator IP set to {split[1]}. Please restart xPilot."));
                            }
                            break;
                        case ".visualip":
                            if (split.Length - 1 < 1)
                            {
                                mConfig.VisualClientIPs.Clear();
                                mConfig.SaveConfig();
                                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"Visual IP reset."));
                            }
                            else
                            {
                                mConfig.VisualClientIPs.Clear();
                                for (int x = 1; x < split.Length; x++)
                                {
                                    mConfig.VisualClientIPs.Add(split[x]);
                                }
                                mConfig.SaveConfig();
                                string tempIPs = "";
                                foreach (string ip in mConfig.VisualClientIPs)
                                {
                                    tempIPs += " " + ip;
                                }
                                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"Visual IP set to{tempIPs}. Please restart xPilot."));
                            }
                            break;
                        case ".copy":
                            if (!string.IsNullOrEmpty(ChatMessageBox.RichTextBox.Text))
                            {
                                Clipboard.SetText(ChatMessageBox.RichTextBox.Text);
                            }
                            break;
                        case ".clear":
                            ChatMessageBox.RichTextBox.Clear();
                            break;
                        case ".inf":
                            if (split.Length - 1 < 1)
                            {
                                throw new ArgumentException("Not enough parameters.");
                            }
                            if (split[1].Length > 10)
                            {
                                throw new ArgumentException("Callsign too long.");
                            }
                            else
                            {
                                mNetworkManager.RequestInfoQuery(split[1].ToUpper());
                            }
                            break;
                        case ".notes":
                            if (!tabControl.TabPages.Contains(tabNotes))
                            {
                                tabNotes = mTabPages.CreateNotesTab();
                                tabNotes.Text = "Notes";
                                tabControl.TabPages.Add(tabNotes);
                                tabControl.SelectedTab = tabNotes;
                            }
                            else
                            {
                                tabControl.SelectedTab = tabNotes;
                            }
                            break;
                        case ".atis":
                            if (!mNetworkManager.IsConnected)
                            {
                                throw new ArgumentException("Not connected to the network.");
                            }
                            else
                            {
                                if (split.Length - 1 < 1)
                                {
                                    throw new ArgumentException("Not enough parameters.");
                                }
                                else
                                {
                                    mAtisManager.RequestControllerAtis(split[1].ToUpper());
                                }
                            }
                            break;
                        case ".x":
                        case ".xpndr":
                        case ".xpdr":
                        case ".squawk":
                            if (split.Length - 1 < 1)
                            {
                                throw new ArgumentException("Not enough parameters.");
                            }
                            else
                            {
                                if (!Regex.IsMatch(split[1], "^[0-7]{4}$"))
                                {
                                    throw new ArgumentException("Invalid transponder code format.");
                                }
                                int code = Convert.ToInt32(split[1]);
                                mXplane.SetTransponderCode(code);
                                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"Transponder code set to {code:0000}."));
                            }
                            break;
                        case ".com1":
                        case ".com2":
                            if (split.Length - 1 < 1)
                            {
                                throw new ArgumentException("Not enough parameters.");
                            }
                            else
                            {
                                if (!Regex.IsMatch(split[1], "^1\\d\\d[\\.\\,]\\d{1,3}$"))
                                {
                                    throw new ArgumentException("Invalid frequency format.");
                                }

                                split[1] = split[1].PadRight(7, '0');
                                uint f = uint.Parse(split[1].Substring(1).Replace(".", "").Replace(",", ""));

                                if (f % 100 == 20 || f % 100 == 70)
                                {
                                    f += 5;
                                }

                                int com = (split[0].ToLower() == ".com1") ? 1 : 2;
                                mXplane.SetRadioFrequency(com, f);
                                // (f+100000)/10
                            }
                            break;
                        case ".tx":
                            if (split.Length - 1 < 1)
                            {
                                throw new ArgumentException("Command syntax error: .tx com<n>");
                            }
                            else
                            {
                                if (split[1].ToLower() != "com1" && split[1].ToLower() != "com2")
                                {
                                    throw new ArgumentException("Command syntax error: .tx com<n>");
                                }
                                int radio = split[1].ToLower() == "com1" ? 1 : 2;
                                switch (radio)
                                {
                                    case 1:
                                        mXplane.SetAudioComSelection(6);
                                        mForceCom1Tx = true;
                                        mForceCom2Tx = false;
                                        RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"COM{radio} transmit enabled."));
                                        break;
                                    case 2:
                                        mXplane.SetAudioComSelection(7);
                                        mForceCom2Tx = true;
                                        mForceCom1Tx = false;
                                        RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"COM{radio} transmit enabled."));
                                        break;
                                }

                                RaiseOverrideRadioStackState?.Invoke(this, new OverrideRadioStackState(mForceCom1Rx, mForceCom1Tx, mForceCom2Rx, mForceCom2Tx));
                            }
                            break;
                        case ".rx":
                            if (split.Length - 1 < 2)
                            {
                                throw new ArgumentException("Command syntax error: .rx com<n> (on|off)");
                            }
                            else
                            {
                                if (split[1].ToLower() != "com1" && split[1].ToLower() != "com2")
                                {
                                    throw new ArgumentException("Command syntax error: .rx com<n> (on|off)");
                                }
                                if (split[2].ToLower() != "on" && split[2].ToLower() != "off")
                                {
                                    throw new ArgumentException("Command syntax error: .rx com<n> (on|off)");
                                }
                                bool on = (split[2].ToLower() == "on");
                                int radio = split[1].ToLower() == "com1" ? 1 : 2;
                                if (on)
                                {
                                    switch (radio)
                                    {
                                        case 1:
                                            mXplane.SetAudioSelectionCom1(true);
                                            mXplane.SetAudioSelectionCom2(false);
                                            mForceCom1Rx = true;
                                            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"COM{radio} receiver on."));
                                            break;
                                        case 2:
                                            mXplane.SetAudioSelectionCom1(false);
                                            mXplane.SetAudioSelectionCom2(true);
                                            mForceCom2Rx = true;
                                            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"COM{radio} receiver on."));
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (radio)
                                    {
                                        case 1:
                                            mXplane.SetAudioSelectionCom1(false);
                                            mXplane.SetAudioSelectionCom2(false);
                                            mForceCom1Rx = false;
                                            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"COM{radio} receiver off."));
                                            break;
                                        case 2:
                                            mXplane.SetAudioSelectionCom1(false);
                                            mXplane.SetAudioSelectionCom2(false);
                                            mForceCom2Rx = false;
                                            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"COM{radio} receiver off."));
                                            break;
                                    }
                                }

                                RaiseOverrideRadioStackState?.Invoke(this, new OverrideRadioStackState(mForceCom1Rx, mForceCom1Tx, mForceCom2Rx, mForceCom2Tx));
                            }
                            break;
                        case ".msg":
                        case ".chat":
                            if (split.Length - 1 < 1)
                            {
                                throw new ArgumentException("Not enough parameters.");
                            }
                            if (split[1].Length > 10)
                            {
                                throw new ArgumentException("Callsign too long.");
                            }
                            else
                            {
                                if (!mNetworkManager.IsConnected)
                                {
                                    throw new ArgumentException("Not connected to the network.");
                                }
                                else
                                {
                                    if (split.Length > 2)
                                    {
                                        RaisePrivateMessageSent?.Invoke(this, new PrivateMessageSent(mNetworkManager.OurCallsign, split[1].ToUpper(), string.Join(" ", split.Skip(2))));
                                    }
                                    else
                                    {
                                        InitializeChatSession(split[1].ToUpper());
                                    }
                                }
                            }
                            break;
                        case ".wallop":
                            if (split.Length - 1 < 1)
                            {
                                throw new ArgumentException("Not enough parameters.");
                            }
                            else
                            {
                                RaiseWallopSent?.Invoke(this, new WallopSent(string.Join(" ", split.Skip(1))));
                            }
                            break;
                        case ".wx":
                        case ".metar":
                            if (split.Length - 1 < 1)
                            {
                                throw new ArgumentException("Not enough parameters.");
                            }
                            else
                            {
                                RaiseMetarRequestedSent?.Invoke(this, new MetarRequestSent(split[1]));
                            }
                            break;
                        case ".towerview":
                            if (mFlightLoaded)
                            {
                                string tvServerAddress = "127.0.0.1";
                                string tvCallsign = "TOWER";
                                if (split.Length >= 2)
                                {
                                    tvServerAddress = split[1];
                                    if (split.Length >= 3)
                                    {
                                        tvCallsign = split[2].ToUpper();
                                    }
                                }
                                ConnectInfo tvConnectInfo = new ConnectInfo(tvCallsign, "", "", true, true);
                                mNetworkManager.Connect(tvConnectInfo, tvServerAddress);
                            }
                            else
                            {
                                throw new ArgumentException("xPilot is unable to connect to X-Plane. Please make sure X-Plane is running and a flight is loaded.");
                            }
                            break;
                    }
                }
                else
                {
                    if (mNetworkManager.IsConnected)
                    {
                        if (!string.IsNullOrEmpty(e.Value))
                        {
                            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.SentRadioMessage, $"{mNetworkManager.OurCallsign}: {e.Value}"));
                            RaiseRadioMessageSent?.Invoke(this, new RadioMessageSent(mNetworkManager.OurCallsign, e.Value, TunedFrequencies().ToArray()));
                            RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent($"{ mNetworkManager.OurCallsign }: {e.Value}"));
                        }
                    }
                    else
                    {
                        throw new ArgumentException("Not connected to the network.");
                    }
                }
            }
            catch (Exception ex)
            {
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, ex.Message));
                RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.Error));
            }
        }

        private PrivateMessageTab InitializeChatSession(string tabIdentifier)
        {
            PrivateMessageTab tab = GetPrivateMessageTabIfExists(tabIdentifier);
            if (tab == null)
            {
                tab = CreatePrivateMessageTab(tabIdentifier);
            }
            tabControl.SelectedTab = tab;
            return tab;
        }

        private PrivateMessageTab GetPrivateMessageTabIfExists(string tabIdentifier)
        {
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (tabPage.Name.Equals(tabIdentifier) && tabPage is PrivateMessageTab)
                {
                    return tabPage as PrivateMessageTab;
                }
            }
            return null;
        }

        private void SetActiveMessageTab(string tabName)
        {
            foreach (TabPage tabPage in tabControl.TabPages)
            {
                if (tabPage.Name.Equals(tabName) && tabPage is PrivateMessageTab)
                {
                    tabControl.SelectedTab = tabPage;
                }
            }
        }

        private PrivateMessageTab CreatePrivateMessageTab(string to, string message = null, bool isOurMessage = false, string ourCallsign = null)
        {
            PrivateMessageTab tab = mTabPages.CreatePrivateMessageTab(to, message, isOurMessage, ourCallsign);
            tabControl.TabPages.Add(tab);
            return tab;
        }

        private void CheckSimConnection_Tick(object sender, EventArgs e)
        {
            if (!mFlightLoaded)
            {
                mCheckSimConnection.Interval = 5000;
                if (mWaitingForConnection)
                {
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, "Waiting for X-Plane connection..."));
                    mWaitingForConnection = false;
                }
            }
        }

        private TreeNode FindController(string callsign)
        {
            TreeNode[] array = treeControllers.Nodes.Find(callsign.Replace("*", ""), true);
            TreeNode result;
            if (array.Length >= 1)
            {
                result = array[0];
            }
            else
            {
                result = null;
            }
            return result;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x84)
            {
                int x = ((short)((long)m.LParam));
                int y = ((short)((long)m.LParam >> 16));
                Point point = PointToClient(new Point(x, y));
                if (point.X >= ClientSize.Width - 6 && point.Y >= ClientSize.Height - 5)
                {
                    m.Result = (IntPtr)(IsMirrored ? 16 : 17);
                }
                else if (point.X <= 6 && point.Y >= ClientSize.Height - 5)
                {
                    m.Result = (IntPtr)(IsMirrored ? 17 : 16);
                }
                else if (point.X <= 6 && point.Y <= 5)
                {
                    m.Result = (IntPtr)(IsMirrored ? 14 : 13);
                }
                else if (point.X >= ClientSize.Width - 6 && point.Y <= 5)
                {
                    m.Result = (IntPtr)(IsMirrored ? 13 : 14);
                }
                else if (point.Y <= 5)
                {
                    m.Result = (IntPtr)12;
                }
                else if (point.Y >= ClientSize.Height - 5)
                {
                    m.Result = (IntPtr)15;
                }
                else if (point.X <= 6)
                {
                    m.Result = (IntPtr)10;
                }
                else if (point.X >= ClientSize.Width - 6)
                {
                    m.Result = (IntPtr)11;
                }
                else
                {
                    base.WndProc(ref m);
                    if ((int)m.Result == 1)
                    {
                        m.Result = (IntPtr)2;
                    }
                }
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x20000;
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Rectangle rect = new Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            using (Pen pen = new Pen(Color.FromArgb(0, 0, 0)))
            {
                pevent.Graphics.DrawRectangle(pen, rect);
            }
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            if (!mInitializing)
            {
                ScreenUtils.SaveWindowProperties(mConfig.ClientWindowProperties, this);
                mConfig.SaveConfig();
            }
        }

        protected override void OnMove(EventArgs e)
        {
            if (!mInitializing)
            {
                ScreenUtils.SaveWindowProperties(mConfig.ClientWindowProperties, this);
                mConfig.SaveConfig();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (mConnected)
            {
                mNetworkManager.Disconnect(new DisconnectInfo
                {
                    Type = DisconnectType.Intentional
                });
            }
            else
            {
                using (ConnectForm dlg = mUserInterface.CreateConnectForm())
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        mConnectInfo = dlg.GetConnectInfo();
                        if (mFlightLoaded)
                        {
                            if (mConfig.ConfigurationRequired)
                            {
                                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, CONFIGURATION_REQUIRED));
                                RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.Error));
                            }
                            else
                            {
                                NetworkServerInfo server = mNetworkManager.ReturnServerList().FirstOrDefault(o => o.Name == mConfig.ServerName);
                                if (server != null)
                                {
                                    mNetworkManager.Connect(mConnectInfo, server.Address);
                                    mReplayMode = false;
                                }
                                else
                                {
                                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "You must first select a server in the settings window."));
                                }
                            }
                        }
                        else
                        {
                            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "xPilot is unable to connect to X-Plane. Please make sure X-Plane is running and a flight is loaded."));
                            RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.Error));
                        }
                    }
                }
            }

            TextCommandFocus();
        }

        private void btnFlightPlan_Click(object sender, EventArgs e)
        {
            Process.Start("https://my.vatsim.net/pilots/flightplan");
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (SettingsForm dlg = mUserInterface.CreateSettingsForm())
            {
                dlg.ShowDialog(this);
            }
            TextCommandFocus();
        }

        private void btnIdent_Click(object sender, EventArgs e)
        {
            if (mNetworkManager.IsConnected)
            {
                mNetworkManager.SquawkIdent();
            }

            //if (mFlightLoaded)
            //{
            //    SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(Commands.TransponderTransponderIdent));
            //}

            TextCommandFocus();
        }

        private void chkModeC_Click(object sender, EventArgs e)
        {
            chkModeC.Clicked = !chkModeC.Clicked;

            //var laminarB738 = new DataRefElement
            //{
            //    DataRef = "laminar/B738/knob/transpoder_pos"
            //};

            //var tolis = new DataRefElement
            //{
            //    DataRef = "ckpt/transponder/mode/anim"
            //};

            //var laminarB738_Dn_Cmd = new XPlaneCommand("laminar/B738/knob/transponder_mode_up", "");
            //var laminarB738_Up_Cmd = new XPlaneCommand("laminar/B738/knob/transponder_mode_up", "");

            if (mFlightLoaded)
            {
                if (chkModeC.Clicked)
                {
                    //SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(laminarB738_Up_Cmd));
                    //SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(laminarB738, 3));
                    //SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(Commands.TransponderTransponderAlt));

                    // tolis a319
                    //SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(tolis, 0));
                    mConfig.SquawkingModeC = true;
                }
                else
                {
                    //SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(laminarB738_Dn_Cmd));
                    //SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(laminarB738, 1));
                    //SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(Commands.TransponderTransponderOff));

                    // tolis a319
                    //SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(tolis, 4));
                    mConfig.SquawkingModeC = false;
                }
            }
            else
            {
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, "xPilot is unable to connect to X-Plane. Please make sure X-Plane is running and a flight is loaded."));
                RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.Error));
            }

            TextCommandFocus();
        }

        private void treeControllers_MouseUp(object sender, MouseEventArgs e)
        {
            FocusTabs();
        }

        private void treeControllers_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
            FocusTabs();
        }

        private void FocusTabs()
        {
            if (tabControl.SelectedTab != null)
            {
                if (tabControl.SelectedTab is PrivateMessageTab)
                {
                    (tabControl.SelectedTab as PrivateMessageTab).FocusTextCommandLine();
                }
                else if (tabControl.SelectedTab == tabPageMessages)
                {
                    ChatMessageBox.TextCommandLine.Focus();
                }
            }
        }

        private void treeControllers_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level != 0)
            {
                if (e.Button == MouseButtons.Left)
                {
                    mAtisManager.RequestControllerAtis(e.Node.Name.Replace("*", ""));
                }
            }
        }

        private void treeControllers_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (mControllerNodes.Contains(e.Node))
            {
                mControllerNodes.Remove(e.Node);
            }
        }

        private void treeControllers_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (!mControllerNodes.Contains(e.Node))
            {
                mControllerNodes.Add(e.Node);
            }
        }

        public List<int> TunedFrequencies()
        {
            List<int> Tuned = new List<int>();
            if (mRadioStackData != null)
            {
                if (mRadioStackData.IsCom1Transmitting)
                {
                    Tuned.Add(mRadioStackData.Com1ActiveFreq.Normalize25KhzFrequency().MatchNetworkFormat());
                    if (!Tuned.Contains(mRadioStackData.Com1ActiveFreq.UnNormalize25KhzFrequency().MatchNetworkFormat()))
                    {
                        Tuned.Add(mRadioStackData.Com1ActiveFreq.UnNormalize25KhzFrequency().MatchNetworkFormat());
                    }
                }
                if (mRadioStackData.IsCom2Transmitting)
                {
                    Tuned.Add(mRadioStackData.Com2ActiveFreq.Normalize25KhzFrequency().MatchNetworkFormat());
                    if (!Tuned.Contains(mRadioStackData.Com2ActiveFreq.UnNormalize25KhzFrequency().MatchNetworkFormat()))
                    {
                        Tuned.Add(mRadioStackData.Com2ActiveFreq.UnNormalize25KhzFrequency().MatchNetworkFormat());
                    }
                }
            }
            return Tuned;
        }

        private void treeControllers_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                treeControllers.SelectedNode = treeControllers.GetNodeAt(e.X, e.Y);

                if (treeControllers.SelectedNode != null && treeControllers.SelectedNode.Nodes.Count > 0)
                {
                    controllerTreeContextMenu.Show(treeControllers, e.Location);
                }
            }
        }

        private void treeControllers_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Level != 0 && e.Button == MouseButtons.Right)
            {
                ShowControllerContextMenu(e.Node.Name.Replace("*", ""), e.Location);
            }
        }

        private void ShowControllerContextMenu(string name, Point point)
        {
            startPrivateChat.Tag = name;
            requestControllerInfo.Tag = name;
            tuneCom1Frequency.Tag = name;
            controllerTreeContextMenu.Show(treeControllers, point);
        }

        private void startPrivateChat_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            RaiseChatSessionStarted?.Invoke(this, new ChatSessionStarted(item.Tag.ToString()));
        }

        private void requestControllerInfo_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            mAtisManager.RequestControllerAtis(item.Tag.ToString());
        }

        private void tuneCom1Frequency_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            if (!string.IsNullOrEmpty(item.Tag.ToString()))
            {
                uint freq = mControllerManager.GetFrequencyByCallsign(item.Tag.ToString()).FsdFrequencyToHertz() / 1000;
                mXplane.SetRadioFrequency(1, freq);
            }
        }

        private void treeControllers_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (mNetworkManager.IsConnected)
            {
                DialogResult dialogResult = MessageBox.Show(this, "You are connected to the network. Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1);
                if (dialogResult != DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }

            RaiseSessionEnded?.Invoke(this, new EventArgs());
            mConfig.SaveConfig();
            mEventBroker.Unregister(this);
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void rtfMessages_MouseUp(object sender, MouseEventArgs e)
        {
            if (ChatMessageBox.RichTextBox.SelectionLength > 0)
            {
                Clipboard.SetText(ChatMessageBox.RichTextBox.SelectedText);
                ChatMessageBox.RichTextBox.SelectionLength = 0;
            }
            TextCommandFocus();
        }

        private void TreeControllers_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
            TextCommandFocus();
        }

        private void TreeControllers_MouseUp(object sender, MouseEventArgs e)
        {
            TextCommandFocus();
        }

        private void TreeControllers_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
            TextCommandFocus();
        }

        private void TextCommandFocus()
        {
            if (tabControl.SelectedTab != null)
            {
                if (tabControl.SelectedIndex > 0)
                {
                    ChatBox chatBox = (ChatBox)tabControl.TabPages[tabControl.SelectedIndex].Controls["chatbox"];
                    if (chatBox != null)
                    {
                        chatBox.TextCommandLine.Focus();
                    }
                }
                else
                {
                    ChatMessageBox.TextCommandLine.Select();
                }
            }
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            TextCommandFocus();
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            tabControl.SelectedTab.ForeColor = Color.Silver;
            if (tabControl.SelectedTab != null)
            {
                if (tabControl.SelectedTab is PrivateMessageTab)
                {
                    (tabControl.SelectedTab as PrivateMessageTab).Focus();
                }
                else if (tabControl.SelectedTab == tabPageMessages)
                {
                    ChatMessageBox.TextCommandLine.Select();
                }
            }
            tabControl.Refresh();
        }

        [EventSubscription(EventTopics.ConnectButtonStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnDisableConnectButton(object sender, ClientEventArgs<bool> e)
        {
            btnConnect.Enabled = e.Value;
            if (e.Value == false && mNetworkManager.IsConnected)
            {
                mNetworkManager.Disconnect(new DisconnectInfo
                {
                    Type = DisconnectType.Intentional
                });
            }
        }

        //[EventSubscription(EventTopics.VoiceServerConnectionLost, typeof(OnUserInterfaceAsync))]
        //public void OnVoiceServerConnectionLost(object sender, EventArgs e)
        //{
        //    FlashTaskbar.Flash(this);
        //}


        [EventSubscription(EventTopics.SocketMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnSocketMessageReceived(object sender, ClientEventArgs<string> e)
        {
            if (!string.IsNullOrEmpty(e.Value))
            {
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.SentRadioMessage, $"{mNetworkManager.OurCallsign}: {e.Value}"));
                RaiseRadioMessageSent?.Invoke(this, new RadioMessageSent(mNetworkManager.OurCallsign, e.Value, TunedFrequencies().ToArray()));
            }
        }

        [EventSubscription(EventTopics.ChatSessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnChatSessionStarted(object sender, ChatSessionStarted e)
        {
            InitializeChatSession(e.Callsign);
        }

        [EventSubscription(EventTopics.ServerListDownloadFailed, typeof(OnUserInterfaceAsync))]
        public void OnServerListDownloadFailed(object sender, ClientEventArgs<string> e)
        {
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Error, e.Value));
        }

        [EventSubscription(EventTopics.NotificationPosted, typeof(OnUserInterfaceAsync))]
        public void OnNotificationPosted(object sender, NotificationPosted e)
        {
            switch (e.Type)
            {
                default:
                case NotificationType.Info:
                    ChatMessageBox.WriteMessage(e.Type, e.Message, Color.Yellow);
                    break;
                case NotificationType.Warning:
                    ChatMessageBox.WriteMessage(e.Type, e.Message, Color.FromArgb(243, 156, 18));
                    break;
                case NotificationType.Error:
                    ChatMessageBox.WriteMessage(e.Type, e.Message, Color.FromArgb(232, 65, 24));
                    break;
                case NotificationType.RadioMessage:
                    ChatMessageBox.WriteMessage(e.Type, e.Message, Color.FromArgb(180, 180, 180));
                    break;
                case NotificationType.DirectRadioMessage:
                    ChatMessageBox.WriteMessage(e.Type, e.Message, Color.White);
                    break;
                case NotificationType.SentRadioMessage:
                    ChatMessageBox.WriteMessage(e.Type, e.Message, Color.Cyan);
                    break;
                case NotificationType.ServerMessage:
                    ChatMessageBox.WriteMessage(e.Type, e.Message, Color.FromArgb(133, 166, 100));
                    break;
                case NotificationType.BroadcastMessage:
                    ChatMessageBox.WriteMessage(e.Type, e.Message, Color.FromArgb(197, 108, 240));
                    break;
            }
        }

        [EventSubscription(EventTopics.ServerMessageReceived, typeof(OnUserInterfaceAsync))]
        public void ServerMessageReceived(object sender, NetworkDataReceived e)
        {
            RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent($"[SERVER] {e.Data}", MessageColor.Green));
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.ServerMessage, $"[SERVER] {e.Data}"));
        }

        [EventSubscription(EventTopics.BroadcastMessageReceived, typeof(OnUserInterfaceAsync))]
        public void BroadcastMessageReceived(object sender, NetworkDataReceived e)
        {
            RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent($"[BROADCAST] {e.Data}", MessageColor.Orange));
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.BroadcastMessage, $"[BROADCAST] {e.From}: {e.Data}"));
            RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.Broadcast));
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnected e)
        {
            mConnected = true;
            btnConnect.Text = "Disconnect";
            btnConnect.BackColor = Color.FromArgb(0, 120, 206);
            lblCallsign.Text = e.ConnectInfo.Callsign;

            Text = $"xPilot - {e.ConnectInfo.Callsign}";

            if (mConnectInfo != null)
            {
                chkModeC.Enabled = mConnectInfo.ObserverMode ? false : true;
                btnIdent.Enabled = mConnectInfo.ObserverMode ? false : true;
            }

            mXplane.SetLoginStatus(true);
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnected e)
        {
            Text = "xPilot";

            chkModeC.Enabled = false;
            btnIdent.Enabled = false;

            mConnected = false;
            btnConnect.Text = "Connect";
            btnConnect.BackColor = Color.FromArgb(39, 44, 46);
            lblCallsign.Text = "-------";

            mXplane.SetLoginStatus(false);

            if (mConfig.FlashTaskbarDisconnect && (e.DisconnectInfo != null && e.DisconnectInfo.Type != DisconnectType.Intentional))
            {
                FlashTaskbar.Flash(this);
            }
        }

        [EventSubscription(EventTopics.ControllerAdded, typeof(OnUserInterfaceAsync))]
        public void OnControllerUpdateReceived(object sender, ControllerUpdateReceived e)
        {
            string frequency = (e.Controller.NormalizedFrequency.FsdFrequencyToHertz() / 1000000.0).ToString("0.000");

            TreeNode treeNode;
            string text;

            if (e.Controller.Callsign.EndsWith("_CTR") || e.Controller.Callsign.EndsWith("_FSS"))
            {
                treeNode = treeControllers.Nodes["Center"];
                text = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_APP") || e.Controller.Callsign.EndsWith("_DEP"))
            {
                treeNode = treeControllers.Nodes["Approach"];
                text = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_TWR"))
            {
                treeNode = treeControllers.Nodes["Tower"];
                text = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_GND"))
            {
                treeNode = treeControllers.Nodes["Ground"];
                text = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_DEL"))
            {
                treeNode = treeControllers.Nodes["Delivery"];
                text = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_ATIS"))
            {
                treeNode = treeControllers.Nodes["ATIS"];
                text = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else
            {
                treeNode = treeControllers.Nodes["Observers"];
                text = e.Controller.Callsign.Replace("*", "");
            }
            if (treeNode != null)
            {
                if (!treeNode.Nodes.ContainsKey(e.Controller.Callsign))
                {
                    TreeNode treeNode2 = treeNode.Nodes.Add(e.Controller.Callsign, text);
                    treeNode2.ToolTipText = (string.IsNullOrEmpty(e.Controller.RealName) ? "Unknown" : e.Controller.RealName);

                    if (!mControllerNodes.Contains(treeNode) && treeNode.Nodes.Count == 1)
                    {
                        treeNode.Expand();
                    }
                }
            }
            treeControllers.Sort();
        }

        [EventSubscription(EventTopics.ControllerDeleted, typeof(OnUserInterfaceAsync))]
        public void OnControllerDeleted(object sender, NetworkDataReceived e)
        {
            if (!treeControllers.IsDisposed && !treeControllers.Disposing)
            {
                foreach (TreeNode treeNode in treeControllers.Nodes)
                {
                    treeNode.Nodes.RemoveByKey(e.From);
                }
            }
        }

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnUserInterfaceAsync))]
        public void RealNameReceived(object sender, NetworkDataReceived e)
        {
            TreeNode treeNode = FindController(e.From.Replace("*", ""));
            if (treeNode != null)
            {
                treeNode.ToolTipText = (string.IsNullOrEmpty(e.Data) ? "Unknown" : e.Data);
            }
        }

        [EventSubscription(EventTopics.ControllerFrequencyChanged, typeof(OnUserInterfaceAsync))]
        public void ControllerFrequencyChanged(object sender, ControllerUpdateReceived e)
        {
            TreeNode node = FindController(e.Controller.Callsign);
            if (node != null)
            {
                if (node.Parent.Name == "Observers")
                {
                    node.Text = e.Controller.Callsign;
                }
                else
                {
                    string frequency = (e.Controller.NormalizedFrequency.FsdFrequencyToHertz() / 1000000.0).ToString("0.000");
                    node.Text = string.Format("{0} - {1}", e.Controller.Callsign, frequency);
                }
            }
        }

        [EventSubscription(EventTopics.SimConnectionStateChanged, typeof(OnUserInterfaceAsync))]
        public void SimConnectionChanged(object sender, ClientEventArgs<bool> e)
        {
            if (e.Value != mFlightLoaded)
            {
                mFlightLoaded = e.Value;

                if (mFlightLoaded)
                {
                    RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent("xPilot client successfully connected to X-Plane.", MessageColor.Yellow));
                    RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, "X-Plane connection established."));
                    if (mCheckSimConnection != null)
                    {
                        mCheckSimConnection.Stop();
                    }
                }
                else
                {
                    mWaitingForConnection = true;
                    mCheckSimConnection.Interval = 10;
                    mCheckSimConnection.Start();

                    if (mNetworkManager.IsConnected)
                    {
                        mNetworkManager.ForceDisconnect("X-Plane connection lost. Disconnected from network.");
                    }
                }
            }
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChanged e)
        {
            if (mRadioStackData == null || !e.RadioStack.Equals(mRadioStackData))
            {
                if (e.RadioStack.HasPower)
                {
                    if (e.RadioStack.Com1ActiveFreq > 0)
                    {
                        Com1Freq.Text = (e.RadioStack.Com1ActiveFreq.Normalize25KhzFrequency() / 1000000.0f).ToString("0.000");
                        Com1TX.ForeColor = e.RadioStack.IsCom1Transmitting ? Color.White : Color.FromArgb(39, 44, 46);
                        Com1RX.ForeColor = e.RadioStack.IsCom1Receiving ? Color.White : Color.FromArgb(39, 44, 46);
                    }
                    else
                    {
                        Com1Freq.Text = "---.---";
                        Com1TX.ForeColor = Color.FromArgb(39, 44, 46);
                        Com1RX.ForeColor = Color.FromArgb(39, 44, 46);
                    }

                    if (e.RadioStack.Com2ActiveFreq > 0)
                    {
                        Com2Freq.Text = (e.RadioStack.Com2ActiveFreq.Normalize25KhzFrequency() / 1000000.0f).ToString("0.000");
                        Com2TX.ForeColor = e.RadioStack.IsCom2Transmitting ? Color.White : Color.FromArgb(39, 44, 46);
                        Com2RX.ForeColor = e.RadioStack.IsCom2Receiving ? Color.White : Color.FromArgb(39, 44, 46);
                    }
                    else
                    {
                        Com2Freq.Text = "---.---";
                        Com2TX.ForeColor = Color.FromArgb(39, 44, 46);
                        Com2RX.ForeColor = Color.FromArgb(39, 44, 46);
                    }
                }
                else
                {
                    Com1Freq.Text = "---.---";
                    Com1TX.ForeColor = Color.FromArgb(39, 44, 46);
                    Com1RX.ForeColor = Color.FromArgb(39, 44, 46);

                    Com2Freq.Text = "---.---";
                    Com2TX.ForeColor = Color.FromArgb(39, 44, 46);
                    Com2RX.ForeColor = Color.FromArgb(39, 44, 46);
                }
                mRadioStackData = e.RadioStack;
            }
        }

        [EventSubscription(EventTopics.HFAliasChanged, typeof(OnUserInterfaceAsync))]
        public void OnCom1FrequencyAliasChanged(object sender, HFAliasChanged e)
        {
            switch (e.Radio)
            {
                case 1:
                    if (e.HighFrequency > 0)
                    {
                        hfTooltip.SetToolTip(Com1Freq, $"HF: {((double)e.HighFrequency / 1000000).ToString("0.00000")}");
                    }
                    else
                    {
                        hfTooltip.SetToolTip(Com1Freq, null);
                    }
                    break;
                case 2:
                    if (e.HighFrequency > 0)
                    {
                        hfTooltip.SetToolTip(Com2Freq, $"HF: {((double)e.HighFrequency / 1000000).ToString("0.00000")}");
                    }
                    else
                    {
                        hfTooltip.SetToolTip(Com2Freq, null);
                    }
                    break;
            }
        }

        [EventSubscription(EventTopics.ComRadioTransmittingChanged, typeof(OnUserInterfaceAsync))]
        public void ComRadioTransmittingChanged(object sender, RadioTxStateChanged e)
        {
            switch (e.Radio)
            {
                case 1:
                    Com1TX.BackColor = e.State ? Color.FromArgb(39, 174, 96) : Color.Transparent;
                    break;
                case 2:
                    Com2TX.BackColor = e.State ? Color.FromArgb(39, 174, 96) : Color.Transparent;
                    break;
            }
        }

        [EventSubscription(EventTopics.RadioReceiveStateChanged, typeof(OnUserInterfaceAsync))]
        public void ComRadioReceivingChanged(object sender, RadioRxStateChanged e)
        {
            switch (e.Radio)
            {
                case 0:
                    Com1RX.BackColor = e.State ? Color.FromArgb(39, 174, 96) : Color.Transparent;
                    break;
                case 1:
                    Com2RX.BackColor = e.State ? Color.FromArgb(39, 174, 96) : Color.Transparent;
                    break;
            }
        }

        [EventSubscription(EventTopics.UserAircraftDataChanged, typeof(OnUserInterfaceAsync))]
        public void UserAircraftDataUpdateReceived(object sender, UserAircraftDataChanged e)
        {
            if (!mReplayMode && (mNetworkManager.IsConnected && e.AircraftData.ReplayModeEnabled))
            {
                RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent($"You have been disconnected from the network because Replay Mode is enabled.", true, MessageColor.Red));
                mNetworkManager.ForceDisconnect("You have been disconnected from the network because Replay Mode is enabled.");
                RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.Error));
                mReplayMode = true;
            }
        }

        [EventSubscription(EventTopics.MetarReceived, typeof(OnUserInterfaceAsync))]
        public void MetarResponseReceived(object sender, NetworkDataReceived e)
        {
            RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent($"METAR: {e.Data}", true, MessageColor.Yellow));
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"METAR: {e.Data}"));
        }

        [EventSubscription(EventTopics.SelcalAlertReceived, typeof(OnUserInterfaceAsync))]
        public void SelcalAlertReceived(object sender, SelcalAlertReceived e)
        {
            RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent($"SELCAL alert received on {e.Frequencies[0].FormatFromNetwork()}", true, MessageColor.Yellow));
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"SELCAL alert received on {e.Frequencies[0].FormatFromNetwork()}."));
            if (mConfig.EnableNotificationSounds)
            {
                RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.SelCal));
            }
            if (mConfig.FlashTaskbarSelcal)
            {
                FlashTaskbar.Flash(this);
            }
        }

        [EventSubscription(EventTopics.RadioMessageReceived, typeof(OnUserInterfaceAsync))]
        public void RadioMessageReceived(object sender, RadioMessageReceived e)
        {
            string message;
            if (mRadioStackData != null)
            {
                if (mRadioStackData.ReceivingOnBothFrequencies)
                {
                    string arg;
                    if (e.Frequencies.Length > 1)
                    {
                        arg = string.Format("{0} & {1}", e.Frequencies[0].FormatFromNetwork(), e.Frequencies[1].FormatFromNetwork());
                    }
                    else
                    {
                        arg = e.Frequencies[0].FormatFromNetwork();
                    }
                    message = string.Format("{0} on {1}: {2}", e.From, arg, e.Data);
                }
                else
                {
                    message = string.Format("{0}: {1}", e.From, e.Data);
                }
                if (e.IsDirect)
                {
                    RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent(message, true, MessageColor.Yellow));
                }
                else
                {
                    RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent(message, MessageColor.Gray));
                }
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(e.IsDirect ? NotificationType.DirectRadioMessage : NotificationType.RadioMessage, message));
                if (e.IsDirect)
                {
                    RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.DirectRadioMessage));
                    if (mConfig.FlashTaskbarRadioMessage)
                    {
                        FlashTaskbar.Flash(this);
                    }
                }

                if (mConfig.EnableNotificationSounds)
                {
                    RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.RadioMessage));
                }

                if (tabControl.SelectedTab != tabPageMessages)
                {
                    tabControl.TabPages[0].ForeColor = Color.Yellow;
                    tabControl.Refresh();
                }
            }
        }

        [EventSubscription(EventTopics.ClientConfigChanged, typeof(OnUserInterfaceAsync))]
        public void ClientConfigChanged(object sender, EventArgs e)
        {
            TopMost = mConfig.KeepWindowVisible;
        }

        [EventSubscription(EventTopics.PrivateMessageReceived, typeof(OnUserInterfaceAsync))]
        public void PrivateMessageReceived(object sender, PrivateMessageReceived e)
        {
            PrivateMessageTab tab = GetPrivateMessageTabIfExists(e.From);
            if (tab == null)
            {
                RaisePlayNotificationSound?.Invoke(this, new PlayNotificationSound(SoundEvent.PrivateMessage));
                if (mConfig.FlashTaskbarPrivateMessage)
                {
                    FlashTaskbar.Flash(this);
                }
                CreatePrivateMessageTab(e.From, e.Message);
            }
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSent e)
        {
            PrivateMessageTab tab = GetPrivateMessageTabIfExists(e.To);
            if (tab == null)
            {
                CreatePrivateMessageTab(e.To, e.Message, true, e.From);
                SetActiveMessageTab(e.To);
            }
        }

        [EventSubscription(EventTopics.AcarsResponseReceived, typeof(OnUserInterfaceAsync))]
        public void OnRequestedAtisReceived(object sender, AcarsResponseReceived e)
        {
            RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent($"{e.From} ATIS:", MessageColor.Green));
            foreach (string line in e.Lines)
            {
                RaiseSimulatorMessageSent?.Invoke(this, new SimulatorMessageSent(line, MessageColor.Green));
            }
            RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, $"{e.From} ATIS:"));
            foreach (string line in e.Lines)
            {
                RaiseNotificationPosted?.Invoke(this, new NotificationPosted(NotificationType.Info, line));
            }
        }

        [EventSubscription(EventTopics.TransponderIdentStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnSquawkingIdentChanged(object sender, TransponderIdentStateChanged e)
        {
            btnIdent.Clicked = e.Identing;
        }

        [EventSubscription(EventTopics.TransponderModeChanged, typeof(OnUserInterfaceAsync))]
        public void OnTransponderModeChanged(object sender, ClientEventArgs<bool> e)
        {
            if (e.Value)
            {
                chkModeC.Clicked = true;
                chkModeC.Text = "Mode C";
                mConfig.SquawkingModeC = true;
            }
            else
            {
                chkModeC.Clicked = false;
                chkModeC.Text = "Standby";
                mConfig.SquawkingModeC = false;
            }
        }
    }

    public class TreeNodeSorter : IComparer
    {
        public int Compare(object x, object y)
        {
            TreeNode treeNode = x as TreeNode;
            TreeNode treeNode2 = y as TreeNode;

            int result;

            if (treeNode.Level == 0)
            {
                string name = treeNode.Name;
                int num;
                switch (name)
                {
                    case "Center":
                        num = 1;
                        break;
                    case "Approach":
                        num = 2;
                        break;
                    case "Tower":
                        num = 3;
                        break;
                    case "Ground":
                        num = 4;
                        break;
                    case "Delivery":
                        num = 5;
                        break;
                    case "ATIS":
                        num = 6;
                        break;
                    default:
                        num = 7;
                        break;
                }
                string name2 = treeNode2.Name;
                int num2;
                switch (name2)
                {
                    case "Center":
                        num2 = 1;
                        break;
                    case "Approach":
                        num2 = 2;
                        break;
                    case "Tower":
                        num2 = 3;
                        break;
                    case "Ground":
                        num2 = 4;
                        break;
                    case "Delivery":
                        num2 = 5;
                        break;
                    case "ATIS":
                        num2 = 6;
                        break;
                    default:
                        num2 = 7;
                        break;
                }
                result = num.CompareTo(num2);
            }
            else
            {
                result = string.Compare(treeNode.Text, treeNode2.Text);
            }
            return result;
        }
    }
}