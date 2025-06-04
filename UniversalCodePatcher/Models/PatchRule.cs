using System;
using System.Collections.Generic;

namespace UniversalCodePatcher.Models
{
    /// <summary>
    /// Типы действий патча
    /// </summary>
    public enum PatchType
    {
        Replace,
        InsertBefore,
        InsertAfter,
        Delete,
        Modify,
        Wrap,
        AddParameter,
        RemoveParameter,
        ChangeSignature,
        AddAttribute,
        RemoveAttribute,
        ChangeVisibility
    }
    
    /// <summary>
    /// Правило для применения патча
    /// </summary>
    public class PatchRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PatchType PatchType { get; set; }
        public string TargetPattern { get; set; } = string.Empty;
        public string TargetLanguage { get; set; } = string.Empty;
        public CodeElementType TargetElementType { get; set; }
        public string NewContent { get; set; } = string.Empty;
        public string SearchCriteria { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public bool IsEnabled { get; set; } = true;
        public int Priority { get; set; } = 0;
        public string[] PreConditions { get; set; } = Array.Empty<string>();
        public string[] PostConditions { get; set; } = Array.Empty<string>();
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        public string Author { get; set; } = Environment.UserName;
        
        /// <summary>
        /// Проверка применимости правила к элементу
        /// </summary>
        public bool IsApplicableTo(CodeElement element)
        {
            if (TargetElementType != CodeElementType.Unknown && element.Type != TargetElementType)
                return false;
                
            if (!string.IsNullOrEmpty(TargetLanguage) && TargetLanguage != "All" && element.Language != TargetLanguage)
                return false;
                
            return true;
        }
        
        /// <summary>
        /// Клонирование правила
        /// </summary>
        public PatchRule Clone()
        {
            return new PatchRule
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"{Name} (Copy)",
                Description = Description,
                PatchType = PatchType,
                TargetPattern = TargetPattern,
                TargetLanguage = TargetLanguage,
                TargetElementType = TargetElementType,
                NewContent = NewContent,
                SearchCriteria = SearchCriteria,
                Parameters = new Dictionary<string, object>(Parameters),
                IsEnabled = IsEnabled,
                Priority = Priority,
                PreConditions = (string[])PreConditions.Clone(),
                PostConditions = (string[])PostConditions.Clone(),
                Author = Environment.UserName
            };
        }
    }
}
