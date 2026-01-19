using System;
using System.IO;
using System.Windows.Forms;
using Renci.SshNet;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SSHFileManager
{
    public partial class Form1 : Form
    { 
        //TODO add multi delete on server

        //FIX prevent going further back than possible in the explorer

        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern Int32 SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        List<string> selectedServerPaths = new List<string>();
        List<string> selectedClientPaths = new List<string>();

        string downloadPath;

        public Form1()
        {
            InitializeComponent();

            //Magic
            SendMessage(textBox1.Handle, EM_SETCUEBANNER, 0, "User");
            SendMessage(textBox2.Handle, EM_SETCUEBANNER, 0, "Host");
            SendMessage(textBox3.Handle, EM_SETCUEBANNER, 0, "Password");

            if (Properties.Settings.Default.downloadPath != null)
            {
                downloadPath = Properties.Settings.Default.downloadPath;
            }

            if(Properties.Settings.Default.lastSessionHost != null
                || Properties.Settings.Default.lastSessionUser != null)
            {
                textBox1.Text = Properties.Settings.Default.lastSessionUser;
                textBox2.Text = Properties.Settings.Default.lastSessionHost;

                
            }

            if(Properties.Settings.Default.lastSessionClientPath == "")
            {
                textBox4.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            else
            {
                textBox4.Text = Properties.Settings.Default.lastSessionClientPath;
            }

            if (Properties.Settings.Default.lastSessionServerPath == "")
            {
                textBox5.Text = "/";
            }
            else
            {
                textBox5.Text = Properties.Settings.Default.lastSessionServerPath;
            }

            if (Properties.Settings.Default.lastSessionPassword != null)
            {
                textBox3.Text = Properties.Settings.Default.lastSessionPassword;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.autoConnect == true)
            {
                Connect();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if(Properties.Settings.Default.lastSessionHost == ""
                || Properties.Settings.Default.lastSessionUser == "")
            {
                DialogResult result;

                result = MessageBox.Show("To save password go to settings", "Save Session ?", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    Properties.Settings.Default.lastSessionUser = textBox1.Text;
                    Properties.Settings.Default.lastSessionHost = textBox2.Text;

                    Properties.Settings.Default.lastSessionClientPath = textBox4.Text;
                    Properties.Settings.Default.lastSessionServerPath = textBox5.Text;

                    Properties.Settings.Default.Save();
                }
            }
            Connect();
        }

        private void Connect()
        {
            try
            {
                var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();
                    client.Disconnect();
                }
            }
            catch (Exception)
            {
                Log("Wrong connection info");
                return;
            }
            

            UpdateClientFolderExplorer();

            UpdateServerFolderExplorer();

            UpdateClientFileExplorer();

            switch (GetMachineType())
            {
                case "Windows":
                    UpdateServerFileExplorer();
                    Log("Connected To Windows Machine");
                    break;

                case "Linux":
                    Log("Connected To Linux Machine");
                    break;

                case "IOS":
                    Log("Connected To IOS Machine");
                    break;

                default:
                    Log("Connected To Unknown Machine");
                    break;
            }
        }

        private string GetMachineType()
        {
            var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    try
                    {
                        client.ListDirectory("C:\\Windows\\System32");

                        return "Windows";
                    }
                    catch { }

                    try
                    {
                        client.ListDirectory("/sys/kernel");

                        return "Linux";
                    }
                    catch { }

                    try
                    {
                        client.ListDirectory("/System/Library/ApplePTP");

                        return "IOS";
                    }
                    catch { }                  
                }

                client.Disconnect();
            }

            return "Unknown";
        }

        private bool isUnix()
        {
            if (textBox5.Text.Contains("/"))
            {
                return true;
            }
            else if (textBox5.Text.Contains("\\"))
            {
                return false;
            }

            return false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string rootPath = "xxxxx";

            foreach (var item in Directory.GetFiles(rootPath))
            {
                Task upload = Task.Run(() =>
                {
                    Upload(item, "xxxxx");
                });
            }
        }

        private void Download(string fileToDownload)
        {
            var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

            try
            {
                using (var client = new SftpClient(connectionInfo))
                {
                    FileStream fs = new FileStream(downloadPath + Path.GetFileName(fileToDownload), FileMode.OpenOrCreate);

                    client.Connect();

                    client.DownloadFile(
                        fileToDownload,
                        fs,
                        downloaded =>
                        {
                            Log($"Downloaded {Path.GetFileName(fileToDownload)} {(double)downloaded / fs.Length * 100:F0}%");
                        });
                    client.Disconnect();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message + ' ' + fileToDownload);
            }
        }

        private void Upload(string fileToUpload, string serverFolder)
        {
            var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

            try
            {
                using (var client = new SftpClient(connectionInfo))
                {
                    FileStream fs = new FileStream(fileToUpload, FileMode.Open);

                    client.Connect();

                    client.UploadFile(
                        fs,
                        serverFolder + Path.GetFileName(fileToUpload),
                        uploaded =>
                        {
                            Log($"{Path.GetFileName(fileToUpload)} Uploaded { (double)uploaded / fs.Length * 100:F0}%");
                        });

                    client.Disconnect();
                }
            }
            catch (Exception ex)
            {
                Log($"Failed to upload {fileToUpload} {ex}");
            }
        }

        private void Log(string log)
        {
            Invoke(new Action(() =>
            {
                richTextBox1.AppendText($"  {log}\n");
            }));   
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string rootPath = "xxxx";

            foreach (var item in Directory.GetFiles(rootPath))
            {
                Task upload = Task.Run(() =>
                {
                    Upload(item, "xxxxx");
                });
            }

            string cssPath = "xxxx";

            foreach (var item in Directory.GetFiles(cssPath))
            {
                Task upload = Task.Run(() =>
                {
                    Upload(item, "xxxx");
                });
            }

            string jsPath = "xxxxx";

            foreach (var item in Directory.GetFiles(jsPath))
            {
                Task upload = Task.Run(() =>
                {
                    Upload(item, "xxxxx");
                });
            }

            string dataPath = "xxxx";

            foreach (var item in Directory.GetFiles(dataPath))
            {
                Task upload = Task.Run(() =>
                {
                    Upload(item, "xxxx");
                });
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            DialogResult result;

            result = MessageBox.Show("Are you sure", "Reboot", MessageBoxButtons.YesNo);

            if(result == DialogResult.Yes)
            {
                var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        if(GetMachineType() == "Linux")
                        {
                            SshClient sshclient = new SshClient(connectionInfo);
                            sshclient.Connect();
                            SshCommand sc = sshclient.CreateCommand("sudo reboot");
                            sc.Execute();
                        } else if(GetMachineType() == "Windows")
                        {
                            SshClient sshclient = new SshClient(connectionInfo);
                            sshclient.Connect();
                            SshCommand sc = sshclient.CreateCommand("shutdown /r");
                            sc.Execute();
                        }
                    }

                    client.Disconnect();
                }
                Log($"Device Rebooted");
            } 
        }

        private void UpdateClientFolderExplorer()
        {
            listBox2.Items.Clear();

            listBox2.Items.Add("..");

            foreach (var item in Directory.GetDirectories(textBox4.Text))
            {
                listBox2.Items.Add(item);
            }

            UpdateClientFileExplorer();
        }

        private void UpdateClientFileExplorer()
        {
            listBox3.Items.Clear();

            foreach (var item in Directory.GetFiles(textBox4.Text))
            {
                listBox3.Items.Add(item);
            }
        }

        private void DeleteServerFile(string fileToDelete)
        {
            var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    try
                    {
                        DialogResult result;

                        result = MessageBox.Show($"{fileToDelete}", "Are you sure you want to delete", MessageBoxButtons.YesNo);

                        if (result == DialogResult.Yes)
                        {
                            client.Delete(fileToDelete);
                            Log($"Deleted {fileToDelete}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"Not able to delete {fileToDelete} {ex.Message}");
                    }
                }
                client.Disconnect();
            }
        }

        private void UpdateServerFolderExplorer()
        {
            Invoke(new Action(() =>
            {
                listBox1.Items.Clear();

                listBox1.Items.Add("..");

                var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        try
                        {
                            foreach (var file in client.ListDirectory(textBox5.Text))
                            {
                                listBox1.Items.Add(file.FullName);
                            }
                        }
                        catch
                        {
                            Log("Folder not found");
                        }
                    }

                    client.Disconnect();

                    if (!isUnix())
                    {
                        UpdateServerFileExplorer();
                    }
                }
            }));

            
        }

        private void UpdateServerFileExplorer()
        {
            listBox4.Items.Clear();

            selectedServerPaths.Clear();

            var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    try
                    {
                        foreach (var file in client.ListDirectory(textBox5.Text))
                        {
                            listBox4.Items.Add(file.FullName);
                        }
                    }
                    catch
                    {
                        Log("Folder not found");
                    }
                }

                client.Disconnect();
            }
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Enter)
            {
                UpdateServerFolderExplorer();
            }
        }

        private void textBox4_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UpdateClientFolderExplorer();
            }
        }

        private void listBox2_DoubleClick(object sender, EventArgs e)
        {
            if (listBox2.GetItemText(listBox2.SelectedItem) == "..")
            {
                textBox4.Text = Directory.GetParent(textBox4.Text).ToString();
            }
            else
            {
                textBox4.Text = listBox2.GetItemText(listBox2.SelectedItem);
            }

            UpdateClientFolderExplorer();
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (isUnix())
            {
                if (listBox1.GetItemText(listBox1.SelectedItem) == "..")
                {
                    var parrentDir = Directory.GetParent(textBox5.Text).ToString();
                    string finalString = parrentDir.Replace("C:", "").Replace("\\", "/");

                    textBox5.Text = finalString;
                }
                else
                {
                    textBox5.Text = listBox1.GetItemText(listBox1.SelectedItem);
                }
            } 
            else
            {
                if (listBox1.GetItemText(listBox1.SelectedItem) == "..")
                {
                    textBox5.Text = Directory.GetParent(textBox4.Text).ToString();
                }
                else
                {
                    textBox5.Text = listBox1.GetItemText(listBox1.SelectedItem);
                }
            }

            UpdateServerFolderExplorer();
        }

        private void listBox3_DoubleClick(object sender, EventArgs e)
        {
            if (isUnix())
            {
                Upload(listBox3.GetItemText(listBox3.SelectedItem), textBox5.Text + "/");
            }
            else
            {
                Upload(listBox3.GetItemText(listBox3.SelectedItem), textBox5.Text);
            }

            UpdateServerFolderExplorer();
        }

        private void listBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listBox1.SelectedItem != null)
            {
                foreach (var item in listBox1.SelectedItems)
                {
                    selectedServerPaths.Add(item.ToString());
                }
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            if(selectedServerPaths.Count > 0)
            {
                DeleteServerFile(selectedServerPaths[0]);

                selectedServerPaths.Clear();

                //foreach (var item in selectedServerPaths)
                //{
                //    Console.WriteLine(item.ToString());
                //
                //    Task upload = Task.Run(() =>
                //    {
                //        DeleteServerFile(item);
                //    });
                //
                //}
            }   

            UpdateServerFolderExplorer();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            if(selectedServerPaths.Count > 0)
            {
                foreach (var item in selectedServerPaths)
                {
                    Console.WriteLine(item.ToString());

                    Task upload = Task.Run(() =>
                    {
                        Download(item);
                    });
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Settings settings = new Settings();

            settings.ShowDialog();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(Properties.Settings.Default.saveSettingsOnClose == true) 
            {
                Properties.Settings.Default.lastSessionUser = textBox1.Text;
                Properties.Settings.Default.lastSessionHost = textBox2.Text;

                Properties.Settings.Default.lastSessionPassword = textBox3.Text;

                Properties.Settings.Default.lastSessionClientPath = textBox4.Text;
                Properties.Settings.Default.lastSessionServerPath = textBox5.Text;

                Properties.Settings.Default.Save();
            }
        }

        private void listBox3_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listBox3.SelectedItem != null)
            {
                foreach (var item in listBox3.SelectedItems)
                {
                    selectedClientPaths.Add(item.ToString());
                }
            }
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            if (selectedClientPaths.Count > 0)
            {
                foreach (var item in selectedClientPaths)
                {
                    Console.WriteLine(item.ToString());

                    Task upload = Task.Run(() =>
                    {
                        if (isUnix())
                        {
                            Upload(item, textBox5.Text + "/");
                        }
                        else
                        {
                            Upload(item, textBox5.Text);
                        }

                        UpdateServerFolderExplorer();
                    });
                }
            }
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (selectedServerPaths.Count > 0) 
            {

                Input input = new Input();
                input.inputPath = selectedServerPaths[0].ToString();
                input.ShowDialog();

                var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

                using (var client = new SftpClient(connectionInfo))
                {
                    client.Connect();

                    if (client.IsConnected)
                    {
                        if(input.DialogResult == DialogResult.OK) 
                        {
                            client.RenameFile(selectedServerPaths[0], input.inputPath);
                            Log($"Renamed {selectedServerPaths[0]} to {input.inputPath}");
                        }
                    }
                    else
                    {
                        Log("Rename Failed");
                    }

                    client.Disconnect();
                }
                input.Dispose();
                selectedServerPaths.Clear();

                UpdateServerFolderExplorer();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            RemoteShell shell = new RemoteShell();
            shell.User = textBox1.Text; 
            shell.Host = textBox2.Text;
            shell.Password = textBox3.Text;
            shell.Show();
        }

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            var connectionInfo = new ConnectionInfo(textBox2.Text, textBox1.Text, new PasswordAuthenticationMethod(textBox1.Text, textBox3.Text));

            Input input = new Input();
            input.inputPath = textBox5.Text;
            input.ShowDialog();

            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();

                if (client.IsConnected)
                {
                    try
                    {
                        client.CreateDirectory(input.inputPath);
                        Log($"Created Folder {input.inputPath}");

                        UpdateServerFolderExplorer();
                    }
                    catch
                    {
                        Log("Failed to create folder");
                    }
                }

                client.Disconnect();
            }
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            Input input = new Input();
            input.inputPath = textBox4.Text + "\\";
            input.ShowDialog();

            try
            {
                Directory.CreateDirectory(input.inputPath);
                Log($"Created Folder {input.inputPath}");
                UpdateClientFolderExplorer();
            }
            catch
            {
                Log("Failed to create folder");
            }
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            if(listBox2.SelectedItem != null)
            {
                try
                {
                    DialogResult result;

                    result = MessageBox.Show($"{listBox2.SelectedItems[0]}", "Are you sure you want to delete", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        Directory.Delete(listBox2.SelectedItems[0].ToString());
                        Log($"Deleted {listBox2.SelectedItems[0]}");
                        UpdateClientFolderExplorer();
                    }
                }
                catch
                {
                    Log("Failed to delete folder");
                }
            }
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            Input input = new Input();
            input.inputPath = textBox4.Text + "\\";
            input.ShowDialog();

            try
            {
                File.Create(input.inputPath);
                Log($"Created File {input.inputPath}");
                UpdateClientFolderExplorer();
            }
            catch
            {
                Log("Failed to create file");
            }
        }

        private void toolStripMenuItem8_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedItem != null)
            {
                try
                {
                    DialogResult result;

                    result = MessageBox.Show($"{listBox3.SelectedItems[0]}", "Are you sure you want to delete", MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        File.Delete(listBox3.SelectedItems[0].ToString());
                        Log($"Deleted {listBox3.SelectedItems[0]}");
                        UpdateClientFolderExplorer();
                    }
                }
                catch
                {
                    Log("Failed to delete file");
                }
            }
        }

        private void toolStripMenuItem9_Click(object sender, EventArgs e)
        {
            if (listBox3.SelectedItem != null)
            {
                Input input = new Input();
                input.inputPath = listBox3.SelectedItems[0].ToString();
                input.ShowDialog();

                try
                {
                    File.Copy(listBox3.SelectedItems[0].ToString(), input.inputPath);
                    Log($"Renamed {listBox3.SelectedItems[0]} to {input.inputPath}");
                    UpdateClientFolderExplorer();
                }
                catch
                {
                    Log("Failed to rename file");
                }
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Connect();
        }
    }
}
