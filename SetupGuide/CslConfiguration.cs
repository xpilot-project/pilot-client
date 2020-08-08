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
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XPilot.PilotClient.Config;
using Newtonsoft.Json;
using SevenZip;

namespace XPilot.PilotClient.Tutorial
{
    public partial class CslConfiguration : SetupScreen, ISetupScreen
    {
        private readonly SynchronizationContext mSyncContext;
        private readonly CancellationTokenSource ts;
        private CancellationToken ct;

        public CslConfiguration(ISetup host, IAppConfig config) : base(host, config)
        {
            InitializeComponent();

            mSyncContext = SynchronizationContext.Current;
            ts = new CancellationTokenSource();
            ct = ts.Token;

            Host.SetTitle("CSL Configuration");
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            DownloadModels();
        }

        private void DownloadModels()
        {
            string bluebell = Path.Combine(mConfig.AppPath, "Bluebell.7z");
            try
            {
                btnDownload.Enabled = false;
                Task.Run(() =>
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    WebClient client = new WebClient();
                    string url = "http://xpilot-project.org/api/v2/CslModelSet";
                    client.DownloadProgressChanged += (s, evt) =>
                    {
                        if (ct.IsCancellationRequested)
                        {
                            client.CancelAsync();
                        }

                        mSyncContext.Post(o =>
                        {
                            btnDownload.Text = $"Downloading... {Math.Round(ConvertBytesToMegabytes(evt.BytesReceived), 2)} MB of 283 MB";
                            double bytesIn = double.Parse(evt.BytesReceived.ToString());
                            double pct = (bytesIn / 297044413) * 100;
                            progressBar1.Value = int.Parse(Math.Truncate(pct).ToString());
                        }, null);
                    };
                    client.DownloadFileCompleted += (s, evt) =>
                    {
                        mSyncContext.Post(o =>
                        {
                            progressBar1.Value = 0;
                            ExtractModels();
                        }, null);
                    };
                    client.DownloadFileAsync(new Uri(url), bluebell);
                }, ct);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error downloading CSL Model Package: " + ex.Message);
            }
        }

        private void ExtractModels()
        {
            Task.Run(() =>
            {
                string bluebell = Path.Combine(mConfig.AppPath, "Bluebell.7z");
                if (File.Exists(bluebell))
                {
                    var sevenZipPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "7zxa.dll");
                    SevenZipBase.SetLibraryPath(sevenZipPath);

                    var file = new SevenZipExtractor(bluebell);
                    file.Extracting += (s, evt) =>
                    {
                        mSyncContext.Post(o =>
                        {
                            btnDownload.Text = "Installing CSL Models...";
                            btnDownload.Enabled = false;
                            progressBar1.Value = evt.PercentDone;
                        }, null);
                    };
                    file.ExtractionFinished += (s, evt) =>
                    {
                        mSyncContext.Post(o =>
                        {
                            File.WriteAllText(Path.Combine(mConfig.XplanePath, @"Resources\plugins\xPilot\Resources\Config.json"),
                            JsonConvert.SerializeObject(new
                            {
                                CSL = new List<object>
                                {
                                    new
                                    {
                                        Enabled = true,
                                        Path = Path.Combine(mConfig.XplanePath, @"Resources\plugins\xPilot\Resources\CSL\Bluebell\")
                                    }
                                }
                            }));
                            btnDownload.Text = "CSL Models Successfully Installed";
                            btnNext.Enabled = true;
                        }, null);
                    };
                    try
                    {
                        file.ExtractArchive(Path.GetDirectoryName(Path.Combine(mConfig.XplanePath, @"Resources\plugins\xPilot\Resources\CSL\")));
                    }
                    catch { }
                }
                else
                {
                    MessageBox.Show("xPilot is unable to find or extract the CSL models.");
                }
            }, ct);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            ts.Cancel();
            Host.EndSetup();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Host.SwitchScreen("NetworkCredentials");
        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }
    }
}