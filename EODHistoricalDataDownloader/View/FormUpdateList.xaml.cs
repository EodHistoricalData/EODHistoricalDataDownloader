using EODHistoricalDataDownloader.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Windows;
using System.Windows.Forms;

namespace EODHistoricalDataDownloader.View
{
    /// <summary>
    /// Логика взаимодействия для FormUpdateList.xaml
    /// </summary>
    public partial class FormUpdateList : Window
    {
        private readonly List<Version> Versions;
        private string fileName;

        internal FormUpdateList(List<Version> versions)
        {
            InitializeComponent();

            Versions = versions;

            foreach (var version in Versions)
            {
                int start = txtUpdatesList.Text.Length;
                string header = $"Version: {version.Text}";
                txtUpdatesList.AppendText(header);
                txtUpdatesList.AppendText(version.Description);
                txtUpdatesList.AppendText("\r\n");
                txtUpdatesList.Select(start, header.Length);
            }
            txtUpdatesList.Select(0, 0);
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DownLoadNewVersion();
            }
            catch (System.Exception)
            {
                throw;
            }
        }

        private void DownLoadNewVersion()
        {
            string[] linksplit = Versions[0].Link.Split('/');

            fileName = Path.GetTempPath() + linksplit[linksplit.Length - 1];
            if (File.Exists(fileName)) File.Delete(fileName);

            WebClient client = new();
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
            client.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
            client.DownloadFileAsync(new System.Uri(Versions[0].Link), fileName);
        }

        private void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                Process.Start(fileName);
            }
            catch (System.Exception)
            {
                System.Windows.Forms.MessageBox.Show("Failed to download update", "Update", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Close();
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progbarDownloading.Maximum = (int)e.TotalBytesToReceive / 100;
            progbarDownloading.Value = (int)e.BytesReceived / 100;
            System.Windows.Forms.Application.DoEvents();
        }
    }
}
