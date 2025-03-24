using System.Diagnostics;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;

namespace FSE_Project
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "Py++";
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

        //private string FileName = string.Empty;

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData) // for Global key bindings
        {
            if (keyData == (Keys.Control | Keys.Oemplus))  // Ctrl +
            {
                IncreaseFontSize();
                return true; // Prevents further processing
            }
            else if (keyData == (Keys.Control | Keys.OemMinus))  // Ctrl -
            {
                DecreaseFontSize();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.N))
            {
                AddNewTab();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.O))
            {
                OpenFolder();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.K))
            {
                OpenFile();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.S))
            {
                SaveCurrentFile();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.Tab))
            {
                SwitchToNextTab();
                return true;
            }
            else if (keyData == (Keys.Control | Keys.W))
            {
                CloseCurrentTab();
                return true;
            }
            else if (keyData == (Keys.F5))
            {
                RunFile();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData); // Pass other keys to default handler
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

        private void OpenFile()
        {
            OpenFileDialog open = new OpenFileDialog
            {
                Filter = "All Files (*.*)|*.*",
                Title = "Open File"
            };

            if (open.ShowDialog() == DialogResult.OK)
            {
                string filePath = open.FileName;
                string content = File.ReadAllText(filePath);

                TabPage newTab = new TabPage(Path.GetFileName(filePath));
                RichTextBox richTextBox = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    Text = content
                };

                richTextBox.TextChanged += (s, ev) => MarkTabUnsaved(newTab);

                newTab.Controls.Add(richTextBox);
                newTab.Tag = filePath;

                tabControl1.TabPages.Add(newTab);
                tabControl1.SelectedTab = newTab;
            }
        }

        private void OpenFileFromTree(string filePath)
        {
            string content = File.ReadAllText(filePath);

            TabPage newTab = new TabPage(Path.GetFileName(filePath));
            RichTextBox richTextBox = new RichTextBox
            {
                Dock = DockStyle.Fill,
                Text = content
            };

            richTextBox.TextChanged += (s, ev) => MarkTabUnsaved(newTab);

            newTab.Controls.Add(richTextBox);
            newTab.Tag = filePath;

            tabControl1.TabPages.Add(newTab);
            tabControl1.SelectedTab = newTab;
        }

        private void RunFile() {
            string filePath = tabControl1.SelectedTab.Tag as string;
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Save the file first");
                return;
            }

            string extension = Path.GetExtension(filePath);
            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileName(filePath);
            string exePath = Path.Combine(directory, Path.GetFileNameWithoutExtension(filePath) + ".exe");

            // Create a batch script to handle execution and pause
            string batchScript = Path.Combine(directory, "run_temp.bat");

            using (StreamWriter writer = new StreamWriter(batchScript))
            {
                writer.WriteLine("@echo off");
                writer.WriteLine("cd /d \"" + directory + "\"");
                writer.WriteLine("cls");

                if (extension == ".cpp" || extension == ".c")
                {
                    writer.WriteLine($"g++ \"{fileName}\" -o \"{exePath}\"");
                    writer.WriteLine($"if %errorlevel% == 0 ( \"{exePath}\" )");
                }
                else if (extension == ".py")
                {
                    writer.WriteLine($"python \"{filePath}\"");
                }
                else
                {
                    writer.WriteLine("echo Please run a valid filetype");
                    writer.WriteLine("echo Supported filetypes are: ");
                    writer.WriteLine("echo 1. Python");
                    writer.WriteLine("echo 2. CPP");
                    writer.WriteLine("echo 3. C");
                }

                writer.WriteLine("echo.");
                writer.WriteLine("echo Press any key to exit...");
                writer.WriteLine("pause >nul");
                writer.WriteLine("del \"%~f0\""); // Deletes itself after execution
            }

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{batchScript}\"",
                UseShellExecute = true, // Required to open CMD in a new window
                CreateNoWindow = false  // Ensures CMD is visible
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
        }
        private void MarkTabUnsaved(TabPage tab)
        {
            if (!tab.Text.EndsWith("*"))
            {
                tab.Text += "*";
            }
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
                LoadFolderIntoTree(Path.GetDirectoryName(filePath)); // to reload the file tree with the new file
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

        private void openToolStripButton_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            CloseCurrentTab();
        }

        private void treeView1_NodeMouseDoubleClick_1(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null && e.Node.Tag != null && File.Exists(e.Node.Tag.ToString()))
            {
                OpenFileFromTree(e.Node.Tag.ToString());
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            OpenFolder();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            RunFile();
        }
    }
}
