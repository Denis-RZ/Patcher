using System.Collections.Generic;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Interfaces
{
    /// <summary>
    /// Интерфейс для анализа кода
    /// </summary>
    public interface ICodeAnalyzer
    {
        /// <summary>
        /// Поддерживаемые языки программирования
        /// </summary>
        IEnumerable<string> SupportedLanguages { get; }
        
        /// <summary>
        /// Анализ кода и извлечение элементов
        /// </summary>
        IEnumerable<CodeElement> AnalyzeCode(string code, string language);
        
        /// <summary>
        /// Поиск элементов по паттерну
        /// </summary>
        IEnumerable<CodeElement> FindElements(IEnumerable<CodeElement> elements, string pattern);
        
        /// <summary>
        /// Проверка синтаксиса кода
        /// </summary>
        SyntaxValidationResult ValidateSyntax(string code, string language);
        
        /// <summary>
        /// Получение структуры проекта
        /// </summary>
        ProjectStructure GetProjectStructure(string projectPath);
    }
}
