using System.Drawing;
using System.Windows.Forms;

namespace UniversalCodePatcher.Controls
{
    /// <summary>
    /// Simple code editor with syntax highlighting stub and line highlighting.
    /// </summary>
    public class CodeEditor : RichTextBox
    {
        public string Language { get; set; } = "csharp";
        public bool ShowLineNumbers { get; set; } = true;

        public CodeEditor()
        {
            Font = new Font("Consolas", 10);
        }

        public void SetSyntaxHighlighting(string language)
        {
            Language = language;
            // Placeholder for syntax highlighting logic
        }

        public void HighlightLine(int lineNumber, Color color)
        {
            if (lineNumber < 1) return;
            int index = GetFirstCharIndexFromLine(lineNumber - 1);
            if (index < 0) return;
            int lineLength = Lines[lineNumber - 1].Length;
            Select(index, lineLength);
            SelectionBackColor = color;
            DeselectAll();
        }
    }
}
