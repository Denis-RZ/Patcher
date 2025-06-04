using System;
using System.Collections.Generic;

namespace UniversalCodePatcher.Models
{
    /// <summary>
    /// Diff патч для файла
    /// </summary>
    public class FileDiff
    {
        public string FileName { get; set; } = string.Empty;
        public string OriginalPath { get; set; } = string.Empty;
        public string ModifiedPath { get; set; } = string.Empty;
        public DiffStatus Status { get; set; }
        public List<DiffHunk> Hunks { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Получение unified diff
        /// </summary>
        public string ToUnifiedDiff()
        {
            var result = new System.Text.StringBuilder();
            
            result.AppendLine($"--- {OriginalPath}");
            result.AppendLine($"+++ {ModifiedPath}");
            
            foreach (var hunk in Hunks)
            {
                result.AppendLine(hunk.ToString());
            }
            
            return result.ToString();
        }
    }
    
    /// <summary>
    /// Статус файла в diff
    /// </summary>
    public enum DiffStatus
    {
        Modified,
        Added,
        Deleted,
        Renamed,
        Copied
    }
    
    /// <summary>
    /// Блок изменений в diff
    /// </summary>
    public class DiffHunk
    {
        public int OriginalStart { get; set; }
        public int OriginalLength { get; set; }
        public int ModifiedStart { get; set; }
        public int ModifiedLength { get; set; }
        public List<DiffLine> Lines { get; set; } = new();
        public string Context { get; set; } = string.Empty;
        
        public override string ToString()
        {
            var result = new System.Text.StringBuilder();
            result.AppendLine($"@@ -{OriginalStart},{OriginalLength} +{ModifiedStart},{ModifiedLength} @@ {Context}");
            
            foreach (var line in Lines)
            {
                result.AppendLine(line.ToString());
            }
            
            return result.ToString();
        }
    }
    
    /// <summary>
    /// Строка в diff
    /// </summary>
    public class DiffLine
    {
        public DiffLineType Type { get; set; }
        public string Content { get; set; } = string.Empty;
        public int OriginalLineNumber { get; set; }
        public int ModifiedLineNumber { get; set; }
        
        public override string ToString()
        {
            var prefix = Type switch
            {
                DiffLineType.Added => "+",
                DiffLineType.Removed => "-",
                DiffLineType.Context => " ",
                _ => " "
            };
            
            return $"{prefix}{Content}";
        }
    }
    
    /// <summary>
    /// Тип строки в diff
    /// </summary>
    public enum DiffLineType
    {
        Context,
        Added,
        Removed
    }
    
    /// <summary>
    /// Полный diff патч
    /// </summary>
    public class DiffPatch
    {
        public List<FileDiff> Files { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string Author { get; set; } = Environment.UserName;
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Сохранение в unified diff формат
        /// </summary>
        public string ToUnifiedDiff()
        {
            var result = new System.Text.StringBuilder();
            
            foreach (var file in Files)
            {
                result.AppendLine(file.ToUnifiedDiff());
                result.AppendLine();
            }
            
            return result.ToString();
        }
    }
}
