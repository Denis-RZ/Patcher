using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using UniversalCodePatcher.DiffEngine;
using UniversalCodePatcher;

namespace UniversalCodePatcher.Forms
{
    public class PatchForm : Form
    {
        private TextBox rootTextBox = new();
        private ListBox diffListBox = new();
        private TextBox backupTextBox = new();
        private CheckBox dryRunCheckBox = new() { Text = "Dry Run" };
        private Button startButton = new() { Text = "Start Patch" };
        private Button browseDiffButton = new() { Text = "Add Diff" };
        private Button browseRootButton = new() { Text = "Browse Root" };
        private Button browseBackupButton = new() { Text = "Browse Backup" };
        private RichTextBox logBox = new() { ReadOnly = true, Dock = DockStyle.Bottom, Height = 150 };
        private ProgressBar progress = new() { Dock = DockStyle.Bottom };
        private ILogger logger = new SimpleLogger();
        private CancellationTokenSource? patchCts;

        public PatchForm()
        {
            Text = "Apply Diff";
            Width = 600;
            Height = 400;
            var panel = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 5, ColumnCount = 3 };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));

            panel.Controls.Add(new Label { Text = "Root Folder" }, 0, 0);
            panel.Controls.Add(rootTextBox, 1, 0);
            panel.Controls.Add(browseRootButton, 2, 0);

            panel.Controls.Add(new Label { Text = "Diff Files" }, 0, 1);
            panel.Controls.Add(diffListBox, 1, 1);
            panel.Controls.Add(browseDiffButton, 2, 1);

            panel.Controls.Add(new Label { Text = "Backup Folder" }, 0, 2);
            panel.Controls.Add(backupTextBox, 1, 2);
            panel.Controls.Add(browseBackupButton, 2, 2);

            panel.Controls.Add(dryRunCheckBox, 1, 3);
            panel.Controls.Add(startButton, 2, 3);

            Controls.Add(panel);
            Controls.Add(progress);
            Controls.Add(logBox);

            logger.OnLogged += e => logBox.AppendText($"{e.Message}\n");

            browseRootButton.Click += (s, e) =>
            {
                using var dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                    rootTextBox.Text = dlg.SelectedPath;
            };
            browseDiffButton.Click += (s, e) =>
            {
                using var dlg = new OpenFileDialog { Filter = "Diff files|*.diff;*.patch", Multiselect = true };
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    diffListBox.Items.AddRange(dlg.FileNames);
                }
            };
            browseBackupButton.Click += (s, e) =>
            {
                using var dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                    backupTextBox.Text = dlg.SelectedPath;
            };

            startButton.Click += OnStart;
        }

        private async void OnStart(object? sender, EventArgs e)
        {
            var diffs = diffListBox.Items.Cast<string>().ToList();
            if (string.IsNullOrWhiteSpace(rootTextBox.Text) || diffs.Count == 0)
            {
                MessageBox.Show("Select root and diff");
                return;
            }
            string backup = backupTextBox.Text;
            if (string.IsNullOrWhiteSpace(backup))
                backup = Path.Combine(rootTextBox.Text, "patch_backups");
            Directory.CreateDirectory(backup);
            progress.Maximum = diffs.Count;
            progress.Value = 0;
            patchCts = new CancellationTokenSource();
            var token = patchCts.Token;
            try
            {
                foreach (var diff in diffs)
                {
                    await System.Threading.Tasks.Task.Run(() =>
                        DiffApplier.ApplyDiff(diff, rootTextBox.Text, backup, dryRunCheckBox.Checked, token), token);
                    progress.Value += 1;
                }
                logBox.AppendText("Done\n");
            }
            finally
            {
                patchCts = null;
            }
        }
    }
}
