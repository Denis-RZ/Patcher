using System.Drawing;
using System.Windows.Forms;
using UniversalCodePatcher.Controls;

namespace UniversalCodePatcher.Forms
{
    partial class MainForm
    {
        private MenuStrip menuStrip = null!;
        private Panel mainPanel = null!;
        private GroupBox inputCard = null!;
        private GroupBox targetCard = null!;
        private GroupBox actionCard = null!;
        private GroupBox resultsCard = null!;

        private void InitializeComponent()
        {
            menuStrip = new MenuStrip();

            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("Open Diff...", null, OnBrowseDiff);
            fileMenu.DropDownItems.Add("Exit", null, OnExit);

            var patchMenu = new ToolStripMenuItem("Patch");
            patchMenu.DropDownItems.Add("Apply All", null, OnApply);
            patchMenu.DropDownItems.Add("Clear", null, OnClear);

            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.Add("About", null, (s, e) =>
                MessageBox.Show("Universal Code Patcher v1.0"));

            menuStrip.Items.Add(fileMenu);
            menuStrip.Items.Add(patchMenu);
            menuStrip.Items.Add(helpMenu);

            diffStatusLabel.AutoSize = true;
            previewButton.Text = "Preview";
            undoButton.Text = "Undo";
            undoButton.Enabled = false;
            backupCheckBox.Text = "Create backup";
            backupCheckBox.Checked = true;
            dryRunCheckBox.Text = "Dry run";

            diffBox.Multiline = true;
            diffBox.Font = new Font("Consolas", 10);
            diffBox.Dock = DockStyle.Fill;
            diffBox.PlaceholderText = "Paste your diff here...";
            diffBox.TextChanged += diffBox_TextChanged;

            folderBox.Dock = DockStyle.Fill;
            logBox.Multiline = true;
            logBox.Dock = DockStyle.Fill;
            logBox.Font = new Font("Consolas", 9);

            browseDiffButton.Click += OnBrowseDiff;
            applyButton.Click += OnApply;
            clearButton.Click += OnClear;
            browseFolderButton.Click += OnBrowseFolder;
            undoButton.Click += OnUndo;

            mainPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };

            // Input card
            inputCard = new GroupBox { Text = "\uD83D\uDCC4 Input Diff", Dock = DockStyle.Top, Height = 180 };
            var inputLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            inputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            inputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inputLayout.Controls.Add(diffBox, 0, 0);
            inputLayout.Controls.Add(diffStatusLabel, 0, 1);
            inputCard.Controls.Add(inputLayout);

            // Target card
            targetCard = new GroupBox { Text = "\uD83D\uDCC1 Target Project", Dock = DockStyle.Top, Height = 80 };
            var targetLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            targetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            targetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            targetLayout.Controls.Add(folderBox, 0, 0);
            targetLayout.Controls.Add(browseFolderButton, 1, 0);
            targetCard.Controls.Add(targetLayout);

            // Action card
            actionCard = new GroupBox { Text = "\u26A1 Apply Changes", Dock = DockStyle.Top, Height = 90 };
            var actionLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            applyButton.Text = "\uD83D\uDE80 APPLY PATCH";
            applyButton.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            applyButton.Height = 40;
            previewButton.Text = "\uD83D\uDD0D Preview";
            undoButton.Text = "\u21A9\uFE0F Undo";
            actionLayout.Controls.Add(applyButton);
            actionLayout.Controls.Add(previewButton);
            actionLayout.Controls.Add(undoButton);
            actionLayout.Controls.Add(backupCheckBox);
            actionLayout.Controls.Add(dryRunCheckBox);
            actionCard.Controls.Add(actionLayout);

            // Results card
            resultsCard = new GroupBox { Text = "\uD83D\uDCCB Results", Dock = DockStyle.Fill };
            var resultLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            resultLayout.Controls.Add(logBox, 0, 0);
            resultLayout.Controls.Add(progress, 0, 1);
            resultsCard.Controls.Add(resultLayout);

            mainPanel.Controls.Add(resultsCard);
            mainPanel.Controls.Add(actionCard);
            mainPanel.Controls.Add(targetCard);
            mainPanel.Controls.Add(inputCard);

            ClientSize = new Size(800, 600);
            Controls.Add(mainPanel);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Text = "Universal Code Patcher";
        }
    }
}
