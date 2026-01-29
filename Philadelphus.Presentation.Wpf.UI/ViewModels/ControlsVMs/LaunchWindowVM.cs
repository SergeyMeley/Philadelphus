using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs.TabItemsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Controls.TabItemsControls.LaunchWindowTabItemsControls;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class LaunchWindowVM : ControlBaseVM
    {
        public List<LaunchWindowTabItemVM> LaunchWindowTabItemsVMs { get; set; }

        private LaunchWindowTabItemVM _selectedLaunchWindowTabItemVM;
        public LaunchWindowTabItemVM SelectedLaunchWindowTabItemVM
        {
            get => _selectedLaunchWindowTabItemVM;
            set
            {
                if (_selectedLaunchWindowTabItemVM != value)
                {
                    _selectedLaunchWindowTabItemVM = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly ApplicationSettingsControlVM _applicationSettingsControlVM;
        public ApplicationSettingsControlVM ApplicationSettingsControlVM { get => _applicationSettingsControlVM; }

        private DataStoragesCollectionVM _dataStoragesCollectionVM;
        public DataStoragesCollectionVM DataStoragesSettingsVM { get => _dataStoragesCollectionVM; }

        private TreeRepositoryCollectionVM _repositoryCollectionVM;
        public TreeRepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private TreeRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
        public TreeRepositoryHeadersCollectionVM RepositoryHeadersCollectionVM { get => _repositoryHeadersCollectionVM; }

        private RepositoryCreationControlVM _repositoryCreationVM;
        public RepositoryCreationControlVM RepositoryCreationVM { get => _repositoryCreationVM; }
        public string UserName { get => Environment.UserName; }
        public LaunchWindowVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            TreeRepositoryCollectionVM repositoryCollectionVM,
            TreeRepositoryHeadersCollectionVM repositoryHeadersCollectionVM,
            RepositoryCreationControlVM repositoryCreationControlVM,
            ApplicationCommandsVM applicationCommandsVM,
            ApplicationSettingsControlVM applicationSettingsControlVM)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _dataStoragesCollectionVM = dataStoragesCollectionVM;
            _repositoryCollectionVM = repositoryCollectionVM;
            _repositoryHeadersCollectionVM = repositoryHeadersCollectionVM;
            _repositoryHeadersCollectionVM.CheckTreeRepositoryAvailableAction = x => CheckTreeRepositoryAvailable(x);
            _repositoryCreationVM = repositoryCreationControlVM;
            _applicationSettingsControlVM = applicationSettingsControlVM;

            var tab1 = _serviceProvider.GetRequiredService<LaunchWindowTabItemVM>();
            tab1.Header = "Главная";
            tab1.IconKey = "imageRepository";
            tab1.Content = new LaunchWindowMainTabControl() { DataContext = this };

            var tab2 = _serviceProvider.GetRequiredService<LaunchWindowTabItemVM>();
            tab2.Header = "Создать";
            tab2.IconKey = "imageAdd";
            tab2.Content = new LaunchWindowCreatingTabControl() { DataContext = this };

            var tab3 = _serviceProvider.GetRequiredService<LaunchWindowTabItemVM>();
            tab3.Header = "Открыть";
            tab3.IconKey = "imageOpen";
            tab3.Content = new LaunchWindowOpeningTabControl() { DataContext = this };

            var tab4 = _serviceProvider.GetRequiredService<LaunchWindowTabItemVM>();
            tab4.Header = "Хранилища";
            tab4.IconKey = "imageStorage";
            tab4.Content = new LaunchWindowStoragesTabControl() { DataContext = this };

            var tab5 = _serviceProvider.GetRequiredService<LaunchWindowTabItemVM>();
            tab5.Header = "Настройки";
            tab5.IconKey = "imageSettings";
            tab5.Content = new LaunchWindowSettingsTabControl() { DataContext = this };

            LaunchWindowTabItemsVMs = new List<LaunchWindowTabItemVM> { tab1, tab2, tab3, tab4, tab5 };

            SelectedLaunchWindowTabItemVM = LaunchWindowTabItemsVMs.FirstOrDefault(t => t.Content is LaunchWindowSettingsTabControl);
        }

        public RelayCommand OpenMainWindowCommand => _applicationCommandsVM.OpenMainWindowCommand;
        public RelayCommand OpenMainWindowWithHeaderCommand
        {
            get
            {
                return new RelayCommand(
                    obj =>
                {
                    var headerVM = RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM;
                    RepositoryCollectionVM.CurrentRepositoryVM = RepositoryCollectionVM.TreeRepositoriesVMs.FirstOrDefault(x => x.Uuid == headerVM.Uuid);

                    if (OpenMainWindowCommand.CanExecute(obj))
                        OpenMainWindowCommand.Execute(obj);
                },
                ce =>
                {
                    if (RepositoryHeadersCollectionVM == null)
                        return false;
                    if (RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM == null)
                        return false;
                    if (RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM is TreeRepositoryHeaderVM == false)
                        return false;
                    return RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM.IsTreeRepositoryAvailable;
                });
                
            }
        }

        private bool CheckTreeRepositoryAvailable(TreeRepositoryHeaderVM header)
        {
            if (header == null)
                return false;
            if (DataStoragesSettingsVM == null)
                throw new NullReferenceException("Не инициализирована модель представления настроек хранилищ данных.");
            if (DataStoragesSettingsVM.DataStorageVMs == null)
                throw new NullReferenceException("Не инициализированы модели представлений хранилищ данных.");

            //var dataStorage = DataStoragesSettingsVM.DataStorageVMs.Select(x => x.Model).FirstOrDefault(x => x.Uuid == header.OwnDataStorageUuid);
            //if (dataStorage == null)
            //{
            //    //var text = $"Не найдено хранилище данных {header.OwnDataStorageName} [{header.OwnDataStorageUuid}] для заголовка репозитория {header.Name} [{header.Uuid}]";
            //    //NotificationService.SendNotification(text, NotificationCriticalLevelModel.Warning, NotificationTypesModel.TextMessage);

            //    return false;
            //}

            if (RepositoryCollectionVM.CheckTreeRepositoryVMAvailable(header.Uuid, out var treeRepositoryVM))
            {
                if (treeRepositoryVM != null)
                {
                    header.IsTreeRepositoryAvailable = true;
                    if (header.Name != treeRepositoryVM.Name)
                        header.Name = treeRepositoryVM.Name;
                    if (header.Description != treeRepositoryVM.Description)
                        header.Description = treeRepositoryVM.Description;
                    if (header.OwnDataStorageName != treeRepositoryVM.OwnDataStorage.Name)
                        header.OwnDataStorageName = treeRepositoryVM.OwnDataStorage.Name;
                    if (header.OwnDataStorageUuid != treeRepositoryVM.OwnDataStorage.Uuid)
                        header.OwnDataStorageUuid = treeRepositoryVM.OwnDataStorage.Uuid;
                    return true;
                }
                return false;
            }
            else
            {
                header.IsTreeRepositoryAvailable = false;
                return false;
            }
                
        }
    }
}
