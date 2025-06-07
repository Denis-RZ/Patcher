using System;
using System.Collections.Generic;
using System.Linq;
using UniversalCodePatcher.Interfaces;
using UniversalCodePatcher.DiffEngine;
using UniversalCodePatcher.Models;
using UniversalCodePatcher.Core;

namespace UniversalCodePatcher.Modules.DiffModule
{
    public class DiffModule : IDiffEngine
    {
        private readonly IServiceContainer _serviceContainer;
        private readonly IDiffAlgorithm _algorithm;

        public DiffModule(IServiceContainer? serviceContainer = null)
        {
            _serviceContainer = serviceContainer ?? new ServiceContainer();

            if (!_serviceContainer.IsRegistered<IDiffAlgorithm>())
            {
                _serviceContainer.RegisterSingleton<IDiffAlgorithm, LineByLineDiffAlgorithm>();
            }

            _algorithm = _serviceContainer.GetService<IDiffAlgorithm>();
        }
        public string CreateUnifiedDiff(string original, string modified, string fileName)
        {
            var diffLines = GetDiff(original, modified).ToList();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"--- {fileName}");
            sb.AppendLine($"+++ {fileName}");
            sb.AppendLine("@@ -1 +1 @@");
            foreach (var line in diffLines)
                sb.AppendLine(line);
            return sb.ToString();
        }

        public string ApplyUnifiedDiff(string original, string diffContent)
        {
            var lines = original.Split('\n').ToList();
            var diffLines = diffContent.Split(new[] {"\r\n","\n"}, StringSplitOptions.None)
                .Where(l => l.StartsWith("+") || l.StartsWith("-"))
                .ToList();
            int index = 0;
            foreach (var pair in diffLines.Chunk(2))
            {
                if (pair.Length == 2 && pair[0].StartsWith("-") && pair[1].StartsWith("+"))
                {
                    if (index < lines.Count)
                        lines[index] = pair[1].Substring(2);
                }
                index++;
            }
            return string.Join("\n", lines);
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
            return _algorithm.GetDiff(oldText, newText);
        }
    }
}
