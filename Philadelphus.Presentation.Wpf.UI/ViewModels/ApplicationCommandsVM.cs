using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels
{
    /// <summary>
    /// Модель представления для команд приложения.
    /// </summary>
    public class ApplicationCommandsVM
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWindowService _windowService;

        private RelayCommand? _openMainWindowCommand;
        private RelayCommand? _openLaunchWindowCommand;
        private RelayCommand? _openFormulaEditorWindowCommand;
        private RelayCommand? _openDataStoragesSettingsControlCommand;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ApplicationCommandsVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="windowService">Сервис управления окнами приложения.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ApplicationCommandsVM(
            IServiceProvider serviceProvider,
            IWindowService windowService)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(windowService);

            _serviceProvider = serviceProvider;
            _windowService = windowService;
        }

        public RelayCommand OpenMainWindowCommand => _openMainWindowCommand ??= new RelayCommand(obj =>
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

        public RelayCommand OpenLaunchWindowCommand => _openLaunchWindowCommand ??= new RelayCommand(obj =>
        {
            var launchVM = _serviceProvider.GetRequiredService<LaunchWindowVM>();
            _windowService.Show(launchVM);
        });

        /// <summary>
        /// Команда открытия редактора формул.
        /// </summary>
        public RelayCommand OpenFormulaEditorWindowCommand => _openFormulaEditorWindowCommand ??= new RelayCommand(obj =>
        {
            var formulaEditorVM = CreateFormulaEditorVM(obj);
            if (formulaEditorVM == null)
            {
                return;
            }

            _windowService.ShowOrActivate(formulaEditorVM);
        },
        obj => obj is FormulaTestControlVM or FormulaEditorOpenRequest);

        public RelayCommand OpenDataStoragesSettingsControlCommand => _openDataStoragesSettingsControlCommand ??= new RelayCommand(obj =>
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
