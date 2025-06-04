using System;
using System.Collections.Concurrent;
using UniversalCodePatcher.Interfaces;

namespace UniversalCodePatcher.Core
{
    /// <summary>
    /// Простой DI контейнер для управления зависимостями
    /// </summary>
    public class ServiceContainer : IServiceContainer
    {
        private readonly ConcurrentDictionary<Type, object> _services = new();
        private readonly ConcurrentDictionary<Type, Func<object>> _factories = new();
        
        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
            where TInterface : class
        {
            _factories[typeof(TInterface)] = () => Activator.CreateInstance<TImplementation>();
        }
        
        public void RegisterInstance<T>(T instance) where T : class
        {
            _services[typeof(T)] = instance;
        }
        
        public T GetService<T>() where T : class
        {
            return (T)GetService(typeof(T));
        }
        
        public object GetService(Type serviceType)
        {
            // Проверяем, есть ли уже созданный экземпляр
            if (_services.TryGetValue(serviceType, out var existingService))
            {
                return existingService;
            }
            
            // Создаем новый экземпляр если есть фабрика
            if (_factories.TryGetValue(serviceType, out var factory))
            {
                var newService = factory();
                _services[serviceType] = newService;
                return newService;
            }
            
            throw new InvalidOperationException($"Service of type {serviceType.Name} is not registered");
        }
        
        public bool IsRegistered<T>() where T : class
        {
            return IsRegistered(typeof(T));
        }
        
        public bool IsRegistered(Type serviceType)
        {
            return _services.ContainsKey(serviceType) || _factories.ContainsKey(serviceType);
        }
    }
}
