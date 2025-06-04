using System.Collections.Generic;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Interfaces
{
    /// <summary>
    /// Интерфейс для создания и применения diff патчей
    /// </summary>
    public interface IDiffEngine
    {
        /// <summary>
        /// Создание unified diff между двумя версиями кода
        /// </summary>
        string CreateUnifiedDiff(string original, string modified, string fileName);
        
        /// <summary>
        /// Применение unified diff к коду
        /// </summary>
        string ApplyUnifiedDiff(string original, string diffContent);
        
        /// <summary>
        /// Парсинг diff файла
        /// </summary>
        DiffPatch ParseDiff(string diffContent);
        
        /// <summary>
        /// Создание патча для файла
        /// </summary>
        FileDiff CreateFileDiff(string filePath, string originalContent, string modifiedContent);
        
        /// <summary>
        /// Сравнение двух версий проекта
        /// </summary>
        IEnumerable<FileDiff> CompareProjects(string originalPath, string modifiedPath);
        
        /// <summary>
        /// Получить простой diff (построчно)
        /// </summary>
        IEnumerable<string> GetDiff(string oldText, string newText);
    }
}
