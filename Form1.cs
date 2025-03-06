using System.IO;
using System.Windows.Forms;

namespace FSE_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "Py++";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            AddNewTab();
        }


        private RichTextBox GetCurrentRichTextBox()
        {
            if (tabControl1.SelectedTab != null && tabControl1.SelectedTab.Controls.Count > 0)
            {
                return tabControl1.SelectedTab.Controls[0] as RichTextBox;
            }
            return null;
        }

        private void richTextBox1_KeyDown_1(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.Oemplus)  // Ctrl +
            {
                IncreaseFontSize();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.OemMinus)  // Ctrl -
            {
                DecreaseFontSize();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                AddNewTab();
                e.SuppressKeyPress = true;  // Prevents unwanted character input
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                SaveCurrentFile();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.Tab)
            {
                SwitchToNextTab();
                e.SuppressKeyPress = true;
            }
            else if (e.Control && e.KeyCode == Keys.W)
            {
                CloseCurrentTab();
                e.SuppressKeyPress = true;
            }
        }
        private void SwitchToNextTab()
        {
            if (tabControl1.TabCount > 1)
            {
                int nextIndex = (tabControl1.SelectedIndex + 1) % tabControl1.TabCount;
                tabControl1.SelectedIndex = nextIndex;
            }
        }

        private void IncreaseFontSize()
        {
            var rtb = GetCurrentRichTextBox();
            if (rtb != null)
            {
                rtb.Font = new Font(rtb.Font.FontFamily, rtb.Font.Size + 1);
            }
        }

        private void DecreaseFontSize()
        {
            var rtb = GetCurrentRichTextBox();
            if (rtb != null && rtb.Font.Size > 6)
            {
                rtb.Font = new Font(rtb.Font.FontFamily, rtb.Font.Size - 1);
            }
        }

        private string FileName = string.Empty;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.Control | Keys.S))
            {
                SaveCurrentFile();
                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void newToolStripButton_Click_1(object sender, EventArgs e)
        {
            AddNewTab();
        }

        private void AddNewTab()
        {
            TabPage newTab = new TabPage("Untitled");
            RichTextBox richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                AcceptsTab = true
            };
            richTextBox.TextChanged += (s, ev) => MarkTabUnsaved(newTab);
            richTextBox.KeyDown += richTextBox1_KeyDown_1;  // Support font size shortcuts

            newTab.Controls.Add(richTextBox);
            tabControl1.TabPages.Add(newTab);
            tabControl1.SelectedTab = newTab;
            newTab.Tag = null;
        }

        private void MarkTabUnsaved(TabPage tab)
        {
            if (!tab.Text.EndsWith("*"))
            {
                tab.Text += "*";
            }
        }

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Open File"
            };

            if (open.ShowDialog() == DialogResult.OK)
            {
                OpenFile(open.FileName);
            }
        }

        private void OpenFile(string filePath)
        {
            string content = File.ReadAllText(filePath);

            TabPage newTab = new TabPage(Path.GetFileName(filePath));
            RichTextBox richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Text = content
            };

            richTextBox.TextChanged += (s, ev) => MarkTabUnsaved(newTab);
            richTextBox.KeyDown += richTextBox1_KeyDown_1;  // Add font size support

            newTab.Controls.Add(richTextBox);
            newTab.Tag = filePath;

            tabControl1.TabPages.Add(newTab);
            tabControl1.SelectedTab = newTab;
        }

        private void SaveCurrentFile()
        {
            if (tabControl1.SelectedTab == null) return;

            TabPage currentTab = tabControl1.SelectedTab;
            RichTextBox richTextBox = GetCurrentRichTextBox();
            if (richTextBox == null) return;

            string filePath = currentTab.Tag as string;

            if (string.IsNullOrEmpty(filePath))
            {
                SaveCurrentFileAs();
            }
            else
            {
                File.WriteAllText(filePath, richTextBox.Text);
                currentTab.Text = Path.GetFileName(filePath);  // Remove "*"
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveCurrentFile();
        }
        private void SaveCurrentFileAs()
        {
            if (tabControl1.SelectedTab == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "All Files|*.*"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                TabPage currentTab = tabControl1.SelectedTab;
                RichTextBox richTextBox = GetCurrentRichTextBox();

                string filePath = saveFileDialog.FileName;
                File.WriteAllText(filePath, richTextBox.Text);

                currentTab.Tag = filePath;
                currentTab.Text = Path.GetFileName(filePath);  // Remove "*"
            }
        }

        private void CloseCurrentTab()
        {
            if (tabControl1.SelectedTab != null)
            {
                TabPage currentTab = tabControl1.SelectedTab;

                if (currentTab.Text.EndsWith("*"))
                {
                    DialogResult result = MessageBox.Show(
                        "Do you want to save changes to " + currentTab.Text.TrimEnd('*') + "?",
                        "Unsaved Changes",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Cancel)
                        return;

                    if (result == DialogResult.Yes)
                        SaveCurrentFile();
                }

                tabControl1.TabPages.Remove(currentTab);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            CloseCurrentTab();
        }

    }
}
