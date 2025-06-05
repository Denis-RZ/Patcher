using System;
using System.IO;
using System.Windows.Forms;
using UniversalCodePatcher.Helpers;
using UniversalCodePatcher.UI.Panels;
using UniversalCodePatcher.Workflow;

namespace UniversalCodePatcher.Forms
{
    public partial class MainForm : Form
    {
        private string _currentProjectPath = string.Empty;
        private WorkflowEngine workflowEngine = null!;
        private RuleBuilderPanel ruleBuilder = null!;

        public MainForm()
        {
            InitializeComponent();
            InitializeNewWorkflow();
        }

        private void InitializeNewWorkflow()
        {
            workflowEngine = new WorkflowEngine();
            ruleBuilder = new RuleBuilderPanel();

            tabRules.Controls.Clear();
            ruleBuilder.Dock = DockStyle.Fill;
            tabRules.Controls.Add(ruleBuilder);
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

            var files = FileScanner.FindPatchableFiles(
                _currentProjectPath,
                new[] { ".cs", ".java", ".py" },
                new[] { "bin", "obj" });

            foreach (var file in files)
            {
                Log($"Found: {file}");
            }
        }

        private void btnApplyRules_Click(object sender, EventArgs e)
        {
            var rule = ruleBuilder.CreateRule();
            Log($"Created rule: {rule.Name}");
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

        private void MainForm_Resize(object sender, EventArgs e)
        {
            layoutManager.UpdateLayout(this);
        }
    }
}
