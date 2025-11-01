using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.OtherEntities;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels;
using Philadelphus.WpfApplication.Views;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Windows;
using Timers = System.Timers;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class LaunchVM : ViewModelBase
    {
        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }

        private TreeRepositoryCollectionVM _repositoryCollectionVM;
        public TreeRepositoryCollectionVM RepositoryCollectionVM { get => _repositoryCollectionVM; }

        private TreeRepositoryHeadersCollectionVM _repositoryHeadersCollectionVM;
        public TreeRepositoryHeadersCollectionVM RepositoryHeadersCollectionVM { get => _repositoryHeadersCollectionVM; }
        public string UserName { get => Environment.UserName; }
        public LaunchVM(DataStoragesSettingsVM dataStoragesSettingsVM, TreeRepositoryCollectionVM repositoryCollectionVM, TreeRepositoryHeadersCollectionVM repositoryHeadersCollectionVM, RelayCommand openRepositoryCreationWindowCommand, RelayCommand openMainWindowCommand)
        {
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
            _repositoryCollectionVM = repositoryCollectionVM;
            _repositoryHeadersCollectionVM = repositoryHeadersCollectionVM;
            _repositoryHeadersCollectionVM.CheckTreeRepositoryAvailableAction = x => CheckTreeRepositoryAvailable(x);
            OpenRepositoryCreationWindowCommand = openRepositoryCreationWindowCommand;
            _openMainWindowCommand = openMainWindowCommand;
        }
        public RelayCommand OpenRepositoryCreationWindowCommand { get; }

        private RelayCommand _openMainWindowCommand;
        public RelayCommand OpenMainWindowWithHeaderCommand
        {
            get
            {
                return new RelayCommand(
                    obj =>
                {
                    var headerVM = RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM;
                    RepositoryCollectionVM.CurrentRepositoryExplorerVM = RepositoryCollectionVM.TreeRepositoriesVMs.FirstOrDefault(x => x.Guid == headerVM.Guid);

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
            if (DataStoragesSettingsVM == null)
                throw new NullReferenceException("Не инициализирована модель представления настроек хранилищ данных.");
            if (DataStoragesSettingsVM.DataStorageVMs == null)
                throw new NullReferenceException("Не инициализированы модели представлений хранилищ данных.");

            //var dataStorage = DataStoragesSettingsVM.DataStorageVMs.Select(x => x.DataStorage).FirstOrDefault(x => x.Guid == header.OwnDataStorageUuid);
            //if (dataStorage == null)
            //{
            //    //var text = $"Не найдено хранилище данных {header.OwnDataStorageName} [{header.OwnDataStorageUuid}] для заголовка репозитория {header.Name} [{header.Guid}]";
            //    //NotificationService.SendNotification(text, NotificationCriticalLevelModel.Warning, NotificationTypesModel.TextMessage);

            //    return false;
            //}

            var treeRepositoryVM = RepositoryCollectionVM.TreeRepositoriesVMs.FirstOrDefault(x => x.Guid == header.Guid);

            if (treeRepositoryVM != null)
            {
                header.IsTreeRepositoryAvailable = true;
                header.Name = treeRepositoryVM.Name;
                header.Description = treeRepositoryVM.Description;
                header.OwnDataStorageName = treeRepositoryVM.OwnDataStorage.Name;
                header.OwnDataStorageUuid = treeRepositoryVM.OwnDataStorage.Guid;
                return true;
            }
            else
            {
                header.IsTreeRepositoryAvailable = false;
                return false;
            }
                
        }
    }
}
