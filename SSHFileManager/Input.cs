using System;
using System.Windows.Forms;

namespace SSHFileManager
{
    public partial class Input : Form
    {
        public string inputPath { get; set; }

        public Input()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            inputPath = textBox1.Text;
            DialogResult = DialogResult.OK; 
        }

        private void RenameInput_Load(object sender, EventArgs e)
        {
            textBox1.Text = inputPath;
        }
    }
}
