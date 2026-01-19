using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace SSHFileManager
{
    public partial class RemoteShell : Form
    {
        public string User { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public RemoteShell()
        {
            InitializeComponent();
        }

        private void RunCommand()
        {
            var connectionInfo = new ConnectionInfo(Host, User, new PasswordAuthenticationMethod(User, Password));

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    try
                    {
                        SshClient sshclient = new SshClient(connectionInfo);
                        sshclient.Connect();
                        SshCommand sc = sshclient.RunCommand(textBox1.Text);
                        sc.Execute();

                        richTextBox1.Text = sc.Result;

                        sc.Dispose();
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("FAK");
                    }
                }

                client.Disconnect();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RunCommand();
        }

        private void RemoteShell_Load(object sender, EventArgs e)
        {
            var connectionInfo = new ConnectionInfo(Host, User, new PasswordAuthenticationMethod(User, Password));

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                if (client.IsConnected)
                {
                    richTextBox1.Text = "Connected";
                }
                else
                {
                    richTextBox1.Text = "Failed to connected";
                }
                client.Disconnect();
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                RunCommand();
            }
        }
    }
}
