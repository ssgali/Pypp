using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using ScintillaNET;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FSE_Project
{
    public partial class Form1 : Form
    {

        PrivateFontCollection pfc = new PrivateFontCollection();
        FontFamily jetBrainsMonoFamily;
        private System.Windows.Forms.ToolTip tabToolTip = new System.Windows.Forms.ToolTip();

        private string pythonKeywords = "def class import as if else elif return while for in break continue try except pass lambda from with raise global assert del nonlocal yield";
        private string cppKeywords = "int float double if else return while for include namespace std class public private protected void cout cin new delete bool true false string const struct enum virtual override template typename";
        private string cKeywords = "int float double if else return while for void char include stdio.h stdlib.h printf scanf sizeof typedef struct enum const static extern string";

        public Form1()
        {
            InitializeComponent();
            string fontPath = Path.Combine(Application.StartupPath, "JetBrainsMonoNL-Regular.ttf");
            pfc.AddFontFile(fontPath);
            jetBrainsMonoFamily = pfc.Families[0];

            this.Text = "Py++";
            treeView1.NodeMouseDoubleClick += treeView1_NodeMouseDoubleClick_1;
            tabControl1.DrawItem += TabControl1_DrawItem;
            tabControl1.ShowToolTips = true;
            tabControl1.MouseMove += TabControl1_MouseMove;
            this.KeyPreview = true;

            AddNewTab();
        }

        // Use the full namespace for the Windows Forms ToolTip
        private void Form1_Load(object sender, EventArgs e)
        {
            this.KeyPreview = true;
            tabControl1.ShowToolTips = true; // Enable tooltips for TabControl
            tabControl1.MouseMove += TabControl1_MouseMove;
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
        private Scintilla CreateScintillaEditor(string extension = ".py")
        {
            Scintilla scintilla = new Scintilla
            {
                Dock = DockStyle.Fill
            };

            Color darkBg = Color.FromArgb(16, 16, 18);
            Color lightText = Color.FromArgb(227, 227, 227);

            scintilla.Styles[Style.Default].Font = jetBrainsMonoFamily.Name;
            scintilla.Styles[Style.Default].Size = 12;
            scintilla.Styles[Style.Default].BackColor = darkBg;
            scintilla.Styles[Style.Default].ForeColor = lightText;
            scintilla.StyleClearAll();

            scintilla.CaretForeColor = lightText;
            scintilla.SetSelectionBackColor(true, Color.FromArgb(60, 60, 60));
            scintilla.Margins[0].Width = 20;

            SetLexerAndKeywords(scintilla, extension);

            return scintilla;
        }

        private void SetLexerAndKeywords(Scintilla scintilla, string extension)
        {
            extension = extension.ToLower();
            if (extension == ".cpp" || extension == ".hpp" || extension == ".cc" || extension == ".cxx")
            {
                scintilla.Lexer = Lexer.Cpp;
                scintilla.SetKeywords(0, cppKeywords);
                ApplyCppStyling(scintilla);
            }
            else if (extension == ".c" || extension == ".h")
            {
                scintilla.Lexer = Lexer.Cpp;
                scintilla.SetKeywords(0, cKeywords);
                ApplyCppStyling(scintilla);
            }
            else if (extension == ".py")
            {
                scintilla.Lexer = Lexer.Python;
                scintilla.SetKeywords(0, pythonKeywords);
                ApplyPythonStyling(scintilla);
            }
            else
            {
                scintilla.Lexer = Lexer.Null;
            }
        }

        private void ApplyPythonStyling(Scintilla scintilla)
        {
            Color darkBg = Color.FromArgb(16, 16, 18);
            scintilla.Styles[Style.Python.CommentLine].ForeColor = Color.Green;
            scintilla.Styles[Style.Python.CommentLine].BackColor = darkBg;
            scintilla.Styles[Style.Python.Number].ForeColor = Color.Orange;
            scintilla.Styles[Style.Python.Number].BackColor = darkBg;
            scintilla.Styles[Style.Python.String].ForeColor = Color.Brown;
            scintilla.Styles[Style.Python.String].BackColor = darkBg;
            scintilla.Styles[Style.Python.Word].ForeColor = Color.SkyBlue;
            scintilla.Styles[Style.Python.Word].BackColor = darkBg;
        }

        private void ApplyCppStyling(Scintilla scintilla)
        {
            Color darkBg = Color.FromArgb(16, 16, 18);
            scintilla.Styles[Style.Cpp.Comment].ForeColor = Color.Green;
            scintilla.Styles[Style.Cpp.Comment].BackColor = darkBg;
            scintilla.Styles[Style.Cpp.Number].ForeColor = Color.Orange;
            scintilla.Styles[Style.Cpp.Number].BackColor = darkBg;
            scintilla.Styles[Style.Cpp.String].ForeColor = Color.Brown;
            scintilla.Styles[Style.Cpp.String].BackColor = darkBg;
            scintilla.Styles[Style.Cpp.Word].ForeColor = Color.Cyan;
            scintilla.Styles[Style.Cpp.Word].BackColor = darkBg;
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
            if (tabControl1.SelectedTab?.Controls[0] is Scintilla scintilla)
            {
                var size = scintilla.Styles[Style.Default].Size;
                if (size < 20) scintilla.Styles[Style.Default].Size += 1;
            }
        }

        private void DecreaseFontSize()
        {
            if (tabControl1.SelectedTab?.Controls[0] is Scintilla scintilla)
            {
                var size = scintilla.Styles[Style.Default].Size;
                if (size > 10) scintilla.Styles[Style.Default].Size -= 1;
            }
        }

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
            Scintilla scintilla = CreateScintillaEditor();
            scintilla.TextChanged += (s, ev) => MarkTabUnsaved(newTab);
            newTab.Controls.Add(scintilla);
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

        private void OpenFileFromPath(string filePath)
        {
            foreach (TabPage tab in tabControl1.TabPages)
            {
                if (tab.Tag is string existingPath && existingPath == filePath)
                {
                    tabControl1.SelectedTab = tab;
                    return;
                }
            }

            string content = File.ReadAllText(filePath);
            TabPage newTab = new TabPage(Path.GetFileName(filePath))
            {
                ToolTipText = filePath,
                Tag = filePath
            };

            Scintilla editor = CreateScintillaEditor();
            editor.Text = content;

            newTab.Controls.Add(editor);
            tabControl1.TabPages.Add(newTab);
            tabControl1.SelectedTab = newTab;
            SetLexerAndKeywords(editor, Path.GetExtension(filePath));
        }

        private void TabControl1_MouseMove(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                Rectangle tabRect = tabControl1.GetTabRect(i);
                if (tabRect.Contains(e.Location))
                {
                    string tooltipText = tabControl1.TabPages[i].Tag as string;
                    tabToolTip.SetToolTip(tabControl1.TabPages[i], tooltipText); // Set tooltip for the specific tab
                    return;
                }
            }
            tabToolTip.SetToolTip(tabControl1, ""); // Reset tooltip when the mouse is not over any tab
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
                OpenFileFromPath(filePath);
            }
        }

        private void RunFile()
        {
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
                CreateNoWindow = true  // Ensures CMD is visible
            };

            Process process = new Process { StartInfo = startInfo };
            process.Start();
        }
        private void MarkTabUnsaved(TabPage tab)
        {
            if (!tab.Text.EndsWith("*"))
                tab.Text += "*";
        }

        private void SaveCurrentFile()
        {
            if (tabControl1.SelectedTab == null) return;

            TabPage currentTab = tabControl1.SelectedTab;
            Scintilla editor = currentTab.Controls[0] as Scintilla;
            if (editor == null) return;

            string filePath = currentTab.Tag as string;

            if (string.IsNullOrEmpty(filePath))
            {
                SaveCurrentFileAs();
            }
            else
            {
                File.WriteAllText(filePath, editor.Text);
                currentTab.Text = Path.GetFileName(filePath);
            }
        }

        private void SaveCurrentFileAs()
        {
            if (tabControl1.SelectedTab == null) return;

            SaveFileDialog saveFileDialog = new SaveFileDialog { Filter = "All Files|*.*" };
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                TabPage currentTab = tabControl1.SelectedTab;
                Scintilla editor = currentTab.Controls[0] as Scintilla;
                string filePath = saveFileDialog.FileName;
                File.WriteAllText(filePath, editor.Text);

                currentTab.Tag = filePath;
                currentTab.Text = Path.GetFileName(filePath);
                LoadFolderIntoTree(Path.GetDirectoryName(filePath));
                SetLexerAndKeywords(editor, Path.GetExtension(filePath));
            }
        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
            SaveCurrentFile();
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

                    if (result == DialogResult.Cancel) return;
                    if (result == DialogResult.Yes) SaveCurrentFile();
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
                OpenFileFromPath(e.Node.Tag.ToString());
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

        private void TabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl != null)
            {
                // Set the background color
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(42, 40, 42)), e.Bounds);

                // Draw the tab text
                string tabText = tabControl.TabPages[e.Index].Text;
                TextRenderer.DrawText(e.Graphics, tabText, e.Font, e.Bounds, Color.White, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }
        }

    }
}
