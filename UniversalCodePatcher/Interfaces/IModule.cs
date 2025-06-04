using System;
using System.Collections.Generic;

namespace UniversalCodePatcher.Interfaces
{
    /// <summary>
    /// Базовый интерфейс для всех модулей системы
    /// </summary>
    public interface IModule
    {
        /// <summary>
        /// Уникальный идентификатор модуля
        /// </summary>
        string ModuleId { get; }
        
        /// <summary>
        /// Название модуля
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Версия модуля
        /// </summary>
        Version Version { get; }
        
        /// <summary>
        /// Описание модуля
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// Зависимости модуля
        /// </summary>
        IEnumerable<string> Dependencies { get; }
        
        /// <summary>
        /// Инициализация модуля
        /// </summary>
        bool Initialize(IServiceContainer serviceContainer);
        
        /// <summary>
        /// Деинициализация модуля
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// Проверка готовности модуля
        /// </summary>
        bool IsReady { get; }
    }
}
