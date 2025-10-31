using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Services;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels;
using Philadelphus.WpfApplication.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class RepositoryCreationVM : ViewModelBase
    {
        private RepositoryCreationWindow _window;

        private TreeRepositoryCollectionService _service;

        private string _name;
        public string Name { get => _name; set => _name = value; }

        private string _description;
        public string Description { get => _description; set => _description = value; }

        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; set => _dataStoragesSettingsVM = value; }

        private TreeRepositoryCollectionVM _repositoryCollectionVM;
        public RepositoryCreationVM(TreeRepositoryCollectionService service, TreeRepositoryCollectionVM repositoryCollectionVM, RelayCommand openDataStoragesSettingsWindowCommand, DataStoragesSettingsVM dataStoragesSettingsVM)
        {
            _service = service;
            _repositoryCollectionVM = repositoryCollectionVM;
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
            OpenDataStoragesSettingsWindowCommand = openDataStoragesSettingsWindowCommand;
        }
        public void OpenWindow()
        {
            _window = new RepositoryCreationWindow(this);
            _window.ShowDialog();
        }
        public void CloseWindow()
        {
            _window.Close();
            _window = null;
        }
        public RelayCommand CreateAndSaveRepositoryCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_dataStoragesSettingsVM.SelectedDataStorageVM == null)
                        return;
                    var result = _service.CreateNewTreeRepository(_dataStoragesSettingsVM.SelectedDataStorageVM.DataStorage);
                    var service = new TreeRepositoryService(result);
                    result.Name = _name;
                    result.Description = _description;
                    service.SaveChanges(result);
                    _repositoryCollectionVM.TreeRepositoriesVMs.Add(new TreeRepositoryVM(result));
                    CloseWindow();
                });
            }
        }
        public RelayCommand OpenDataStoragesSettingsWindowCommand { get; }
    }
}
