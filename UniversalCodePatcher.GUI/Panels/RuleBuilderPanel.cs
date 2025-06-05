using System;
using System.Linq;
using System.Windows.Forms;
using UniversalCodePatcher.Controls;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.UI.Panels
{
    public class RuleBuilderPanel : UserControl
    {
        private ComboBox patchTypeCombo = default!;
        private CodeEditor patternEditor = default!;
        private CodeEditor contentEditor = default!;
        private ModernButton saveButton = default!;

        public RuleBuilderPanel()
        {
            InitializeComponent();
        }

        public PatchRule CreateRule()
        {
            return new PatchRule
            {
                Name = "User Rule",
                PatchType = (PatchType)patchTypeCombo.SelectedItem!,
                TargetPattern = patternEditor.Text,
                NewContent = contentEditor.Text
            };
        }

        private void InitializeComponent()
        {
            patchTypeCombo = new ComboBox();
            patchTypeCombo.Items.AddRange(Enum.GetValues(typeof(PatchType))
                .Cast<object>()
                .ToArray());

            patternEditor = new CodeEditor() { Multiline = true };
            contentEditor = new CodeEditor() { Multiline = true };
            saveButton = new ModernButton() { Text = "Save Rule" };

            Controls.Add(patchTypeCombo);
            Controls.Add(patternEditor);
            Controls.Add(contentEditor);
            Controls.Add(saveButton);
        }
    }
}
