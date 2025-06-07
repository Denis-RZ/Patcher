using System.Windows.Forms;
using System.Drawing;

namespace UniversalCodePatcher.Forms
{
    public class NewProjectForm : Form
    {
        private readonly TextBox nameBox = new();
        private readonly TextBox pathBox = new();
        public string ProjectName => nameBox.Text;
        public string ProjectPath => pathBox.Text;

        public NewProjectForm()
        {
            Text = "New Project";
            Width = 400;
            Height = 150;
            StartPosition = FormStartPosition.CenterParent;
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));

            var browseBtn = new Button { Text = "Browse" };
            var okBtn = new Button { Text = "OK", DialogResult = DialogResult.OK };
            var cancelBtn = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel };

            layout.Controls.Add(new Label { Text = "Name", TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            layout.Controls.Add(nameBox, 1, 0);
            layout.SetColumnSpan(nameBox, 2);
            layout.Controls.Add(new Label { Text = "Location", TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            layout.Controls.Add(pathBox, 1, 1);
            layout.Controls.Add(browseBtn, 2, 1);
            layout.Controls.Add(okBtn, 1, 2);
            layout.Controls.Add(cancelBtn, 2, 2);

            Controls.Add(layout);
            AcceptButton = okBtn;
            CancelButton = cancelBtn;

            browseBtn.Click += (s, e) =>
            {
                using var dlg = new FolderBrowserDialog();
                if (dlg.ShowDialog() == DialogResult.OK)
                    pathBox.Text = dlg.SelectedPath;
            };
        }
    }
}
