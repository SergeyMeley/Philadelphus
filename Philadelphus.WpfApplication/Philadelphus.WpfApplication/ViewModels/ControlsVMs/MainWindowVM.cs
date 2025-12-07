using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Philadelphus.Business.Config;
using Philadelphus.Core.Domain.ExtensionSystem.Services;
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
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationCommandsVM _applicationCommandsVM;
        private readonly ExtensionsControlVM _extensionVM;
        public ExtensionsControlVM ExtensionVM => _extensionVM;

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

        private RepositoryExplorerControlVM _repositoryExplorerVM;
        public RepositoryExplorerControlVM RepositoryExplorerVM
        {
            get
            {
                return _repositoryExplorerVM;
            }
            set
            {
                _repositoryExplorerVM = value;
            }
        }
        private NotificationsVM _notificationsVM;
        public NotificationsVM NotificationsVM { get => _notificationsVM; }
        public ViewModelBase SelectedElementVM { get => _repositoryExplorerVM?.SelectedRepositoryMember; } //TODO: Временно только элементы репозитория

        public MainWindowVM(
            IServiceProvider serviceProvider,
            IOptions<ApplicationSettings> options,
            ApplicationCommandsVM applicationCommandsVM,
            ExtensionsControlVM extensionVM)
        {
            _serviceProvider = serviceProvider;
            _extensionVM = extensionVM;
            _applicationCommandsVM = applicationCommandsVM;

            extensionVM.InitializeAsync(options.Value.PluginsDirectoriesString);
        }

        public RelayCommand OpenLaunchWindowCommand => _applicationCommandsVM.OpenLaunchWindowCommand;
        public RelayCommand OpenRepositoryMemberDetailsWindowCommand => _applicationCommandsVM.OpenRepositoryMemberDetailsWindowCommand;
    }
}
