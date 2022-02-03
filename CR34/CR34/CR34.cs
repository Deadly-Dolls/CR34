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

namespace CR34
{
    public partial class CR34 : KryptonForm
    {
        bool mouse_down = false;
        private Point offset;

        private BackgroundWorker worker = new BackgroundWorker();
        private string url = $"https://api.rule34.xxx/index.php?page=dapi&s=post&q=index&tags=";

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

            worker.RunWorkerAsync();
            while (worker.IsBusy)
            {
                Application.DoEvents();
            }

            button1.Enabled = true;
            pages.Enabled = true;
            tag.Enabled = true;
        }

        private async Task<string> cleanner(string downloads, string data)
        {
            return ($"{downloads}\\{data.Replace(" ", "_")}");
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
                await update_logs(full_url);
                result = client.DownloadString(full_url);
                post.LoadXml(result);
                node = post.SelectNodes("posts");

                for (int i = 0; i < node.Count; i++)
                {
                    for (int child = 0; child < node[i].ChildNodes.Count; child++)
                    {
                        address = node[i].ChildNodes[child].Attributes[i].OwnerElement.GetAttribute("file_url");
                        output_name = address.Split('/')[5];
                        if (File.Exists($"{output_path}\\{output_name}") == false)
                        {
                            await update_logs(output_name);
                            client.DownloadFile(address, $"{output_path}\\{output_name}");
                        }
                    }
                }
            }
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
