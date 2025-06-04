using System.Collections.Generic;
using UniversalCodePatcher.Models;

namespace UniversalCodePatcher.Interfaces
{
    /// <summary>
    /// Интерфейс для применения патчей
    /// </summary>
    public interface IPatcher
    {
        /// <summary>
        /// Поддерживаемые типы патчей
        /// </summary>
        IEnumerable<PatchType> SupportedPatchTypes { get; }
        
        /// <summary>
        /// Применение патча к коду
        /// </summary>
        PatchResult ApplyPatch(string originalCode, PatchRule rule, string language);
        
        /// <summary>
        /// Проверка возможности применения патча
        /// </summary>
        bool CanApplyPatch(string code, PatchRule rule, string language);
        
        /// <summary>
        /// Предварительный просмотр результата патча
        /// </summary>
        string PreviewPatch(string originalCode, PatchRule rule, string language);
        
        /// <summary>
        /// Валидация правила патча
        /// </summary>
        ValidationResult ValidateRule(PatchRule rule);
    }
}
