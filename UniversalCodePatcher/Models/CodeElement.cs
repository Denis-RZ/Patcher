using System;
using System.Collections.Generic;

namespace UniversalCodePatcher.Models
{
    /// <summary>
    /// Типы элементов кода
    /// </summary>
    public enum CodeElementType
    {
        Unknown,
        Function,
        Method, 
        Class,
        Interface,
        Property,
        Field,
        Variable,
        Import,
        Export,
        Namespace,
        Using,
        Constructor,
        Destructor,
        Event,
        Delegate,
        Enum,
        Struct,
        Lambda,
        AsyncFunction
    }
    
    /// <summary>
    /// Элемент кода с метаданными
    /// </summary>
    public class CodeElement
    {
        public CodeElementType Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public int StartLine { get; set; }
        public int EndLine { get; set; }
        public int StartColumn { get; set; }
        public int EndColumn { get; set; }
        public string Language { get; set; } = string.Empty;
        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<CodeElement> Children { get; set; } = new();
        public CodeElement? Parent { get; set; }
        public string[] Parameters { get; set; } = Array.Empty<string>();
        public string ReturnType { get; set; } = string.Empty;
        public string[] Modifiers { get; set; } = Array.Empty<string>();
        public string[] Attributes { get; set; } = Array.Empty<string>();
        public string Documentation { get; set; } = string.Empty;
        
        /// <summary>
        /// Получение полного пути элемента
        /// </summary>
        public string GetPath()
        {
            var parts = new List<string>();
            var current = this;
            
            while (current != null)
            {
                if (!string.IsNullOrEmpty(current.Name))
                {
                    parts.Insert(0, current.Name);
                }
                current = current.Parent;
            }
            
            return string.Join(".", parts);
        }
        
        /// <summary>
        /// Поиск дочернего элемента по имени
        /// </summary>
        public CodeElement? FindChild(string name)
        {
            return Children.FirstOrDefault(c => c.Name == name);
        }
        
        /// <summary>
        /// Получение всех потомков определенного типа
        /// </summary>
        public IEnumerable<CodeElement> GetDescendants(CodeElementType type)
        {
            foreach (var child in Children)
            {
                if (child.Type == type)
                    yield return child;
                    
                foreach (var descendant in child.GetDescendants(type))
                    yield return descendant;
            }
        }
    }
}
