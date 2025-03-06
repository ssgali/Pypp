using System.IO;

namespace FSE_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "Py++";
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = "Current Directory:   " + Environment.CurrentDirectory;
            textBox1.ReadOnly = true;
            textBox1.TabStop = false;
            ActiveControl = null;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
