using Microsoft.Extensions.DependencyInjection;
using Philadelphus.WpfApplication.Infrastructure;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs;
using Philadelphus.WpfApplication.ViewModels.SupportiveVMs;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class MainWindowVM : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationCommandsVM _applicationCommandsVM;

        public string Title
        {
            get
            {
                var title = "Чубушник";
                var repositoryName = RepositoryExplorerVM?.Name;
                if (String.IsNullOrEmpty(repositoryName) == false)
                {
                    title = $"{repositoryName} - Чубушник";
                }
                return title;
            }
        }

        private TreeRepositoryVM _repositoryExplorerVM;
        public TreeRepositoryVM RepositoryExplorerVM
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
            ApplicationCommandsVM applicationCommandsVM)
        {
            _serviceProvider = serviceProvider;
            _applicationCommandsVM = applicationCommandsVM;
        }

        public RelayCommand OpenLaunchWindowCommand => _applicationCommandsVM.OpenLaunchWindowCommand;
        public RelayCommand OpenRepositoryMemberDetailsWindowCommand => _applicationCommandsVM.OpenRepositoryMemberDetailsWindowCommand;
    }
}
