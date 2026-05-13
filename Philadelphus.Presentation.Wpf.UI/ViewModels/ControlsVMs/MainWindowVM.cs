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
    public class MainWindowVM : ControlBaseVM
    {
        private readonly ExtensionsControlVM _extensionsControlVM;
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;
        private readonly ApplicationSettingsControlVM _applicationSettingsControlVM;
        private readonly ReportsControlVM _reportsControlVM;
        private readonly MainWindowNotificationsVM _mainWindowNotificationsVM;

        public ApplicationCommandsVM ApplicationCommandsVM { get => _applicationCommandsVM; }
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
        public string AssemblyVersion { get => $"v.{Assembly.GetExecutingAssembly().GetName().Version}"; }
        public string UserName { get => _mainWindowNotificationsVM.MessageLogControlVM.MessagingUserName; }
        public MainWindowNotificationsVM MainWindowNotificationsVM { get => _mainWindowNotificationsVM; }
        public IMainEntityVM<IMainEntityModel> SelectedElementVM { get => _repositoryExplorerControlVM?.SelectedRepositoryMember; } //TODO: Временно только элементы репозитория

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
