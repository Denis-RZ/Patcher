using System.Drawing;
using System.Windows.Forms;

namespace UniversalCodePatcher.Forms
{
    partial class MainForm
    {
        private SplitContainer splitMain;
        private TreeView treeProjectFiles;
        private TabControl tabMain;
        private TabPage tabRules;
        private TabPage tabPreview;
        private TextBox txtLogOutput;
        private Button btnLoadProject;
        private Button btnScan;
        private Button btnApplyRules;
        private TextBox txtRuleName;
        private TextBox txtRulePattern;
        private TextBox txtRuleContent;
        private Button btnAddRule;
        private SplitContainer splitPreview;
        private RichTextBox rtbOriginal;
        private RichTextBox rtbPatched;
        private Panel topPanel;

        private void InitializeComponent()
        {
            this.topPanel = new Panel();
            this.btnLoadProject = new Button();
            this.btnScan = new Button();
            this.btnApplyRules = new Button();
            this.splitMain = new SplitContainer();
            this.treeProjectFiles = new TreeView();
            this.tabMain = new TabControl();
            this.tabRules = new TabPage();
            this.tabPreview = new TabPage();
            this.txtLogOutput = new TextBox();
            this.txtRuleName = new TextBox();
            this.txtRulePattern = new TextBox();
            this.txtRuleContent = new TextBox();
            this.btnAddRule = new Button();
            this.splitPreview = new SplitContainer();
            this.rtbOriginal = new RichTextBox();
            this.rtbPatched = new RichTextBox();
            this.topPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).BeginInit();
            this.splitMain.Panel1.SuspendLayout();
            this.splitMain.Panel2.SuspendLayout();
            this.splitMain.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabRules.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitPreview)).BeginInit();
            this.splitPreview.Panel1.SuspendLayout();
            this.splitPreview.Panel2.SuspendLayout();
            this.splitPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // topPanel
            // 
            this.topPanel.Controls.Add(this.btnLoadProject);
            this.topPanel.Controls.Add(this.btnScan);
            this.topPanel.Controls.Add(this.btnApplyRules);
            this.topPanel.Dock = DockStyle.Top;
            this.topPanel.Height = 30;
            // 
            // btnLoadProject
            // 
            this.btnLoadProject.Text = "Load Project";
            this.btnLoadProject.Width = 100;
            this.btnLoadProject.Left = 5;
            this.btnLoadProject.Click += new System.EventHandler(this.btnLoadProject_Click);
            // 
            // btnScan
            // 
            this.btnScan.Text = "Scan";
            this.btnScan.Width = 80;
            this.btnScan.Left = 110;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnApplyRules
            // 
            this.btnApplyRules.Text = "Apply";
            this.btnApplyRules.Width = 80;
            this.btnApplyRules.Left = 195;
            this.btnApplyRules.Click += new System.EventHandler(this.btnApplyRules_Click);
            // 
            // splitMain
            // 
            this.splitMain.Dock = DockStyle.Fill;
            this.splitMain.Panel1MinSize = 150;
            this.splitMain.Panel2MinSize = 300;
            // 
            // treeProjectFiles
            // 
            this.treeProjectFiles.Dock = DockStyle.Fill;
            // 
            // tabMain
            // 
            this.tabMain.Dock = DockStyle.Fill;
            this.tabMain.Controls.Add(this.tabRules);
            this.tabMain.Controls.Add(this.tabPreview);
            // 
            // tabRules
            // 
            this.tabRules.Text = "Rules";
            this.tabRules.Controls.Add(this.txtRuleName);
            this.tabRules.Controls.Add(this.txtRulePattern);
            this.tabRules.Controls.Add(this.txtRuleContent);
            this.tabRules.Controls.Add(this.btnAddRule);
            // 
            // txtRuleName
            // 
            this.txtRuleName.PlaceholderText = "Rule Name";
            this.txtRuleName.Top = 10;
            this.txtRuleName.Left = 10;
            this.txtRuleName.Width = 200;
            // 
            // txtRulePattern
            // 
            this.txtRulePattern.PlaceholderText = "Pattern";
            this.txtRulePattern.Top = 40;
            this.txtRulePattern.Left = 10;
            this.txtRulePattern.Width = 200;
            // 
            // txtRuleContent
            // 
            this.txtRuleContent.PlaceholderText = "Replacement";
            this.txtRuleContent.Top = 70;
            this.txtRuleContent.Left = 10;
            this.txtRuleContent.Width = 200;
            this.txtRuleContent.Height = 60;
            this.txtRuleContent.Multiline = true;
            // 
            // btnAddRule
            // 
            this.btnAddRule.Text = "Add";
            this.btnAddRule.Top = 140;
            this.btnAddRule.Left = 10;
            this.btnAddRule.Width = 80;
            // 
            // tabPreview
            // 
            this.tabPreview.Text = "Preview";
            this.tabPreview.Controls.Add(this.splitPreview);
            // 
            // splitPreview
            // 
            this.splitPreview.Dock = DockStyle.Fill;
            this.splitPreview.Panel1.Controls.Add(this.rtbOriginal);
            this.splitPreview.Panel2.Controls.Add(this.rtbPatched);
            // 
            // rtbOriginal
            // 
            this.rtbOriginal.Dock = DockStyle.Fill;
            this.rtbOriginal.WordWrap = false;
            // 
            // rtbPatched
            // 
            this.rtbPatched.Dock = DockStyle.Fill;
            this.rtbPatched.WordWrap = false;
            // 
            // txtLogOutput
            // 
            this.txtLogOutput.Dock = DockStyle.Bottom;
            this.txtLogOutput.Multiline = true;
            this.txtLogOutput.Height = 100;
            // 
            // splitMain Panels
            // 
            this.splitMain.Panel1.Controls.Add(this.treeProjectFiles);
            this.splitMain.Panel2.Controls.Add(this.tabMain);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.splitMain);
            this.Controls.Add(this.topPanel);
            this.Controls.Add(this.txtLogOutput);
            this.Text = "Universal Code Patcher";
            this.topPanel.ResumeLayout(false);
            this.splitMain.Panel1.ResumeLayout(false);
            this.splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitMain)).EndInit();
            this.splitMain.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabRules.ResumeLayout(false);
            this.tabRules.PerformLayout();
            this.tabPreview.ResumeLayout(false);
            this.splitPreview.Panel1.ResumeLayout(false);
            this.splitPreview.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitPreview)).EndInit();
            this.splitPreview.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
