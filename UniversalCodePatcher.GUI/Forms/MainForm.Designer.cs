using System.Drawing;
using System.Windows.Forms;

namespace UniversalCodePatcher.Forms
{
    partial class MainForm
    {
        private TableLayoutPanel tableMain;
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
        private TableLayoutPanel tablePreview;
        private RichTextBox rtbOriginal;
        private RichTextBox rtbPatched;
        private Panel topPanel;

        private void InitializeComponent()
        {
            this.topPanel = new Panel();
            this.btnLoadProject = new Button();
            this.btnScan = new Button();
            this.btnApplyRules = new Button();
            this.tableMain = new TableLayoutPanel();
            this.treeProjectFiles = new TreeView();
            this.tabMain = new TabControl();
            this.tabRules = new TabPage();
            this.tabPreview = new TabPage();
            this.txtLogOutput = new TextBox();
            this.txtRuleName = new TextBox();
            this.txtRulePattern = new TextBox();
            this.txtRuleContent = new TextBox();
            this.btnAddRule = new Button();
            this.tablePreview = new TableLayoutPanel();
            this.rtbOriginal = new RichTextBox();
            this.rtbPatched = new RichTextBox();
            this.topPanel.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.tabRules.SuspendLayout();
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
            // tableMain
            //
            this.tableMain.Dock = DockStyle.Fill;
            this.tableMain.ColumnCount = 2;
            this.tableMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            this.tableMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            this.tableMain.RowCount = 2;
            this.tableMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tableMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 120F));
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
            this.tabPreview.Controls.Add(this.tablePreview);
            //
            // tablePreview
            //
            this.tablePreview.Dock = DockStyle.Fill;
            this.tablePreview.ColumnCount = 2;
            this.tablePreview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tablePreview.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            this.tablePreview.RowCount = 1;
            this.tablePreview.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            this.tablePreview.Controls.Add(this.rtbOriginal, 0, 0);
            this.tablePreview.Controls.Add(this.rtbPatched, 1, 0);
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
            this.txtLogOutput.Dock = DockStyle.Fill;
            this.txtLogOutput.Multiline = true;
            this.txtLogOutput.Height = 120;
            // 
            // tableMain Controls
            //
            this.tableMain.Controls.Add(this.treeProjectFiles, 0, 0);
            this.tableMain.Controls.Add(this.tabMain, 1, 0);
            this.tableMain.Controls.Add(this.txtLogOutput, 0, 1);
            this.tableMain.SetColumnSpan(this.txtLogOutput, 2);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.tableMain);
            this.Controls.Add(this.topPanel);
            this.Text = "Universal Code Patcher";
            this.topPanel.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.tabRules.ResumeLayout(false);
            this.tabRules.PerformLayout();
            this.tabPreview.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
