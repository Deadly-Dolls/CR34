using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using ComponentFactory.Krypton.Toolkit;
using System.Xml;
using RestSharp;
using System.Net;
using System.Threading;

namespace CR34
{
    public partial class CR34 : KryptonForm
    {
        bool mouse_down = false;
        private Point offset;

        private BackgroundWorker worker = new BackgroundWorker();
        private string url = $"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&tags=";

        Thread download_thread = null;

        public CR34()
        {
            InitializeComponent();
            InitializeWorker();
        }

        private void InitializeWorker()
        {
            worker.DoWork += new DoWorkEventHandler(download);
        }

        private void button_close_Click(object sender, EventArgs e)
        {
            if (worker.IsBusy == true)
                worker.CancelAsync();
            if (download_thread != null)
                if (download_thread.IsAlive == true)
                    download_thread.Join();
            Close();
        }

        private void button_reduce_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void CR34_Load(object sender, EventArgs e)
        {
            
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            button1.Enabled = false;
            pages.Enabled = false;
            tag.Enabled = false;

            if (tag.Text.Length >= 1)
            {
                worker.RunWorkerAsync();
                while (worker.IsBusy == true || download_thread.IsAlive == true)
                {
                    Application.DoEvents();
                }
            }
            
            button1.Enabled = true;
            pages.Enabled = true;
            tag.Enabled = true;
        }

        private async Task<string> cleanner(string downloads, string data)
        {
            return ($"{downloads}\\{data.ToLower().Replace(" ", "_")}");
        }

        private async void download(object sender, EventArgs e)
        {
            int aligner = 42;
            WebClient client = new WebClient();
            string result = null;
            int pages_number = Convert.ToInt32(pages.Value);
            string full_url = null;
            XmlDocument post = new XmlDocument();
            XmlNodeList node = null;
            string downloads = "downloads";
            string output_path = await cleanner(downloads, tag.Text);
            string output_name = null;
            string address = null;

            if (Directory.Exists(downloads) == false)
                Directory.CreateDirectory(downloads);
            if (Directory.Exists(output_path) == false)
                Directory.CreateDirectory(output_path);

            for (int pid = 0; pid < pages_number; pid++)
            {
                full_url = $"{url}{tag.Text}+&pid={pid * aligner}";
                result = client.DownloadString(full_url);
                post.LoadXml(result);
                node = post.SelectNodes("posts");
                download_thread = new Thread(() => downloader(address, node, output_path, output_name));
                download_thread.Start();
                client.Dispose();
            }
        }

        private async void downloader(string address, XmlNodeList node, string output_path, string output_name)
        {
            WebClient client = new WebClient();

            for (int i = 0; i < node.Count; i++)
            {
                for (int child = 0; child < node[i].ChildNodes.Count; child++)
                {
                    address = node[i].ChildNodes[child].Attributes[i].OwnerElement.GetAttribute("file_url");
                    output_name = address.Split('/')[5];
                    if (File.Exists($"{output_path}\\{output_name}") == false)
                    {
                        await update_logs(output_name);
                        client = new WebClient();
                        client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(progress_callback);
                        client.DownloadFileCompleted += new AsyncCompletedEventHandler(progress_file_completed);
                        client.DownloadFileAsync(new Uri(address), $"{output_path}\\{output_name}");
                    }
                }
            }
        }

        private async void progress_callback(object sender, DownloadProgressChangedEventArgs e)
        {
            await update_progress(e.ProgressPercentage);
        }

        private async void progress_file_completed(object sender, AsyncCompletedEventArgs e)
        {
            await update_progress(100);
        }

        private async Task<Task> update_progress(int percent)
        {
            current_progress_bar.Invoke(new MethodInvoker(delegate
            {
                current_progress_bar.Value = percent;
            }));
            current_progress.Invoke(new MethodInvoker(delegate
            {
                current_progress.Text = $"{percent} %";
            }));

            return (Task.CompletedTask);
        }


        private async Task<Task> update_logs(string data)
        {
            logs.Invoke(new MethodInvoker(async delegate
            {
                logs.AppendText($"{data}\n");
            }));

            return (Task.CompletedTask);
        }

        private void border_MouseDown(object sender, MouseEventArgs e)
        {
            offset.X = e.X;
            offset.Y = e.Y;

            mouse_down = true;
        }

        private void border_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouse_down == true)
            {
                Point pos = PointToScreen(e.Location);
                Location = new Point(pos.X - offset.X, pos.Y - offset.Y);
            }
        }

        private void border_MouseUp(object sender, MouseEventArgs e)
        {
            mouse_down = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Author: Neo\nGithub: https://github.com/Neotoxic-off", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
