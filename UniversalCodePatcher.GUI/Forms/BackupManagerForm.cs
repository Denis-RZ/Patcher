using System;
using System.IO;
using System.Windows.Forms;

namespace UniversalCodePatcher.Forms
{
    public class BackupManagerForm : Form
    {
        private readonly ListView list = new() { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };
        private readonly string rootDir;
        private readonly Button restoreButton = new() { Text = "Restore", Width = 80 };
        private readonly Button deleteButton = new() { Text = "Delete", Width = 80 };
        private readonly Button closeButton = new() { Text = "Close", Width = 80 };

        public BackupManagerForm(string directory)
        {
            rootDir = directory;
            Text = "Backups";
            Width = 600;
            Height = 400;
            StartPosition = FormStartPosition.CenterParent;
            list.Columns.Add("File", 400);
            list.Columns.Add("Date", 150);
            Controls.Add(list);

            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(8),
                Height = 40
            };
            panel.Controls.Add(closeButton);
            panel.Controls.Add(deleteButton);
            panel.Controls.Add(restoreButton);
            Controls.Add(panel);

            LoadBackups();

            closeButton.Click += (_, __) => Close();
            restoreButton.Click += OnRestore;
            deleteButton.Click += OnDelete;
        }

        private void LoadBackups()
        {
            if (!Directory.Exists(rootDir)) return;
            foreach (var file in Directory.GetFiles(rootDir, "*.bak_*", SearchOption.AllDirectories))
            {
                var info = new FileInfo(file);
                var item = list.Items.Add(file);
                item.SubItems.Add(info.LastWriteTime.ToString());
            }
        }

        private void OnRestore(object? sender, EventArgs e)
        {
            if (list.SelectedItems.Count == 0) return;
            var path = list.SelectedItems[0].Text;
            var original = GetOriginalPath(path);
            try
            {
                File.Copy(path, original, true);
                MessageBox.Show($"Restored {original}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void OnDelete(object? sender, EventArgs e)
        {
            if (list.SelectedItems.Count == 0) return;
            var item = list.SelectedItems[0];
            var path = item.Text;
            try
            {
                File.Delete(path);
                list.Items.Remove(item);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private static string GetOriginalPath(string backupPath)
        {
            var idx = backupPath.LastIndexOf(".bak_");
            return idx > 0 ? backupPath.Substring(0, idx) : backupPath;
        }
    }
}
