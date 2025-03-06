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
            OpenFileDialog open = new OpenFileDialog();

            open.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            open.Title = "Open File";
            open.FileName = "";

            if (open.ShowDialog() == DialogResult.OK)
            {
                // save the opened FileName in our variable
                this.FileName = open.FileName;
                this.Text = string.Format("{0}", Path.GetFileNameWithoutExtension(open.FileName));
                StreamReader reader = new StreamReader(open.FileName);
                richTextBox1.Text = reader.ReadToEnd();
                reader.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog saving = new SaveFileDialog();

            saving.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            saving.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
            saving.Title = "Save As";
            saving.FileName = "Untitled";

            if (saving.ShowDialog() == DialogResult.OK)
            {
                // save the new FileName in our variable
                this.FileName = saving.FileName;
                StreamWriter writing = new StreamWriter(saving.FileName);
                writing.Write(richTextBox1.Text);
                writing.Close();
            }
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

        private void button4_Click(object sender, EventArgs e)
        {

        }

        private string FileName = string.Empty;
        private void button2_Click(object sender, EventArgs e)
        {
            this.FileName = string.Empty;
            richTextBox1.Clear();
        }
        private void richTextBox1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Oemplus)  // Ctrl +
            {
                IncreaseFontSize();
                e.SuppressKeyPress = true; // Prevents the default behavior
            }
            else if (e.Control && e.KeyCode == Keys.OemMinus) // Ctrl -
            {
                DecreaseFontSize();
                e.SuppressKeyPress = true; // Prevents the default behavior
            }
        }
        private void IncreaseFontSize()
        {
            float newSize = richTextBox1.Font.Size + 1;
            richTextBox1.Font = new Font(richTextBox1.Font.FontFamily, newSize);
        }

        private void DecreaseFontSize()
        {
            float newSize = richTextBox1.Font.Size - 1;
            if (newSize >= 6)  // Minimum font size to avoid invisible text
            {
                richTextBox1.Font = new Font(richTextBox1.Font.FontFamily, newSize);
            }
        }

        
    }
}
