using System;
using System.Drawing;
using System.Windows.Forms;

namespace UniversalCodePatcher.Forms
{
    public partial class MainForm : Form
    {
        private MenuStrip menuStrip = null!;
        private ToolStrip toolStrip = null!;
        private StatusStrip statusStrip = null!;
        private SplitContainer verticalSplit = null!;
        private SplitContainer horizontalSplit = null!;
        private Label projectFilesLabel = null!;
        private TreeView projectTree = null!;
        private TabControl tabControl = null!;
        private TabPage sourceTab = null!;
        private TabPage previewTab = null!;
        private TabPage rulesTab = null!;
        private RichTextBox sourceBox = null!;
        private RichTextBox previewBox = null!;
        private DataGridView rulesGrid = null!;
        private GroupBox resultsGroup = null!;
        private ListView resultsList = null!;
        private FlowLayoutPanel actionFlow = null!;
        private Button applyButton = null!;
        private Button previewButton = null!;
        private Button cancelButton = null!;
        private ToolStripStatusLabel statusLabel = null!;
        private ToolStripProgressBar progressBar = null!;
        private ToolStripStatusLabel infoLabel = null!;

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form settings
            ClientSize = new Size(1024, 768);
            MinimumSize = new Size(800, 600);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Universal Code Patcher";

            // MenuStrip
            menuStrip = new MenuStrip();
            var fileMenu = new ToolStripMenuItem("File");
            fileMenu.DropDownItems.Add("New Project");
            fileMenu.DropDownItems.Add("Open Project");
            fileMenu.DropDownItems.Add("Save");
            fileMenu.DropDownItems.Add("Recent");
            fileMenu.DropDownItems.Add(new ToolStripSeparator());
            fileMenu.DropDownItems.Add("Exit", null, (s, e) => Close());
            var editMenu = new ToolStripMenuItem("Edit");
            editMenu.DropDownItems.AddRange(new[]
            {
                new ToolStripMenuItem("Undo"),
                new ToolStripMenuItem("Redo"),
                new ToolStripSeparator(),
                new ToolStripMenuItem("Cut"),
                new ToolStripMenuItem("Copy"),
                new ToolStripMenuItem("Paste"),
                new ToolStripMenuItem("Find")
            });
            var viewMenu = new ToolStripMenuItem("View");
            viewMenu.DropDownItems.AddRange(new[]
            {
                new ToolStripMenuItem("Refresh"),
                new ToolStripMenuItem("Show All Files"),
                new ToolStripMenuItem("Expand Tree")
            });
            var toolsMenu = new ToolStripMenuItem("Tools");
            toolsMenu.DropDownItems.AddRange(new[]
            {
                new ToolStripMenuItem("Options"),
                new ToolStripMenuItem("Backup Manager"),
                new ToolStripMenuItem("Module Settings")
            });
            var helpMenu = new ToolStripMenuItem("Help");
            helpMenu.DropDownItems.AddRange(new[]
            {
                new ToolStripMenuItem("Documentation"),
                new ToolStripMenuItem("About")
            });
            menuStrip.Items.AddRange(new[] { fileMenu, editMenu, viewMenu, toolsMenu, helpMenu });
            menuStrip.Dock = DockStyle.Top;

            // ToolStrip
            toolStrip = new ToolStrip { Dock = DockStyle.Top, ImageScalingSize = new Size(16, 16), Height = 25 };
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "New"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Open"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Save"
            });
            toolStrip.Items.Add(new ToolStripSeparator());
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Patch"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Preview"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Undo"
            });
            toolStrip.Items.Add(new ToolStripButton
            {
                Image = SystemIcons.Application.ToBitmap(),
                ToolTipText = "Redo"
            });

            // StatusStrip
            statusStrip = new StatusStrip();
            statusLabel = new ToolStripStatusLabel("Ready");
            progressBar = new ToolStripProgressBar { Visible = false, AutoSize = false, Size = new Size(200, 16) };
            infoLabel = new ToolStripStatusLabel("Files: 0 | Modified: 0");
            statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar, infoLabel });
            statusStrip.Dock = DockStyle.Bottom;

            // Split containers
            verticalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 250,
                Panel1MinSize = 200
            };

            horizontalSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 400
            };

            // Panel1 of verticalSplit
            projectFilesLabel = new Label { Text = "Project Files", Dock = DockStyle.Top, Height = 20 };
            projectTree = new TreeView
            {
                Dock = DockStyle.Fill,
                CheckBoxes = true,
                HideSelection = false,
                ShowLines = true,
                Font = new Font("Segoe UI", 9F)
            };
            verticalSplit.Panel1.Controls.Add(projectTree);
            verticalSplit.Panel1.Controls.Add(projectFilesLabel);
            verticalSplit.Panel1.Controls.SetChildIndex(projectFilesLabel, 0);

            // Tab control for code and rules
            tabControl = new TabControl { Dock = DockStyle.Fill };
            sourceTab = new TabPage("Source Code");
            previewTab = new TabPage("Preview Changes");
            rulesTab = new TabPage("Patch Rules");
            sourceBox = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9F) };
            previewBox = new RichTextBox { Dock = DockStyle.Fill, ReadOnly = true, Font = new Font("Consolas", 9F) };
            rulesGrid = new DataGridView { Dock = DockStyle.Fill };
            sourceTab.Controls.Add(sourceBox);
            previewTab.Controls.Add(previewBox);
            rulesTab.Controls.Add(rulesGrid);
            tabControl.TabPages.AddRange(new[] { sourceTab, previewTab, rulesTab });
            horizontalSplit.Panel1.Controls.Add(tabControl);

            // Results group
            resultsGroup = new GroupBox { Text = "Patch Results", Dock = DockStyle.Fill };
            resultsList = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true };
            resultsGroup.Controls.Add(resultsList);
            horizontalSplit.Panel2.Controls.Add(resultsGroup);

            verticalSplit.Panel2.Controls.Add(horizontalSplit);

            // Bottom panel with buttons
            var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 40 };
            actionFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.RightToLeft
            };
            applyButton = new Button { Text = "Apply Patches", Size = new Size(90, 23), Margin = new Padding(3) };
            previewButton = new Button { Text = "Preview", Size = new Size(75, 23), Margin = new Padding(3) };
            cancelButton = new Button { Text = "Cancel", Size = new Size(75, 23), Margin = new Padding(3) };
            actionFlow.Controls.Add(applyButton);
            actionFlow.Controls.Add(previewButton);
            actionFlow.Controls.Add(cancelButton);
            bottomPanel.Controls.Add(actionFlow);
            verticalSplit.Panel2.Controls.Add(bottomPanel);
            verticalSplit.Panel2.Controls.SetChildIndex(bottomPanel, 0);

            // Add root controls
            Controls.Add(verticalSplit);
            Controls.Add(statusStrip);
            Controls.Add(toolStrip);
            Controls.Add(menuStrip);
            MainMenuStrip = menuStrip;

            ResumeLayout(false);
            PerformLayout();
        }
    }
}
