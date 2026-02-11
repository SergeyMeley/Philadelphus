using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Core.Domain.Configurations;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.SupportiveVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class MainWindowVM : ControlBaseVM
    {
        private readonly ExtensionsControlVM _extensionsControlVM;
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;
        private readonly ApplicationSettingsControlVM _applicationSettingsControlVM;
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
        public string Title
        {
            get
            {
                var title = "Чубушник";
                if (string.IsNullOrEmpty(RepositoryExplorerVM?.CurentRepositoryName) == false)
                {
                    title = $"{RepositoryExplorerVM?.CurentRepositoryName} - Чубушник";
                }
                return title;
            }
        }

        private NotificationsVM _notificationsVM;
        public NotificationsVM NotificationsVM { get => _notificationsVM; }
        public ViewModelBase SelectedElementVM { get => _repositoryExplorerControlVM?.SelectedRepositoryMember; } //TODO: Временно только элементы репозитория

        public MainWindowVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            IOptions<ApplicationSettingsConfig> options,
            ApplicationCommandsVM applicationCommandsVM,
            RepositoryExplorerControlVM repositoryExplorerControlVM,
            IExtensionsControlVMFactory extensionVMFactory,
            ApplicationSettingsControlVM applicationSettingsControlVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _repositoryExplorerControlVM = repositoryExplorerControlVM;
            _extensionsControlVM = extensionVMFactory.Create(repositoryExplorerControlVM);
            _applicationSettingsControlVM = applicationSettingsControlVM;

            _notificationService.SendTextMessage("Основное окно. Начало инициализации расширений", NotificationCriticalLevelModel.Info);
            _extensionsControlVM.InitializeAsync(options.Value.PluginsDirectories);
            _notificationService.SendTextMessage($"Основное окно. Расширения инициализированы ({ExtensionsControlVM.Extensions?.Count()} шт)", NotificationCriticalLevelModel.Info);
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
