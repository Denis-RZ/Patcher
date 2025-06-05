using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.DiffEngine
{
    public static class DiffApplier
    {
        public static PatchResult ApplyDiff(string diffPath, string rootFolder, string backupFolder, bool dryRun)
        {
            var result = new PatchResult();
            var parser = new UnifiedDiffParser();
            var diffText = File.ReadAllText(diffPath);
            var patch = parser.Parse(diffText);
            var sessionFolder = Path.Combine(backupFolder, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            if (!dryRun)
                Directory.CreateDirectory(sessionFolder);

            foreach (var file in patch.Files)
            {
                var relative = file.ModifiedPath;
                if (relative.StartsWith("./"))
                    relative = relative.Substring(2);
                var targetPath = Path.Combine(rootFolder, relative.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(targetPath))
                {
                    result.SkippedFiles[targetPath] = "File not found";
                    continue;
                }

                var backupPath = Path.Combine(sessionFolder, file.ModifiedPath);
                if (!dryRun)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                    File.Copy(targetPath, backupPath, true);
                }

                var originalLines = File.ReadAllLines(targetPath).ToList();
                if (TryApply(file.Hunks, originalLines, out var patched))
                {
                    if (!dryRun)
                    {
                        var temp = targetPath + ".tmp";
                        File.WriteAllLines(temp, patched);
                        File.Copy(temp, targetPath, true);
                        File.Delete(temp);
                    }
                    result.PatchedFiles.Add(targetPath);
                }
                else
                {
                    if (!dryRun && File.Exists(backupPath))
                        File.Copy(backupPath, targetPath, true);
                    result.RolledBackFiles[targetPath] = "Hunk context mismatch";
                }
            }

            return result;
        }

        private static bool TryApply(IEnumerable<DiffHunk> hunks, List<string> originalLines, out List<string> newLines)
        {
            newLines = new List<string>();
            int currentIndex = 0;

            foreach (var hunk in hunks)
            {
                while (currentIndex < hunk.OriginalStart - 1 && currentIndex < originalLines.Count)
                {
                    newLines.Add(originalLines[currentIndex]);
                    currentIndex++;
                }

                foreach (var line in hunk.Lines)
                {
                    switch (line.Type)
                    {
                        case DiffLineType.Context:
                            if (currentIndex >= originalLines.Count || originalLines[currentIndex] != line.Content)
                                return false;
                            newLines.Add(originalLines[currentIndex]);
                            currentIndex++;
                            break;
                        case DiffLineType.Removed:
                            if (currentIndex >= originalLines.Count || originalLines[currentIndex] != line.Content)
                                return false;
                            currentIndex++;
                            break;
                        case DiffLineType.Added:
                            newLines.Add(line.Content);
                            break;
                    }
                }
            }

            while (currentIndex < originalLines.Count)
            {
                newLines.Add(originalLines[currentIndex]);
                currentIndex++;
            }

            return true;
        }

        public static IEnumerable<string> FindPatchableFiles(string rootFolder, IReadOnlyList<string> includeExtensions, IReadOnlyList<string> excludeFolderPatterns)
        {
            var extensions = new HashSet<string>(includeExtensions.Select(e => e.ToLowerInvariant()));
            foreach (var file in Directory.EnumerateFiles(rootFolder, "*", SearchOption.AllDirectories))
            {
                var directory = Path.GetDirectoryName(file);
                if (directory != null && excludeFolderPatterns.Any(p => directory.Split(Path.DirectorySeparatorChar).Contains(p)))
                    continue;
                if (extensions.Contains(Path.GetExtension(file).ToLowerInvariant()))
                    yield return file;
            }
        }
    }
}
