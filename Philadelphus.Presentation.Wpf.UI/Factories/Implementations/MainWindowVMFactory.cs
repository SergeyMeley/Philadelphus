using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.Factories.Implementations
{
    /// <summary>
    /// Фабрика создания главного окна.
    /// </summary>
    internal class MainWindowVMFactory : IMainWindowVMFactory
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainWindowVMFactory" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public MainWindowVMFactory(IServiceProvider serviceProvider)
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
        public MainWindowVM Create(RepositoryExplorerControlVM repositoryExplorerControlVM)
        {
            ArgumentNullException.ThrowIfNull(repositoryExplorerControlVM);

            return ActivatorUtilities.CreateInstance<MainWindowVM>(_serviceProvider, repositoryExplorerControlVM);
        }
    }
}
