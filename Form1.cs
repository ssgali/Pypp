using System.IO;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FSE_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "Py++";
            treeView1.Dock = DockStyle.Left;
            treeView1.Width = 250;  // Adjust to your liking

            // Add to the Form
            this.Controls.Add(treeView1);

            // Add event for double click to open files
            treeView1.NodeMouseDoubleClick += treeView1_NodeMouseDoubleClick_1;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            AddNewTab();
        }

        private void LoadFolderIntoTree(string folderPath)
        {
            treeView1.Nodes.Clear();
            DirectoryInfo rootDir = new DirectoryInfo(folderPath);

            TreeNode rootNode = new TreeNode(rootDir.Name)
            {
                Tag = rootDir.FullName
            };

            treeView1.Nodes.Add(rootNode);
            LoadSubDirectories(rootNode, rootDir);
            if (treeView1.Nodes.Count > 0)
            {
                treeView1.Nodes[0].Expand();  // Expand only the root folder
            }
        }
        private void LoadSubDirectories(TreeNode parentNode, DirectoryInfo parentDir)
        {
            try
            {
                foreach (var dir in parentDir.GetDirectories())
                {
                    TreeNode dirNode = new TreeNode(dir.Name)
                    {
                        Tag = dir.FullName
                    };
                    parentNode.Nodes.Add(dirNode);
                    LoadSubDirectories(dirNode, dir);
                }

                foreach (var file in parentDir.GetFiles())
                {
                    TreeNode fileNode = new TreeNode(file.Name)
                    {
                        Tag = file.FullName
                    };
                    parentNode.Nodes.Add(fileNode);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading folder: " + ex.Message);
            }
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
            else if (e.Control && e.KeyCode == Keys.O)
            {
                OpenFolder();
                e.SuppressKeyPress = true;
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
        private void OpenFolder()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    LoadFolderIntoTree(folderDialog.SelectedPath);
                }
            }
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

        private void treeView1_NodeMouseDoubleClick_1(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null && File.Exists(e.Node.Tag.ToString()))
            {
                OpenFile(e.Node.Tag.ToString());
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            OpenFolder();
        }
    }
}
