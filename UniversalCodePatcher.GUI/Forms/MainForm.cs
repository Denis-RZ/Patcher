using System;
using System.IO;
using System.Windows.Forms;

namespace UniversalCodePatcher.Forms
{
    public partial class MainForm : Form
    {
        private string _currentProjectPath = string.Empty;

        public MainForm()
        {
            InitializeComponent();
            // Ensure splitter positions are set after layout is calculated
            Load += MainForm_Load;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            // Center splitters once the form has been created
            if (splitMain.Width > 0)
                splitMain.SplitterDistance = splitMain.Width / 2;
            if (splitPreview.Width > 0)
                splitPreview.SplitterDistance = splitPreview.Width / 2;
        }

        private void btnLoadProject_Click(object sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _currentProjectPath = dlg.SelectedPath;
                LoadProjectTree(_currentProjectPath);
                Log($"Project loaded: {_currentProjectPath}");
            }
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentProjectPath))
            {
                MessageBox.Show("Select project first");
                return;
            }
            Log("Scanning files ... (not implemented)");
        }

        private void btnApplyRules_Click(object sender, EventArgs e)
        {
            Log("Applying rules ... (not implemented)");
        }

        private void LoadProjectTree(string path)
        {
            treeProjectFiles.Nodes.Clear();
            var root = new TreeNode(Path.GetFileName(path)) { Tag = path };
            BuildTree(path, root);
            treeProjectFiles.Nodes.Add(root);
            root.Expand();
        }

        private void BuildTree(string dir, TreeNode node)
        {
            foreach (var d in Directory.GetDirectories(dir))
            {
                var child = new TreeNode(Path.GetFileName(d)) { Tag = d };
                BuildTree(d, child);
                node.Nodes.Add(child);
            }
            foreach (var f in Directory.GetFiles(dir))
            {
                node.Nodes.Add(new TreeNode(Path.GetFileName(f)) { Tag = f });
            }
        }

        private void Log(string message)
        {
            txtLogOutput.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
        }
    }
}
