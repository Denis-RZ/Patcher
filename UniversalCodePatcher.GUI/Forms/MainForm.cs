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
                    diffStatusLabel.Text = $"\u2714 Valid diff - {patch.Files.Count} files";
                else
                    diffStatusLabel.Text = "\u274C Invalid diff";
            }
            catch
            {
                diffStatusLabel.Text = "\u274C Invalid diff";
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

                var result = await Task.Run(() =>
                    DiffApplier.ApplyDiff(tempDiffFile, folderBox.Text, backupRoot, dryRunCheckBox.Checked));

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
    }
}
