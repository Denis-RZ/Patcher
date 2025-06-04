using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UniversalCodePatcher.Interfaces;

namespace UniversalCodePatcher.Core
{
    /// <summary>
    /// Менеджер для загрузки и управления модулями
    /// </summary>
    public class ModuleManager
    {
        private readonly IServiceContainer _serviceContainer;
        private readonly Dictionary<string, IModule> _loadedModules = new();
        private readonly List<string> _moduleSearchPaths = new();
        
        public event EventHandler<ModuleEventArgs>? ModuleLoaded;
        public event EventHandler<ModuleEventArgs>? ModuleUnloaded;
        public event EventHandler<ModuleErrorEventArgs>? ModuleError;
        
        public IEnumerable<IModule> LoadedModules => _loadedModules.Values;
        
        public ModuleManager(IServiceContainer serviceContainer)
        {
            _serviceContainer = serviceContainer ?? throw new ArgumentNullException(nameof(serviceContainer));
            
            // Добавляем пути поиска модулей
            _moduleSearchPaths.Add(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Modules"));
            _moduleSearchPaths.Add(AppDomain.CurrentDomain.BaseDirectory);
        }
        
        /// <summary>
        /// Добавление пути для поиска модулей
        /// </summary>
        public void AddSearchPath(string path)
        {
            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                _moduleSearchPaths.Add(path);
            }
        }
        
        /// <summary>
        /// Автоматическая загрузка всех модулей
        /// </summary>
        public void LoadAllModules()
        {
            foreach (var searchPath in _moduleSearchPaths)
            {
                LoadModulesFromPath(searchPath);
            }
        }
        
        /// <summary>
        /// Загрузка модулей из указанного пути
        /// </summary>
        public void LoadModulesFromPath(string path)
        {
            if (!Directory.Exists(path)) return;
            
            try
            {
                // Ищем DLL файлы модулей
                var moduleFiles = Directory.GetFiles(path, "*Module.dll", SearchOption.AllDirectories);
                
                foreach (var moduleFile in moduleFiles)
                {
                    LoadModuleFromFile(moduleFile);
                }
                
                // Ищем модули в сборке приложения
                LoadModulesFromAssembly(Assembly.GetExecutingAssembly());
            }
            catch (Exception ex)
            {
                OnModuleError($"Error loading modules from path {path}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Загрузка модуля из файла
        /// </summary>
        public bool LoadModuleFromFile(string filePath)
        {
            try
            {
                var assembly = Assembly.LoadFrom(filePath);
                return LoadModulesFromAssembly(assembly);
            }
            catch (Exception ex)
            {
                OnModuleError($"Error loading module from file {filePath}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Загрузка модулей из сборки
        /// </summary>
        public bool LoadModulesFromAssembly(Assembly assembly)
        {
            try
            {
                var moduleTypes = assembly.GetTypes()
                    .Where(t => t.IsClass && !t.IsAbstract && typeof(IModule).IsAssignableFrom(t))
                    .ToList();
                
                foreach (var moduleType in moduleTypes)
                {
                    LoadModule(moduleType);
                }
                
                return moduleTypes.Any();
            }
            catch (Exception ex)
            {
                OnModuleError($"Error loading modules from assembly {assembly.FullName}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Загрузка конкретного модуля
        /// </summary>
        public bool LoadModule(Type moduleType)
        {
            try
            {
                if (Activator.CreateInstance(moduleType) is not IModule module)
                {
                    OnModuleError($"Cannot create instance of module {moduleType.Name}");
                    return false;
                }
                
                if (_loadedModules.ContainsKey(module.ModuleId))
                {
                    OnModuleError($"Module {module.ModuleId} is already loaded");
                    return false;
                }
                
                if (module.Initialize(_serviceContainer))
                {
                    _loadedModules[module.ModuleId] = module;
                    OnModuleLoaded(module);
                    return true;
                }
                else
                {
                    OnModuleError($"Failed to initialize module {module.Name}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                OnModuleError($"Error loading module {moduleType.Name}: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Выгрузка модуля
        /// </summary>
        public bool UnloadModule(string moduleId)
        {
            if (_loadedModules.TryGetValue(moduleId, out var module))
            {
                try
                {
                    module.Shutdown();
                    _loadedModules.Remove(moduleId);
                    OnModuleUnloaded(module);
                    return true;
                }
                catch (Exception ex)
                {
                    OnModuleError($"Error unloading module {module.Name}: {ex.Message}");
                    return false;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Получение модуля по ID
        /// </summary>
        public T? GetModule<T>() where T : class, IModule
        {
            return _loadedModules.Values.OfType<T>().FirstOrDefault();
        }
        
        /// <summary>
        /// Получение модуля по ID
        /// </summary>
        public IModule? GetModule(string moduleId)
        {
            _loadedModules.TryGetValue(moduleId, out var module);
            return module;
        }
        
        /// <summary>
        /// Выгрузка всех модулей
        /// </summary>
        public void UnloadAllModules()
        {
            var moduleIds = _loadedModules.Keys.ToList();
            foreach (var moduleId in moduleIds)
            {
                UnloadModule(moduleId);
            }
        }
        
        protected virtual void OnModuleLoaded(IModule module)
        {
            ModuleLoaded?.Invoke(this, new ModuleEventArgs(module));
        }
        
        protected virtual void OnModuleUnloaded(IModule module)
        {
            ModuleUnloaded?.Invoke(this, new ModuleEventArgs(module));
        }
        
        protected virtual void OnModuleError(string error)
        {
            ModuleError?.Invoke(this, new ModuleErrorEventArgs(error));
        }
    }
    
    /// <summary>
    /// Аргументы события модуля
    /// </summary>
    public class ModuleEventArgs : EventArgs
    {
        public IModule Module { get; }
        
        public ModuleEventArgs(IModule module)
        {
            Module = module;
        }
    }
    
    /// <summary>
    /// Аргументы события ошибки модуля
    /// </summary>
    public class ModuleErrorEventArgs : EventArgs
    {
        public string Error { get; }
        
        public ModuleErrorEventArgs(string error)
        {
            Error = error;
        }
    }
}
