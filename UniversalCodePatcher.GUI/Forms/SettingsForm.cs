using System;
using System.Windows.Forms;
using UniversalCodePatcher.GUI.Models;

namespace UniversalCodePatcher.Forms
{
    public class SettingsForm : Form
    {
        private readonly AppSettings _settings;
        private readonly CheckBox _showHidden = new() { Dock = DockStyle.Top, Text = "Show hidden files" };
        private readonly ComboBox _themeBox = new() { Dock = DockStyle.Top, DropDownStyle = ComboBoxStyle.DropDownList };

        public AppSettings Settings => _settings;

 
 
        public SettingsForm() : this(new AppSettings()) { }

 
        public SettingsForm(AppSettings settings)
        {
            _settings = settings;

            Text = "Settings";
            Width = 400;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;

            var tabs = new TabControl { Dock = DockStyle.Fill };
            var general = new TabPage("General");
            var modules = new TabPage("Modules");
            var appearance = new TabPage("Appearance");

            tabs.TabPages.Add(general);
            tabs.TabPages.Add(modules);
            tabs.TabPages.Add(appearance);

            Controls.Add(tabs);

            // general tab
            _showHidden.Checked = _settings.ShowHiddenFiles;
            general.Controls.Add(_showHidden);

            // appearance tab
            _themeBox.Items.AddRange(new[] { "Default", "Light", "Dark" });
            _themeBox.SelectedItem = _settings.ThemeVariant;
            appearance.Controls.Add(_themeBox);

            var buttons = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.RightToLeft,
                Dock = DockStyle.Bottom,
                Padding = new Padding(8),
                Height = 40
            };

            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Width = 80 };
            var cancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Width = 80 };

            buttons.Controls.Add(cancel);
            buttons.Controls.Add(ok);

            Controls.Add(buttons);

            AcceptButton = ok;
            CancelButton = cancel;

            ok.Click += OnOk;
        }

 
        private void OnOk(object? sender, System.EventArgs e)
        {
            _settings.ShowHiddenFiles = _showHidden.Checked;
            _settings.ThemeVariant = _themeBox.SelectedItem?.ToString() ?? "Default";
        }
    }
}
