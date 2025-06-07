using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using UniversalCodePatcher.Interfaces;

namespace UniversalCodePatcher.Core
{
    /// <summary>
    /// Simple wrapper over Microsoft.Extensions.DependencyInjection
    /// </summary>
    public class ServiceContainer : IServiceContainer, IDisposable
    {
        private readonly ServiceCollection _collection = new();
        private ServiceProvider? _provider;

        public ServiceContainer()
        {
            _collection.AddSingleton<ILogger, SimpleLogger>();
        }

        private ServiceProvider Provider => _provider ??= _collection.BuildServiceProvider();

        public void RegisterSingleton<TInterface, TImplementation>()
            where TImplementation : class, TInterface
            where TInterface : class
        {
            _collection.AddSingleton<TInterface, TImplementation>();
            _provider = null;
        }

        public void RegisterInstance<T>(T instance) where T : class
        {
            _collection.AddSingleton(instance);
            _provider = null;
        }

        public T GetService<T>() where T : class
        {
            return Provider.GetRequiredService<T>();
        }

        public object GetService(Type serviceType)
        {
            return Provider.GetRequiredService(serviceType);
        }

        public bool IsRegistered<T>() where T : class
        {
            return IsRegistered(typeof(T));
        }

        public bool IsRegistered(Type serviceType)
        {
            return _collection.Any(sd => sd.ServiceType == serviceType);
        }

        public void Dispose()
        {
            if (_provider is IDisposable disposable)
            {
                disposable.Dispose();
                _provider = null;
            }
        }
    }
}
