using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.ViewModels.ImportExport;
using Serilog;
using System.Reflection;
using IApplicationCommandsVM = Philadelphus.Presentation.Services.Interfaces.IApplicationCommandsVM;
using IRelayCommand = Philadelphus.Presentation.Infrastructure.IRelayCommand;
using IWindowService = Philadelphus.Presentation.Services.Interfaces.IWindowService;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления для главного окна.
    /// </summary>
    public class MainWindowVM : ControlBaseVM
    {
        private readonly ExtensionsControlVM _extensionsControlVM;
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;
        private readonly ImportExportControlVM _importExportControlVM;
        private readonly ApplicationSettingsControlVM _applicationSettingsControlVM;
        private readonly ReportsControlVM _reportsControlVM;
        private readonly FormulaTestControlVM _formulaTestControlVM;
        private readonly MainWindowNotificationsVM _mainWindowNotificationsVM;
        private readonly IRelayCommandFactory _commandFactory;
        private readonly IWindowService _windowService;
        private readonly ThemeSettingsVM? _themeSettingsVM;

        /// <summary>
        /// Команды приложения.
        /// </summary>
        public IApplicationCommandsVM ApplicationCommandsVM { get => _applicationCommandsVM; }
      
        /// <summary>
        /// Расширение.
        /// </summary>
        public ExtensionsControlVM ExtensionsControlVM { get => _extensionsControlVM; }
        public RepositoryExplorerControlVM RepositoryExplorerVM
        {
            get
            {
                return _repositoryExplorerControlVM;
            }
        }
        public ImportExportControlVM ImportExportVM
        {
            get
            {
                return _importExportControlVM;
            }
        }
        public ApplicationSettingsControlVM ApplicationSettingsControlVM
        {
            get
            {
                return _applicationSettingsControlVM;
            }
        }
        public ReportsControlVM ReportsControlVM
        {
            get
            {
                return _reportsControlVM;
            }
        }

        /// <summary>
        /// Модель представления тестового интерфейса Formula Engine.
        /// </summary>
        public FormulaTestControlVM FormulaTestControlVM
        {
            get
            {
                return _formulaTestControlVM;
            }
        }
        public string Title
        {
            get
            {
                var title = $"Чубушник {AssemblyVersion}";
                if (string.IsNullOrEmpty(RepositoryExplorerVM?.CurentRepositoryName) == false)
                {
                    title = $"{RepositoryExplorerVM?.CurentRepositoryName} - Чубушник {AssemblyVersion}";
                }
                return title;
            }
        }
    
        /// <summary>
        /// Выполняет операцию AssemblyVersion.
        /// </summary>
        /// <returns>Полученные данные.</returns>
        public string AssemblyVersion { get => $"v.{Assembly.GetExecutingAssembly().GetName().Version}"; }
       
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string UserName { get => _mainWindowNotificationsVM.MessageLogControlVM.MessagingUserName; }
    
        /// <summary>
        /// Уведомление.
        /// </summary>
        public MainWindowNotificationsVM MainWindowNotificationsVM { get => _mainWindowNotificationsVM; }

        /// <summary>
        /// Модель представления выбора темы оформления (вкладка ленты «Вид»).
        /// На платформах без поддержки тем (текущий WPF) может быть null.
        /// </summary>
        public ThemeSettingsVM? ThemeSettingsVM { get => _themeSettingsVM; }
     
        /// <summary>
        /// Выбранная модель представления элемента.
        /// </summary>
        public IMainEntityVM<IMainEntityModel> SelectedElementVM { get => _repositoryExplorerControlVM?.SelectedRepositoryMember; } //TODO: Временно только элементы репозитория

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="MainWindowVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="options">Параметры конфигурации приложения.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <param name="repositoryExplorerControlVM">Параметр repositoryExplorerControlVM.</param>
        /// <param name="extensionVMFactory">Фабрика создания модели представления расширений.</param>
        /// <param name="applicationSettingsControlVM">Параметр applicationSettingsControlVM.</param>
        /// <param name="reportsControlVM">Параметр reportsControlVM.</param>
        /// <param name="mainWindowNotificationsVM">Параметр mainWindowNotificationsVM.</param>
        /// <param name="commandFactory">Фабрика синхронных команд.</param>
        /// <param name="windowService">Сервис управления окнами.</param>
        /// <param name="themeSettingsVM">Модель представления выбора темы. Опционально: на платформах
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public MainWindowVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IOptions<ApplicationSettingsConfig> options,
            ApplicationCommandsVM applicationCommandsVM,
            RepositoryExplorerControlVM repositoryExplorerControlVM,
            IExtensionsControlVMFactory extensionVMFactory,
            ApplicationSettingsControlVM applicationSettingsControlVM,
            ReportsControlVM reportsControlVM,
            MainWindowNotificationsVM mainWindowNotificationsVM,
            IRelayCommandFactory commandFactory,
            IWindowService windowService,
            ThemeSettingsVM? themeSettingsVM = null)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);
            ArgumentNullException.ThrowIfNull(repositoryExplorerControlVM);
            ArgumentNullException.ThrowIfNull(extensionVMFactory);
            ArgumentNullException.ThrowIfNull(applicationSettingsControlVM);
            ArgumentNullException.ThrowIfNull(reportsControlVM);
            ArgumentNullException.ThrowIfNull(mainWindowNotificationsVM);
            ArgumentNullException.ThrowIfNull(commandFactory);
            ArgumentNullException.ThrowIfNull(windowService);

            _repositoryExplorerControlVM = repositoryExplorerControlVM;
            _importExportControlVM = ActivatorUtilities.CreateInstance<ImportExportControlVM>(_serviceProvider, repositoryExplorerControlVM);
            _formulaTestControlVM = ActivatorUtilities.CreateInstance<FormulaTestControlVM>(_serviceProvider, repositoryExplorerControlVM);
            _extensionsControlVM = extensionVMFactory.Create(repositoryExplorerControlVM);
            _applicationSettingsControlVM = applicationSettingsControlVM;
            _reportsControlVM = reportsControlVM;
            _mainWindowNotificationsVM = mainWindowNotificationsVM;
            _commandFactory = commandFactory;
            _windowService = windowService;
            // Опционально: на платформах без поддержки тем (текущий WPF) сервис не зарегистрирован в DI,
            // и ActivatorUtilities подставит значение по умолчанию (null).
            _themeSettingsVM = themeSettingsVM;

            _notificationService.SendTextMessage<MainWindowVM>("Основное окно. Начало инициализации расширений.", NotificationCriticalLevelModel.Info);
            _extensionsControlVM.InitializeAsync(options.Value.PluginsDirectories);
            _notificationService.SendTextMessage<MainWindowVM>($"Основное окно. Расширения инициализированы ({ExtensionsControlVM.Extensions?.Count()} шт.).", NotificationCriticalLevelModel.Ok);
        }

        /// <summary>
        /// Команда выполнения операции стартового окна.
        /// </summary>
        public IRelayCommand OpenLaunchWindowCommand => _applicationCommandsVM.OpenLaunchWindowCommand;

        /// <summary>
        /// Команда открытия редактора формул.
        /// </summary>
        public IRelayCommand OpenFormulaEditorWindowCommand => _applicationCommandsVM.OpenFormulaEditorWindowCommand;

        public IRelayCommand OpenRepositoryMemberDetailsWindowCommand
        {
            get
            {
                return _commandFactory.Create(obj =>
                {
                    var vm = _serviceProvider.GetRequiredService<IMainWindowVMFactory>().Create(RepositoryExplorerVM);
                    if (vm.SelectedElementVM is null)
                    {
                        return;
                    }

                    _windowService.Show(new DetailsWindowVM(vm.SelectedElementVM));
                });
            }
        }
    }
}