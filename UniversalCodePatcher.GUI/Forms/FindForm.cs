using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace UniversalCodePatcher.Forms
{
    public class FindForm : Form
    {
        private readonly TextBox searchBox = new();
        private readonly Button searchButton = new() { Text = "Search" };
        private readonly ListBox resultsList = new() { Dock = DockStyle.Fill };
        private readonly string rootPath;

        public event Action<string>? FileSelected;

        public FindForm(string root)
        {
            rootPath = root;
            Text = "Find in Files";
            Width = 600;
            Height = 400;
            StartPosition = FormStartPosition.CenterParent;

            var panel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 30 };
            panel.Controls.Add(searchBox);
            panel.Controls.Add(searchButton);
            searchBox.Width = 400;

            Controls.Add(resultsList);
            Controls.Add(panel);

            searchButton.Click += OnSearch;
            resultsList.DoubleClick += (s, e) =>
            {
                if (resultsList.SelectedItem is string file)
                    FileSelected?.Invoke(file);
            };
        }

        private void OnSearch(object? sender, EventArgs e)
        {
            resultsList.Items.Clear();
            if (string.IsNullOrWhiteSpace(searchBox.Text) || !Directory.Exists(rootPath))
                return;

            var files = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    var text = File.ReadAllText(file);
                    if (text.Contains(searchBox.Text, StringComparison.OrdinalIgnoreCase))
                        resultsList.Items.Add(file);
                }
                catch { }
            }
        }
    }
}
