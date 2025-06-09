using System;
using System.Drawing;
using System.Windows.Forms;
using UniversalCodePatcher.Models;
using UniversalCodePatcher.Controls;

namespace UniversalCodePatcher.Forms
{
    public class ProgressForm : Form
    {
        private readonly ProgressBar progressBar = new() { Dock = DockStyle.Top };
        private readonly Label infoLabel = new() { Dock = DockStyle.Top, Height = 20 };
        private readonly ListBox logBox = new() { Dock = DockStyle.Fill };
        private readonly Button cancelButton = new() { Text = "Cancel", Dock = DockStyle.Bottom };

        public event Action? CancellationRequested;

        public ProgressForm()
        {
            Text = "Progress";
            Width = 400;
            Height = 300;
            StartPosition = FormStartPosition.CenterParent;

            Controls.Add(logBox);
            Controls.Add(cancelButton);
            Controls.Add(infoLabel);
            Controls.Add(progressBar);

            cancelButton.Click += (_, __) => CancellationRequested?.Invoke();
        }

        public void UpdateProgress(int value, string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => UpdateProgress(value, message)));
                return;
            }
            progressBar.Value = Math.Max(progressBar.Minimum, Math.Min(value, progressBar.Maximum));
            infoLabel.Text = message;
        }

        public void AddLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AddLog), message);
                return;
            }
            logBox.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
            logBox.TopIndex = logBox.Items.Count - 1;
        }
    }
}
