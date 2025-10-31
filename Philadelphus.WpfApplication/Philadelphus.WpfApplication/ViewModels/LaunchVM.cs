using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels;
using Philadelphus.WpfApplication.Views;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }
        public RelayCommand OpenRepositoryCreationWindowCommand { get; }
        public RelayCommand OpenMainWindowCommand { get; }
        public RelayCommand OpenMainWindowWithHeaderCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var result = GetTreeRepositoryByHeader(_repositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM, out TreeRepositoryVM model);
                    RepositoryCollectionVM.TreeRepositoriesVMs.Add(model);
                    RepositoryCollectionVM.CurrentRepositoryExplorerVM = model;
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
                    return GetTreeRepositoryByHeader(_repositoryHeadersCollectionVM.SelectedTreeRepositoryHeaderVM, out TreeRepositoryVM model);
                });
            }
        }

        private bool GetTreeRepositoryByHeader(TreeRepositoryHeaderVM treeRepositoryHeaderVM, out TreeRepositoryVM outTreeRepositoryVM)
        {
            if (DataStoragesSettingsVM == null)
                throw new NullReferenceException("Не инициализирована модель представления настроек хранилищ данных.");
            if (DataStoragesSettingsVM.DataStorageVMs == null)
                throw new NullReferenceException("Не инициализированы модели представлений хранилищ данных.");

            var dataStorage = DataStoragesSettingsVM.DataStorageVMs.Select(x => x.DataStorage).FirstOrDefault(x => x.Guid == treeRepositoryHeaderVM.OwnDataStorageUuid);
            if (dataStorage == null)
                throw new NullReferenceException("Отсутствует подходящее хранилище данных.");

            outTreeRepositoryVM = RepositoryCollectionVM.TreeRepositoriesVMs.FirstOrDefault(x => x.Guid == treeRepositoryHeaderVM.Guid);

            if (outTreeRepositoryVM != null)
                return true;

            return false;
        }

    }
}
