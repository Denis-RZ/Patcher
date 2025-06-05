using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniversalCodePatcher.DiffEngine
{
    public class DiffParseException : Exception
    {
        public DiffParseException(string message) : base(message) { }
    }

    public enum ChangeType
    {
        Context,
        Add,
        Remove
    }

    public class DiffLine
    {
        public ChangeType Type { get; set; }
        public string Text { get; set; } = string.Empty;
        public int? LineNumber { get; set; }
    }

    public class Hunk
    {
        public int OriginalStartLine { get; set; }
        public int OriginalLineCount { get; set; }
        public int NewStartLine { get; set; }
        public int NewLineCount { get; set; }
        public IList<DiffLine> Lines { get; set; } = new List<DiffLine>();
    }

    public class DiffFile
    {
        public string OriginalFilePath { get; set; } = string.Empty;
        public string NewFilePath { get; set; } = string.Empty;
        public bool IsNewFile { get; set; }
        public bool IsDeletedFile { get; set; }
        public bool IsBinary { get; set; }
        public IList<Hunk> Hunks { get; set; } = new List<Hunk>();
    }

    public class DiffParser
    {
        public IList<DiffFile> Parse(string diffFilePath)
        {
            if (!File.Exists(diffFilePath))
                throw new FileNotFoundException(diffFilePath);

            var lines = File.ReadAllLines(diffFilePath);
            var result = new List<DiffFile>();
            DiffFile? currentFile = null;
            Hunk? currentHunk = null;
            int lineIndex = 0;
            while (lineIndex < lines.Length)
            {
                string line = lines[lineIndex];
                if (line.StartsWith("diff --git"))
                {
                    currentFile = new DiffFile();
                    result.Add(currentFile);
                    var parts = line.Split(' ');
                    if (parts.Length >= 4)
                    {
                        currentFile.OriginalFilePath = parts[2].Substring(2);
                        currentFile.NewFilePath = parts[3].Substring(2);
                    }
                    lineIndex++;
                    continue;
                }
                if (line.StartsWith("--- "))
                {
                    if (currentFile == null)
                    {
                        currentFile = new DiffFile();
                        result.Add(currentFile);
                    }
                    currentFile.OriginalFilePath = line.Substring(4).Trim();
                    if (currentFile.OriginalFilePath.StartsWith("a/"))
                        currentFile.OriginalFilePath = currentFile.OriginalFilePath.Substring(2);
                    if (currentFile.OriginalFilePath == "/dev/null")
                        {
                            currentFile.IsNewFile = true;
                            currentFile.OriginalFilePath = currentFile.NewFilePath;
                        }
                    lineIndex++;
                    if (lineIndex < lines.Length && lines[lineIndex].StartsWith("+++ "))
                    {
                        var newLine = lines[lineIndex];
                        currentFile.NewFilePath = newLine.Substring(4).Trim();
                        if (currentFile.NewFilePath.StartsWith("b/"))
                            currentFile.NewFilePath = currentFile.NewFilePath.Substring(2);
                        if (currentFile.NewFilePath == "/dev/null")
                        {
                            currentFile.IsDeletedFile = true;
                            currentFile.NewFilePath = currentFile.OriginalFilePath;
                        }
                        lineIndex++;
                    }
                    continue;
                }
                if (line.StartsWith("Binary files") || line.StartsWith("GIT binary patch"))
                {
                    if (currentFile != null)
                    {
                        currentFile.IsBinary = true;
                    }
                    lineIndex++;
                    continue;
                }
                if (line.StartsWith("@@"))
                {
                    if (currentFile == null)
                        throw new DiffParseException("Hunk without file header");

                    var header = line;
                    var numbers = header.Split(' ');
                    if (numbers.Length < 3)
                        throw new DiffParseException($"Malformed hunk header: {header}");
                    var orig = numbers[1];
                    var mod = numbers[2];
                    var origParts = orig.Substring(1).Split(',');
                    var modParts = mod.Substring(1).Split(',');
                    currentHunk = new Hunk
                    {
                        OriginalStartLine = int.Parse(origParts[0]),
                        OriginalLineCount = origParts.Length > 1 ? int.Parse(origParts[1]) : 1,
                        NewStartLine = int.Parse(modParts[0]),
                        NewLineCount = modParts.Length > 1 ? int.Parse(modParts[1]) : 1
                    };
                    currentFile.Hunks.Add(currentHunk);
                    lineIndex++;
                    while (lineIndex < lines.Length)
                    {
                        var l = lines[lineIndex];
                        if (l.StartsWith("@@") || l.StartsWith("diff --git") || l.StartsWith("--- "))
                            break;
                        if (currentHunk == null)
                            throw new DiffParseException("Line outside hunk");
                        if (l.StartsWith("+"))
                        {
                            currentHunk.Lines.Add(new DiffLine { Type = ChangeType.Add, Text = l.Substring(1) });
                        }
                        else if (l.StartsWith("-"))
                        {
                            currentHunk.Lines.Add(new DiffLine { Type = ChangeType.Remove, Text = l.Substring(1) });
                        }
                        else if (l.StartsWith(" "))
                        {
                            currentHunk.Lines.Add(new DiffLine { Type = ChangeType.Context, Text = l.Substring(1) });
                        }
                        lineIndex++;
                    }
                    continue;
                }
                lineIndex++;
            }
            if (result.Count == 0)
                throw new DiffParseException("No diff content");
            return result;
        }
    }
}
