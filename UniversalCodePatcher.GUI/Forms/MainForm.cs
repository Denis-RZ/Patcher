using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UniversalCodePatcher.Core;
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
        private SplitContainer verticalSplit = null!;
        private SplitContainer horizontalSplit = null!;
        private Label projectFilesLabel = null!;
        private TreeView projectTree = null!;
        private TabControl tabControl = null!;
        private TabPage sourceTab = null!;
        private TabPage previewTab = null!;
        private TabPage rulesTab = null!;
        private RichTextBox sourceBox = null!;
        private RichTextBox previewBox = null!;
        private DataGridView rulesGrid = null!;
        private GroupBox resultsGroup = null!;
        private ListView resultsList = null!;
        private FlowLayoutPanel actionFlow = null!;
        private Button applyButton = null!;
        private Button previewButton = null!;
        private Button cancelButton = null!;
        private ToolStripStatusLabel statusLabel = null!;
        private ToolStripProgressBar progressBar = null!;
        private ToolStripStatusLabel infoLabel = null!;
        private string? projectPath;

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

            // Form settings
            ClientSize = new Size(1024, 768);
            MinimumSize = new Size(800, 600);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Universal Code Patcher";

            // MenuStrip
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

            // ToolStrip
            toolStrip = new ToolStrip { Dock = DockStyle.Top, ImageScalingSize = new Size(16, 16), Height = 25 };
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "New"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Open"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Save"
            });
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Patch"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Preview"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Undo"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Redo"
            });

            // StatusStrip
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Ready");
            progressBar = new ToolStripProgressBar { Visible = false, AutoSize = false, Size = new Size(200, 16) };
            infoLabel = new ToolStripStatusLabel("Files: 0 | Modified: 0");
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar, infoLabel });
            statusStrip.Dock = DockStyle.Bottom;

            // Split containers
            verticalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 250,
                Panel1MinSize = 200
            };

            horizontalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };

            // Panel1 of verticalSplit
            projectFilesLabel = new Label { Text = "Project Files", Dock = DockStyle.Top, Height = 20 };
            projectTree = new TreeView
            {
                Dock = DockStyle.Fill,
                CheckBoxes = true,
                HideSelection = false,
                ShowLines = true,
                Font = new Font("Segoe UI", 9F)
            };
            verticalSplit.Panel1.Controls.Add(projectTree);
            verticalSplit.Panel1.Controls.Add(projectFilesLabel);
            verticalSplit.Panel1.Controls.SetChildIndex(projectFilesLabel, 0);

            // Tab control for code and rules
            tabControl = new TabControl { Dock = DockStyle.Fill };
            sourceTab = new TabPage("Source Code");
            previewTab = new TabPage("Preview Changes");
            rulesTab = new TabPage("Patch Rules");
            sourceBox = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9F) };
            previewBox = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9F) };
            rulesGrid = new DataGridView { Dock = DockStyle.Fill };
            sourceTab.Controls.Add(sourceBox);
            previewTab.Controls.Add(previewBox);
            rulesTab.Controls.Add(rulesGrid);
            tabControl.TabPages.AddRange(new[] { sourceTab, previewTab, rulesTab });
            horizontalSplit.Panel1.Controls.Add(tabControl);

            // Results group
            resultsGroup = new GroupBox { Text = "Patch Results", Dock = DockStyle.Fill };
            resultsList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };
            resultsGroup.Controls.Add(resultsList);
            horizontalSplit.Panel2.Controls.Add(resultsGroup);

            verticalSplit.Panel2.Controls.Add(horizontalSplit);

            // Bottom panel with buttons
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            actionFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft
            };
            applyButton = new Button { Text = "Apply Patches", Size = new Size(90, 23), Margin = new Padding(3) };
            previewButton = new Button { Text = "Preview", Size = new Size(75, 23), Margin = new Padding(3) };
            cancelButton = new Button { Text = "Cancel", Size = new Size(75, 23), Margin = new Padding(3) };
            actionFlow.Controls.Add(applyButton);
            actionFlow.Controls.Add(previewButton);
            actionFlow.Controls.Add(cancelButton);
            bottomPanel.Controls.Add(actionFlow);
            verticalSplit.Panel2.Controls.Add(bottomPanel);
            verticalSplit.Panel2.Controls.SetChildIndex(bottomPanel, 0);

            // Add root controls
            Controls.Add(verticalSplit);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;

            ResumeLayout(false);
            PerformLayout();

            // sample tree structure
            LoadSampleTree();

            // wire events
            previewButton.Click += OnPreview;
            applyButton.Click += OnApplyPatches;
            projectTree.AfterSelect += OnTreeAfterSelect;

            fileMenu = menuStrip.Items[0] as ToolStripMenuItem;
            if (fileMenu != null)
            {
                foreach (ToolStripItem item in fileMenu.DropDownItems)
                {
                    if (item.Text == "Open Project")
                        item.Click += OnOpenProject;
                }
            }

            // configure grids and lists
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
                LoadProject(dlg.SelectedPath);
            }
        }

        private void LoadProject(string path)
        {
            try
            {
                projectTree.BeginUpdate();
                projectTree.Nodes.Clear();
                var rootDir = new System.IO.DirectoryInfo(path);
                var rootNode = projectTree.Nodes.Add(rootDir.Name);
                AddDirectoryNodes(rootDir, rootNode);
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
                var node = parent.Nodes.Add(sub.Name);
                node.Tag = sub.FullName;
                AddDirectoryNodes(sub, node);
            }
            foreach (var file in dir.GetFiles())
            {
                var node = parent.Nodes.Add(file.Name);
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

    }
}
