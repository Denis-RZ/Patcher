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
        public IList<PatchFailure> Failures { get; } = new List<PatchFailure>();
        public IList<string> SkippedFiles { get; } = new List<string>();
        public void Merge(PatchResult other)
        {
            foreach (var f in other.PatchedFiles) PatchedFiles.Add(f);
            foreach (var f in other.Failures) Failures.Add(f);
            foreach (var s in other.SkippedFiles) SkippedFiles.Add(s);
        }
    }

    public class PatchApplier
    {
        private readonly ILogger _logger;
        public PatchApplier(ILogger logger)
        {
            _logger = logger;
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
            var newline = originalLines.Any(l => l.EndsWith("\r")) ? "\r\n" : "\n";
            BackupFile(targetPath, backupFolder);
            for (int i = 0; i < diff.Hunks.Count; i++)
            {
                var hunk = diff.Hunks[i];
                var position = hunk.OriginalStartLine - 1;
                if (position < 0 || position > originalLines.Count)
                {
                    result.Failures.Add(new PatchFailure
                    {
                        FilePath = diff.OriginalFilePath,
                        HunkIndex = i,
                        ErrorMessage = "Context mismatch",
                    });
                    return result;
                }
                foreach (var line in hunk.Lines)
                {
                    if (line.Type == ChangeType.Context)
                    {
                        if (position >= originalLines.Count || originalLines[position] != line.Text)
                        {
                            result.Failures.Add(new PatchFailure { FilePath = diff.OriginalFilePath, HunkIndex = i, ErrorMessage = "Context mismatch" });
                            return result;
                        }
                        position++;
                    }
                    else if (line.Type == ChangeType.Remove)
                    {
                        if (position >= originalLines.Count || originalLines[position] != line.Text)
                        {
                            result.Failures.Add(new PatchFailure { FilePath = diff.OriginalFilePath, HunkIndex = i, ErrorMessage = "Context mismatch" });
                            return result;
                        }
                        originalLines.RemoveAt(position);
                    }
                    else if (line.Type == ChangeType.Add)
                    {
                        originalLines.Insert(position, line.Text);
                        position++;
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
    }
}
