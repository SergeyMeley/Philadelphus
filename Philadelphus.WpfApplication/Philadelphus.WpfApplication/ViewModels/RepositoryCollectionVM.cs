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
        public RepositoryCollectionVM()
        {
            PropertyGridRepresentation = PropertyGridRepresentations.DataGrid;
        }
        private static RepositoryExplorerVM _currentRepositoryExplorerVM = new RepositoryExplorerVM();
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
        private ObservableCollection<RepositoryExplorerVM> _treeRepositoriesVMs;
        public ObservableCollection<RepositoryExplorerVM> TreeRepositoriesVMs
        {
            get
            {
                DataTreeProcessingService treeRepositoryService = new DataTreeProcessingService();
                var treeRepositories = treeRepositoryService.GetRepositoryCollection();
                if (treeRepositories != null && _treeRepositoriesVMs == null)
                {
                    _treeRepositoriesVMs = new ObservableCollection<RepositoryExplorerVM>();
                    foreach (var item in treeRepositories)
                    {
                        var vm = new RepositoryExplorerVM();
                        vm.TreeRepository = item;
                        _treeRepositoriesVMs.Add(vm);
                    }
                    //for (int i = 0; i < 5; i++)
                    //{
                    //    var vm = new RepositoryExplorerVM();
                        
                    //    //TreeRepository treeRepository = new TreeRepository(Guid.NewGuid());
                    //    //Временно
                        
                    //    TreeRepositoryModel treeRepository = treeRepositoryService.CreateSampleRepository();
                    //    //Временно
                    //    treeRepository.Name = $"TEST {i}";
                    //    vm.TreeRepository = treeRepository;
                    //    _treeRepositoriesVMs.Add(vm);
                    //}
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
                    DataTreeProcessingService service = new DataTreeProcessingService();
                    service.AddExistTreeRepository(new DirectoryInfo(""));
                });
            }
        }
        public RelayCommand CreateNewRepository
        {
            get
            {
                return new RelayCommand(obj =>
                {
                    DataTreeProcessingService service = new DataTreeProcessingService();
                    var repositoryExplorerViewModel = new RepositoryExplorerVM();
                    repositoryExplorerViewModel.TreeRepository = service.CreateNewTreeRepository();
                    TreeRepositoriesVMs.Add(repositoryExplorerViewModel);
                     
                });
            }
        }
    }
}
