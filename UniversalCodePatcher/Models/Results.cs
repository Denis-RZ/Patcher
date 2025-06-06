using System;
using System.Collections.Generic;

namespace UniversalCodePatcher.Models
{
    /// <summary>
    /// Результат применения патча
    /// </summary>
    public class PatchResult
    {
        public bool Success { get; set; }
        public string ModifiedCode { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public int ElementsModified { get; set; }
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Additional data produced during patching
        /// </summary>
 
        public PatchMetadata Metadata { get; set; } = new();
 
    }
    
    /// <summary>
    /// Результат валидации синтаксиса
    /// </summary>
    public class SyntaxValidationResult
    {
        public bool IsValid { get; set; }
        public List<SyntaxError> Errors { get; set; } = new();
        public List<SyntaxWarning> Warnings { get; set; } = new();
    }
    
    /// <summary>
    /// Синтаксическая ошибка
    /// </summary>
    public class SyntaxError
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Синтаксическое предупреждение
    /// </summary>
    public class SyntaxWarning
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Message { get; set; } = string.Empty;
        public string WarningCode { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Результат валидации правила
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
        public List<string> Suggestions { get; set; } = new();
    }
    
    /// <summary>
    /// Структура проекта
    /// </summary>
    public class ProjectStructure
    {
        public string RootPath { get; set; } = string.Empty;
        public List<ProjectFile> Files { get; set; } = new();
        public List<string> Languages { get; set; } = new();
        public Dictionary<string, int> Statistics { get; set; } = new();
    }
    
    /// <summary>
    /// Файл в проекте
    /// </summary>
    public class ProjectFile
    {
        public string FilePath { get; set; } = string.Empty;
        public string Language { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime LastModified { get; set; }
        public int LineCount { get; set; }
        public List<CodeElement> Elements { get; set; } = new();
    }
}
