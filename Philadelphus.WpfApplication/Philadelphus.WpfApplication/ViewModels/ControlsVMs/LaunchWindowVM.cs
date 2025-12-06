using Philadelphus.WpfApplication.Infrastructure;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.WpfApplication.ViewModels.EntitiesVMs.MainEntitiesVMs;

namespace Philadelphus.WpfApplication.ViewModels.ControlsVMs
{
    public class LaunchWindowVM : ControlVM
    {
        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }

        private TreeRepositoryCollectionVM _repositoryCollectionVM;
        public TreeRepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private TreeRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
        public TreeRepositoryHeadersCollectionVM RepositoryHeadersCollectionVM { get => _repositoryHeadersCollectionVM; }
        public string UserName { get => Environment.UserName; }
        public LaunchWindowVM(
            IServiceProvider serviceProvider,
            DataStoragesSettingsVM dataStoragesSettingsVM,
            TreeRepositoryCollectionVM repositoryCollectionVM,
            TreeRepositoryHeadersCollectionVM repositoryHeadersCollectionVM,
            ApplicationCommandsVM applicationCommandsVM)
        {
            ;
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
            _repositoryCollectionVM = repositoryCollectionVM;
            _repositoryHeadersCollectionVM = repositoryHeadersCollectionVM;
            _repositoryHeadersCollectionVM.CheckTreeRepositoryAvailableAction = x => CheckTreeRepositoryAvailable(x);
            _openMainWindowCommand = applicationCommandsVM.OpenMainWindowCommand;
        }

        private RelayCommand _openMainWindowCommand;
        public RelayCommand OpenMainWindowCommand { get => _openMainWindowCommand; }
        public RelayCommand OpenMainWindowWithHeaderCommand
        {
            get
            {
                return new RelayCommand(
                    obj =>
                {
                    var headerVM = RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM;
                    RepositoryCollectionVM.CurrentRepositoryVM = RepositoryCollectionVM.TreeRepositoriesVMs.FirstOrDefault(x => x.Guid == headerVM.Guid);

                    if (_openMainWindowCommand.CanExecute(obj))
                        _openMainWindowCommand.Execute(obj);
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

            //var dataStorage = DataStoragesSettingsVM.DataStorageVMs.Select(x => x.Model).FirstOrDefault(x => x.Guid == header.OwnDataStorageUuid);
            //if (dataStorage == null)
            //{
            //    //var text = $"Не найдено хранилище данных {header.OwnDataStorageName} [{header.OwnDataStorageUuid}] для заголовка репозитория {header.Name} [{header.Guid}]";
            //    //NotificationService.SendNotification(text, NotificationCriticalLevelModel.Warning, NotificationTypesModel.TextMessage);

            //    return false;
            //}

            if (RepositoryCollectionVM.CheckTreeRepositoryVMAvailable(header.Guid, out var treeRepositoryVM))
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
                    if (header.OwnDataStorageUuid != treeRepositoryVM.OwnDataStorage.Guid)
                        header.OwnDataStorageUuid = treeRepositoryVM.OwnDataStorage.Guid;
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
