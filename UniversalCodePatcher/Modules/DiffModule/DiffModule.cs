using System;
using System.Collections.Generic;
using System.Linq;
using UniversalCodePatcher.Interfaces;

namespace UniversalCodePatcher.Modules.DiffModule
{
    public class DiffModule : IDiffEngine
    {
        public IEnumerable<string> GetDiff(string oldText, string newText)
        {
            // Простейший line-by-line diff (можно заменить на более умный алгоритм)
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
