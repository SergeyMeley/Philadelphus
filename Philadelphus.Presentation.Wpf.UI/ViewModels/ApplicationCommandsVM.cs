using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using IApplicationCommandsVM = Philadelphus.Presentation.Services.Interfaces.IApplicationCommandsVM;
using IRelayCommand = Philadelphus.Presentation.Infrastructure.IRelayCommand;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    /// <summary>
    /// Модель представления для команд приложения.
    /// </summary>
    public class ApplicationCommandsVM : IApplicationCommandsVM
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWindowService _windowService;
        private readonly IRelayCommandFactory _commandFactory;

        private IRelayCommand? _openMainWindowCommand;
        private IRelayCommand? _openLaunchWindowCommand;
        private IRelayCommand? _openFormulaEditorWindowCommand;
        private IRelayCommand? _openDataStoragesSettingsControlCommand;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ApplicationCommandsVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="windowService">Сервис управления окнами приложения.</param>
        /// <param name="commandFactory">Фабрика команд приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ApplicationCommandsVM(
            IServiceProvider serviceProvider,
            IWindowService windowService,
            IRelayCommandFactory commandFactory)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(windowService);
            ArgumentNullException.ThrowIfNull(commandFactory);

            _serviceProvider = serviceProvider;
            _windowService = windowService;
            _commandFactory = commandFactory;
        }

        public IRelayCommand OpenMainWindowCommand => _openMainWindowCommand ??= _commandFactory.Create(obj =>
        {
            var launchVM = _serviceProvider.GetRequiredService<LaunchWindowVM>();
            var currentRepositoryVM = launchVM.RepositoryCollectionVM.CurrentRepositoryVM;
            if (currentRepositoryVM != null)
            {
                var headerVM = launchVM.RepositoryHeadersCollectionVM.PhiladelphusRepositoryHeadersVMs.FirstOrDefault(x => x.Uuid == currentRepositoryVM.Uuid);
                if (headerVM == null)
                {
                    headerVM = launchVM.RepositoryHeadersCollectionVM.AddPhiladelphusRepositoryHeaderVMFromPhiladelphusRepositoryVM(currentRepositoryVM);
                }
                headerVM.LastOpening = DateTime.UtcNow;
            }

            var appVM = _serviceProvider.GetRequiredService<ApplicationVM>();
            var repositoryExplorerControlVM = _serviceProvider.GetRequiredService<IRepositoryExplorerControlVMFactory>().Create(currentRepositoryVM);
            var mainWindowVM = _serviceProvider.GetRequiredService<IMainWindowVMFactory>().Create(repositoryExplorerControlVM);
            _windowService.Show(mainWindowVM);
            _windowService.Hide(launchVM);
        });

        public IRelayCommand OpenLaunchWindowCommand => _openLaunchWindowCommand ??= _commandFactory.Create(obj =>
        {
            var launchVM = _serviceProvider.GetRequiredService<LaunchWindowVM>();
            _windowService.Show(launchVM);
        });

        /// <summary>
        /// Команда открытия редактора формул.
        /// </summary>
        public IRelayCommand OpenFormulaEditorWindowCommand => _openFormulaEditorWindowCommand ??= _commandFactory.Create(obj =>
        {
            var formulaEditorVM = CreateFormulaEditorVM(obj);
            if (formulaEditorVM == null)
            {
                return;
            }

            _windowService.ShowOrActivate(formulaEditorVM);
        },
        obj => obj is FormulaTestControlVM or FormulaEditorOpenRequest);

        public IRelayCommand OpenDataStoragesSettingsControlCommand => _openDataStoragesSettingsControlCommand ??= _commandFactory.Create(obj =>
        {
            var qwe = obj.GetType();
            var launchVM = _serviceProvider.GetRequiredService<LaunchWindowVM>();
            _windowService.Show(launchVM);
        });

        private FormulaTestControlVM? CreateFormulaEditorVM(object obj)
        {
            if (obj is FormulaTestControlVM formulaEditorVM)
            {
                return formulaEditorVM;
            }

            if (obj is FormulaEditorOpenRequest request)
            {
                var vm = ActivatorUtilities.CreateInstance<FormulaTestControlVM>(
                    _serviceProvider,
                    request.RepositoryExplorerControlVM);
                vm.FormulaText = request.FormulaText ?? string.Empty;
                return vm;
            }

            return null;
        }
    }

    /// <summary>
    /// Запрос открытия редактора формул из интерфейса репозитория.
    /// </summary>
    /// <param name="RepositoryExplorerControlVM">Обозреватель репозитория, задающий контекст формулы.</param>
    /// <param name="FormulaText">Текущий текст формулы.</param>
    public sealed record FormulaEditorOpenRequest(
        RepositoryExplorerControlVM RepositoryExplorerControlVM,
        string? FormulaText);
}
