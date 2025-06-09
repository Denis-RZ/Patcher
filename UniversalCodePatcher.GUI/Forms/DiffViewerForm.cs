using System;
using System.Drawing;
using System.Windows.Forms;
using UniversalCodePatcher.Models;
using UniversalCodePatcher.Controls;
using UniversalCodePatcher.DiffEngine;

namespace UniversalCodePatcher.Forms
{
    public class DiffViewerForm : Form
    {
        private readonly SplitContainer split = new() { Dock = DockStyle.Fill, Orientation = Orientation.Vertical };
        private readonly RichTextBox originalBox = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 9), ReadOnly = true, WordWrap = false };
        private readonly RichTextBox modifiedBox = new() { Dock = DockStyle.Fill, Font = new Font("Consolas", 9), ReadOnly = true, WordWrap = false };

        public DiffViewerForm()
        {
            Text = "Diff Viewer";
            Width = 800;
            Height = 600;
            StartPosition = FormStartPosition.CenterParent;

            split.Panel1.Controls.Add(originalBox);
            split.Panel2.Controls.Add(modifiedBox);
            Controls.Add(split);
        }

        public void LoadDiff(string diffText)
        {
            originalBox.Clear();
            modifiedBox.Clear();

            var parser = new UnifiedDiffParser();
            var patch = parser.Parse(diffText);
            foreach (var file in patch.Files)
            {
                originalBox.AppendText($"--- {file.OriginalPath}\n");
                modifiedBox.AppendText($"+++ {file.ModifiedPath}\n");
                foreach (var hunk in file.Hunks)
                {
                    originalBox.AppendText($"@@ -{hunk.OriginalStart},{hunk.OriginalLength} +{hunk.ModifiedStart},{hunk.ModifiedLength} @@\n");
                    modifiedBox.AppendText($"@@ -{hunk.OriginalStart},{hunk.OriginalLength} +{hunk.ModifiedStart},{hunk.ModifiedLength} @@\n");
                    foreach (var line in hunk.Lines)
                    {
                        switch (line.Type)
                        {
                            case DiffLineType.Context:
                                originalBox.AppendText(line.Content + "\n");
                                modifiedBox.AppendText(line.Content + "\n");
                                break;
                            case DiffLineType.Removed:
                                AppendColoredLine(originalBox, line.Content, Color.LightCoral);
                                modifiedBox.AppendText("\n");
                                break;
                            case DiffLineType.Added:
                                originalBox.AppendText("\n");
                                AppendColoredLine(modifiedBox, line.Content, Color.LightGreen);
                                break;
                        }
                    }
                }
            }
            HighlightChanges();
        }

        private void AppendColoredLine(RichTextBox box, string text, Color color)
        {
            int start = box.TextLength;
            box.AppendText(text + "\n");
            box.Select(start, text.Length);
            box.SelectionBackColor = color;
            box.SelectionLength = 0;
        }

        public void HighlightChanges()
        {
            // selection done during append, ensure caret at start
            originalBox.SelectionStart = 0;
            modifiedBox.SelectionStart = 0;
        }
    }
}
