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
using Newtonsoft.Json;
using SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using XPilot.PilotClient.Config;

namespace XPilot.PilotClient.Tutorial
{
    public partial class CslConfiguration : SetupScreen, ISetupScreen
    {
        private readonly SynchronizationContext mSyncContext;

        public CslConfiguration(ISetup host, IAppConfig config) : base(host, config)
        {
            InitializeComponent();
            mSyncContext = SynchronizationContext.Current;
            Host.SetTitle("CSL Configuration");
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            string bluebell = Path.Combine(mConfig.AppPath, "Bluebell.7z");
            if (File.Exists(bluebell))
            {
                ExtractModels();
            }
            else
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                var cookieJar = new CookieContainer();
                CookieAwareWebClient client = new CookieAwareWebClient(cookieJar);
                var html = client.DownloadString("https://drive.google.com/uc?export=download&id=1T2TyUuAHtxjh4eFdfzjd2YyhU_hYY-Ts");
                var confirm = Regex.Match(html, @".*confirm=([0-9A-Za-z_]+).*");
                if (confirm.Success)
                {
                    string confirmationCode = confirm.Groups[1].Value;
                    btnDownload.Enabled = false;
                    Task.Run(() =>
                    {
                        string url = $"https://drive.google.com/uc?export=download&confirm={confirmationCode}&id=1T2TyUuAHtxjh4eFdfzjd2YyhU_hYY-Ts";
                        client.DownloadProgressChanged += (s, evt) =>
                        {
                            mSyncContext.Post(o =>
                            {
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
                            }, null);
                            ExtractModels();
                        };
                        client.DownloadFileAsync(new Uri(url), bluebell);
                    });
                }
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
                            btnDownload.Enabled = false;
                            progressBar1.Value = evt.PercentDone;
                        }, null);
                    };
                    file.ExtractionFinished += (s, evt) =>
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
                        mSyncContext.Post(o =>
                        {
                            btnNext.Enabled = true;
                        }, null);
                    };
                    file.ExtractArchive(Path.GetDirectoryName(Path.Combine(mConfig.XplanePath, @"Resources\plugins\xPilot\Resources\CSL\")));
                }
                else
                {
                    MessageBox.Show("Models file not found.");
                }
            });
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Host.EndSetup();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            Host.SwitchScreen("NetworkCredentials");
        }
    }
}