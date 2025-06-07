using System.Windows.Forms;
using System.Reflection;

namespace UniversalCodePatcher.Forms
{
    public class AboutForm : Form
    {
        public AboutForm()
        {
            Text = "About";
            Width = 300;
            Height = 200;
            StartPosition = FormStartPosition.CenterParent;
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0";
            var label = new Label
            {
                Text = $"Universal Code Patcher\nVersion {version}",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter
            };
            var ok = new Button { Text = "OK", Dock = DockStyle.Bottom, DialogResult = DialogResult.OK };
            Controls.Add(label);
            Controls.Add(ok);
            AcceptButton = ok;
        }
    }
}
