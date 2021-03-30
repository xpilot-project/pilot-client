﻿/*
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
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Appccelerate.EventBroker;
using Appccelerate.EventBroker.Handlers;
using Vatsim.Xpilot.Config;
using Vatsim.Xpilot.Networking;
using Vatsim.Xpilot.Controllers;
using Vatsim.Xpilot.Simulator;
using Vatsim.Xpilot.Core;
using Vatsim.Xpilot.Events.Arguments;
using Vatsim.Xpilot.Common;
using Vatsim.Xpilot.Aircrafts;
using Vatsim.FsdClient;

namespace Vatsim.Xpilot
{
    public partial class MainForm : Form
    {
        [EventPublication(EventTopics.MainFormShown)]
        public event EventHandler<EventArgs> MainFormShown;

        [EventPublication(EventTopics.SessionStarted)]
        public event EventHandler<EventArgs> SessionStarted;

        [EventPublication(EventTopics.SessionEnded)]
        public event EventHandler<EventArgs> SessionEnded;

        [EventPublication(EventTopics.PlayNotificationSound)]
        public event EventHandler<PlayNotifictionSoundEventArgs> PlayNotificationSound;

        [EventPublication(EventTopics.RadioMessageSent)]
        public event EventHandler<RadioMessageSentEventArgs> RadioMessageSent;

        private readonly IEventBroker mEventBroker;
        private readonly IAppConfig mConfig;
        private readonly INetworkManager mNetworkManager;
        private readonly IUserInterface mUserInterface;
        private readonly ITabPages mTabPages;
        private readonly IControllerAtisManager mAtisManager;
        private readonly IControllerManager mControllerManager;
        private readonly IXplaneAdapter mXplaneAdapter;

        private bool mInitializing = true;
        private ConnectInfo mConnectInfo;
        private NotesTab mTabNotes;
        private RadioStackState mRadioStackState;
        private Color mActiveColor = Color.FromArgb(39, 174, 96);

        private const string CONFIGURATION_REQUIRED = "xPilot has not been fully configured yet. You will not be able to connect to the network until it is configured. Open Settings and verify your network login credentials are saved.";

        public MainForm(IEventBroker eventBroker, IAppConfig appConfig, INetworkManager networkManager, IUserInterface userInterface, ITabPages tabPages, IControllerAtisManager atisManager, IControllerManager controllerManager, IXplaneAdapter xplane)
        {
            InitializeComponent();

            mEventBroker = eventBroker;
            mEventBroker.Register(this);

            mConfig = appConfig;
            mNetworkManager = networkManager;
            mUserInterface = userInterface;
            mTabPages = tabPages;
            mAtisManager = atisManager;
            mControllerManager = controllerManager;
            mXplaneAdapter = xplane;

            RtfMessages.TextCommandLine.TextCommandReceived += TextCommandLine_TextCommandReceived;
            RtfMessages.RichTextBox.MouseUp += RichTextBox_MouseUp;

            TreeControllers.ExpandAll();
            TreeControllers.TreeViewNodeSorter = new TreeNodeSorter();
            
            TopMost = mConfig.KeepWindowVisible;

            mTabNotes = mTabPages.CreateNotesTab();
            mTabNotes.Text = "Notes";
            TabsMain.TabPages.Add(mTabNotes);
        }

        [EventSubscription(EventTopics.NotificationPosted, typeof(OnUserInterfaceAsync))]
        public void OnNotificationPosted(object sender, NotificationPostedEventArgs e)
        {
            switch (e.Type)
            {
                case NotificationType.Info:
                    WriteInfoMessage(e.Message);
                    break;
                case NotificationType.Warning:
                    WriteWarningMessage(e.Message);
                    break;
                case NotificationType.Error:
                    WriteErrorMessage(e.Message);
                    break;
            }
        }

        [EventSubscription(EventTopics.SettingsModified, typeof(OnUserInterfaceAsync))]
        public void OnSettingsModified(object sender, EventArgs e)
        {
            TopMost = mConfig.KeepWindowVisible;
        }

        [EventSubscription(EventTopics.ConnectButtonDisabled, typeof(OnUserInterfaceAsync))]
        public void OnConnectButtonDisabled(object sender, EventArgs e)
        {
            BtnConnect.Enabled = false;
        }

        [EventSubscription(EventTopics.ConnectButtonEnabled, typeof(OnUserInterfaceAsync))]
        public void OnConnectButtonEnabled(object sender, EventArgs e)
        {
            BtnConnect.Enabled = true;
        }

        [EventSubscription(EventTopics.SimulatorConnected, typeof(OnUserInterfaceAsync))]
        public void OnSimulatorConnected(object sender, EventArgs e)
        {
            ChkModeC.Enabled = true;
            ChkIdent.Enabled = true;
            SyncRadioStackState();
        }

        [EventSubscription(EventTopics.SimulatorDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnSimulatorDisconnected(object sender, EventArgs e)
        {
            ChkModeC.Enabled = false;
            ChkModeC.Clicked = false;

            ChkIdent.Enabled = false;
            ChkIdent.Clicked = false;

            Com1Freq.Text = "---.---";
            Com2Freq.Text = "---.---";

            if (mNetworkManager.IsConnected)
            {
                mNetworkManager.Disconnect();
                WriteErrorMessage("X-Plane connection lost. Disconnected from network.");
                if (mConfig.FlashTaskbarDisconnect)
                {
                    FlashTaskbar.Flash(this);
                }
            }
        }

        [EventSubscription(EventTopics.NetworkConnectionInitiated, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnectionInitiated(object sender, EventArgs e)
        {
            BtnConnect.Enabled = false;
            BtnConnect.Text = "Connecting";
            BtnConnect.Pushed = true;
        }

        [EventSubscription(EventTopics.NetworkDisconnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkDisconnected(object sender, NetworkDisconnectedEventArgs e)
        {
            HandleNetworkDisconnected();
        }

        [EventSubscription(EventTopics.NetworkConnectionFailed, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnectionFailed(object sender, EventArgs e)
        {
            HandleNetworkDisconnected();
        }

        [EventSubscription(EventTopics.NetworkConnected, typeof(OnUserInterfaceAsync))]
        public void OnNetworkConnected(object sender, NetworkConnectedEventArgs e)
        {
            LblCallsign.Text = e.ConnectInfo.Callsign;
            BtnConnect.Enabled = true;
            BtnConnect.Text = "Disconnect";
            BtnConnect.Pushed = true;
        }

        [EventSubscription(EventTopics.RequestedAtisReceived, typeof(OnUserInterfaceAsync))]
        public void OnRequestAtisReceived(object sender, RequestedAtisReceivedEventArgs e)
        {
            WriteMessage(Color.Green, $"{e.From} ATIS:", true);
            foreach (string line in e.Lines)
            {
                WriteMessage(Color.Green, line, false);
            }
        }

        [EventSubscription(EventTopics.ChatSessionStarted, typeof(OnUserInterfaceAsync))]
        public void OnChatSessionStarted(object sender, ChatSessionStartedEventArgs e)
        {
            HandleChatSessionStarted(e.Callsign);
        }

        [EventSubscription(EventTopics.PrivateMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageSent(object sender, PrivateMessageSentEventArgs e)
        {
            HandlePrivateMessageSent(e.To, e.Message);
        }

        [EventSubscription(EventTopics.PrivateMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnPrivateMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            PrivateMessageTab tab = GetPrivateMessageTabIfExists(e.From);
            if (tab == null)
            {
                if (mConfig.FlashTaskbarPrivateMessage)
                {
                    FlashTaskbar.Flash(this);
                }
                CreatePrivateMessageTab(e.From, e.Data);
            }
        }

        [EventSubscription(EventTopics.RealNameReceived, typeof(OnUserInterfaceAsync))]
        public void OnRealNameReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            TreeNode treeNode = FindController(e.From);
            if (treeNode != null)
            {
                treeNode.ToolTipText = string.IsNullOrEmpty(e.Data) ? "Unknown" : e.Data;
            }
        }

        [EventSubscription(EventTopics.ControllerAdded, typeof(OnUserInterfaceAsync))]
        public void OnControllerAdded(object sender, ControllerEventArgs e)
        {
            TreeNode selectedNode = TreeControllers.SelectedNode;
            string frequency = e.Controller.NormalizedFrequency.FormatFromNetwork();
            TreeNode treeNode;
            string nodeText;
            if (e.Controller.Callsign.EndsWith("_CTR") || e.Controller.Callsign.EndsWith("_FSS"))
            {
                treeNode = TreeControllers.Nodes["Center"];
                nodeText = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_APP") || e.Controller.Callsign.EndsWith("_DEP"))
            {
                treeNode = TreeControllers.Nodes["Approach"];
                nodeText = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_TWR"))
            {
                treeNode = TreeControllers.Nodes["Tower"];
                nodeText = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_GND"))
            {
                treeNode = TreeControllers.Nodes["Ground"];
                nodeText = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_DEL"))
            {
                treeNode = TreeControllers.Nodes["Delivery"];
                nodeText = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else if (e.Controller.Callsign.EndsWith("_ATIS"))
            {
                treeNode = TreeControllers.Nodes["ATIS"];
                nodeText = string.Format("{0} - {1}", e.Controller.Callsign.Replace("*", ""), frequency);
            }
            else
            {
                treeNode = TreeControllers.Nodes["Observers"];
                nodeText = e.Controller.Callsign.Replace("*", "");
            }
            treeNode.Nodes.Add(e.Controller.Callsign, nodeText).ToolTipText = string.IsNullOrEmpty(e.Controller.RealName) ? "Unknown" : e.Controller.RealName;
            treeNode.Expand();
            TreeControllers.Sort();
            TreeControllers.SelectedNode = selectedNode;
        }

        [EventSubscription(EventTopics.ControllerDeleted, typeof(OnUserInterfaceAsync))]
        public void OnControllerDeleted(object sender, ControllerEventArgs e)
        {
            if (TreeControllers.IsDisposed || TreeControllers.Disposing)
            {
                return;
            }
            foreach (TreeNode node in TreeControllers.Nodes)
            {
                node.Nodes.RemoveByKey(e.Controller.Callsign);
            }
        }

        [EventSubscription(EventTopics.ControllerFrequencyChanged, typeof(OnUserInterfaceAsync))]
        public void OnControllerFrequencyChanged(object sender, ControllerEventArgs e)
        {
            TreeNode treeNode = FindController(e.Controller.Callsign);
            if (treeNode != null)
            {
                if (treeNode.Parent.Name == "Observers")
                {
                    treeNode.Text = e.Controller.Callsign;
                }
                else
                {
                    treeNode.Text = $"{e.Controller.Callsign} - {e.Controller.NormalizedFrequency.FormatFromNetwork()}";
                }
            }
        }

        [EventSubscription(EventTopics.RadioStackStateChanged, typeof(OnUserInterfaceAsync))]
        public void OnRadioStackStateChanged(object sender, RadioStackStateChangedEventArgs e)
        {
            if (!mRadioStackState.Equals(e.RadioStackState))
            {
                mRadioStackState = e.RadioStackState;
                SyncRadioStackState();
            }
        }

        [EventSubscription(EventTopics.ReplayModeEnabled, typeof(OnUserInterfaceAsync))]
        public void OnReplayModeEnabled(object sender, EventArgs e)
        {
            if (mNetworkManager.IsConnected)
            {
                WriteErrorMessage("Disconnected from network because Replay Mode is enabled");
                mNetworkManager.Disconnect();
            }
        }

        private void SyncRadioStackState()
        {
            if (!mXplaneAdapter.ValidSimConnection)
            {
                return;
            }

            Color inactiveColor = Color.FromArgb(39, 44, 46);

            ChkModeC.Clicked = mRadioStackState.SquawkingModeC;
            ChkIdent.Clicked = mRadioStackState.SquawkingIdent;

            if (mRadioStackState.AvionicsPowerOn)
            {
                if (mRadioStackState.Com1ActiveFrequency > 0)
                {
                    Com1Freq.Text = (mRadioStackState.Com1ActiveFrequency.Normalize25KhzFrequency() / 1000000.0f).ToString("0.000");
                    Com1TX.ForeColor = mRadioStackState.Com1TransmitEnabled ? Color.White : inactiveColor;
                    Com1RX.ForeColor = mRadioStackState.Com1ReceiveEnabled ? Color.White : inactiveColor;
                }
                else
                {
                    Com1Freq.Text = "---.---";
                    Com1TX.ForeColor = inactiveColor;
                    Com1RX.ForeColor = inactiveColor;
                }

                if (mRadioStackState.Com2ActiveFrequency > 0)
                {
                    Com2Freq.Text = (mRadioStackState.Com2ActiveFrequency.Normalize25KhzFrequency() / 1000000.0f).ToString("0.000");
                    Com2TX.ForeColor = mRadioStackState.Com2TransmitEnabled ? Color.White : inactiveColor;
                    Com2RX.ForeColor = mRadioStackState.Com2ReceiveEnabled ? Color.White : inactiveColor;
                }
                else
                {
                    Com2Freq.Text = "---.---";
                    Com2TX.ForeColor = inactiveColor;
                    Com2RX.ForeColor = inactiveColor;
                }
            }
            else
            {
                Com1Freq.Text = "---.---";
                Com1TX.ForeColor = inactiveColor;
                Com1RX.ForeColor = inactiveColor;

                Com2Freq.Text = "---.---";
                Com2TX.ForeColor = inactiveColor;
                Com2RX.ForeColor = inactiveColor;
            }
        }

        [EventSubscription(EventTopics.ComRadioTransmittingChanged, typeof(OnUserInterfaceAsync))]
        public void OnComRadioTransmittingChanged(object sender, ComRadioTxRxChangedEventArgs e)
        {
            switch (e.Radio)
            {
                case 0:
                    Com1TX.BackColor = e.TxRx && mRadioStackState.Com1ReceiveEnabled ? mActiveColor : Color.Transparent;
                    break;
                case 1:
                    Com2TX.BackColor = e.TxRx && mRadioStackState.Com2ReceiveEnabled ? mActiveColor : Color.Transparent;
                    break;
            }
        }

        [EventSubscription(EventTopics.ComRadioReceivingChanged, typeof(OnUserInterfaceAsync))]
        public void OnComRadioReceivingChanged(object sender, ComRadioTxRxChangedEventArgs e)
        {
            switch (e.Radio)
            {
                case 0:
                    Com1RX.BackColor = e.TxRx ? mActiveColor : Color.Transparent;
                    break;
                case 1:
                    Com2RX.BackColor = e.TxRx ? mActiveColor : Color.Transparent;
                    break;
            }
        }

        [EventSubscription(EventTopics.RadioMessageSent, typeof(OnUserInterfaceAsync))]
        public void OnRadioMessageSent(object sender, RadioMessageSentEventArgs e)
        {
            mNetworkManager.SendRadioMessage(mXplaneAdapter.TunedFrequencies, e.Message);
            WriteMessage(Color.Cyan, $"{e.OurCallsign}: {e.Message}", true);
        }

        [EventSubscription(EventTopics.WallopSent, typeof(OnUserInterfaceAsync))]
        public void OnWallopSent(object sender, WallopSentEventArgs e)
        {
            WriteMessage(Color.Red, $"[WALLOP] {e.OurCallsign}: {e.Message}", true);
            PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Broadcast));
        }

        [EventSubscription(EventTopics.MetarReceived, typeof(OnUserInterfaceAsync))]
        public void OnMetarReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            WriteMessage(Color.Green, e.Data, true);
        }

        [EventSubscription(EventTopics.RadioMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnRadioMessageReceived(object sender, RadioMessageReceivedEventArgs e)
        {
            string message;
            if (mRadioStackState.ReceivingOnBothComRadios)
            {
                string arg;
                if (e.Frequencies.Length > 1)
                {
                    arg = $"{e.Frequencies[0].FormatFromNetwork()} & {e.Frequencies[1].FormatFromNetwork()}";
                }
                else
                {
                    arg = e.Frequencies[0].FormatFromNetwork();
                }
                message = $"{e.From} on {arg}: {e.Data}";
            }
            else
            {
                message = $"{e.From}: {e.Data}";
            }
            WriteMessage(e.IsDirect ? Color.White : Color.Silver, message, e.IsDirect);
            if (e.IsDirect)
            {
                PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.DirectRadioMessage));
            }
            else
            {
                PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.RadioMessage));
            }
            if (mConfig.FlashTaskbarRadioMessage)
            {
                FlashTaskbar.Flash(this);
            }
        }

        [EventSubscription(EventTopics.BroadcastMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnBroadcastMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            WriteMessage(Color.Orange, $"[BROADCAST] {e.From}: {e.Data}", false);
            PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Broadcast));
        }

        [EventSubscription(EventTopics.ServerMessageReceived, typeof(OnUserInterfaceAsync))]
        public void OnServerMessageReceived(object sender, NetworkDataReceivedEventArgs e)
        {
            WriteMessage(Color.SeaGreen, $"[SERVER]: {e.Data}", false);
        }

        [EventSubscription(EventTopics.SelcalAlertReceived, typeof(OnUserInterfaceAsync))]
        public void OnSelcalAlertReceived(object sender, SelcalAlertReceivedEventArgs e)
        {
            WriteInfoMessage($"SELCAL alert received on {e.Frequencies[0].FormatFromNetwork()}");
            PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.SelCal));
        }

        private void WriteErrorMessage(string message)
        {
            WriteMessage(Color.Red, message, true);
            PlayNotificationSound?.Invoke(this, new PlayNotifictionSoundEventArgs(SoundEvent.Error));
        }

        private void WriteWarningMessage(string message)
        {
            WriteMessage(Color.Orange, message, true);
        }

        private void WriteInfoMessage(string message)
        {
            WriteMessage(Color.Yellow, message, false);
        }

        private void WriteMessage(Color color, string message, bool activateMessageTab)
        {
            RtfMessages.WriteMessage(color, message);
            if (activateMessageTab)
            {
                TabsMain.SelectedTab = TabPageMessages;
            }
            TabPageMessages.ForeColor = (TabsMain.SelectedTab == TabPageMessages) ? Color.Silver : Color.Cyan;
            TabsMain.Refresh();
        }

        private void TreeControllers_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
            FocusTextCommandLine();
        }

        private void TreeControllers_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Cancel = true;
            FocusTextCommandLine();
        }

        private void TreeControllers_MouseUp(object sender, MouseEventArgs e)
        {
            if (RtfMessages.RichTextBox.SelectionLength > 0)
            {
                Clipboard.SetText(RtfMessages.RichTextBox.SelectedText);
                RtfMessages.RichTextBox.SelectionLength = 0;
            }
            FocusTextCommandLine();
        }

        private void FocusTextCommandLine()
        {
            if (TabsMain.SelectedTab != null)
            {
                if (TabsMain.SelectedTab is PrivateMessageTab)
                {
                    (TabsMain.SelectedTab as PrivateMessageTab).FocusTextCommandLine();
                }
                else if (TabsMain.SelectedTab == TabPageMessages)
                {
                    RtfMessages.TextCommandLine.Focus();
                }
            }
        }

        private void RichTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            FocusTextCommandLine();
        }

        private void TextCommandLine_TextCommandReceived(object sender, TextCommandReceivedEventArgs e)
        {
            string[] cmd = e.Command.Split(new char[] { ' ' });
            try
            {
                if (e.Command.StartsWith("."))
                {
                    switch (cmd[0].ToLower())
                    {
                        case ".simip":
                            if (cmd.Length - 1 < 1)
                            {
                                mConfig.SimulatorIP = "";
                                mConfig.SaveConfig();
                                WriteInfoMessage("Simulator IP reset.");
                            }
                            else
                            {
                                mConfig.SimulatorIP = cmd[1];
                                mConfig.SaveConfig();
                                WriteInfoMessage($"Simulator IP set to {cmd[1]}. You must restart xPilot for the changes to take effect.");
                            }
                            break;
                        case ".visualip":
                            if (cmd.Length - 1 < 1)
                            {
                                mConfig.VisualClientIPs.Clear();
                                mConfig.SaveConfig();
                                WriteInfoMessage("Visual IP reset.");
                            }
                            else
                            {
                                mConfig.VisualClientIPs.Clear();
                                for (int i = 1; i < cmd.Length; i++)
                                {
                                    mConfig.VisualClientIPs.Add(cmd[i]);
                                }
                                mConfig.SaveConfig();
                                WriteInfoMessage($"Visual IP set to {string.Join(", ", mConfig.VisualClientIPs)}. You must restart xPilot for the changes to take effect.");
                            }
                            break;
                        case ".copy":
                            if (!string.IsNullOrEmpty(RtfMessages.RichTextBox.Text))
                            {
                                Clipboard.SetText(RtfMessages.RichTextBox.Text);
                            }
                            break;
                        case ".clear":
                            RtfMessages.RichTextBox.Clear();
                            break;
                        case ".atis":
                            if (!mNetworkManager.IsConnected)
                            {
                                throw new ArgumentException("Not connected to network.");
                            }
                            else
                            {
                                CheckMinLength(cmd);
                                mAtisManager.RequestControllerAtis(cmd[1].ToUpper());
                            }
                            break;
                        case ".x":
                        case ".xpndr":
                        case ".xpdr":
                        case ".squawk":
                            if (mXplaneAdapter.ValidSimConnection)
                            {
                                CheckMinLength(cmd);
                                if (!Regex.IsMatch(cmd[1], "^[0-7]{4}$"))
                                {
                                    throw new ArgumentException("Invalid transponder code.");
                                }
                                int code = int.Parse(cmd[1]);
                                mXplaneAdapter.SetTransponderCode(code);
                                WriteInfoMessage($"Transponder code set to {code:0000}");
                            }
                            else
                            {
                                throw new ArgumentException("X-Plane connection not found");
                            }
                            break;
                        case ".com1":
                        case ".com2":
                            {
                                if (mXplaneAdapter.ValidSimConnection)
                                {
                                    CheckMinLength(cmd);
                                    if (!Regex.IsMatch(cmd[1], "^1\\d\\d[\\.\\,]\\d{1,3}$"))
                                    {
                                        throw new ArgumentException("Invalid frequency format.");
                                    }
                                    cmd[1] = cmd[1].PadRight(7, '0');
                                    uint freq = uint.Parse(cmd[1].Replace(".", "").Replace(",", "")).Normalize25KhzFrequency();
                                    int radio = cmd[0].ToLower() == ".com1" ? 1 : 2;
                                    mXplaneAdapter.SetRadioFrequency(radio, freq);
                                }
                            }
                            break;
                        case ".tx":
                            {
                                if (mXplaneAdapter.ValidSimConnection)
                                {
                                    CheckMinLength(cmd);
                                    if (cmd[1].ToLower() != "com1" && cmd[1].ToLower() != "com2")
                                    {
                                        throw new ArgumentException("Command syntax error. Expected format: .tx com1 or .tx com2");
                                    }
                                    int radio = cmd[1].ToLower() == "com1" ? 1 : 2;
                                    mXplaneAdapter.SetRadioTransmit(radio);
                                    WriteInfoMessage($"COM{radio} transmit enabled.");
                                }
                            }
                            break;
                        case ".rx":
                            {
                                if (mXplaneAdapter.ValidSimConnection)
                                {
                                    CheckMinLength(cmd, 2);
                                    if (cmd[1].ToLower() != "com1" && cmd[1].ToLower() != "com2")
                                    {
                                        throw new ArgumentException("Command syntax error. Expected format: .rx com1 on|off");
                                    }
                                    if (cmd[2].ToLower() != "on" && cmd[2].ToLower() != "off")
                                    {
                                        throw new ArgumentException("Command syntax error. Expected format: .rx com1 on|off");
                                    }
                                    int radio = cmd[1].ToLower() == "com1" ? 1 : 2;
                                    bool enabled = cmd[2].ToLower() == "on";
                                    mXplaneAdapter.SetRadioReceive(radio, enabled);
                                }
                            }
                            break;
                        case ".msg":
                        case ".chat":
                            CheckMinLength(cmd);
                            if (cmd[1].Length > 10)
                            {
                                throw new ArgumentException("Callsign too long.");
                            }
                            if (!mNetworkManager.IsConnected)
                            {
                                throw new ArgumentException("Not connected to network.");
                            }
                            if (cmd.Length > 2)
                            {
                                HandlePrivateMessageSent(cmd[1].ToUpper(), string.Join(" ", cmd.Skip(2)));
                                mNetworkManager.SendPrivateMessage(cmd[1].ToUpper(), string.Join(" ", cmd.Skip(2)));
                            }
                            else
                            {
                                HandleChatSessionStarted(cmd[1].ToUpper());
                            }
                            break;
                        case ".wallop":
                            CheckMinLength(cmd);
                            if (!mNetworkManager.IsConnected)
                            {
                                throw new ArgumentException("Not connected to network.");
                            }
                            mNetworkManager.SendWallop(cmd[1]);
                            break;
                        case ".wx":
                        case ".metar":
                            CheckMinLength(cmd);
                            if (!mNetworkManager.IsConnected)
                            {
                                throw new ArgumentException("Not connected to network.");
                            }
                            mNetworkManager.SendMetarRequest(cmd[1]);
                            break;
                        case ".towerview":
                            if(mConfig.ConfigurationRequired)
                            {
                                throw new ArgumentException("xPilot configuration required.");
                            }
                            string tvServer = "127.0.0.1";
                            string tvCallsign = "TOWER";
                            if (cmd.Length >= 2)
                            {
                                tvServer = cmd[1];
                                if (cmd.Length >= 3)
                                {
                                    tvCallsign = cmd[2].ToUpper();
                                }
                            }
                            ConnectInfo connectInfo = new ConnectInfo(tvCallsign, "", "", true, true);
                            mNetworkManager.Connect(connectInfo, tvServer);
                            break;
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(e.Command))
                    {
                        if (mNetworkManager.IsConnected)
                        {
                            RadioMessageSent?.Invoke(this, new RadioMessageSentEventArgs(mConnectInfo.Callsign, e.Command));
                        }
                        else
                        {
                            throw new ArgumentException("Not connected to network");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteErrorMessage(ex.Message);
            }
        }

        private static void CheckMinLength(string[] cmd, int minLength = 1)
        {
            if (cmd.Length - 1 < minLength)
            {
                throw new ArgumentException("Not enough parameters");
            }
        }

        private TreeNode FindController(string callsign)
        {
            TreeNode[] array = TreeControllers.Nodes.Find(callsign.Replace("*", ""), true);
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

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.Style |= 0x20000;
                return cp;
            }
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

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            WriteInfoMessage($"xPilot version {Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion}");
            MainFormShown(this, EventArgs.Empty);
            SessionStarted(this, EventArgs.Empty);
            FocusTextCommandLine();

            if (!string.IsNullOrEmpty(mConfig.SimulatorIP))
            {
                WriteInfoMessage($"Looking for simuator at IP {mConfig.SimulatorIP}");
            }
            if (mConfig.VisualClientIPs.Count > 0)
            {
                WriteInfoMessage($"Looking for Visuals machine at IP: {string.Join(", ", mConfig.VisualClientIPs)}.");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ScreenUtils.ApplyWindowProperties(mConfig.ClientWindowProperties, this);
            mInitializing = false;

            if (mConfig.ConfigurationRequired)
            {
                using (var dlg = mUserInterface.CreateSetupGuideForm())
                {
                    if (dlg.ShowDialog(this) == DialogResult.No)
                    {
                        WriteErrorMessage(CONFIGURATION_REQUIRED);
                    }
                }
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

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Rectangle rect = new Rectangle(ClientRectangle.Left, ClientRectangle.Top, ClientRectangle.Width - 1, ClientRectangle.Height - 1);
            using (Pen pen = new Pen(Color.FromArgb(0, 0, 0)))
            {
                pevent.Graphics.DrawRectangle(pen, rect);
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (mNetworkManager.IsConnected && MessageBox.Show(this, "You are connected to the network. Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1) != DialogResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            SessionEnded(this, EventArgs.Empty);
            mEventBroker.Unregister(this);
        }

        private void BtnExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (mNetworkManager.IsConnected)
            {
                mNetworkManager.Disconnect();
            }
            else
            {
                using (ConnectForm connectForm = mUserInterface.CreateConnectForm())
                {
                    if (connectForm.ShowDialog(this) == DialogResult.OK)
                    {
                        mConnectInfo = connectForm.GetConnectInfo();
                        if (mConfig.ConfigurationRequired)
                        {
                            WriteErrorMessage(CONFIGURATION_REQUIRED);
                        }
                        else
                        {
                            NetworkServerInfo server = mNetworkManager.ServerList.FirstOrDefault(t => t.Name == mConfig.ServerName);
                            if (server != null)
                            {
                                mNetworkManager.Connect(mConnectInfo, server.Address);
                            }
                            else
                            {
                                WriteErrorMessage("You must first select a network server in the Settings.");
                            }
                        }
                    }
                }
            }
            FocusTextCommandLine();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            using (SettingsForm settingsForm = mUserInterface.CreateSettingsForm())
            {
                settingsForm.ShowDialog(this);
            }
            FocusTextCommandLine();
        }

        private void BtnMinimize_Click(object sender, EventArgs e)
        {
            base.WindowState = FormWindowState.Minimized;
        }

        private void ChkModeC_Click(object sender, EventArgs e)
        {
            if (mXplaneAdapter.ValidSimConnection)
            {
                ChkModeC.Clicked = !ChkModeC.Clicked;
                mXplaneAdapter.EnableTransponderModeC(ChkModeC.Clicked);
                FocusTextCommandLine();
            }
        }

        private void ChkIdent_Click(object sender, EventArgs e)
        {
            if (mXplaneAdapter.ValidSimConnection)
            {
                mXplaneAdapter.TriggerTransponderIdent();
                FocusTextCommandLine();
            }
        }

        private void BtnFlightPlan_Click(object sender, EventArgs e)
        {
            Process.Start("https://my.vatsim.net/pilots/flightplan");
        }

        private void HandleNetworkDisconnected()
        {
            LblCallsign.Text = "-------";
            ClearControllerListNodes();
            BtnConnect.Enabled = true;
            BtnConnect.Text = "Connect";
            BtnConnect.Pushed = false;

            Com1TX.BackColor = Color.Transparent;
            Com1RX.BackColor = Color.Transparent;
            Com2TX.BackColor = Color.Transparent;
            Com2RX.BackColor = Color.Transparent;
        }

        private void ClearControllerListNodes()
        {
            foreach (TreeNode node in TreeControllers.Nodes)
            {
                node.Nodes.Clear();
            }
        }

        private void HandleChatSessionStarted(string callsign)
        {
            PrivateMessageTab tab = GetPrivateMessageTabIfExists(callsign);
            if (tab == null)
            {
                tab = CreatePrivateMessageTab(callsign);
            }
            TabsMain.SelectedTab = tab;
        }

        private void HandlePrivateMessageSent(string callsign, string message)
        {
            PrivateMessageTab tab = GetPrivateMessageTabIfExists(callsign);
            if (tab == null)
            {
                tab = CreatePrivateMessageTab(callsign, message, true, mNetworkManager.OurCallsign);
            }
            TabsMain.SelectedTab = tab;
        }

        private PrivateMessageTab GetPrivateMessageTabIfExists(string tabIdentifier)
        {
            foreach (TabPage tabPage in TabsMain.TabPages)
            {
                if (tabPage.Name.Equals(tabIdentifier) && tabPage is PrivateMessageTab)
                {
                    return tabPage as PrivateMessageTab;
                }
            }
            return null;
        }

        private PrivateMessageTab CreatePrivateMessageTab(string to, string message = null, bool isOurMessage = false, string ourCallsign = null)
        {
            PrivateMessageTab tab = mTabPages.CreatePrivateMessageTab(to, message, isOurMessage, ourCallsign);
            TabsMain.TabPages.Add(tab);
            return tab;
        }

        private void TabsMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabsMain.SelectedTab.ForeColor = Color.Silver;
            if (TabsMain.SelectedTab != null)
            {
                if (TabsMain.SelectedTab is PrivateMessageTab)
                {
                    (TabsMain.SelectedTab as PrivateMessageTab).Focus();
                }
                else if (TabsMain.SelectedTab == TabPageMessages)
                {
                    RtfMessages.TextCommandLine.Select();
                }
            }
            TabsMain.Refresh();
        }

        private void TreeControllers_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                TreeControllers.SelectedNode = TreeControllers.GetNodeAt(e.X, e.Y);
                if (TreeControllers.SelectedNode != null && TreeControllers.SelectedNode.Nodes.Count > 0)
                {
                    ControllerTreeContextMenu.Show(TreeControllers, e.Location);
                }
            }
        }

        private void TreeControllers_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if(e.Node.Level != 0 && e.Button == MouseButtons.Right)
            {
                ShowControllerContextMenu(e.Node.Name, e.Location);
            }
        }

        private void ShowControllerContextMenu(string name, Point point)
        {
            startPrivateChat.Tag = name;
            requestControllerInfo.Tag = name;
            tuneCom1Frequency.Tag = name;
            ControllerTreeContextMenu.Show(TreeControllers, point);
        }

        private void requestControllerInfo_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            mAtisManager.RequestControllerAtis(item.Tag.ToString());
        }

        private void startPrivateChat_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            HandleChatSessionStarted(item.Tag.ToString());
        }

        private void tuneCom1Frequency_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = (sender as ToolStripMenuItem);
            if (!string.IsNullOrEmpty(item.Tag.ToString()))
            {
                uint freq = mControllerManager.GetFrequencyByCallsign(item.Tag.ToString()).FsdFrequencyToHertz() / 1000;
                mXplaneAdapter.SetRadioFrequency(1, freq);
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