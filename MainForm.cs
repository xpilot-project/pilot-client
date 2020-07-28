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
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Gma.System.MouseKeyHook;
using Vatsim.Fsd.Connector;
using XPilot.PilotClient.Common;
using XPilot.PilotClient.Config;
using XPilot.PilotClient.Core;
using XPilot.PilotClient.Core.Events;
using XPilot.PilotClient.Network;
using XPilot.PilotClient.Network.Controllers;
using XPilot.PilotClient.XplaneAdapter;
using XPlaneConnector;

namespace XPilot.PilotClient
{
    public partial class MainForm : Form
    {
        [EventPublication(EventTopics.XPlaneEventPosted)]
        public event EventHandler<ClientEventArgs<string>> XPlaneEventPosted;

        [EventPublication(EventTopics.SessionStarted)]
        public event EventHandler<EventArgs> SessionStarted;

        [EventPublication(EventTopics.SessionEnded)]
        public event EventHandler<EventArgs> SessionEnded;

        [EventPublication(EventTopics.MainFormShown)]
        public event EventHandler<EventArgs> MainFormShown;

        [EventPublication(EventTopics.PlaySoundRequested)]
        public event EventHandler<PlaySoundEventArgs> PlaySoundRequested;

        [EventPublication(EventTopics.RadioMessageSent)]
        public event EventHandler<RadioMessageSentEventArgs> RadioMessageSent;

        [EventPublication(EventTopics.ChatSessionStarted)]
        public event EventHandler<ChatSessionStartedEventArgs> ChatSessionStarted;

        [EventPublication(EventTopics.SetXplaneDataRefValue)]
        public event EventHandler<DataRefEventArgs> SetXplaneDataRefValue;

        [EventPublication(EventTopics.SendXplaneCommand)]
        public event EventHandler<ClientEventArgs<XPlaneCommand>> SendXplaneCommand;

        [EventPublication(EventTopics.WallopRequestSent)]
        public event EventHandler<WallopReceivedEventArgs> WallopRequestSent;

        [EventPublication(EventTopics.PlaySelcalRequested)]
        public event EventHandler<EventArgs> PlaySelcalRequested;

        [EventPublication(EventTopics.RadioTextMessage)]
        public event EventHandler<SimulatorMessageEventArgs> XPlaneRadioTextMessage;

        [EventPublication(EventTopics.MetarRequested)]
        public event EventHandler<MetarRequestedEventArgs> MetarRequestedSent;

        [EventPublication(EventTopics.PrivateMessageSent)]
        public event EventHandler<PrivateMessageSentEventArgs> PrivateMessageSent;

        [EventPublication(EventTopics.OverrideComStatus)]
        public event EventHandler<OverrideComStatusEventArgs> OverrideComStatusSent;

        [EventPublication(EventTopics.ValidateCslPaths)]
        public event EventHandler<EventArgs> ValidateCslPaths;

        [EventPublication(EventTopics.NotificationPosted)]
        public event EventHandler<NotificationPostedEventArgs> NotificationPosted;

        private readonly IEventBroker mEventBroker;
        private readonly IAppConfig mConfig;
        private readonly IFsdManger mNetworkManager;
        private readonly IUserInterface mUserInterface;
        private readonly ITabPages mTabPages;
        private readonly IControllerAtisManager mAtisManager;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private IKeyboardMouseEvents mGlobalHook;

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

        private const string CONFIGURATION_REQUIRED = "xPilot hasn't been fully configured yet. You will not be able to connect to the network until it is configured. Open the settings and verify that your network login credentials are provided and the path to your X-Plane installation is provided.";

        public MainForm(IEventBroker eventBroker, IAppConfig appConfig, IFsdManger networkManager, IUserInterface userInterface, ITabPages tabPages, IControllerAtisManager atisManager)
        {
            InitializeComponent();

            mEventBroker = eventBroker;
            mConfig = appConfig;
            mNetworkManager = networkManager;
            mUserInterface = userInterface;
            mTabPages = tabPages;
            mAtisManager = atisManager;

            mCheckSimConnection = new Timer
            {
                Interval = 1000
            };
            mCheckSimConnection.Tick += CheckSimConnection_Tick;
            mCheckSimConnection.Start();

            ChatMessageBox.TextCommandLine.TextCommandReceived += MainForm_TextCommandReceived;
            ChatMessageBox.RichTextBox.MouseUp += rtfMessages_MouseUp;
            ChatMessageBox.KeyDown += ChatMessageBox_KeyDown;

            treeControllers.MouseUp += TreeControllers_MouseUp;
            treeControllers.BeforeSelect += TreeControllers_BeforeSelect;
            treeControllers.BeforeCollapse += TreeControllers_BeforeCollapse;
            treeControllers.ExpandAll();
            treeControllers.TreeViewNodeSorter = new TreeNodeSorter();

            tabNotes = mTabPages.CreateNotesTab();
            tabNotes.Text = "Notes";
            tabControl.TabPages.Add(tabNotes);

            GlobalHookSubscribe();
            mEventBroker.Register(this);
        }

        protected override void OnLoad(EventArgs e)
        {
            SessionStarted?.Invoke(this, EventArgs.Empty);
            ValidateCslPaths?.Invoke(this, EventArgs.Empty);
            MainFormShown?.Invoke(this, EventArgs.Empty);
            mNetworkManager.DownloadNetworkServers();
            ScreenUtils.ApplyWindowProperties(mConfig.ClientWindowProperties, this);
            mInitializing = false;
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"xPilot Version {Application.ProductVersion}"));

            if (mConfig.ConfigurationRequired)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, CONFIGURATION_REQUIRED));
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
            }
        }

        private void ChatMessageBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (mConfig.ToggleDisplayConfiguration != null)
            {
                if (e.KeyCode == (Keys)mConfig.ToggleDisplayConfiguration.KeyCode)
                {
                    e.SuppressKeyPress = true;
                }
            }
        }

        public void GlobalHookSubscribe()
        {
            mGlobalHook = Hook.GlobalEvents();
            mGlobalHook.KeyDown += MGlobalHook_KeyDown;
        }

        public void GlobalHookUnsubscribe()
        {
            if (mGlobalHook == null) return;
            mGlobalHook.KeyDown -= MGlobalHook_KeyDown;
            mGlobalHook.Dispose();
            mGlobalHook = null;
        }

        private void MGlobalHook_KeyDown(object sender, KeyEventArgs e)
        {
            if (mConfig.ToggleDisplayConfiguration != null)
            {
                if (e.KeyCode == (Keys)mConfig.ToggleDisplayConfiguration.KeyCode)
                {
                    ScreenUtils.ToggleVisibility(mConfig.ClientWindowProperties, this);
                }
            }
        }

        private void MainForm_TextCommandReceived(object sender, ClientEventArgs<string> e)
        {
            string[] split = e.Value.Split(new char[] { ' ' });

            XPlaneConnector.XPlaneConnector xp = new XPlaneConnector.XPlaneConnector();

            try
            {
                if (e.Value.StartsWith("."))
                {
                    switch (split[0].ToLower())
                    {
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

                                SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                {
                                    DataRef = "sim/cockpit/radios/transponder_code"
                                }, code));

                                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"Transponder code set to {code:0000}."));
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

                                SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                {
                                    DataRef = $"sim/cockpit2/radios/actuators/{ (com == 1 ? "com1" : "com2") }_frequency_hz"
                                }, (f + 100000) / 10));
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
                                        SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                        {
                                            DataRef = "sim/cockpit2/radios/actuators/audio_com_selection"
                                        }, 6));
                                        mForceCom1Tx = true;
                                        mForceCom2Tx = false;
                                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"COM{radio} transmit enabled."));
                                        break;
                                    case 2:
                                        SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                        {
                                            DataRef = "sim/cockpit2/radios/actuators/audio_com_selection"
                                        }, 7));
                                        mForceCom2Tx = true;
                                        mForceCom1Tx = false;
                                        NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"COM{radio} transmit enabled."));
                                        break;
                                }

                                OverrideComStatusSent?.Invoke(this, new OverrideComStatusEventArgs(mForceCom1Rx, mForceCom1Tx, mForceCom2Rx, mForceCom2Tx));
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
                                            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                            {
                                                DataRef = "sim/cockpit2/radios/actuators/audio_selection_com1"
                                            }, 1));
                                            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                            {
                                                DataRef = "sim/cockpit2/radios/actuators/audio_selection_com2"
                                            }, 0));
                                            mForceCom1Rx = true;
                                            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"COM{radio} receiver on."));
                                            break;
                                        case 2:
                                            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                            {
                                                DataRef = "sim/cockpit2/radios/actuators/audio_selection_com1"
                                            }, 0));
                                            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                            {
                                                DataRef = "sim/cockpit2/radios/actuators/audio_selection_com2"
                                            }, 1));
                                            mForceCom2Rx = true;
                                            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"COM{radio} receiver on."));
                                            break;
                                    }
                                }
                                else
                                {
                                    switch (radio)
                                    {
                                        case 1:
                                            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                            {
                                                DataRef = "sim/cockpit2/radios/actuators/audio_selection_com1"
                                            }, 0));
                                            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                            {
                                                DataRef = "sim/cockpit2/radios/actuators/audio_selection_com2"
                                            }, 0));
                                            mForceCom1Rx = false;
                                            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"COM{radio} receiver off."));
                                            break;
                                        case 2:
                                            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                            {
                                                DataRef = "sim/cockpit2/radios/actuators/audio_selection_com1"
                                            }, 0));
                                            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
                                            {
                                                DataRef = "sim/cockpit2/radios/actuators/audio_selection_com2"
                                            }, 0));
                                            mForceCom2Rx = false;
                                            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"COM{radio} receiver off."));
                                            break;
                                    }
                                }

                                OverrideComStatusSent?.Invoke(this, new OverrideComStatusEventArgs(mForceCom1Rx, mForceCom1Tx, mForceCom2Rx, mForceCom2Tx));
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
                                        PrivateMessageSent?.Invoke(this, new PrivateMessageSentEventArgs(mNetworkManager.OurCallsign, split[1].ToUpper(), string.Join(" ", split.Skip(2))));
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
                                WallopRequestSent?.Invoke(this, new WallopReceivedEventArgs(string.Join(" ", split.Skip(1))));
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
                                MetarRequestedSent?.Invoke(this, new MetarRequestedEventArgs(split[1]));
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
                            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.SentRadioMessage, $"{mNetworkManager.OurCallsign}: {e.Value}"));
                            RadioMessageSent?.Invoke(this, new RadioMessageSentEventArgs(mNetworkManager.OurCallsign, e.Value, TunedFrequencies().ToArray()));
                            XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs($"{ mNetworkManager.OurCallsign }: {e.Value}"));
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
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, ex.Message));
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
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
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "Waiting for X-Plane connection..."));
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
                                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, CONFIGURATION_REQUIRED));
                                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
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
                                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "You must first select a server in the settings window."));
                                }
                            }
                        }
                        else
                        {
                            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "xPilot is unable to connect to X-Plane. Please make sure X-Plane is running and a flight is loaded."));
                            PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
                        }
                    }
                }
            }

            TextCommandFocus();
        }

        private void btnFlightPlan_Click(object sender, EventArgs e)
        {
            if (btnFlightPlan.Enabled)
            {
                using (FlightPlanForm dlg = mUserInterface.CreateFlightPlanForm())
                {
                    dlg.ShowDialog(this);
                }
                TextCommandFocus();
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            using (SettingsForm dlg = mUserInterface.CreateSettingsForm())
            {
                dlg.ShowDialog(this);
            }
            TextCommandFocus();
        }

        private void chkModeC_Click(object sender, EventArgs e)
        {
            var laminarB738 = new DataRefElement
            {
                DataRef = "laminar/B738/knob/transpoder_pos"
            };

            var tolis = new DataRefElement
            {
                DataRef = "ckpt/transponder/mode/anim"
            };

            var laminarB738_Dn_Cmd = new XPlaneCommand("laminar/B738/knob/transponder_mode_up", "");
            var laminarB738_Up_Cmd = new XPlaneCommand("laminar/B738/knob/transponder_mode_up", "");

            if (mFlightLoaded)
            {
                if (chkModeC.Checked)
                {
                    SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(laminarB738_Up_Cmd));
                    SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(laminarB738, 3));
                    SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(Commands.TransponderTransponderAlt));

                    // tolis a319
                    SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(tolis, 0));
                    mConfig.SquawkingModeC = true;
                }
                else
                {
                    SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(laminarB738_Dn_Cmd));
                    SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(laminarB738, 1));
                    SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(Commands.TransponderTransponderOff));

                    // tolis a319
                    SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(tolis, 4));
                    mConfig.SquawkingModeC = false;
                }
            }
            else
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, "xPilot is unable to connect to X-Plane. Please make sure X-Plane is running and a flight is loaded."));
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
            }

            TextCommandFocus();
        }

        private void btnIdent_Click(object sender, EventArgs e)
        {
            if (mNetworkManager.IsConnected)
            {
                mNetworkManager.SquawkIdent();
            }

            if (mFlightLoaded)
            {
                SendXplaneCommand?.Invoke(this, new ClientEventArgs<XPlaneCommand>(Commands.TransponderTransponderIdent));
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
            controllerTreeContextMenu.Show(treeControllers, point);
        }

        private void startPrivateChat_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            ChatSessionStarted?.Invoke(this, new ChatSessionStartedEventArgs(item.Tag.ToString()));
        }

        private void requestControllerInfo_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            mAtisManager.RequestControllerAtis(item.Tag.ToString());
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
                else
                {
                    XplaneConnect Data = new XplaneConnect
                    {
                        Type = XplaneConnect.MessageType.RemoveAllPlanes,
                        Timestamp = DateTime.Now
                    };
                    XPlaneEventPosted?.Invoke(this, new ClientEventArgs<string>(Data.ToJSON()));
                }
            }

            SessionEnded?.Invoke(this, new EventArgs());
            mConfig.SaveConfig();
            mEventBroker.Unregister(this);
            GlobalHookUnsubscribe();
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

        [EventSubscription(EventTopics.ToggleConnectButtonState, typeof(OnUserInterfaceAsync))]
        public void OnDisableConnectButton(object sender, ClientEventArgs<bool> e)
        {
            if (e.Value == false)
            {
                btnConnect.Enabled = false;
                btnConnect.ForeColor = Color.FromArgb(100, 100, 100);
                if (mNetworkManager.IsConnected)
                {
                    mNetworkManager.Disconnect(new DisconnectInfo
                    {
                        Type = DisconnectType.Intentional
                    });
                }
            }
            else
            {
                btnConnect.Enabled = true;
                btnConnect.ForeColor = Color.FromArgb(230, 230, 230);
                btnConnect.BackColor = Color.Transparent;
            }
        }

        [EventSubscription(EventTopics.VoiceServerConnectionLost, typeof(OnUserInterfaceAsync))]
        public void OnVoiceServerConnectionLost(object sender, EventArgs e)
        {
            FlashTaskbar.Flash(this);
        }


        [EventSubscription(EventTopics.SocketMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnSocketMessageReceived(object sender, ClientEventArgs<string> e)
        {
            if (!string.IsNullOrEmpty(e.Value))
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.SentRadioMessage, $"{mNetworkManager.OurCallsign}: {e.Value}"));
                RadioMessageSent?.Invoke(this, new RadioMessageSentEventArgs(mNetworkManager.OurCallsign, e.Value, TunedFrequencies().ToArray()));
            }
        }

        [EventSubscription(EventTopics.ToggleClientDisplay, typeof(OnUserInterfaceAsync))]
        public void OnToggleClientDisplayReceived(object sender, ClientEventArgs<bool> e)
        {
            WindowState = e.Value ? FormWindowState.Normal : FormWindowState.Minimized;
        }

        [EventSubscription(EventTopics.ChatSessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnChatSessionStarted(object sender, ChatSessionStartedEventArgs e)
        {
            InitializeChatSession(e.Callsign);
        }

        [EventSubscription(EventTopics.ServerListDownloadFailed, typeof(OnUserInterfaceAsync))]
        public void OnServerListDownloadFailed(object sender, ClientEventArgs<string> e)
        {
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Error, e.Value));
        }

        [EventSubscription(EventTopics.NotificationPosted, typeof(OnUserInterfaceAsync))]
        public void OnNotificationPosted(object sender, NotificationPostedEventArgs e)
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
        public void ServerMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs($"[SERVER] {e.Data}", 189, 195, 199));
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.ServerMessage, $"[SERVER] {e.Data}"));
        }

        [EventSubscription(EventTopics.BroadcastMessageReceived, typeof(OnUserInterfaceAsync))]
        public void BroadcastMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs($"[BROADCAST] {e.Data}", 22, 160, 133, true));
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.BroadcastMessage, $"[BROADCAST] {e.From}: {e.Data}"));
            PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Broadcast));
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            mConnected = true;
            btnConnect.Text = "Disconnect";
            btnConnect.ForeColor = Color.White;
            btnConnect.BackColor = Color.FromArgb(0, 120, 206);
            lblCallsign.Text = e.ConnectInfo.Callsign;

            Text = $"xPilot - {e.ConnectInfo.Callsign}";

            if (mConnectInfo != null)
            {
                chkModeC.Enabled = mConnectInfo.ObserverMode ? false : true;
                btnIdent.Enabled = mConnectInfo.ObserverMode ? false : true;

                chkModeC.ForeColor = mConnectInfo.ObserverMode ? Color.FromArgb(100, 100, 100) : Color.White;
                btnIdent.ForeColor = mConnectInfo.ObserverMode ? Color.FromArgb(100, 100, 100) : Color.White;
            }

            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
            {
                DataRef = "xpilot/login/status"
            }, 1));
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            Text = "xPilot";

            chkModeC.Enabled = false;
            btnIdent.Enabled = false;

            chkModeC.ForeColor = Color.FromArgb(100, 100, 100);
            btnIdent.ForeColor = Color.FromArgb(100, 100, 100);

            mConnected = false;
            btnConnect.Text = "Connect";
            btnConnect.ForeColor = Color.FromArgb(230, 230, 230);
            btnConnect.BackColor = Color.Transparent;
            lblCallsign.Text = "----------";

            XplaneConnect Data = new XplaneConnect
            {
                Type = XplaneConnect.MessageType.RemoveAllPlanes,
                Timestamp = DateTime.Now
            };
            XPlaneEventPosted?.Invoke(this, new ClientEventArgs<string>(Data.ToJSON()));

            Data = new XplaneConnect
            {
                Type = XplaneConnect.MessageType.NetworkDisconnected,
                Timestamp = DateTime.Now
            };
            XPlaneEventPosted?.Invoke(this, new ClientEventArgs<string>(Data.ToJSON()));

            SetXplaneDataRefValue?.Invoke(this, new DataRefEventArgs(new DataRefElement
            {
                DataRef = "xpilot/login/status"
            }, 0));

            if (mConfig.FlashTaskbarDisconnect && (e.Info != null && e.Info.Type != DisconnectType.Intentional))
            {
                FlashTaskbar.Flash(this);
            }
        }

        [EventSubscription(EventTopics.ControllerAdded, typeof(OnUserInterfaceAsync))]
        public void OnControllerUpdateReceived(object sender, ControllerEventArgs e)
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
        public void OnDeleteControllerReceived(object sender, ControllerEventArgs e)
        {
            if (!treeControllers.IsDisposed && !treeControllers.Disposing)
            {
                foreach (TreeNode treeNode in treeControllers.Nodes)
                {
                    treeNode.Nodes.RemoveByKey(e.Controller.Callsign);
                }
            }
        }

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnUserInterfaceAsync))]
        public void RealNameReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            TreeNode treeNode = FindController(e.From.Replace("*", ""));
            if (treeNode != null)
            {
                treeNode.ToolTipText = (string.IsNullOrEmpty(e.Data) ? "Unknown" : e.Data);
            }
        }

        [EventSubscription(EventTopics.ControllerFrequencyChanged, typeof(OnUserInterfaceAsync))]
        public void ControllerFrequencyChanged(object sender, ControllerEventArgs e)
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
                    XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs("xPilot client successfully connected to X-Plane.", 0, 168, 255));
                    NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, "X-Plane connection established."));
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
        public void OnRadioStackStateChanged(object sender, RadioStackStateChangedEventArgs e)
        {
            if (mRadioStackData == null || !e.RadioStackState.Equals(mRadioStackData))
            {
                if (e.RadioStackState.HasPower)
                {
                    if (e.RadioStackState.Com1ActiveFreq > 0)
                    {
                        Com1Freq.Text = (e.RadioStackState.Com1ActiveFreq.Normalize25KhzFrequency() / 1000000.0f).ToString("0.000");
                        Com1TX.ForeColor = e.RadioStackState.IsCom1Transmitting ? Color.White : Color.FromArgb(39, 44, 46);
                        Com1RX.ForeColor = e.RadioStackState.IsCom1Receiving ? Color.White : Color.FromArgb(39, 44, 46);
                    }
                    else
                    {
                        Com1Freq.Text = "---.---";
                        Com1TX.ForeColor = Color.FromArgb(39, 44, 46);
                        Com1RX.ForeColor = Color.FromArgb(39, 44, 46);
                    }

                    if (e.RadioStackState.Com2ActiveFreq > 0)
                    {
                        Com2Freq.Text = (e.RadioStackState.Com2ActiveFreq.Normalize25KhzFrequency() / 1000000.0f).ToString("0.000");
                        Com2TX.ForeColor = e.RadioStackState.IsCom2Transmitting ? Color.White : Color.FromArgb(39, 44, 46);
                        Com2RX.ForeColor = e.RadioStackState.IsCom2Receiving ? Color.White : Color.FromArgb(39, 44, 46);
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
                mRadioStackData = e.RadioStackState;
            }
        }

        [EventSubscription(EventTopics.Com1FrequencyAliasChanged, typeof(OnUserInterfaceAsync))]
        public void OnCom1FrequencyAliasChanged(object sender, ComFrequencyAliasChangedEventArgs e)
        {
            if (e.HfFrequency > 0)
            {
                hfTooltip.SetToolTip(Com1Freq, $"HF: {((double)e.HfFrequency / 1000000).ToString("0.00000")}");
            }
            else
            {
                hfTooltip.SetToolTip(Com1Freq, null);
            }
        }

        [EventSubscription(EventTopics.Com2FrequencyAliasChanged, typeof(OnUserInterfaceAsync))]
        public void OnCom2FrequencyAliasChanged(object sender, ComFrequencyAliasChangedEventArgs e)
        {
            if (e.HfFrequency > 0)
            {
                hfTooltip.SetToolTip(Com2Freq, $"HF: {((double)e.HfFrequency / 1000000).ToString("0.00000")}");
            }
            else
            {
                hfTooltip.SetToolTip(Com2Freq, null);
            }
        }

        [EventSubscription(EventTopics.ComRadioTransmittingChanged, typeof(OnUserInterfaceAsync))]
        public void ComRadioTransmittingChanged(object sender, ComRadioTxRxChangedEventArgs e)
        {
            switch (e.Radio)
            {
                case 1:
                    Com1TX.BackColor = e.TxRx ? Color.FromArgb(39, 174, 96) : Color.Transparent;
                    break;
                case 2:
                    Com2TX.BackColor = e.TxRx ? Color.FromArgb(39, 174, 96) : Color.Transparent;
                    break;
            }
        }

        [EventSubscription(EventTopics.ComRadioReceivingChanged, typeof(OnUserInterfaceAsync))]
        public void ComRadioReceivingChanged(object sender, ComRadioTxRxChangedEventArgs e)
        {
            switch (e.Radio)
            {
                case 1:
                    Com1RX.BackColor = e.TxRx ? Color.FromArgb(39, 174, 96) : Color.Transparent;
                    break;
                case 2:
                    Com2RX.BackColor = e.TxRx ? Color.FromArgb(39, 174, 96) : Color.Transparent;
                    break;
            }
        }

        [EventSubscription(EventTopics.UserAircraftDataUpdated, typeof(OnUserInterfaceAsync))]
        public void UserAircraftDataUpdateReceived(object sender, UserAircraftDataUpdatedEventArgs e)
        {
            if (!mReplayMode && (mNetworkManager.IsConnected && e.UserAircraftData.ReplayModeEnabled))
            {
                XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs($"You have been disconnected from the network because Replay Mode is enabled.", 211, 84, 0, true));
                mNetworkManager.ForceDisconnect("You have been disconnected from the network because Replay Mode is enabled.");
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.Error));
                mReplayMode = true;
            }
        }

        [EventSubscription(EventTopics.MetarReceived, typeof(OnUserInterfaceAsync))]
        public void MetarResponseReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs($"METAR: {e.Data}", 243, 156, 18));
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"METAR: {e.Data}"));
        }

        [EventSubscription(EventTopics.SelcalAlertReceived, typeof(OnUserInterfaceAsync))]
        public void SelcalAlertReceived(object sender, SelcalAlertReceivedEventArgs e)
        {
            XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs($"SELCAL alert received on {e.Frequencies[0].FormatFromNetwork()}", 39, 174, 96, true));
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"SELCAL alert received on {e.Frequencies[0].FormatFromNetwork()}."));
            if (mConfig.PlayGenericSelCalAlert)
            {
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.SelCal));
            }
            else
            {
                PlaySelcalRequested?.Invoke(this, EventArgs.Empty);
            }
            if (mConfig.FlashTaskbarSelCal)
            {
                FlashTaskbar.Flash(this);
            }
        }

        [EventSubscription(EventTopics.RadioMessageReceived, typeof(OnUserInterfaceAsync))]
        public void RadioMessageReceived(object sender, RadioMessageReceivedEventArgs e)
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
                    XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs(message, 241, 196, 15, true));
                }
                else
                {
                    XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs(message, 189, 195, 199));
                }
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(e.IsDirect ? NotificationType.DirectRadioMessage : NotificationType.RadioMessage, message));
                if (e.IsDirect)
                {
                    PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.DirectRadioMessage));
                    if (mConfig.FlashTaskbarRadioMessage)
                    {
                        FlashTaskbar.Flash(this);
                    }
                }

                if (mConfig.PlayRadioMessageAlert)
                {
                    PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.RadioMessage));
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
            TopMost = mConfig.KeepClientWindowVisible;
        }

        [EventSubscription(EventTopics.PrivateMessageReceived, typeof(OnUserInterfaceAsync))]
        public void PrivateMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            PrivateMessageTab tab = GetPrivateMessageTabIfExists(e.From);
            if (tab == null)
            {
                PlaySoundRequested?.Invoke(this, new PlaySoundEventArgs(SoundEvent.PrivateMessage));
                if (mConfig.FlashTaskbarPrivateMessage)
                {
                    FlashTaskbar.Flash(this);
                }
                CreatePrivateMessageTab(e.From, e.Data);
            }
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSentEventArgs e)
        {
            PrivateMessageTab tab = GetPrivateMessageTabIfExists(e.To);
            if (tab == null)
            {
                CreatePrivateMessageTab(e.To, e.Message, true, e.From);
                SetActiveMessageTab(e.To);
            }
        }

        [EventSubscription(EventTopics.RequestedAtisReceived, typeof(OnUserInterfaceAsync))]
        public void OnRequestedAtisReceived(object sender, RequestedAtisReceivedEventArgs e)
        {
            XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs($"{e.From} ATIS:", 39, 174, 96));
            foreach (string line in e.Lines)
            {
                XPlaneRadioTextMessage?.Invoke(this, new SimulatorMessageEventArgs(line, 39, 174, 96));
            }
            NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, $"{e.From} ATIS:"));
            foreach (string line in e.Lines)
            {
                NotificationPosted?.Invoke(this, new NotificationPostedEventArgs(NotificationType.Info, line));
            }
        }

        [EventSubscription(EventTopics.SquawkingIdentChanged, typeof(OnUserInterfaceAsync))]
        public void OnSquawkingIdentChanged(object sender, SquawkingIdentChangedEventArgs e)
        {
            btnIdent.Checked = e.SquawkingIdent;
        }

        [EventSubscription(EventTopics.TransponderModeChanged, typeof(OnUserInterfaceAsync))]
        public void OnTransponderModeChanged(object sender, ClientEventArgs<bool> e)
        {
            if (e.Value)
            {
                chkModeC.Checked = true;
                chkModeC.Text = "Mode C";
                mConfig.SquawkingModeC = true;
            }
            else
            {
                chkModeC.Checked = false;
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
