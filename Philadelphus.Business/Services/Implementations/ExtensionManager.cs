using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.Business.Services.Implementations
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

        public async Task LoadExtensionsAsync()
        {
            // В реальном приложении здесь была бы загрузка из DLL файлов через рефлексию
            // Для демонстрации пока пусто - расширения создаются тестовыми классами
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

        public async Task<IRepositoryElementModel> ExecuteExtensionAsync(ExtensionInstance extension, IRepositoryElementModel element)
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

        public async Task<List<ExtensionInstance>> GetCompatibleExtensionsAsync(IRepositoryElementModel element)
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
