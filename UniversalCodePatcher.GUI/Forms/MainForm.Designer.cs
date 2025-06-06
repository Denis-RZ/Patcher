using System.Drawing;
using System.Windows.Forms;
using UniversalCodePatcher.Controls;

namespace UniversalCodePatcher.Forms
{
    partial class MainForm
    {
        private MenuStrip menuStrip = null!;
        private Panel mainPanel = null!;
        private Panel inputCard = null!;
        private Panel targetCard = null!;
        private Panel actionCard = null!;
        private Panel resultsCard = null!;

        private void InitializeComponent()
        {
            menuStrip = new MenuStrip
            {
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White
            };

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
            diffStatusLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            diffStatusLabel.ForeColor = Color.FromArgb(179, 179, 179);

            previewButton.Text = "Preview";
            undoButton.Text = "Undo";
            undoButton.Enabled = false;
            backupCheckBox.Text = "Create backup";
            backupCheckBox.Checked = true;
            backupCheckBox.ForeColor = Color.FromArgb(179, 179, 179);
            backupCheckBox.Font = new Font("Segoe UI", 10F);
            dryRunCheckBox.Text = "Dry run";
            dryRunCheckBox.ForeColor = Color.FromArgb(179, 179, 179);
            dryRunCheckBox.Font = new Font("Segoe UI", 10F);

            diffBox.Multiline = true;
            diffBox.Font = new Font("Consolas", 10);
            diffBox.Dock = DockStyle.Fill;
            diffBox.BackColor = Color.FromArgb(40, 40, 40);
            diffBox.ForeColor = Color.White;
            diffBox.PlaceholderText = "Paste your diff here...";
            diffBox.TextChanged += diffBox_TextChanged;

            folderBox.Dock = DockStyle.Fill;
            folderBox.BackColor = Color.FromArgb(40, 40, 40);
            folderBox.ForeColor = Color.White;
            folderBox.Font = new Font("Segoe UI", 10F);
            logBox.Multiline = true;
            logBox.Dock = DockStyle.Fill;
            logBox.Font = new Font("Consolas", 9);
            logBox.BackColor = Color.FromArgb(40, 40, 40);
            logBox.ForeColor = Color.White;

            browseDiffButton.Click += OnBrowseDiff;
            applyButton.Click += OnApply;
            clearButton.Click += OnClear;
            browseFolderButton.Click += OnBrowseFolder;
            undoButton.Click += OnUndo;

            mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(30, 30, 30)
            };

            // Input card
            inputCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 180,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45),
                Margin = new Padding(0, 0, 0, 16)
            };
            var inputHeader = new Label
            {
                Text = "\uD83D\uDCC4 DIFF INPUT",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var inputLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            inputLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            inputLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            inputLayout.Controls.Add(diffBox, 0, 0);
            inputLayout.Controls.Add(diffStatusLabel, 0, 1);
            inputCard.Controls.Add(inputLayout);
            inputCard.Controls.Add(inputHeader);
            inputCard.Controls.SetChildIndex(inputHeader, 0);

            // Target card
            targetCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45),
                Margin = new Padding(0, 0, 0, 16)
            };
            var targetHeader = new Label
            {
                Text = "\uD83D\uDCC1 PROJECT FOLDER",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var targetLayout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2 };
            targetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            targetLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            targetLayout.Controls.Add(folderBox, 0, 0);
            targetLayout.Controls.Add(browseFolderButton, 1, 0);
            targetCard.Controls.Add(targetLayout);
            targetCard.Controls.Add(targetHeader);
            targetCard.Controls.SetChildIndex(targetHeader, 0);

            // Action card
            actionCard = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45),
                Margin = new Padding(0, 0, 0, 16)
            };
            var actionHeader = new Label
            {
                Text = "\u26A1 ACTIONS",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var actionLayout = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
            applyButton.Text = "\uD83D\uDE80 APPLY PATCH";
            applyButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            applyButton.Height = 40;
            previewButton.Text = "\uD83D\uDD0D Preview";
            previewButton.AccentColor = Color.FromArgb(63, 63, 70);
            undoButton.Text = "\u21A9\uFE0F Undo";
            undoButton.AccentColor = Color.FromArgb(63, 63, 70);
            actionLayout.Controls.Add(applyButton);
            actionLayout.Controls.Add(previewButton);
            actionLayout.Controls.Add(undoButton);
            actionLayout.Controls.Add(backupCheckBox);
            actionLayout.Controls.Add(dryRunCheckBox);
            actionCard.Controls.Add(actionLayout);
            actionCard.Controls.Add(actionHeader);
            actionCard.Controls.SetChildIndex(actionHeader, 0);

            // Results card
            resultsCard = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16),
                BackColor = Color.FromArgb(45, 45, 45)
            };
            var resultsHeader = new Label
            {
                Text = "\uD83D\uDCCB RESULTS",
                Dock = DockStyle.Top,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Padding = new Padding(0, 0, 0, 8)
            };
            var resultLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2 };
            resultLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            resultLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            resultLayout.Controls.Add(logBox, 0, 0);
            resultLayout.Controls.Add(progress, 0, 1);
            resultsCard.Controls.Add(resultLayout);
            resultsCard.Controls.Add(resultsHeader);
            resultsCard.Controls.SetChildIndex(resultsHeader, 0);

            mainPanel.Controls.Add(resultsCard);
            mainPanel.Controls.Add(actionCard);
            mainPanel.Controls.Add(targetCard);
            mainPanel.Controls.Add(inputCard);

            ClientSize = new Size(800, 600);
            Font = new Font("Segoe UI", 10F);
            ForeColor = Color.White;
            BackColor = Color.FromArgb(30, 30, 30);
            Controls.Add(mainPanel);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Text = "Universal Code Patcher";
        }
    }
}
