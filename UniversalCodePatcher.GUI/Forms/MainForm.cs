using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UniversalCodePatcher.Controls;
using System.Text.Json;
using UniversalCodePatcher.Core;
using System.Threading.Tasks;
using UniversalCodePatcher.Interfaces;
using UniversalCodePatcher.Modules.JavaScriptModule;
using UniversalCodePatcher.Modules.CSharpModule;
using UniversalCodePatcher.Modules.BackupModule;
using UniversalCodePatcher.Modules.DiffModule;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Forms
{
    public partial class MainForm : Form
    {
        private MenuStrip menuStrip = null!;
        private ToolStrip toolStrip = null!;
        private StatusStrip statusStrip = null!;
        private SplitContainer mainSplitter = null!;
        private SplitContainer rightSplit = null!;
        private TableLayoutPanel buttonTable = null!;
        private Label projectFilesLabel = null!;
        private TreeView projectTree = null!;
        private TabControl tabControl = null!;
        private TabPage sourceTab = null!;
        private TabPage previewTab = null!;
        private TabPage rulesTab = null!;
        private CodeEditor sourceBox = null!;
        private CodeEditor previewBox = null!;
        private DataGridView rulesGrid = null!;
        private GroupBox resultsGroup = null!;
        private ListView resultsList = null!;
        private ModernButton applyButton = null!;
        private ModernButton previewButton = null!;
        private ModernButton cancelButton = null!;
        private ToolStripStatusLabel statusLabel = null!;
        private ToolStripProgressBar progressBar = null!;
        private ToolStripStatusLabel infoLabel = null!;
        private string? projectPath;

        // state helpers
        private readonly Stack<string> undoStack = new();
        private readonly Stack<string> redoStack = new();
        private readonly List<string> recentProjects = new();
        private bool showHiddenFiles;
        private bool isDirty;

        private UniversalCodePatcher.Core.ServiceContainer services = null!;
        private UniversalCodePatcher.Core.ModuleManager moduleManager = null!;
        private Dictionary<string, UniversalCodePatcher.Interfaces.IPatcher> patchersByLang = new();
        private UniversalCodePatcher.Modules.BackupModule.BackupModule? backupModule;
        private UniversalCodePatcher.Interfaces.IDiffEngine diffEngine = null!;
        private string? currentFile;

        public MainForm()
        {
            InitializeComponent();
            InitializeBusinessLogic();
            LoadRecentProjects();
            Load += (_, __) => SetInitialSplitterPosition();
        }

        private void InitializeBusinessLogic()
        {
            services = new ServiceContainer();
            moduleManager = new ModuleManager(services);
            moduleManager.ModuleError += (_, e) => MessageBox.Show(e.Error);
            moduleManager.LoadModule(typeof(JavaScriptModule));
            moduleManager.LoadModule(typeof(CSharpModule));
            moduleManager.LoadModule(typeof(BackupModule));

            foreach (var module in moduleManager.LoadedModules)
            {
                if (module is BackupModule bm)
                    backupModule = bm;

                if (module is IPatcher p && module is ICodeAnalyzer analyzer)
                {
                    foreach (var lang in analyzer.SupportedLanguages)
                        patchersByLang[lang] = p;
                }
            }

            diffEngine = new DiffModule(services);
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            ClientSize = new Size(1024, 768);
            MinimumSize = new Size(800, 600);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Universal Code Patcher";

            CreateMenuStrip();
            CreateToolStrip();
            CreateStatusBar();
            CreateMainLayout();

            ResumeLayout(false);
            PerformLayout();

            LoadSampleTree();
            WireEvents();
            ConfigureDataViews();
        }
        private void CreateMenuStrip()
        {
            menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("New Project");
            fileMenu.DropDownItems.Add("Open Project");
            fileMenu.DropDownItems.Add("Save");
            fileMenu.DropDownItems.Add("Recent");
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Close());
            var editMenu = new ToolStripMenuItem("Edit");
            editMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Undo"),
                new ToolStripMenuItem("Redo"),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Cut"),
                new ToolStripMenuItem("Copy"),
                new ToolStripMenuItem("Paste"),
                new ToolStripMenuItem("Find")
            });
            var viewMenu = new ToolStripMenuItem("View");
            viewMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Refresh"),
                new ToolStripMenuItem("Show All Files"),
                new ToolStripMenuItem("Expand Tree")
            });
            var toolsMenu = new ToolStripMenuItem("Tools");
            toolsMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Options"),
                new ToolStripMenuItem("Backup Manager"),
                new ToolStripMenuItem("Module Settings")
            });
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.AddRange(new ToolStripItem[]
            {
                new ToolStripMenuItem("Documentation"),
                new ToolStripMenuItem("About")
            });
            menuStrip.Items.AddRange(new[] { fileMenu, editMenu, viewMenu, toolsMenu, helpMenu });
            menuStrip.Dock = DockStyle.Top;
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
        }

        private void CreateToolStrip()
        {
            toolStrip = new ToolStrip { Dock = DockStyle.Top, ImageScalingSize = new Size(16, 16), Height = 25 };
            toolStrip.Items.Add(new ToolStripButton { Image = SystemIcons.Application.ToBitmap(), ToolTipText = "New" });
            toolStrip.Items.Add(new ToolStripButton { Image = SystemIcons.Application.ToBitmap(), ToolTipText = "Open" });
            toolStrip.Items.Add(new ToolStripButton { Image = SystemIcons.Application.ToBitmap(), ToolTipText = "Save" });
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton { Image = SystemIcons.Application.ToBitmap(), ToolTipText = "Patch" });
            toolStrip.Items.Add(new ToolStripButton { Image = SystemIcons.Application.ToBitmap(), ToolTipText = "Preview" });
            toolStrip.Items.Add(new ToolStripButton { Image = SystemIcons.Application.ToBitmap(), ToolTipText = "Undo" });
            toolStrip.Items.Add(new ToolStripButton { Image = SystemIcons.Application.ToBitmap(), ToolTipText = "Redo" });
            Controls.Add(toolStrip);
        }

        private void CreateStatusBar()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Ready");
            progressBar = new ToolStripProgressBar { Visible = false, AutoSize = false, Size = new Size(200, 16) };
            infoLabel = new ToolStripStatusLabel("Files: 0 | Modified: 0");
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar, infoLabel });
            statusStrip.Dock = DockStyle.Bottom;
            Controls.Add(statusStrip);
        }

        private void CreateMainLayout()
        {
            mainSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Panel1MinSize = 200,
                Panel2MinSize = 300,
                SplitterWidth = 4
            };

            projectFilesLabel = new Label { Text = "Project Files", Dock = DockStyle.Top, Height = 20 };
            projectTree = new TreeView
            {
                Dock = DockStyle.Fill,
                CheckBoxes = true,
                HideSelection = false,
                ShowLines = true,
                Font = new Font("Segoe UI", 9F)
            };
            mainSplitter.Panel1.Controls.Add(projectTree);
            mainSplitter.Panel1.Controls.Add(projectFilesLabel);
            mainSplitter.Panel1.Controls.SetChildIndex(projectFilesLabel, 0);

            rightSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                Panel1MinSize = 100,
                Panel2MinSize = 100,
                SplitterWidth = 4
            };

            tabControl = new TabControl { Dock = DockStyle.Fill };
            sourceTab = new TabPage("Source Code");
            previewTab = new TabPage("Preview Changes");
            rulesTab = new TabPage("Patch Rules");
            sourceBox = new CodeEditor { Dock = DockStyle.Fill, ReadOnly = false, Font = new Font("Consolas", 9F) };
            previewBox = new CodeEditor { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9F) };
            rulesGrid = new DataGridView { Dock = DockStyle.Fill };
            sourceTab.Controls.Add(sourceBox);
            previewTab.Controls.Add(previewBox);
            rulesTab.Controls.Add(rulesGrid);
            tabControl.TabPages.AddRange(new[] { sourceTab, previewTab, rulesTab });
            rightSplit.Panel1.Controls.Add(tabControl);

            resultsGroup = new GroupBox { Text = "Patch Results", Dock = DockStyle.Fill, Padding = new Padding(16) };
            resultsList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };
            resultsGroup.Controls.Add(resultsList);
            rightSplit.Panel2.Controls.Add(resultsGroup);

            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(0, 0, 16, 16) };
            buttonTable = new TableLayoutPanel
            {
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 1
            };
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            buttonTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            applyButton = new ModernButton
            {
                Text = "Apply",
                Size = new Size(100, 30),
                Margin = new Padding(3),
                AccentColor = ColorTranslator.FromHtml("#0078d4")
            };
            previewButton = new ModernButton { Text = "Preview", Size = new Size(80, 30), Margin = new Padding(3) };
            cancelButton = new ModernButton { Text = "Cancel", Size = new Size(80, 30), Margin = new Padding(3) };
            buttonTable.Controls.Add(applyButton, 0, 0);
            buttonTable.Controls.Add(previewButton, 1, 0);
            buttonTable.Controls.Add(cancelButton, 2, 0);

            bottomPanel.Controls.Add(buttonTable);
            rightSplit.Panel2.Controls.Add(bottomPanel);

            mainSplitter.Panel2.Controls.Add(rightSplit);
            Controls.Add(mainSplitter);
        }

        private void WireEvents()
        {
            previewButton.Click += OnPreview;
            applyButton.Click += OnApplyPatches;
            projectTree.AfterSelect += OnTreeAfterSelect;
            projectTree.AfterCheck += (_, __) => isDirty = true;
            rulesGrid.CellValueChanged += (_, __) => isDirty = true;

            var fileMenu = menuStrip.Items[0] as ToolStripMenuItem;
            if (fileMenu != null)
            {
                foreach (ToolStripItem item in fileMenu.DropDownItems)
                {
                    switch (item.Text)
                    {
                        case "Open Project":
                            item.Click += OnOpenProject;
                            break;
                        case "New Project":
                            item.Click += OnNewProject;
                            break;
                        case "Save":
                            item.Click += OnSaveProject;
                            break;
                        case "Recent":
                            break;
                        case "Exit":
                            item.Click += OnExit;
                            break;
                    }
                }
            }

            var editMenuItem = menuStrip.Items[1] as ToolStripMenuItem;
            if (editMenuItem != null)
            {
                foreach (ToolStripItem item in editMenuItem.DropDownItems)
                {
                    switch (item.Text)
                    {
                        case "Undo":
                            item.Click += OnUndo;
                            break;
                        case "Redo":
                            item.Click += OnRedo;
                            break;
                        case "Cut":
                            item.Click += (s, e) => sourceBox.Cut();
                            break;
                        case "Copy":
                            item.Click += (s, e) => sourceBox.Copy();
                            break;
                        case "Paste":
                            item.Click += (s, e) => sourceBox.Paste();
                            break;
                        case "Find":
                            item.Click += OnFindInFiles;
                            break;
                    }
                }
            }

            var viewMenuItem = menuStrip.Items[2] as ToolStripMenuItem;
            if (viewMenuItem != null)
            {
                foreach (ToolStripItem item in viewMenuItem.DropDownItems)
                {
                    switch (item.Text)
                    {
                        case "Refresh":
                            item.Click += async (s, e) => { if (projectPath != null) await LoadProjectAsync(projectPath); };
                            break;
                        case "Show All Files":
                            item.Click += OnToggleHidden;
                            break;
                        case "Expand Tree":
                            item.Click += (s, e) => projectTree.ExpandAll();
                            break;
                    }
                }
                viewMenuItem.DropDownItems.Add("Collapse Tree", null, (s, e) => projectTree.CollapseAll());
            }

            var toolsMenuItem = menuStrip.Items[3] as ToolStripMenuItem;
            if (toolsMenuItem != null)
            {
                foreach (ToolStripItem item in toolsMenuItem.DropDownItems)
                {
                    switch (item.Text)
                    {
                        case "Options":
                            item.Click += (s, e) => new SettingsForm().ShowDialog(this);
                            break;
                        case "Backup Manager":
                            item.Click += (s, e) =>
                            {
                                if (projectPath != null)
                                    new BackupManagerForm(projectPath).ShowDialog(this);
                            };
                            break;
                        case "Module Settings":
                            item.Click += (s, e) => new ModuleManagerForm(moduleManager).ShowDialog(this);
                            break;
                    }
                }
            }

            var helpMenuItem = menuStrip.Items[4] as ToolStripMenuItem;
            if (helpMenuItem != null)
            {
                foreach (ToolStripItem item in helpMenuItem.DropDownItems)
                {
                    switch (item.Text)
                    {
                        case "Documentation":
                            item.Click += (s, e) => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = "https://github.com/",
                                UseShellExecute = true
                            });
                            break;
                        case "About":
                            item.Click += (s, e) => new AboutForm().ShowDialog(this);
                            break;
                    }
                }
            }
        }

        private void ConfigureDataViews()
        {
            rulesGrid.Columns.Add("Rule", "Rule");
            rulesGrid.Columns.Add("Pattern", "Pattern");
            rulesGrid.Columns.Add("Replacement", "Replacement");

            resultsList.Columns.Add("File", 250);
            resultsList.Columns.Add("Status", 80);
            resultsList.Columns.Add("Changes", 80);
            resultsList.Columns.Add("Result", 120);
        }

        private void LoadSampleTree()
        {
            projectTree.Nodes.Clear();
            var root = projectTree.Nodes.Add("SampleProject");
            root.Nodes.Add("Program.cs");
            var folder = root.Nodes.Add("SubFolder");
            folder.Nodes.Add("Class1.cs");
            folder.Nodes.Add("Class2.cs");
            root.ExpandAll();
        }
        private void OnOpenProject(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                projectPath = dlg.SelectedPath;
                _ = LoadProjectAsync(dlg.SelectedPath);
                AddRecent(projectPath);
            }
        }

        private async Task LoadProjectAsync(string path)
        {
            try
            {
                projectTree.BeginUpdate();
                projectTree.Nodes.Clear();
                var rootDir = new System.IO.DirectoryInfo(path);
                var rootNode = new TreeNode(rootDir.FullName) { Tag = rootDir.FullName };
                await Task.Run(() => AddDirectoryNodes(rootDir, rootNode));
                projectTree.Nodes.Add(rootNode);
                rootNode.Expand();
                infoLabel.Text = $"Files: {projectTree.GetNodeCount(true)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load project: {ex.Message}");
            }
            finally
            {
                projectTree.EndUpdate();
            }
        }

        private void AddDirectoryNodes(System.IO.DirectoryInfo dir, TreeNode parent)
        {
            foreach (var sub in dir.GetDirectories())
            {
                if (!showHiddenFiles && (sub.Attributes & FileAttributes.Hidden) != 0)
                    continue;
                var node = parent.Nodes.Add(sub.FullName);
                node.Tag = sub.FullName;
                AddDirectoryNodes(sub, node);
            }
            foreach (var file in dir.GetFiles())
            {
                if (!showHiddenFiles && (file.Attributes & FileAttributes.Hidden) != 0)
                    continue;
                var node = parent.Nodes.Add(file.FullName);
                node.Tag = file.FullName;
            }
        }

        private void OnTreeAfterSelect(object? sender, TreeViewEventArgs e)
        {
            if (e.Node?.Tag is string file && System.IO.File.Exists(file))
            {
                try
                {
                    sourceBox.Text = System.IO.File.ReadAllText(file);
                    previewBox.Clear();
                    statusLabel.Text = file;
                    currentFile = file;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}");
                }
            }
        }

        private void OnPreview(object? sender, EventArgs e)
        {
            if (currentFile == null) return;
            var language = GetLanguageFromFile(currentFile);
            if (!patchersByLang.TryGetValue(language, out var patcher))
            {
                MessageBox.Show($"No patcher for language {language}");
                return;
            }

            var text = sourceBox.Text;
            foreach (DataGridViewRow row in rulesGrid.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells[1].Value is string pattern && !string.IsNullOrEmpty(pattern))
                {
                    var replace = row.Cells[2].Value?.ToString() ?? string.Empty;
                    var rule = new PatchRule
                    {
                        Name = row.Cells[0].Value?.ToString() ?? "Rule",
                        TargetPattern = pattern,
                        NewContent = replace,
                        PatchType = PatchType.Replace,
                        TargetLanguage = language
                    };
                    text = patcher.PreviewPatch(text, rule, language);
                }
            }
            previewBox.Text = text;
            tabControl.SelectedTab = previewTab;
        }

        private void OnApplyPatches(object? sender, EventArgs e)
        {
            if (projectPath == null) return;
            if (MessageBox.Show("Apply patches to selected files?", "Confirm", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;

            resultsList.Items.Clear();
            if (projectTree.Nodes.Count == 0)
                return;
            foreach (TreeNode node in EnumerateNodes(projectTree.Nodes[0]))
            {
                if (!node.Checked || node.Tag is not string file || !File.Exists(file))
                    continue;

                var language = GetLanguageFromFile(file);
                if (!patchersByLang.TryGetValue(language, out var patcher))
                {
                    var item = resultsList.Items.Add(file);
                    item.SubItems.Add("Skipped");
                    item.SubItems.Add("0");
                    item.SubItems.Add($"No patcher for {language}");
                    continue;
                }

                var original = File.ReadAllText(file);
                var text = original;
                foreach (DataGridViewRow row in rulesGrid.Rows)
                {
                    if (row.IsNewRow) continue;
                    if (row.Cells[1].Value is string pattern && !string.IsNullOrEmpty(pattern))
                    {
                        var replace = row.Cells[2].Value?.ToString() ?? string.Empty;
                        var rule = new PatchRule
                        {
                            Name = row.Cells[0].Value?.ToString() ?? "Rule",
                            TargetPattern = pattern,
                            NewContent = replace,
                            PatchType = PatchType.Replace,
                            TargetLanguage = language
                        };
                        var result = patcher.ApplyPatch(text, rule, language);
                        text = result.ModifiedCode;
                    }
                }
                try
                {
                    backupModule?.CreateBackup(file);
                    File.WriteAllText(file, text);
                    var item = resultsList.Items.Add(file);
                    item.SubItems.Add("OK");
                    item.SubItems.Add((text == original ? "0" : "1"));
                    item.SubItems.Add("Patched");
                }
                catch (Exception ex)
                {
                    var item = resultsList.Items.Add(file);
                    item.SubItems.Add("Error");
                    item.SubItems.Add("0");
                    item.SubItems.Add(ex.Message);
                }
            }
            statusLabel.Text = "Patching complete";
        }

        private string GetLanguageFromFile(string file)
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            return ext switch
            {
                ".cs" => "CSharp",
                ".js" => "JavaScript",
                ".ts" => "TypeScript",
                _ => "Unknown"
            };
        }

        private IEnumerable<TreeNode> EnumerateNodes(TreeNode node)
        {
            yield return node;
            foreach (TreeNode child in node.Nodes)
            {
                foreach (var n in EnumerateNodes(child))
                    yield return n;
            }
        }

        private async void OnNewProject(object? sender, EventArgs e)
        {
            using var dlg = new NewProjectForm();
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                if (!Directory.Exists(dlg.ProjectPath))
                    Directory.CreateDirectory(dlg.ProjectPath);
                projectPath = dlg.ProjectPath;
                await LoadProjectAsync(projectPath);
                AddRecent(projectPath);
            }
        }

        private void OnSaveProject(object? sender, EventArgs e)
        {
            if (projectPath == null) return;
            using var dlg = new SaveFileDialog { Filter = "Project|*.json" };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            var state = new ProjectState { ProjectPath = projectPath };
            foreach (DataGridViewRow row in rulesGrid.Rows)
            {
                if (row.IsNewRow) continue;
                state.Rules.Add(new SimplePatchRule
                {
                    Name = row.Cells[0].Value?.ToString(),
                    Pattern = row.Cells[1].Value?.ToString() ?? string.Empty,
                    Replace = row.Cells[2].Value?.ToString() ?? string.Empty
                });
            }
            if (projectTree.Nodes.Count > 0)
            {
                foreach (var node in EnumerateNodes(projectTree.Nodes[0]))
                    if (node.Checked && node.Tag is string file)
                        state.SelectedFiles.Add(file);
            }
            File.WriteAllText(dlg.FileName, JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true }));
            isDirty = false;
        }

        private void OnExit(object? sender, EventArgs e)
        {
            if (isDirty && MessageBox.Show("Exit without saving?", "Confirm", MessageBoxButtons.OKCancel) != DialogResult.OK)
                return;
            Close();
        }

        private void OnUndo(object? sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                redoStack.Push(sourceBox.Text);
                var text = undoStack.Pop();
                sourceBox.Text = text;
            }
        }

        private void OnRedo(object? sender, EventArgs e)
        {
            if (redoStack.Count > 0)
            {
                undoStack.Push(sourceBox.Text);
                sourceBox.Text = redoStack.Pop();
            }
        }

        private void OnFindInFiles(object? sender, EventArgs e)
        {
            if (projectPath == null) return;
            using var dlg = new FindForm(projectPath);
            dlg.FileSelected += file =>
            {
                var nodes = projectTree.Nodes.Find(Path.GetFileName(file), true);
                if (nodes.Length > 0)
                    projectTree.SelectedNode = nodes[0];
            };
            dlg.ShowDialog(this);
        }

        private void OnToggleHidden(object? sender, EventArgs e)
        {
            showHiddenFiles = !showHiddenFiles;
            if (projectPath != null) _ = LoadProjectAsync(projectPath);
        }

        private void AddRecent(string path)
        {
            recentProjects.Remove(path);
            recentProjects.Insert(0, path);
            if (recentProjects.Count > 5) recentProjects.RemoveAt(5);
            UpdateRecentMenu();
            SaveRecentProjects();
        }

        private void LoadRecentProjects()
        {
            var file = Path.Combine(Application.StartupPath, "recent.txt");
            if (File.Exists(file))
                recentProjects.AddRange(File.ReadAllLines(file));
            UpdateRecentMenu();
        }

        private void SaveRecentProjects()
        {
            var file = Path.Combine(Application.StartupPath, "recent.txt");
            File.WriteAllLines(file, recentProjects.Take(5));
        }

        private void UpdateRecentMenu()
        {
            if (menuStrip.Items.Count == 0) return;
            var fileMenuItem = menuStrip.Items[0] as ToolStripMenuItem;
            if (fileMenuItem == null) return;
            var recent = fileMenuItem.DropDownItems.Cast<ToolStripItem>().FirstOrDefault(i => i.Text == "Recent") as ToolStripMenuItem;
            if (recent == null) return;
            recent.DropDownItems.Clear();
            foreach (var p in recentProjects.Take(5))
            {
                var item = new ToolStripMenuItem(p);
                item.Click += async (s, e) => { projectPath = p; await LoadProjectAsync(p); };
                recent.DropDownItems.Add(item);
            }
        }

        private void SetInitialSplitterPosition()
        {
            if (mainSplitter == null || mainSplitter.Width <= 0)
                return;

            int min = mainSplitter.Panel1MinSize;
            int max = mainSplitter.Width - mainSplitter.Panel2MinSize;
            int target = Math.Max(min, (int)(mainSplitter.Width * 0.25));
            mainSplitter.SplitterDistance = Math.Max(min, Math.Min(max, target));

            if (rightSplit != null)
            {
                int height = mainSplitter.Panel2.ClientSize.Height;
                int rmin = rightSplit.Panel1MinSize;
                int rmax = height - rightSplit.Panel2MinSize;
                int rtarget = (int)(height * 0.7);
                rightSplit.SplitterDistance = Math.Max(rmin, Math.Min(rmax, rtarget));
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (mainSplitter != null && mainSplitter.Width > 0)
            {
                int min = mainSplitter.Panel1MinSize;
                int max = mainSplitter.Width - mainSplitter.Panel2MinSize;
                int target = (int)(mainSplitter.Width * 0.25);
                mainSplitter.SplitterDistance = Math.Max(min, Math.Min(max, target));
            }

            if (rightSplit != null && mainSplitter != null && mainSplitter.Panel2.Width > 0)
            {
                int height = mainSplitter.Panel2.ClientSize.Height;
                int min = rightSplit.Panel1MinSize;
                int max = height - rightSplit.Panel2MinSize;
                int target = (int)(height * 0.7);
                rightSplit.SplitterDistance = Math.Max(min, Math.Min(max, target));
            }
        }

    }
}
