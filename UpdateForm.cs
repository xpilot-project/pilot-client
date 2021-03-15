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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace Vatsim.Xpilot
{
    public partial class UpdateForm : Form
    {
        public string NewVersion { get; set; }
        public string DownloadUrl { get; set; }

        private string mTempFile;
        private WebClient mWebClient;

        public UpdateForm()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            lblCurrentVersion.Text = Application.ProductVersion.ToString();
            lblNewVersion.Text = NewVersion;

            using (WebClient client = new WebClient())
            {
                txtChangeLog.Text = client.DownloadString("http://xpilot-project.org/ChangeLog.txt");
            }
        }

        private void BtnYes_Click(object sender, EventArgs e)
        {
            Process[] xpProcess = Process.GetProcessesByName("X-Plane");
            if (xpProcess.Length > 0)
            {
                MessageBox.Show(this, "You must completely close X-Plane before installing the xPilot update.", "Close X-Plane");
            }
            btnYes.Enabled = false;
            btnNo.Text = "Cancel";
            progressBar.Visible = true;

            mWebClient = new WebClient();
            mWebClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            mWebClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;

            Uri uri = new Uri(DownloadUrl);
            mTempFile = Path.Combine(Path.GetTempPath(), uri.Segments.Last());
            mWebClient.DownloadFileAsync(uri, mTempFile);
        }

        private void BtnNo_Click(object sender, EventArgs e)
        {
            if (mWebClient != null)
            {
                mWebClient.CancelAsync();
            }
            else
            {
                Close();
            }
        }

        private void WebClient_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                Close();
            }
            else if (e.Error != null)
            {
                string message = e.Error.Message;
                if (e.Error.InnerException != null)
                {
                    message = e.Error.InnerException.Message;
                }
                MessageBox.Show(this, string.Format("The following error occurred while attempting to download the update:\r\n\r\n{0}", message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                Close();
            }
            else
            {
                progressBar.Visible = false;
                Process.Start(mTempFile);
                Environment.Exit(0);
            }
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }
    }
}
