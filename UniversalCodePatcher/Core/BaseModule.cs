using System;
using System.Collections.Generic;
using UniversalCodePatcher.Interfaces;

namespace UniversalCodePatcher.Core
{
    /// <summary>
    /// Базовый класс для всех модулей
    /// </summary>
    public abstract class BaseModule : IModule
    {
        protected IServiceContainer? ServiceContainer { get; private set; }
        
        public abstract string ModuleId { get; }
        public abstract string Name { get; }
        public abstract Version Version { get; }
        public abstract string Description { get; }
        public virtual IEnumerable<string> Dependencies => Array.Empty<string>();
        
        public bool IsReady { get; protected set; }
        
        public virtual bool Initialize(IServiceContainer serviceContainer)
        {
            ServiceContainer = serviceContainer;
            
            try
            {
                // Проверяем зависимости
                if (!CheckDependencies())
                {
                    return false;
                }
                
                // Выполняем инициализацию модуля
                if (OnInitialize())
                {
                    IsReady = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError($"Failed to initialize module {Name}: {ex.Message}");
            }
            
            return false;
        }
        
        public virtual void Shutdown()
        {
            try
            {
                OnShutdown();
            }
            catch (Exception ex)
            {
                OnError($"Error during module {Name} shutdown: {ex.Message}");
            }
            finally
            {
                IsReady = false;
                ServiceContainer = null;
            }
        }
        
        /// <summary>
        /// Инициализация конкретного модуля
        /// </summary>
        protected abstract bool OnInitialize();
        
        /// <summary>
        /// Деинициализация конкретного модуля
        /// </summary>
        protected virtual void OnShutdown() { }
        
        /// <summary>
        /// Проверка зависимостей модуля
        /// </summary>
        protected virtual bool CheckDependencies()
        {
            if (ServiceContainer == null) return false;
            
            foreach (var dependency in Dependencies)
            {
                // Здесь можно добавить логику проверки зависимостей
                // Например, проверить наличие других модулей
            }
            
            return true;
        }
        
        /// <summary>
        /// Получение сервиса из контейнера
        /// </summary>
        protected T? GetService<T>() where T : class
        {
            return ServiceContainer?.GetService<T>();
        }
        
        /// <summary>
        /// Обработка ошибок модуля
        /// </summary>
        protected virtual void OnError(string message)
        {
            // Логирование ошибок (можно расширить)
            System.Diagnostics.Debug.WriteLine($"[{Name}] ERROR: {message}");
        }
        
        /// <summary>
        /// Логирование информации
        /// </summary>
        protected virtual void OnInfo(string message)
        {
            System.Diagnostics.Debug.WriteLine($"[{Name}] INFO: {message}");
        }
    }
}
