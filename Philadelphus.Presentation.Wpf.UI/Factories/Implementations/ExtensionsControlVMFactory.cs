using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Implementations
{
    /// <summary>
    /// Фабрика создания расширения.
    /// </summary>
    internal class ExtensionsControlVMFactory : IExtensionsControlVMFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExtensionsControlVMFactory" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ExtensionsControlVMFactory(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Создает объект Create.
        /// </summary>
        /// <param name="repositoryExplorerControlVM">Параметр repositoryExplorerControlVM.</param>
        /// <returns>Созданный объект.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ExtensionsControlVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM)
        {
            ArgumentNullException.ThrowIfNull(repositoryExplorerControlVM);

            return ActivatorUtilities.CreateInstance<ExtensionsControlVM>(_serviceProvider, repositoryExplorerControlVM);
        }
    }
}
