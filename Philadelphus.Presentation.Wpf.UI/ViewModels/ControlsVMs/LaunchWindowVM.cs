using Microsoft.Extensions.Logging;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class LaunchWindowVM : ControlBaseVM
    {
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
            ILogger<RepositoryCreationControlVM> logger,
            INotificationService notificationService,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            TreeRepositoryCollectionVM repositoryCollectionVM,
            TreeRepositoryHeadersCollectionVM repositoryHeadersCollectionVM,
            RepositoryCreationControlVM repositoryCreationControlVM,
            ApplicationCommandsVM applicationCommandsVM)
            : base(serviceProvider, logger, notificationService, applicationCommandsVM)
        {
            _dataStoragesCollectionVM = dataStoragesCollectionVM;
            _repositoryCollectionVM = repositoryCollectionVM;
            _repositoryHeadersCollectionVM = repositoryHeadersCollectionVM;
            _repositoryHeadersCollectionVM.CheckTreeRepositoryAvailableAction = x => CheckTreeRepositoryAvailable(x);
            _repositoryCreationVM = repositoryCreationControlVM;
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
