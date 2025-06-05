using System.Drawing;
using System.Windows.Forms;
using UniversalCodePatcher.Controls;

namespace UniversalCodePatcher.Forms
{
    partial class MainForm
    {
        private MenuStrip menuStrip = null!;
        private TableLayoutPanel layout = null!;

        private void InitializeComponent()
        {
            menuStrip = new MenuStrip();
            menuStrip.Items.Add("File");
            menuStrip.Items.Add("Patch");
            menuStrip.Items.Add("Help");

            diffBox.Multiline = true;
            diffBox.Font = new Font("Consolas", 10);
            diffBox.Dock = DockStyle.Fill;
            diffBox.PlaceholderText = "Paste your diff here...";

            folderBox.Dock = DockStyle.Fill;
            logBox.Multiline = true;
            logBox.Dock = DockStyle.Fill;
            logBox.Font = new Font("Consolas", 9);

            browseDiffButton.Click += OnBrowseDiff;
            applyButton.Click += OnApply;
            clearButton.Click += OnClear;
            browseFolderButton.Click += OnBrowseFolder;

            layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 5 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 15));

            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            layout.Controls.Add(diffBox, 0, 0);
            layout.SetColumnSpan(diffBox, 3);

            var ctrlPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            ctrlPanel.Controls.Add(browseDiffButton);
            ctrlPanel.Controls.Add(applyButton);
            ctrlPanel.Controls.Add(clearButton);
            layout.Controls.Add(ctrlPanel, 0, 1);
            layout.SetColumnSpan(ctrlPanel, 3);

            layout.Controls.Add(folderBox, 0, 2);
            layout.SetColumnSpan(folderBox, 2);
            layout.Controls.Add(browseFolderButton, 2, 2);

            layout.Controls.Add(logBox, 0, 3);
            layout.SetColumnSpan(logBox, 3);

            layout.Controls.Add(progress, 0, 4);
            layout.SetColumnSpan(progress, 3);

            ClientSize = new Size(800, 600);
            Controls.Add(layout);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;
            Text = "Universal Code Patcher";
        }
    }
}
