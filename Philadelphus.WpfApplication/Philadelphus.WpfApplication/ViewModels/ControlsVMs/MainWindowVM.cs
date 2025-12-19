using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Philadelphus.Business.Config;
using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Services.Interfaces;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
using Philadelphus.WpfApplication.Factories.Interfaces;
using Philadelphus.WpfApplication.Infrastructure;
using Philadelphus.WpfApplication.ViewModels.SupportiveVMs;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels.ControlsVMs
{
    public class MainWindowVM : ControlVM
    {
        private readonly ApplicationCommandsVM _applicationCommandsVM;
        private readonly ExtensionsControlVM _extensionsControlVM;
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;
        public ApplicationCommandsVM ApplicationCommandsVM { get => _applicationCommandsVM; }
        public ExtensionsControlVM ExtensionsControlVM { get => _extensionsControlVM; }
        public RepositoryExplorerControlVM RepositoryExplorerVM
        {
            get
            {
                return _repositoryExplorerControlVM;
            }
        }
        public string Title
        {
            get
            {
                var title = "Чубушник";
                var repositoryName = RepositoryExplorerVM?.CurentRepositoryName;
                if (String.IsNullOrEmpty(repositoryName) == false)
                {
                    title = $"{repositoryName} - Чубушник";
                }
                return title;
            }
        }

        private NotificationsVM _notificationsVM;
        public NotificationsVM NotificationsVM { get => _notificationsVM; }
        public ViewModelBase SelectedElementVM { get => _repositoryExplorerControlVM?.SelectedRepositoryMember; } //TODO: Временно только элементы репозитория

        public MainWindowVM(
            IServiceProvider serviceProvider,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            IOptions<ApplicationSettings> options,
            ApplicationCommandsVM applicationCommandsVM,
            ExtensionsControlVM extensionVM,
            RepositoryExplorerControlVM repositoryExplorerControlVM)
            : base(serviceProvider, logger, notificationService)
        {
            _applicationCommandsVM = applicationCommandsVM;
            _extensionsControlVM = extensionVM;
            _repositoryExplorerControlVM = repositoryExplorerControlVM;

            _notificationService.SendTextMessage("Основное окно. Начало инициализации расширений", NotificationCriticalLevelModel.Info);
            _extensionsControlVM.InitializeAsync(options.Value.PluginsDirectoriesString);
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
