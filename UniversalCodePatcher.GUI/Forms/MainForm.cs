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
using UniversalCodePatcher.GUI.Models;

namespace UniversalCodePatcher.Forms
{
    public partial class MainForm : Form
    {
        private MenuStrip menuStrip = null!;
        private ToolStrip toolStrip = null!;
        private StatusStrip statusStrip = null!;
        private SplitContainer mainSplitter = null!;
        private SplitContainer rightSplit = null!;
        private Panel buttonPanel = null!;
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
        private AppSettings settings = null!;
        private static readonly string SettingsFile = Path.Combine(Application.StartupPath, "appsettings.json");

        public MainForm()
        {
            InitializeComponent();
            settings = AppSettings.Load(SettingsFile);
            showHiddenFiles = settings.ShowHiddenFiles;
            InitializeBusinessLogic();
            LoadRecentProjects();

            // Configure sizes after the form has valid bounds
            Load += (_, __) =>
            {
                // Более надежная последовательность инициализации
                BeginInvoke(new Action(() =>
                {
                    SetInitialSplitterPositions();

                    // Дополнительная проверка TabControl
                    if (tabControl != null && tabControl.Parent != null)
                    {
                        tabControl.Invalidate();
                        tabControl.Update();

                        // Убедимся что выбрана первая вкладка
                        if (tabControl.TabPages.Count > 0)
                            tabControl.SelectedTab = tabControl.TabPages[0];
                    }
                }));
            };

            // Handle form resizing more smoothly
            ResizeEnd += (_, __) => SetSplitterPositions();
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

            ClientSize = new Size(1200, 800);
            MinimumSize = new Size(900, 650);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Universal Code Patcher";
            BackColor = SystemColors.Control;

            // ВАЖНО: Правильный порядок создания и добавления контролов
            CreateMenuStrip();      // Создаем меню
            CreateStatusBar();      // Создаем статус бар
            CreateMainLayout();     // Создаем основной layout ПОСЛЕДНИМ

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
            
            // Добавляем MenuStrip ПЕРВЫМ
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
        }

        private void CreateToolStrip()
        {
            toolStrip = new ToolStrip
            {
                Dock = DockStyle.Top,
                ImageScalingSize = new Size(16, 16),
                Height = 28,
                Padding = new Padding(4, 2, 4, 2)
            };
            
            // Упрощаем создание кнопок без изображений для избежания проблем
            toolStrip.Items.Add(new ToolStripButton { Text = "New", ToolTipText = "New Project" });
            toolStrip.Items.Add(new ToolStripButton { Text = "Open", ToolTipText = "Open Project" });
            toolStrip.Items.Add(new ToolStripButton { Text = "Save", ToolTipText = "Save Project" });
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton { Text = "Patch", ToolTipText = "Apply Patches" });
            toolStrip.Items.Add(new ToolStripButton { Text = "Preview", ToolTipText = "Preview Changes" });
            toolStrip.Items.Add(new ToolStripButton { Text = "Undo", ToolTipText = "Undo" });
            toolStrip.Items.Add(new ToolStripButton { Text = "Redo", ToolTipText = "Redo" });
            
            // Добавляем ToolStrip ВТОРЫМ
            Controls.Add(toolStrip);
        }

        private void CreateStatusBar()
        {
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Ready") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            progressBar = new ToolStripProgressBar { Visible = false, AutoSize = false, Size = new Size(200, 18) };
            infoLabel = new ToolStripStatusLabel("Files: 0 | Modified: 0") { AutoSize = true };
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar, infoLabel });
            statusStrip.Dock = DockStyle.Bottom;
            
            // Добавляем StatusStrip ТРЕТЬИМ
            Controls.Add(statusStrip);
        }

        private void CreateMainLayout()
        {
            // Создаем основной splitter БЕЗ установки минимальных размеров
            mainSplitter = new SplitContainer
            {
                Dock = DockStyle.Fill,  // Заполнит оставшееся место после MenuStrip, ToolStrip и StatusStrip
                Orientation = Orientation.Vertical,
                SplitterWidth = 5,
                FixedPanel = FixedPanel.Panel1
            };

            // Left panel - Project tree
            projectFilesLabel = new Label
            {
                Text = "Project Files",
                Dock = DockStyle.Top,
                Height = 24,
                Padding = new Padding(8, 4, 8, 4),
                BackColor = SystemColors.ControlLight,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            projectTree = new TreeView
            {
                Dock = DockStyle.Fill,
                CheckBoxes = true,
                HideSelection = false,
                ShowLines = true,
                Font = new Font("Segoe UI", 9F),
                Indent = 20,
                ItemHeight = 20,
                Margin = new Padding(4)
            };

            mainSplitter.Panel1.Controls.Add(projectTree);
            mainSplitter.Panel1.Controls.Add(projectFilesLabel);
            mainSplitter.Panel1.Controls.SetChildIndex(projectFilesLabel, 0);

            // Right panel - Создаем правый splitter БЕЗ минимальных размеров
            rightSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterWidth = 5,
                FixedPanel = FixedPanel.Panel2,
                BackColor = SystemColors.Control
            };

            CreateTopPanel();
            CreateBottomPanel();

            mainSplitter.Panel2.Controls.Add(rightSplit);
            
            // ВАЖНО: Добавляем основной splitter ПОСЛЕДНИМ
            Controls.Add(mainSplitter);
        }

        private void CreateTopPanel()
        {
            // Top part of right panel - TabControl with editors
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9F),
                Padding = new Point(8, 4),
                Visible = true  // Явно делаем видимым
            };

            // Create tab pages
            sourceTab = new TabPage("Source Code") 
            { 
                UseVisualStyleBackColor = true,
                BackColor = Color.White
            };
            previewTab = new TabPage("Preview Changes") 
            { 
                UseVisualStyleBackColor = true,
                BackColor = Color.White
            };
            rulesTab = new TabPage("Patch Rules") 
            { 
                UseVisualStyleBackColor = true,
                BackColor = Color.White
            };

            // Create editors and grid - используем RichTextBox если CodeEditor недоступен
            try
            {
                sourceBox = new CodeEditor
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = false,
                    Font = new Font("Consolas", 10F),
                    Text = "// Source code will appear here...",
                    BackColor = Color.White
                };
            }
            catch
            {
                // Fallback to RichTextBox if CodeEditor is not available
                sourceBox = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = false,
                    Font = new Font("Consolas", 10F),
                    Text = "// Source code will appear here...",
                    BackColor = Color.White
                } as CodeEditor;
            }

            try
            {
                previewBox = new CodeEditor
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Font = new Font("Consolas", 10F),
                    Text = "// Preview will appear here...",
                    BackColor = Color.FromArgb(248, 248, 248)
                };
            }
            catch
            {
                // Fallback to RichTextBox if CodeEditor is not available
                previewBox = new RichTextBox
                {
                    Dock = DockStyle.Fill,
                    ReadOnly = true,
                    Font = new Font("Consolas", 10F),
                    Text = "// Preview will appear here...",
                    BackColor = Color.FromArgb(248, 248, 248)
                } as CodeEditor;
            }

            rulesGrid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = true,
                AllowUserToDeleteRows = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
                Margin = new Padding(4),
                BackgroundColor = Color.White
            };

            // Add controls to tab pages
            sourceTab.Controls.Add(sourceBox);
            previewTab.Controls.Add(previewBox);
            rulesTab.Controls.Add(rulesGrid);

            // Add tab pages to tab control
            tabControl.TabPages.Add(sourceTab);
            tabControl.TabPages.Add(previewTab);
            tabControl.TabPages.Add(rulesTab);

            // Set default selected tab
            tabControl.SelectedIndex = 0;

            // ВАЖНО: Убедимся что rightSplit.Panel1 готов принять TabControl
            rightSplit.Panel1.Controls.Clear(); // Очищаем перед добавлением
            rightSplit.Panel1.Controls.Add(tabControl);
            
            // Принудительно обновляем
            tabControl.Refresh();
            rightSplit.Panel1.Refresh();
        }

        private void CreateBottomPanel()
        {
            // Create button panel FIRST - it will be at the bottom of results group
            buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                Padding = new Padding(8, 8, 8, 8),
                BackColor = SystemColors.Control
            };

            // Create buttons with consistent styling
            applyButton = new ModernButton
            {
                Text = "Apply Patches",
                Size = new Size(120, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                AccentColor = ColorTranslator.FromHtml("#0078d4"),
                Font = new Font("Segoe UI", 9F)
            };

            previewButton = new ModernButton
            {
                Text = "Preview",
                Size = new Size(90, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F)
            };

            cancelButton = new ModernButton
            {
                Text = "Cancel",
                Size = new Size(80, 32),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                Font = new Font("Segoe UI", 9F)
            };

            // Add buttons to button panel
            buttonPanel.Controls.Add(applyButton);
            buttonPanel.Controls.Add(previewButton);
            buttonPanel.Controls.Add(cancelButton);

            // Handle panel resize to keep buttons positioned correctly
            buttonPanel.Resize += (s, e) =>
            {
                var panel = s as Panel;
                if (panel == null) return;

                cancelButton.Location = new Point(panel.Width - cancelButton.Width - 12,
                                                (panel.Height - cancelButton.Height) / 2);
                previewButton.Location = new Point(cancelButton.Left - previewButton.Width - 8,
                                                 cancelButton.Top);
                applyButton.Location = new Point(previewButton.Left - applyButton.Width - 8,
                                               cancelButton.Top);
            };

            // Create results list
            resultsList = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                Font = new Font("Segoe UI", 9F)
            };

            // Create results group
            resultsGroup = new GroupBox
            {
                Text = "Patch Results",
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                Font = new Font("Segoe UI", 9F)
            };

            // Add controls to results group in CORRECT order (buttons FIRST as Dock.Bottom)
            resultsGroup.Controls.Add(buttonPanel);  // Add button panel FIRST
            resultsGroup.Controls.Add(resultsList);  // Then results list (will fill remaining space)

            // Add results group to right split panel 2
            rightSplit.Panel2.Controls.Add(resultsGroup);
        }

        private void SetInitialSplitterPositions()
        {
            // Отложенная установка позиций splitter-ов
            try
            {
                if (mainSplitter?.Width > 0 && mainSplitter.Width > 300)
                {
                    // Устанавливаем минимальные размеры только после инициализации
                    mainSplitter.Panel1MinSize = 200;
                    mainSplitter.Panel2MinSize = 300;

                    int targetWidth = Math.Max(220, Math.Min(350, (int)(mainSplitter.Width * 0.25)));
                    int maxDistance = mainSplitter.Width - mainSplitter.Panel2MinSize - mainSplitter.SplitterWidth;

                    if (targetWidth >= mainSplitter.Panel1MinSize && targetWidth <= maxDistance)
                    {
                        mainSplitter.SplitterDistance = targetWidth;
                    }
                }

                if (rightSplit?.Height > 0 && rightSplit.Height > 200)
                {
                    // Устанавливаем минимальные размеры для правого splitter
                    rightSplit.Panel1MinSize = 150;
                    rightSplit.Panel2MinSize = 120;

                    // 75% верх (TabControl), 25% низ (результаты)
                    int targetHeight = Math.Max(200, (int)(rightSplit.Height * 0.75));
                    int maxDistance = rightSplit.Height - rightSplit.Panel2MinSize - rightSplit.SplitterWidth;

                    if (targetHeight >= rightSplit.Panel1MinSize && targetHeight <= maxDistance)
                    {
                        rightSplit.SplitterDistance = targetHeight;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Splitter positioning error: {ex.Message}");
            }
        }

        private void SetSplitterPositions()
        {
            // Only adjust if window is large enough and not minimized
            if (WindowState == FormWindowState.Minimized) return;

            try
            {
                if (mainSplitter?.Width > 500)
                {
                    // Безопасная установка для главного splitter
                    if (mainSplitter.Panel1MinSize == 0) mainSplitter.Panel1MinSize = 200;
                    if (mainSplitter.Panel2MinSize == 0) mainSplitter.Panel2MinSize = 300;

                    int targetWidth = Math.Max(220, Math.Min(400, (int)(mainSplitter.Width * 0.25)));
                    int maxDistance = mainSplitter.Width - mainSplitter.Panel2MinSize - mainSplitter.SplitterWidth;

                    if (targetWidth >= mainSplitter.Panel1MinSize && targetWidth <= maxDistance)
                    {
                        mainSplitter.SplitterDistance = targetWidth;
                    }
                }

                if (rightSplit?.Height > 300)
                {
                    // Безопасная установка для правого splitter
                    if (rightSplit.Panel1MinSize == 0) rightSplit.Panel1MinSize = 150;
                    if (rightSplit.Panel2MinSize == 0) rightSplit.Panel2MinSize = 120;

                    int targetHeight = Math.Max(200, (int)(rightSplit.Height * 0.75));
                    int maxDistance = rightSplit.Height - rightSplit.Panel2MinSize - rightSplit.SplitterWidth;

                    if (targetHeight >= rightSplit.Panel1MinSize && targetHeight <= maxDistance)
                    {
                        rightSplit.SplitterDistance = targetHeight;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Splitter resize error: {ex.Message}");
            }
        }

        // Все остальные методы остаются без изменений...
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
                            item.Click += (s, e) =>
                            {
                                using var dlg = new SettingsForm(settings);
                                if (dlg.ShowDialog(this) == DialogResult.OK)
                                {
                                    showHiddenFiles = settings.ShowHiddenFiles;
                                    if (projectPath != null) _ = LoadProjectAsync(projectPath);
                                    settings.Save(SettingsFile);
                                }
                            };
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
            // Configure rules grid
            rulesGrid.Columns.Add("Rule", "Rule Name");
            rulesGrid.Columns.Add("Pattern", "Search Pattern");
            rulesGrid.Columns.Add("Replacement", "Replacement");

            // Make columns fill available space proportionally
            rulesGrid.Columns[0].FillWeight = 25;
            rulesGrid.Columns[1].FillWeight = 40;
            rulesGrid.Columns[2].FillWeight = 35;

            // Add sample data to make it visible
            rulesGrid.Rows.Add("Example Rule", "old_code", "new_code");

            // Configure results list
            resultsList.Columns.Add("File", 280);
            resultsList.Columns.Add("Status", 80);
            resultsList.Columns.Add("Changes", 80);
            resultsList.Columns.Add("Result", 150);
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

            // Ensure tab control is visible and properly initialized
            if (tabControl != null && tabControl.TabPages.Count > 0)
            {
                tabControl.SelectedIndex = 0;
                tabControl.Visible = true;
                if (sourceBox != null)
                    sourceBox.Text = "// Select a file from the project tree to edit";

                // Force refresh of TabControl
                tabControl.Refresh();
                tabControl.Update();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            // Only adjust splitters when the form is properly sized and not minimized
            if (WindowState != FormWindowState.Minimized && Width > 0 && Height > 0)
            {
                SetSplitterPositions();
            }
        }

        // Остальные методы (OnOpenProject, LoadProjectAsync, OnTreeAfterSelect, OnPreview, OnApplyPatches, и т.д.)
        // остаются без изменений из исходного кода...
        
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
                var rootNode = new TreeNode(rootDir.Name) { Tag = rootDir.FullName };
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
                var node = parent.Nodes.Add(sub.Name);
                node.Tag = sub.FullName;
                AddDirectoryNodes(sub, node);
            }
            foreach (var file in dir.GetFiles())
            {
                if (!showHiddenFiles && (file.Attributes & FileAttributes.Hidden) != 0)
                    continue;
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
            settings.ShowHiddenFiles = showHiddenFiles;
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            settings.ShowHiddenFiles = showHiddenFiles;
            settings.Save(SettingsFile);
            base.OnFormClosed(e);
        }
    }
}
