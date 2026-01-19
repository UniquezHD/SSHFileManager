using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SSHFileManager
{
    public partial class Settings : Form
    {
        public Settings()
        {
            InitializeComponent();

            checkBox1.Checked = Properties.Settings.Default.autoConnect;
            checkBox2.Checked = Properties.Settings.Default.saveSettingsOnClose;

            textBox1.Text = Properties.Settings.Default.lastSessionUser;
            textBox2.Text = Properties.Settings.Default.lastSessionHost;
            textBox3.Text = Properties.Settings.Default.lastSessionPassword;
            textBox4.Text = Properties.Settings.Default.downloadPath;
            textBox5.Text = Properties.Settings.Default.lastSessionClientPath;
            textBox6.Text = Properties.Settings.Default.lastSessionServerPath;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.autoConnect = checkBox1.Checked;
            Properties.Settings.Default.saveSettingsOnClose = checkBox2.Checked;

            Properties.Settings.Default.lastSessionUser = textBox1.Text;
            Properties.Settings.Default.lastSessionHost = textBox2.Text;
            Properties.Settings.Default.lastSessionPassword = textBox3.Text;
            Properties.Settings.Default.downloadPath = textBox4.Text;
            Properties.Settings.Default.lastSessionClientPath = textBox5.Text;
            Properties.Settings.Default.lastSessionServerPath = textBox6.Text;

            Properties.Settings.Default.Save();

            this.Close();
        }
    }
}
