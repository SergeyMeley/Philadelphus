using Philadelphus.Business.Services;
using Philadelphus.WpfApplication.ViewModels.InfrastructureVMs;
using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs;
using Philadelphus.WpfApplication.Views.Windows;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class RepositoryCreationVM : ViewModelBase
    {
        private TreeRepositoryCollectionService _service;

        private string _name;
        public string Name { get => _name; set => _name = value; }

        private string _description;
        public string Description { get => _description; set => _description = value; }

        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; set => _dataStoragesSettingsVM = value; }

        private TreeRepositoryCollectionVM _repositoryCollectionVM;
        public RepositoryCreationVM(TreeRepositoryCollectionService service, TreeRepositoryCollectionVM repositoryCollectionVM, DataStoragesSettingsVM dataStoragesSettingsVM)
        {
            _service = service;
            _repositoryCollectionVM = repositoryCollectionVM;
            _dataStoragesSettingsVM = dataStoragesSettingsVM;
        }
        public RelayCommand CreateAndSaveRepositoryCommand
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    if (_dataStoragesSettingsVM.SelectedDataStorageVM == null)
                        return;
                    var result = _service.CreateNewTreeRepository(_dataStoragesSettingsVM.SelectedDataStorageVM.Model);
                    var service = new TreeRepositoryService(result);
                    result.Name = _name;
                    result.Description = _description;
                    service.SaveChanges(result);
                    _repositoryCollectionVM.TreeRepositoriesVMs.Add(new TreeRepositoryVM(result));
                });
            }
        }
    }
}
