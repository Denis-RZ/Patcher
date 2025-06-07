using System.IO;
using System.Windows.Forms;

namespace UniversalCodePatcher.Forms
{
    public class BackupManagerForm : Form
    {
        private readonly ListView list = new() { Dock = DockStyle.Fill, View = View.Details };
        private readonly string rootDir;

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
            LoadBackups();
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
    }
}
