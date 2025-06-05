using System;
using System.Collections.Generic;

namespace UniversalCodePatcher.Modules.DiffModule
{
    /// <summary>
    /// Simple line by line diff algorithm used as default.
    /// </summary>
    public class LineByLineDiffAlgorithm : IDiffAlgorithm
    {
        public IEnumerable<string> GetDiff(string oldText, string newText)
        {
            var oldLines = oldText.Split('\n');
            var newLines = newText.Split('\n');
            int max = Math.Max(oldLines.Length, newLines.Length);
            for (int i = 0; i < max; i++)
            {
                string oldLine = i < oldLines.Length ? oldLines[i] : string.Empty;
                string newLine = i < newLines.Length ? newLines[i] : string.Empty;
                if (oldLine != newLine)
                {
                    yield return $"- {oldLine}";
                    yield return $"+ {newLine}";
                }
            }
        }
    }
}
