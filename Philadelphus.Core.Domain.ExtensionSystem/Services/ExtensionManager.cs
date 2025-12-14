using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.RepositoryMembers;
using Philadelphus.Core.Domain.ExtensionSystem.Infrastructure;
using Philadelphus.Core.Domain.ExtensionSystem.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.Core.Domain.ExtensionSystem.Services
{
    /// <summary>
    /// Реализация менеджера расширений
    /// </summary>
    public class ExtensionManager : IExtensionManager
    {
        private readonly List<ExtensionInstance> _extensions;

        public event EventHandler<ExtensionLoadedEventArgs> ExtensionLoaded;
        public event EventHandler<ExtensionErrorEventArgs> ExtensionError;

        public ExtensionManager()
        {
            _extensions = new List<ExtensionInstance>();
        }

        public IReadOnlyList<ExtensionInstance> GetExtensions() => _extensions.AsReadOnly();

        public async Task LoadExtensionsAsync(string pluginsFolderPath)
        {
            if (!Directory.Exists(pluginsFolderPath))
                throw new DirectoryNotFoundException($"Папка расширений не найдена: {pluginsFolderPath}");

            var dllFiles = Directory.GetFiles(pluginsFolderPath, "*.dll");

            foreach (var dll in dllFiles)
            {
                try
                {
                    // Загружаем сборку
                    var assembly = Assembly.LoadFrom(dll);
                    // Ищем типы, реализующие интерфейс IExtension
                    var extensionTypes = assembly.GetTypes()
                        .Where(t => typeof(IExtensionModel).IsAssignableFrom(t) && !t.IsAbstract);

                    foreach (var type in extensionTypes)
                    {
                        // Создаем экземпляр расширения (нужен конструктор без параметров или DI)
                        if (Activator.CreateInstance(type) is IExtensionModel extension)
                        {
                            var extensionInstance = new ExtensionInstance(extension);
                            // Регистрируем расширение (добавляем в список)
                            RegisterExtension(extensionInstance);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Логируем ошибку загрузки конкретной DLL, но не прерываем загрузку остальных
                    Debug.WriteLine($"Ошибка загрузки расширения из {dll}: {ex.Message}");
                }
            }

            await Task.CompletedTask;
        }

        public async Task StartExtensionAsync(ExtensionInstance extension)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            try
            {
                await extension.StartAsync();
            }
            catch (Exception ex)
            {
                ExtensionError?.Invoke(this, new ExtensionErrorEventArgs
                {
                    ExtensionName = extension.Metadata.Name,
                    Exception = ex
                });
                throw;
            }
        }

        public async Task StopExtensionAsync(ExtensionInstance extension)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));

            try
            {
                await extension.StopAsync();
            }
            catch (Exception ex)
            {
                ExtensionError?.Invoke(this, new ExtensionErrorEventArgs
                {
                    ExtensionName = extension.Metadata.Name,
                    Exception = ex
                });
                throw;
            }
        }

        public async Task<MainEntityBaseModel> ExecuteExtensionAsync(ExtensionInstance extension, MainEntityBaseModel element)
        {
            if (extension == null)
                throw new ArgumentNullException(nameof(extension));
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            try
            {
                return await extension.ExecuteAsync(element);
            }
            catch (Exception ex)
            {
                ExtensionError?.Invoke(this, new ExtensionErrorEventArgs
                {
                    ExtensionName = extension.Metadata.Name,
                    Exception = ex
                });
                throw;
            }
        }

        public async Task AutoStartExtensionsAsync()
        {
            var autoStartExtensions = _extensions.Where(e => e.Metadata.AutoStart).ToList();

            foreach (var extension in autoStartExtensions)
            {
                try
                {
                    await StartExtensionAsync(extension);
                }
                catch
                {
                    // Ошибка при автозагрузке не должна завешивать приложение
                }
            }

            await Task.CompletedTask;
        }

        public async Task<List<ExtensionInstance>> GetCompatibleExtensionsAsync(MainEntityBaseModel element)
        {
            var compatible = new List<ExtensionInstance>();

            foreach (var extension in _extensions)
            {
                var canExecute = await extension.Extension.CanExecuteAsync(element);
                if (canExecute.CanExecute)
                {
                    compatible.Add(extension);
                }
            }

            return compatible;
        }

        public void RegisterExtension(ExtensionInstance extensionInstance)
        {
            if (extensionInstance == null)
                throw new ArgumentNullException(nameof(extensionInstance));

            _extensions.Add(extensionInstance);
            ExtensionLoaded?.Invoke(this, new ExtensionLoadedEventArgs { Extension = extensionInstance });
        }
    }

}
