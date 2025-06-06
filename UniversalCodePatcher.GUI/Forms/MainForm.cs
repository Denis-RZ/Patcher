using System;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using UniversalCodePatcher.DiffEngine;
using UniversalCodePatcher.Controls;
using UniversalCodePatcher.Models;
using UniversalCodePatcher.Helpers;
using UniversalCodePatcher.UI.Panels;

namespace UniversalCodePatcher.Forms
{
    public partial class MainForm : Form
    {
        private readonly TextBox diffBox = new();
        private readonly TextBox folderBox = new();
        private readonly TextBox logBox = new() { ReadOnly = true };
        private readonly ProgressBar progress = new() { Dock = DockStyle.Bottom };
        private readonly ModernButton browseDiffButton = new() { Text = "Browse" };
        private readonly ModernButton applyButton = new() { Text = "Apply" };
        private readonly ModernButton clearButton = new() { Text = "Clear" };
        private readonly ModernButton browseFolderButton = new() { Text = "Browse Folder" };
        private readonly ModernButton previewButton = new();
        private readonly ModernButton undoButton = new();
        private readonly CheckBox backupCheckBox = new();
        private readonly CheckBox dryRunCheckBox = new();
        private readonly Label diffStatusLabel = new();
        // main window containers
        private MenuStrip menuStrip = null!;
        private ToolStrip toolStrip = null!;
        private StatusStrip statusStrip = null!;
        private SplitContainer mainSplit = null!;
        private Panel navigationPanel = null!;
        private TabControl tabControl = null!;
        private TabPage diffPage = null!;
        private TabPage targetPage = null!;
        private TabPage filesPage = null!;
        private TabPage rulesPage = null!;
        private TabPage resultsPage = null!;
        private Panel inputCard = null!;
        private Panel targetCard = null!;
        private Panel actionCard = null!;
        private Panel resultsCard = null!;
        private Panel scanCard = null!;
        private Panel ruleCard = null!;
        private DataGridView filesGrid = null!;
        private TextBox searchBox = null!;
        private ModernButton scanButton = null!;
        private RuleBuilderPanel ruleBuilder = null!;
        private ListBox ruleList = null!;
        private ModernButton addRuleButton = null!;
        private readonly List<PatchRule> rules = new();
        private string? lastBackupDir;
        private CancellationTokenSource? applyCts;
        private bool isApplying;

        public MainForm()
        {
            InitializeComponent();
        }

        private void OnBrowseDiff(object? sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog { Filter = "Diff files|*.diff;*.patch" };
            if (dlg.ShowDialog() == DialogResult.OK)
                diffBox.Text = File.ReadAllText(dlg.FileName);
        }

        private void OnBrowseFolder(object? sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
                folderBox.Text = dlg.SelectedPath;
        }

        private void OnClear(object? sender, EventArgs e)
        {
            diffBox.Clear();
            logBox.Clear();
            diffStatusLabel.Text = string.Empty;
        }

        private void diffBox_TextChanged(object? sender, EventArgs e)
        {
            var parser = new UnifiedDiffParser();
            try
            {
                var patch = parser.Parse(diffBox.Text);
                if (patch.Files.Count > 0)
                {
                    diffStatusLabel.Text = $"\u2714 Valid diff - {patch.Files.Count} files";
                    diffStatusLabel.ForeColor = Color.FromArgb(22, 198, 12);
                }
                else
                {
                    diffStatusLabel.Text = "\u274C Invalid diff";
                    diffStatusLabel.ForeColor = Color.FromArgb(231, 72, 86);
                }
            }
            catch
            {
                diffStatusLabel.Text = "\u274C Invalid diff";
                diffStatusLabel.ForeColor = Color.FromArgb(231, 72, 86);
            }
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(diffBox.Text))
            {
                MessageBox.Show("Paste diff text");
                return false;
            }
            if (string.IsNullOrWhiteSpace(folderBox.Text) || !Directory.Exists(folderBox.Text))
            {
                MessageBox.Show("Select project folder");
                return false;
            }
            var parser = new UnifiedDiffParser();
            if (parser.Parse(diffBox.Text).Files.Count == 0)
            {
                MessageBox.Show("Invalid diff format");
                return false;
            }
            return true;
        }

        private void OnUndo(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(lastBackupDir) || !Directory.Exists(lastBackupDir))
            {
                MessageBox.Show("No backup available");
                return;
            }

            foreach (var file in Directory.GetFiles(lastBackupDir, "*", SearchOption.AllDirectories))
            {
                var relative = file.Substring(lastBackupDir.Length + 1);
                var dest = Path.Combine(folderBox.Text, relative);
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                File.Copy(file, dest, true);
            }
            logBox.AppendText($"Undo from {lastBackupDir}{Environment.NewLine}");
        }

        private void OnScan(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(folderBox.Text) || !Directory.Exists(folderBox.Text))
            {
                MessageBox.Show("Select project folder");
                return;
            }

            var files = FileScanner.FindPatchableFiles(folderBox.Text,
                    new[] { ".cs", ".js" }, new[] { "bin", "obj" })
                .ToList();

            var filtered = string.IsNullOrWhiteSpace(searchBox.Text)
                ? files
                : files.Where(f => f.Contains(searchBox.Text, StringComparison.OrdinalIgnoreCase)).ToList();

            filesGrid.DataSource = filtered.Select(f => new { File = f }).ToList();
        }

        private void OnAddRule(object? sender, EventArgs e)
        {
            var rule = ruleBuilder.CreateRule();
            rules.Add(rule);
            ruleList.Items.Add(rule.Name);
        }

        private void OnPreview(object? sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;
            MessageBox.Show("Preview not implemented");
        }

        private async void OnApply(object? sender, EventArgs e)
        {
            if (isApplying)
            {
                applyCts?.Cancel();
                return;
            }

            if (!ValidateInputs())
                return;

            isApplying = true;
            applyButton.Text = "Cancel";
            progress.Style = ProgressBarStyle.Marquee;
            progress.MarqueeAnimationSpeed = 30;

            string backupRoot = Path.Combine(folderBox.Text, "patch_backups");
            Directory.CreateDirectory(backupRoot);

            string tempDiffFile = Path.GetTempFileName();
            applyCts = new CancellationTokenSource();
            try
            {
                File.WriteAllText(tempDiffFile, diffBox.Text);

                var token = applyCts.Token;
                var result = await Task.Run(() =>
                    DiffApplier.ApplyDiff(tempDiffFile, folderBox.Text, backupRoot, dryRunCheckBox.Checked, token), token);

                result.Metadata.TryGetValue("PatchedFiles", out var patchedObj);
                var modified = patchedObj as List<string> ?? new List<string>();
                logBox.AppendText($"Modified: {string.Join(", ", modified)}{Environment.NewLine}");
 
                if (!applyCts.IsCancellationRequested)
                {
                    var dirs = Directory.GetDirectories(backupRoot);
                    if (dirs.Length > 0)
                        lastBackupDir = dirs.OrderByDescending(d => d).First();
                    undoButton.Enabled = lastBackupDir != null;
                }
                else
                {
                    logBox.AppendText($"Operation canceled{Environment.NewLine}");
                }
            }
            catch (OperationCanceledException)
            {
                logBox.AppendText($"Operation canceled{Environment.NewLine}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to apply patch: {ex.Message}");
            }
            finally
            {
                isApplying = false;
                applyCts = null;
                applyButton.Text = "Apply";
                progress.Style = ProgressBarStyle.Blocks;
                try
                {
                    if (File.Exists(tempDiffFile))
                        File.Delete(tempDiffFile);
                }
                catch
                {
                }
            }
        }

        private void OnExit(object? sender, EventArgs e)
        {
            Close();
        }

#region Designer
        private void InitializeComponent()
        {
            SuspendLayout();

            var headerFont = new Font("Segoe UI", 12F, FontStyle.Bold);

            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White
            };

            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Open Diff...", null, OnBrowseDiff);
            fileMenu.DropDownItems.Add("Exit", null, OnExit);

            var patchMenu = new ToolStripMenuItem("Patch");
            patchMenu.DropDownItems.Add("Apply All", null, OnApply);
            patchMenu.DropDownItems.Add("Clear", null, OnClear);

            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("About", null, (s, e) =>
                MessageBox.Show("Universal Code Patcher v1.0"));

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(patchMenu);
            menuStrip.Items.Add(helpMenu);

            toolStrip = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White
            };
            toolStrip.Items.Add(new ToolStripButton("Browse Diff", null, OnBrowseDiff));
            toolStrip.Items.Add(new ToolStripButton("Apply", null, OnApply));
            toolStrip.Items.Add(new ToolStripButton("Clear", null, OnClear));

            statusStrip = new StatusStrip
            {
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White
            };
            statusStrip.Items.Add(new ToolStripStatusLabel("Ready"));

            mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterDistance = 200,
                SplitterWidth = 5,
                FixedPanel = FixedPanel.Panel1,
                BackColor = Color.FromArgb(30, 30, 30)
            };

            navigationPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45),
                Margin = new Padding(0, 0, 16, 0)
            };
            var navHeader = new Label
            {
                Text = "\uD83D\uDCC1 PROJECTS",
                Dock = DockStyle.Top,
                Font = headerFont,
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var projectList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };
            navigationPanel.Controls.Add(projectList);
            navigationPanel.Controls.Add(navHeader);
            navigationPanel.Controls.SetChildIndex(navHeader, 0);

            tabControl = new TabControl { Dock = DockStyle.Fill };
            diffPage = new TabPage("Diff") { BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
            targetPage = new TabPage("Project") { BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
            filesPage = new TabPage("Files") { BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
            rulesPage = new TabPage("Rules") { BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };
            resultsPage = new TabPage("Results") { BackColor = Color.FromArgb(45, 45, 45), ForeColor = Color.White };

            // input card
            inputCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 180,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45),
                Margin = new Padding(0, 0, 0, 16)
            };
            var inputHeader = new Label
            {
                Text = "\uD83D\uDCC4 DIFF INPUT",
                Dock = DockStyle.Top,
                Font = headerFont,
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var inputLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            inputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            inputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            diffBox.Multiline = true;
            diffBox.Font = new Font("Consolas", 10);
            diffBox.Dock = DockStyle.Fill;
            diffBox.BackColor = Color.FromArgb(40, 40, 40);
            diffBox.ForeColor = Color.White;
            diffBox.PlaceholderText = "Paste your diff here...";
            diffBox.TextChanged += diffBox_TextChanged;

            diffStatusLabel.AutoSize = true;
            diffStatusLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            diffStatusLabel.ForeColor = Color.FromArgb(179, 179, 179);

            inputLayout.Controls.Add(diffBox, 0, 0);
            inputLayout.Controls.Add(diffStatusLabel, 0, 1);
            inputCard.Controls.Add(inputLayout);
            inputCard.Controls.Add(inputHeader);
            inputCard.Controls.SetChildIndex(inputHeader, 0);

            // target card
            targetCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45),
                Margin = new Padding(0, 0, 0, 16)
            };
            var targetHeader = new Label
            {
                Text = "\uD83D\uDCC1 PROJECT FOLDER",
                Dock = DockStyle.Top,
                Font = headerFont,
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var targetLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            targetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            targetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            folderBox.Dock = DockStyle.Fill;
            folderBox.BackColor = Color.FromArgb(40, 40, 40);
            folderBox.ForeColor = Color.White;
            folderBox.Font = new Font("Segoe UI", 10F);
            targetLayout.Controls.Add(folderBox, 0, 0);
            targetLayout.Controls.Add(browseFolderButton, 1, 0);
            targetCard.Controls.Add(targetLayout);
            targetCard.Controls.Add(targetHeader);
            targetCard.Controls.SetChildIndex(targetHeader, 0);

            // scan card
            scanCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 200,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45),
                Margin = new Padding(0, 0, 0, 16)
            };
            var scanHeader = new Label
            {
                Text = "\uD83D\uDD0D FILES",
                Dock = DockStyle.Top,
                Font = headerFont,
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var scanLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            scanLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            scanLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            var scanTop = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
            searchBox = new TextBox { Width = 200 };
            searchBox.PlaceholderText = "Search...";
            scanButton = new ModernButton { Text = "Scan", Height = 40 };
            scanTop.Controls.Add(searchBox);
            scanTop.Controls.Add(scanButton);
            filesGrid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true };
            scanLayout.Controls.Add(scanTop, 0, 0);
            scanLayout.Controls.Add(filesGrid, 0, 1);
            scanCard.Controls.Add(scanLayout);
            scanCard.Controls.Add(scanHeader);
            scanCard.Controls.SetChildIndex(scanHeader, 0);

            // rule card
            ruleCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 220,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45),
                Margin = new Padding(0, 0, 0, 16)
            };
            var ruleHeader = new Label
            {
                Text = "\u270D\uFE0F PATCH RULE",
                Dock = DockStyle.Top,
                Font = headerFont,
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var ruleLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            ruleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            ruleLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            ruleList = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White
            };
            var builderLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            builderLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            builderLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            ruleBuilder = new RuleBuilderPanel { Dock = DockStyle.Fill };
            addRuleButton = new ModernButton { Text = "Add Rule", Height = 40 };
            builderLayout.Controls.Add(ruleBuilder, 0, 0);
            builderLayout.Controls.Add(addRuleButton, 0, 1);
            ruleLayout.Controls.Add(ruleList, 0, 0);
            ruleLayout.Controls.Add(builderLayout, 1, 0);
            ruleCard.Controls.Add(ruleLayout);
            ruleCard.Controls.Add(ruleHeader);
            ruleCard.Controls.SetChildIndex(ruleHeader, 0);

            // action card
            actionCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45),
                Margin = new Padding(0, 0, 0, 16)
            };
            var actionHeader = new Label
            {
                Text = "\u26A1 ACTIONS",
                Dock = DockStyle.Top,
                Font = headerFont,
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var actionLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            applyButton.Text = "\uD83D\uDE80 APPLY PATCH";
            applyButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            applyButton.Height = 40;
            previewButton.Text = "\uD83D\uDD0D Preview";
            previewButton.AccentColor = Color.FromArgb(63, 63, 70);
            undoButton.Text = "\u21A9\uFE0F Undo";
            undoButton.AccentColor = Color.FromArgb(63, 63, 70);
            clearButton.Text = "Clear";
            actionLayout.Controls.Add(applyButton);
            actionLayout.Controls.Add(previewButton);
            actionLayout.Controls.Add(undoButton);
            actionLayout.Controls.Add(clearButton);
            actionLayout.Controls.Add(backupCheckBox);
            actionLayout.Controls.Add(dryRunCheckBox);
            backupCheckBox.Text = "Create backup";
            backupCheckBox.Checked = true;
            backupCheckBox.ForeColor = Color.FromArgb(179, 179, 179);
            backupCheckBox.Font = new Font("Segoe UI", 10F);
            dryRunCheckBox.Text = "Dry run";
            dryRunCheckBox.ForeColor = Color.FromArgb(179, 179, 179);
            dryRunCheckBox.Font = new Font("Segoe UI", 10F);
            actionCard.Controls.Add(actionLayout);
            actionCard.Controls.Add(actionHeader);
            actionCard.Controls.SetChildIndex(actionHeader, 0);

            // results card
            resultsCard = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45)
            };
            var resultsHeader = new Label
            {
                Text = "\uD83D\uDCCB RESULTS",
                Dock = DockStyle.Top,
                Font = headerFont,
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var resultLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            logBox.Multiline = true;
            logBox.Dock = DockStyle.Fill;
            logBox.Font = new Font("Consolas", 9);
            logBox.BackColor = Color.FromArgb(40, 40, 40);
            logBox.ForeColor = Color.White;
            resultLayout.Controls.Add(logBox, 0, 0);
            resultLayout.Controls.Add(progress, 0, 1);
            resultsCard.Controls.Add(resultLayout);
            resultsCard.Controls.Add(resultsHeader);
            resultsCard.Controls.SetChildIndex(resultsHeader, 0);

            diffPage.Controls.Add(inputCard);
            targetPage.Controls.Add(targetCard);
            filesPage.Controls.Add(scanCard);
            rulesPage.Controls.Add(ruleCard);
            resultsPage.Controls.Add(actionCard);
            resultsPage.Controls.Add(resultsCard);
            resultsPage.Controls.SetChildIndex(actionCard, 0);

            tabControl.TabPages.AddRange(new[] { diffPage, targetPage, filesPage, rulesPage, resultsPage });

            mainSplit.Panel1.Controls.Add(navigationPanel);
            mainSplit.Panel2.Controls.Add(tabControl);

            browseDiffButton.Click += OnBrowseDiff;
            applyButton.Click += OnApply;
            clearButton.Click += OnClear;
            browseFolderButton.Click += OnBrowseFolder;
            undoButton.Click += OnUndo;
            scanButton.Click += OnScan;
            addRuleButton.Click += OnAddRule;
            previewButton.Click += OnPreview;

            ClientSize = new Size(800, 600);
            Font = new Font("Segoe UI", 10F);
            ForeColor = Color.White;
            BackColor = Color.FromArgb(30, 30, 30);
            StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(mainSplit);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Text = "Universal Code Patcher";

            ResumeLayout(false);
        }
#endregion
    }
}
