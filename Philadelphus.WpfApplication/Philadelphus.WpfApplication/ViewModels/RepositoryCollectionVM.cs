using Philadelphus.Business.Entities.Infrastructure;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.WpfApplication.Models.Entities.Enums;
using Philadelphus.WpfApplication.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class RepositoryCollectionVM : ViewModelBase
    {
        private DataStoragesSettingsVM _dataStoragesSettingsVM;
        public DataStoragesSettingsVM DataStoragesSettingsVM { get => _dataStoragesSettingsVM; }
        public RepositoryCollectionVM(DataStoragesSettingsVM dataStoragesSettings)
        {
            _dataStoragesSettingsVM = dataStoragesSettings;
            InitRepositoryCollection();
            PropertyGridRepresentation = PropertyGridRepresentations.DataGrid;
        }
        private static RepositoryExplorerVM _currentRepositoryExplorerVM;
        public  RepositoryExplorerVM CurrentRepositoryExplorerVM 
        { 
            get 
            { 
                return _currentRepositoryExplorerVM; 
            }
            set
            {
                _currentRepositoryExplorerVM = value;
                OnPropertyChanged(nameof(CurrentRepositoryExplorerVM));
                OnPropertyChanged(nameof(PropertyList));
            }
        }

        private ObservableCollection<RepositoryExplorerVM> _treeRepositoriesVMs = new ObservableCollection<RepositoryExplorerVM>();
        public ObservableCollection<RepositoryExplorerVM> TreeRepositoriesVMs
        {
            get
            {
                var treeRepositoryService = new DataTreeProcessingService();
                var treeRepositories = treeRepositoryService.GetExistTreeRepositories(_dataStoragesSettingsVM.DataStorages);
                if (treeRepositories != null && _treeRepositoriesVMs == null)
                {
                    _treeRepositoriesVMs = new ObservableCollection<RepositoryExplorerVM>();
                    foreach (var item in treeRepositories)
                    {
                        var vm = new RepositoryExplorerVM(item);
                        _treeRepositoriesVMs.Add(vm);
                    }
                    for (int i = 0; i < 5; i++)
                    {
                        //Временно
                        var dataStorage = DataStoragesSettingsVM.DataStorages[i % _dataStoragesSettingsVM.DataStorages.Count];
                        var treeRepository = treeRepositoryService.CreateSampleRepository(dataStorage);
                        var vm = new RepositoryExplorerVM(treeRepository);
                        _treeRepositoriesVMs.Add(vm);
                        //Временно
                    }
                }
                return _treeRepositoriesVMs;
            }
            //private set
            //{
            //    _treeRepositoriesVMs = value.ToList();
            //    OnPropertyChanged(nameof(TreeRepositoriesVMs));
            //}
        }
        public List<string> PropertyGridRepresentationsCollection
        {
            get
            {
                var list = new List<string>();
                foreach (var item in Enum.GetNames(typeof(PropertyGridRepresentations)))
                {
                    list.Add(item);
                }
                return list;
            }
        }

        private PropertyGridRepresentations _propertyGridRepresentation;
        public PropertyGridRepresentations PropertyGridRepresentation
        {
            get
            {
                return _propertyGridRepresentation;
            }
            set
            {
                _propertyGridRepresentation = (PropertyGridRepresentations)value;
                OnPropertyChanged(nameof(PropertyGridRepresentation));
            }
        }
        public Dictionary<string, string>? PropertyList
        {
            get
            {
                if (_currentRepositoryExplorerVM == null)
                    return null;
                return PropertyGridHelper.GetProperties(_currentRepositoryExplorerVM.TreeRepository);
            }
        }
        //TODO: Перенести в ApplicationViewModel
        public RelayCommand OpenRepositoryCollectionSettingsWindow
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var window = new RepositorySettingsWindow() { DataContext = this }; 
                    window.Show();
                });
            }
        }
        public RelayCommand AddExistRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeProcessingService();
                    service.AddExistTreeRepository(new DirectoryInfo(""));
                });
            }
        }

        private string _newTreeRepositoryName;
        public string NewTreeRepositoryName
        {
            get
            {
                return _newTreeRepositoryName;
            }
            set
            {
                _newTreeRepositoryName = value;
            }
        }
        public RelayCommand CreateNewRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    var service = new DataTreeProcessingService();
                    var builder = new DataStorageBuilder();
                    var repositoryExplorerViewModel = new RepositoryExplorerVM(service.CreateNewTreeRepository(_newTreeRepositoryName, builder.Build()));
                    TreeRepositoriesVMs.Add(repositoryExplorerViewModel);
                     
                });
            }
        }
        private bool InitRepositoryCollection()
        {
            var service = new DataTreeProcessingService();
            var repositories = service.GetExistTreeRepositories(_dataStoragesSettingsVM.DataStorages);
            foreach (var item in repositories)
            {
                _treeRepositoriesVMs.Add(new RepositoryExplorerVM(item));
            }
            return true;
        }

    }
}
