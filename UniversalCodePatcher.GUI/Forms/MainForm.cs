using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using UniversalCodePatcher.DiffEngine;
using UniversalCodePatcher.Controls;

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
        }

        private void OnApply(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(folderBox.Text))
            {
                MessageBox.Show("Select project folder");
                return;
            }
            string backup = Path.Combine(folderBox.Text, "patch_backups");

            string tempDiffFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempDiffFile, diffBox.Text);

                var result = DiffApplier.ApplyDiff(tempDiffFile, folderBox.Text, backup, false);

                logBox.AppendText($"Patched files: {result.PatchedFiles.Count}{Environment.NewLine}");
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Failed to apply patch: {ex.Message}");
            }
            finally
            {
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
