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
            var patchedFiles = new List<string>();
            var rolledBackFiles = new Dictionary<string, string>();
            var skippedFiles = new List<string>();

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
                    skippedFiles.Add(targetPath);
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
                    patchedFiles.Add(targetPath);
                }
                else
                {
                    if (!dryRun && File.Exists(backupPath))
                        File.Copy(backupPath, targetPath, true);
                    rolledBackFiles[targetPath] = "Hunk context mismatch";
                }
            }

            var result = new PatchResult();
            result.Metadata["PatchedFiles"] = patchedFiles;
            result.Metadata["RolledBackFiles"] = rolledBackFiles;
            result.Metadata["SkippedFiles"] = skippedFiles;
            result.Success = rolledBackFiles.Count == 0;
            return result;
        }

        private static bool TryApply(IEnumerable<DiffHunk> hunks, List<string> originalLines, out List<string> newLines)
        {
            newLines = new List<string>(originalLines);
            int lineOffset = 0;

            foreach (var hunk in hunks)
            {
                var context = hunk.Lines
                    .Where(l => l.Type != DiffLineType.Added)
                    .Select(l => l.Content)
                    .ToList();
                int expected = hunk.OriginalStart - 1 + lineOffset;
                int position = FindContextPosition(newLines, context, expected);
                if (position < 0)
                    return false;

                int index = position;
                foreach (var line in hunk.Lines)
                {
                    switch (line.Type)
                    {
                        case DiffLineType.Context:
                            index++;
                            break;
                        case DiffLineType.Removed:
                            if (index >= newLines.Count || !LinesMatch(newLines[index], line.Content))
                                return false;
                            newLines.RemoveAt(index);
                            lineOffset--;
                            break;
                        case DiffLineType.Added:
                            newLines.Insert(index, line.Content);
                            index++;
                            lineOffset++;
                            break;
                    }
                }
            }

            return true;
        }

        private static bool LinesMatch(string sourceLine, string contextLine)
        {
            if (sourceLine == contextLine)
                return true;
            return NormalizeWhitespace(sourceLine) == NormalizeWhitespace(contextLine);
        }

        private static string NormalizeWhitespace(string line)
        {
            return string.Concat(line.Where(c => !char.IsWhiteSpace(c)));
        }

        private static bool ContextMatches(List<string> sourceLines, IList<string> context, int position)
        {
            if (position < 0 || position + context.Count > sourceLines.Count)
                return false;
            for (int i = 0; i < context.Count; i++)
            {
                if (!LinesMatch(sourceLines[position + i], context[i]))
                    return false;
            }
            return true;
        }

        private static int FindContextPosition(List<string> sourceLines, IList<string> context, int expectedIndex)
        {
            if (context.Count == 0)
                return expectedIndex;

            int maxPos = sourceLines.Count - context.Count;
            if (maxPos < 0)
                return -1;

            int searchRadius = 3;
            for (int offset = 0; offset <= searchRadius; offset++)
            {
                int forward = expectedIndex + offset;
                if (forward <= maxPos && ContextMatches(sourceLines, context, forward))
                    return forward;
                int backward = expectedIndex - offset;
                if (offset != 0 && backward >= 0 && ContextMatches(sourceLines, context, backward))
                    return backward;
            }

            for (int pos = 0; pos <= maxPos; pos++)
            {
                if (ContextMatches(sourceLines, context, pos))
                    return pos;
            }

            return -1;
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
