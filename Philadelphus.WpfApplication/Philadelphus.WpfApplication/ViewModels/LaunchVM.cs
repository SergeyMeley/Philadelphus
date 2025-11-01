using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Services;
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
            OpenRepositoryCreationWindowCommand = openRepositoryCreationWindowCommand;
            OpenMainWindowCommand = openMainWindowCommand;
            StartCheckingTreeRepositoriesAvailable();
        }
        public RelayCommand OpenRepositoryCreationWindowCommand { get; }
        public RelayCommand OpenMainWindowCommand { get; }
        public RelayCommand OpenMainWindowWithHeaderCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    RepositoryCollectionVM.TreeRepositoriesVMs.Add(RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM.TreeRepositoryVM);
                    RepositoryCollectionVM.CurrentRepositoryExplorerVM = RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM.TreeRepositoryVM;
                    var command = OpenMainWindowCommand;
                },
                ce =>
                {
                    if (RepositoryHeadersCollectionVM == null)
                        return false;
                    if (RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM == null)
                        return false;
                    if (RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM is TreeRepositoryHeaderModel == false)
                        return false;
                    return RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM.IsTreeRepositoryAvailable;
                });
            }
        }

        private void StartCheckingTreeRepositoriesAvailable()
        {
            Timers.Timer timer = new Timers.Timer(5000);
            timer.Elapsed += CheckTreeRepositoriesAvailable;
            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }

        private void CheckTreeRepositoriesAvailable(object? sender, ElapsedEventArgs e)
        {
            foreach (var header in RepositoryHeadersCollectionVM.TreeRepositoryHeadersVMs)
            {
                if (DataStoragesSettingsVM == null)
                    throw new NullReferenceException("Не инициализирована модель представления настроек хранилищ данных.");
                if (DataStoragesSettingsVM.DataStorageVMs == null)
                    throw new NullReferenceException("Не инициализированы модели представлений хранилищ данных.");

                var dataStorage = DataStoragesSettingsVM.DataStorageVMs.Select(x => x.DataStorage).FirstOrDefault(x => x.Guid == header.OwnDataStorageUuid);
                if (dataStorage == null)
                {
                    //var text = $"Не найдено хранилище данных {header.OwnDataStorageName} [{header.OwnDataStorageUuid}] для заголовка репозитория {header.Name} [{header.Guid}]";
                    //NotificationService.SendNotification(text, NotificationCriticalLevelModel.Warning, NotificationTypesModel.TextMessage);
                    return;
                }

                var treeRepositoryVM = RepositoryCollectionVM.TreeRepositoriesVMs.FirstOrDefault(x => x.Guid == header.Guid);

                RepositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM.TreeRepositoryVM = treeRepositoryVM;

                if (treeRepositoryVM != null)
                {
                    RepositoryCollectionVM.CurrentRepositoryExplorerVM = treeRepositoryVM;
                }
            }
        }
    }
}
