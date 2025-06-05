using System;
using System.Collections.Generic;
using System.Linq;
using UniversalCodePatcher.Interfaces;
using UniversalCodePatcher.DiffEngine;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Modules.DiffModule
{
    public class DiffModule : IDiffEngine
    {
        public string CreateUnifiedDiff(string original, string modified, string fileName)
        {
            // simplistic diff using existing GetDiff
            var diffLines = GetDiff(original, modified);
            return string.Join(System.Environment.NewLine, diffLines);
        }

        public string ApplyUnifiedDiff(string original, string diffContent)
        {
            // not implemented - return original
            return original;
        }

        public DiffPatch ParseDiff(string diffContent)
        {
            var parser = new UnifiedDiffParser();
            return parser.Parse(diffContent);
        }

        public FileDiff CreateFileDiff(string filePath, string originalContent, string modifiedContent)
        {
            return new FileDiff { OriginalPath = filePath, ModifiedPath = filePath, Status = DiffStatus.Modified };
        }

        public IEnumerable<FileDiff> CompareProjects(string originalPath, string modifiedPath)
        {
            return Enumerable.Empty<FileDiff>();
        }

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
