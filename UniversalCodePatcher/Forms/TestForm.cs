using System;
using System.Windows.Forms;

namespace UniversalCodePatcher.Forms
{
    public class TestForm : Form
    {
        public TestForm()
        {
            Text = "TestForm";
            Size = new System.Drawing.Size(400, 200);
            StartPosition = FormStartPosition.CenterScreen;
            var label = new Label
            {
                Text = "Тестовая форма успешно загружена!",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                Font = new System.Drawing.Font("Segoe UI", 14, System.Drawing.FontStyle.Bold)
            };
            Controls.Add(label);
        }
    }
}
