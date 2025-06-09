using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UniversalCodePatcher.Models;
using UniversalCodePatcher.Controls;

namespace UniversalCodePatcher.Forms
{
    public class RuleEditorForm : Form
    {
        private readonly PatchRule _rule;
        private readonly TextBox nameBox = new();
        private readonly TextBox descriptionBox = new();
        private readonly ComboBox patchTypeCombo = new() { DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly TextBox patternBox = new();
        private readonly TextBox newContentBox = new() { Multiline = true, ScrollBars = ScrollBars.Both, Height = 120 };
        private readonly Button okButton = new() { Text = "OK", Width = 80 };
        private readonly Button cancelButton = new() { Text = "Cancel", Width = 80 };

        public PatchRule Rule => _rule;

        public RuleEditorForm(PatchRule rule = null)
        {
            _rule = rule ?? new PatchRule();
            InitializeComponent();
            LoadRuleToControls();
        }

        private void InitializeComponent()
        {
            Text = "Rule Editor";
            Width = 500;
            Height = 400;
            StartPosition = FormStartPosition.CenterParent;

            patchTypeCombo.Items.AddRange(Enum.GetValues(typeof(PatchType)).Cast<object>().ToArray());

            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5, Padding = new Padding(6) };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            layout.Controls.Add(new Label { Text = "Name", TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
            layout.Controls.Add(nameBox, 1, 0);

            layout.Controls.Add(new Label { Text = "Description", TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
            layout.Controls.Add(descriptionBox, 1, 1);

            layout.Controls.Add(new Label { Text = "Patch Type", TextAlign = ContentAlignment.MiddleLeft }, 0, 2);
            layout.Controls.Add(patchTypeCombo, 1, 2);

            layout.Controls.Add(new Label { Text = "Pattern", TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
            layout.Controls.Add(patternBox, 1, 3);

            layout.Controls.Add(new Label { Text = "New Content", TextAlign = ContentAlignment.MiddleLeft }, 0, 4);
            layout.Controls.Add(newContentBox, 1, 4);

            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Bottom, Padding = new Padding(8), Height = 40 };
            buttons.Controls.Add(cancelButton);
            buttons.Controls.Add(okButton);

            Controls.Add(layout);
            Controls.Add(buttons);

            AcceptButton = okButton;
            CancelButton = cancelButton;

            okButton.Click += OnOk;
            cancelButton.Click += (_, __) => Close();
        }

        private void OnOk(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameBox.Text))
            {
                MessageBox.Show("Name is required");
                return;
            }
            SaveControlsToRule();
            DialogResult = DialogResult.OK;
            Close();
        }

        public void LoadRuleToControls()
        {
            nameBox.Text = _rule.Name;
            descriptionBox.Text = _rule.Description;
            patchTypeCombo.SelectedItem = _rule.PatchType;
            patternBox.Text = _rule.TargetPattern;
            newContentBox.Text = _rule.NewContent;
        }

        public void SaveControlsToRule()
        {
            _rule.Name = nameBox.Text;
            _rule.Description = descriptionBox.Text;
            if (patchTypeCombo.SelectedItem is PatchType pt)
                _rule.PatchType = pt;
            _rule.TargetPattern = patternBox.Text;
            _rule.NewContent = newContentBox.Text;
            _rule.UpdatedAt = DateTime.Now;
        }
    }
}
