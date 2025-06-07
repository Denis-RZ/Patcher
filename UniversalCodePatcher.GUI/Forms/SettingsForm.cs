using System.Windows.Forms;

namespace UniversalCodePatcher.Forms
{
    public class SettingsForm : Form
    {
        public SettingsForm()
        {
            Text = "Settings";
            Width = 400;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;
            var tabs = new TabControl { Dock = DockStyle.Fill };
            tabs.TabPages.Add("General");
            tabs.TabPages.Add("Modules");
            tabs.TabPages.Add("Appearance");
            Controls.Add(tabs);
        }
    }
}
