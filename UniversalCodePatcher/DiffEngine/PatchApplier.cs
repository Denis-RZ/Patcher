using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniversalCodePatcher.DiffEngine
{
    public class PatchFailure
    {
        public string FilePath { get; set; } = string.Empty;
        public int HunkIndex { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public IList<string> ExpectedContext { get; set; } = new List<string>();
        public IList<string> ActualContext { get; set; } = new List<string>();
    }

    public class PatchResult
    {
        public bool OverallSuccess => Failures.Count == 0;
        public IList<string> PatchedFiles { get; } = new List<string>();
        public IDictionary<string, string> RolledBackFiles { get; } = new Dictionary<string, string>();
        public IList<PatchFailure> Failures { get; } = new List<PatchFailure>();
        public IList<string> SkippedFiles { get; } = new List<string>();

        public void Merge(PatchResult other)
        {
            foreach (var f in other.PatchedFiles) PatchedFiles.Add(f);
            foreach (var kv in other.RolledBackFiles) RolledBackFiles[kv.Key] = kv.Value;
            foreach (var f in other.Failures) Failures.Add(f);
            foreach (var s in other.SkippedFiles) SkippedFiles.Add(s);
        }
    }

    public class PatchApplier
    {
        private readonly UniversalCodePatcher.ILogger _logger;
        public PatchApplier(UniversalCodePatcher.ILogger logger)
        {
            _logger = logger;
        }

        private static bool EqualsIgnoreWhitespace(string a, string b)
        {
            string sa = string.Concat(a.Where(c => !char.IsWhiteSpace(c)));
            string sb = string.Concat(b.Where(c => !char.IsWhiteSpace(c)));
            return sa == sb;
        }

        private static int Levenshtein(string s, string t)
        {
            int n = s.Length;
            int m = t.Length;
            int[,] d = new int[n + 1, m + 1];
            for (int i = 0; i <= n; i++) d[i, 0] = i;
            for (int j = 0; j <= m; j++) d[0, j] = j;
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = s[i - 1] == t[j - 1] ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[n, m];
        }

        private static double Similarity(string a, string b)
        {
            if (a.Length == 0 && b.Length == 0) return 1.0;
            int dist = Levenshtein(a, b);
            return 1.0 - (double)dist / Math.Max(a.Length, b.Length);
        }

        private int FindBestMatch(List<string> source, IList<string> context, int startIndex)
        {
            if (context.Count == 0)
                return startIndex;

            int maxPos = source.Count - context.Count;
            var exactMatches = new List<int>();

            for (int i = 0; i <= maxPos; i++)
            {
                bool exact = true;
                for (int j = 0; j < context.Count; j++)
                {
                    if (source[i + j] != context[j])
                    {
                        exact = false;
                        break;
                    }
                }
                if (exact)
                    exactMatches.Add(i);
            }

            if (exactMatches.Count == 1)
                return exactMatches[0];
            if (exactMatches.Count > 1)
            {
                _logger.LogWarning("Ambiguous context match, using closest index");
                return exactMatches.OrderBy(i => Math.Abs(i - startIndex)).First();
            }

            // whitespace tolerant search
            for (int i = 0; i <= maxPos; i++)
            {
                bool ok = true;
                for (int j = 0; j < context.Count; j++)
                {
                    if (!EqualsIgnoreWhitespace(source[i + j], context[j]))
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                    return i;
            }

            // fuzzy search
            double bestScore = 0.0;
            int bestIndex = -1;
            for (int i = 0; i <= maxPos; i++)
            {
                double score = 0;
                bool fail = false;
                for (int j = 0; j < context.Count; j++)
                {
                    var sim = Similarity(source[i + j].Trim(), context[j].Trim());
                    if (sim < 0.5)
                    {
                        fail = true;
                        break;
                    }
                    score += sim;
                }
                if (fail) continue;
                score /= context.Count;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }
            if (bestScore >= 0.8)
                return bestIndex;

            return -1;
        }

        private static bool AreLinesEquivalent(string a, string b)
        {
            if (a == b) return true;
            if (EqualsIgnoreWhitespace(a, b)) return true;
            return Similarity(a.Trim(), b.Trim()) >= 0.8;
        }

        public PatchResult Apply(DiffFile diff, string rootFolder, string backupFolder, bool dryRun)
        {
            var result = new PatchResult();
            var targetPath = Path.Combine(rootFolder, diff.OriginalFilePath);
            if (diff.IsBinary)
            {
                result.SkippedFiles.Add(diff.OriginalFilePath);
                _logger.LogWarning($"Binary file {diff.OriginalFilePath} skipped");
                return result;
            }
            if (diff.IsDeletedFile)
            {
                if (File.Exists(targetPath))
                {
                    BackupFile(targetPath, backupFolder);
                    if (!dryRun)
                        File.Delete(targetPath);
                    result.PatchedFiles.Add(diff.OriginalFilePath);
                }
                else
                {
                    result.SkippedFiles.Add(diff.OriginalFilePath);
                }
                return result;
            }
            if (!File.Exists(targetPath))
            {
                if (diff.IsNewFile)
                {
                    if (!dryRun)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath)!);
                        File.WriteAllLines(targetPath, diff.Hunks.SelectMany(h => h.Lines.Where(l => l.Type == ChangeType.Add).Select(l => l.Text)));
                    }
                    result.PatchedFiles.Add(diff.NewFilePath);
                    return result;
                }
                result.SkippedFiles.Add(diff.OriginalFilePath);
                return result;
            }

            var originalLines = File.ReadAllLines(targetPath).ToList();
            BackupFile(targetPath, backupFolder);
            int lineOffset = 0;
            for (int i = 0; i < diff.Hunks.Count; i++)
            {
                var hunk = diff.Hunks[i];
                var context = hunk.Lines.Where(l => l.Type != ChangeType.Add).Select(l => l.Text).ToList();
                int expected = hunk.OriginalStartLine - 1 + lineOffset;
                var position = FindBestMatch(originalLines, context, expected);
                if (position < 0)
                {
                    result.Failures.Add(new PatchFailure
                    {
                        FilePath = diff.OriginalFilePath,
                        HunkIndex = i,
                        ErrorMessage = "Context mismatch",
                        ExpectedContext = context,
                        ActualContext = originalLines.Skip(Math.Max(0, expected - 2)).Take(context.Count + 4).ToList()
                    });
                    return result;
                }

                int index = position;
                foreach (var line in hunk.Lines)
                {
                    if (line.Type == ChangeType.Context)
                    {
                        index++;
                    }
                    else if (line.Type == ChangeType.Remove)
                    {
                        if (index >= originalLines.Count || !AreLinesEquivalent(originalLines[index], line.Text))
                        {
                            result.Failures.Add(new PatchFailure { FilePath = diff.OriginalFilePath, HunkIndex = i, ErrorMessage = "Context mismatch" });
                            return result;
                        }
                        originalLines.RemoveAt(index);
                        lineOffset--;
                    }
                    else if (line.Type == ChangeType.Add)
                    {
                        originalLines.Insert(index, line.Text);
                        index++;
                        lineOffset++;
                    }
                }
            }
            if (!dryRun)
            {
                File.WriteAllLines(targetPath, originalLines.Select(l => l.Replace("\n", string.Empty)), System.Text.Encoding.UTF8);
            }
            result.PatchedFiles.Add(diff.OriginalFilePath);
            return result;
        }

        private void BackupFile(string path, string backupFolder)
        {
            Directory.CreateDirectory(backupFolder);
            var backupPath = Path.Combine(backupFolder, Path.GetFileName(path) + ".bak");
            File.Copy(path, backupPath, true);
        }

        public PatchResult ApplyDiff(string diffFilePath, string rootFolder, string backupFolder, bool dryRun)
        {
            var parser = new DiffParser();
            var diffFiles = parser.Parse(diffFilePath);
            var aggregate = new PatchResult();
            foreach (var df in diffFiles)
            {
                var r = Apply(df, rootFolder, backupFolder, dryRun);
                aggregate.Merge(r);
            }
            return aggregate;
        }

        public PatchResult ApplyDiffText(string diffText, string rootFolder, string backupFolder, bool dryRun)
        {
            var temp = Path.GetTempFileName();
            File.WriteAllText(temp, diffText);
            try
            {
                return ApplyDiff(temp, rootFolder, backupFolder, dryRun);
            }
            finally
            {
                try { File.Delete(temp); } catch { }
            }
        }
    }
}
