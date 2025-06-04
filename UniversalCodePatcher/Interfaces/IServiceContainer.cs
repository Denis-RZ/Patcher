using System;

namespace UniversalCodePatcher.Interfaces
{
    /// <summary>
    /// Интерфейс контейнера зависимостей
    /// </summary>
    public interface IServiceContainer
    {
        /// <summary>
        /// Регистрация сервиса
        /// </summary>
        void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
            where TInterface : class;
        
        /// <summary>
        /// Регистрация экземпляра сервиса
        /// </summary>
        void RegisterInstance<T>(T instance) where T : class;
        
        /// <summary>
        /// Получение сервиса
        /// </summary>
        T GetService<T>() where T : class;
        
        /// <summary>
        /// Получение сервиса по типу
        /// </summary>
        object GetService(Type serviceType);
        
        /// <summary>
        /// Проверка регистрации сервиса
        /// </summary>
        bool IsRegistered<T>() where T : class;
        
        /// <summary>
        /// Проверка регистрации сервиса по типу
        /// </summary>
        bool IsRegistered(Type serviceType);
    }
}
