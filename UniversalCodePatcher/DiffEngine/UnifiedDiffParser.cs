using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Models = UniversalCodePatcher.Models;

namespace UniversalCodePatcher.DiffEngine
{
    public class UnifiedDiffParser
    {
        private static readonly Regex HunkHeader = new(@"^@@ -(\d+),(\d+) \+(\d+),(\d+) @@ ?(.*)");

        public Models.DiffPatch Parse(string diffText)
        {
            var patch = new Models.DiffPatch();
            var lines = diffText.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            int i = 0;
            while (i < lines.Length)
            {
                if (lines[i].StartsWith("--- "))
                {
                    var fileDiff = new Models.FileDiff();
                    fileDiff.OriginalPath = lines[i].Substring(4).Trim();
                    i++;
                    if (i >= lines.Length) break;
                    if (lines[i].StartsWith("+++ "))
                    {
                        fileDiff.ModifiedPath = lines[i].Substring(4).Trim();
                        fileDiff.Status = Models.DiffStatus.Modified;
                        i++;
                    }
                    else
                    {
                        // malformed diff
                        break;
                    }

                    while (i < lines.Length && lines[i].StartsWith("@@"))
                    {
                        var match = HunkHeader.Match(lines[i]);
                        if (!match.Success) { i++; continue; }
                        var hunk = new Models.DiffHunk
                        {
                            OriginalStart = int.Parse(match.Groups[1].Value),
                            OriginalLength = int.Parse(match.Groups[2].Value),
                            ModifiedStart = int.Parse(match.Groups[3].Value),
                            ModifiedLength = int.Parse(match.Groups[4].Value),
                            Context = match.Groups[5].Value
                        };
                        i++;
                        while (i < lines.Length && !lines[i].StartsWith("@@") && !lines[i].StartsWith("--- "))
                        {
                            if (i >= lines.Length) break;
                            var line = lines[i];
                            if (line.Length == 0)
                            {
                                hunk.Lines.Add(new Models.DiffLine { Type = Models.DiffLineType.Context, Content = string.Empty });
                            }
                            else
                            {
                                char prefix = line[0];
                                string content = line.Length > 1 ? line.Substring(1) : string.Empty;
                                var type = prefix switch
                                {
                                    '+' => Models.DiffLineType.Added,
                                    '-' => Models.DiffLineType.Removed,
                                    ' ' => Models.DiffLineType.Context,
                                    _ => Models.DiffLineType.Context
                                };
                                hunk.Lines.Add(new Models.DiffLine { Type = type, Content = content });
                            }
                            i++;
                        }
                        fileDiff.Hunks.Add(hunk);
                    }
                    patch.Files.Add(fileDiff);
                }
                else
                {
                    i++;
                }
            }

            return patch;
        }
    }
}
