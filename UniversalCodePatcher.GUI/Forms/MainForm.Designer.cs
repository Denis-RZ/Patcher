using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using UniversalCodePatcher.Controls;

namespace UniversalCodePatcher.Forms
{
    partial class MainForm
    {
        private DockLayoutManager layoutManager;
        private ResizablePanel sidePanel;
        private Panel mainContentPanel;
        private TreeView treeProjectFiles;
        private TextBox txtLogOutput;
        private Button btnLoadProject;
        private Button btnScan;
        private Button btnApplyRules;
        private Panel topPanel;

        private void InitializeComponent()
        {
            this.topPanel = new Panel();
            this.btnLoadProject = new Button();
            this.btnScan = new Button();
            this.btnApplyRules = new Button();
            this.treeProjectFiles = new TreeView();
            this.txtLogOutput = new TextBox();

            // topPanel
            this.topPanel.Controls.Add(this.btnLoadProject);
            this.topPanel.Controls.Add(this.btnScan);
            this.topPanel.Controls.Add(this.btnApplyRules);
            this.topPanel.Dock = DockStyle.Top;
            this.topPanel.Height = 30;

            // btnLoadProject
            this.btnLoadProject.Text = "Load Project";
            this.btnLoadProject.Width = 100;
            this.btnLoadProject.Left = 5;
            this.btnLoadProject.Click += new System.EventHandler(this.btnLoadProject_Click);

            // btnScan
            this.btnScan.Text = "Scan";
            this.btnScan.Width = 80;
            this.btnScan.Left = 110;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);

            // btnApplyRules
            this.btnApplyRules.Text = "Apply";
            this.btnApplyRules.Width = 80;
            this.btnApplyRules.Left = 195;
            this.btnApplyRules.Click += new System.EventHandler(this.btnApplyRules_Click);

            // txtLogOutput
            this.txtLogOutput.Dock = DockStyle.Bottom;
            this.txtLogOutput.Multiline = true;
            this.txtLogOutput.Height = 120;

            // layout
            this.layoutManager = new DockLayoutManager();
            this.sidePanel = new ResizablePanel() { WidthPercentage = 0.3 };
            this.mainContentPanel = new Panel() { Dock = DockStyle.Fill };

            var dockableControls = new List<DockableControl>
            {
                new() { Control = sidePanel, Dock = DockStyle.Left, WidthPercentage = 0.3 },
                new() { Control = mainContentPanel, Dock = DockStyle.Fill }
            };

            this.layoutManager.ArrangeControls(this, dockableControls);

            // move existing controls
            this.sidePanel.Controls.Add(this.treeProjectFiles);

            // form
            this.ClientSize = new Size(800, 600);
            this.Controls.Add(this.txtLogOutput);
            this.Controls.Add(this.topPanel);
            this.Text = "Universal Code Patcher";
        }
    }
}
