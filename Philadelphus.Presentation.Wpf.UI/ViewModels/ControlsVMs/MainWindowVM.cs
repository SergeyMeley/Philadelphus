using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.NotificationsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using Serilog;
using System.Reflection;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления для главного окна.
    /// </summary>
    public class MainWindowVM : ControlBaseVM
    {
        private readonly ExtensionsControlVM _extensionsControlVM;
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;
        private readonly ApplicationSettingsControlVM _applicationSettingsControlVM;
        private readonly ReportsControlVM _reportsControlVM;
        private readonly MainWindowNotificationsVM _mainWindowNotificationsVM;

        /// <summary>
        /// Команды приложения.
        /// </summary>
        public ApplicationCommandsVM ApplicationCommandsVM { get => _applicationCommandsVM; }
      
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
            MainWindowNotificationsVM mainWindowNotificationsVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.Value);
            ArgumentNullException.ThrowIfNull(repositoryExplorerControlVM);
            ArgumentNullException.ThrowIfNull(extensionVMFactory);
            ArgumentNullException.ThrowIfNull(applicationSettingsControlVM);
            ArgumentNullException.ThrowIfNull(reportsControlVM);
            ArgumentNullException.ThrowIfNull(mainWindowNotificationsVM);

            _repositoryExplorerControlVM = repositoryExplorerControlVM;
            _extensionsControlVM = extensionVMFactory.Create(repositoryExplorerControlVM);
            _applicationSettingsControlVM = applicationSettingsControlVM;
            _reportsControlVM = reportsControlVM;
            _mainWindowNotificationsVM = mainWindowNotificationsVM;

            _notificationService.SendTextMessage<MainWindowVM>("Основное окно. Начало инициализации расширений.", NotificationCriticalLevelModel.Info);
            _extensionsControlVM.InitializeAsync(options.Value.PluginsDirectories);
            _notificationService.SendTextMessage<MainWindowVM>($"Основное окно. Расширения инициализированы ({ExtensionsControlVM.Extensions?.Count()} шт.).", NotificationCriticalLevelModel.Ok);
        }

        /// <summary>
        /// Команда выполнения операции стартового окна.
        /// </summary>
        public RelayCommand OpenLaunchWindowCommand => _applicationCommandsVM.OpenLaunchWindowCommand;
        public RelayCommand OpenRepositoryMemberDetailsWindowCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var vm = _serviceProvider.GetRequiredService<IMainWindowVMFactory>().Create(RepositoryExplorerVM);
                    var window = new DetailsWindow(vm.SelectedElementVM);
                    window.Show();
                });
            }
        }
    }
}
