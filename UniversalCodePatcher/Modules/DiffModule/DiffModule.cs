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
            return _algorithm.GetDiff(oldText, newText);
        }
    }
}
