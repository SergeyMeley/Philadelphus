using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Helpers;
using Philadelphus.Business.Services;
using Philadelphus.WpfApplication.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class RepositoryCollectionViewModel : ViewModelBase
    {
        private static RepositoryExplorerViewModel _currentRepositoryExplorerVM = new RepositoryExplorerViewModel();
        public  RepositoryExplorerViewModel CurrentRepositoryExplorerVM 
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
        private ObservableCollection<RepositoryExplorerViewModel> _treeRepositoriesVMs;
        public ObservableCollection<RepositoryExplorerViewModel> TreeRepositoriesVMs
        {
            get
            {
                if (_treeRepositoriesVMs == null)
                {
                    _treeRepositoriesVMs = new ObservableCollection<RepositoryExplorerViewModel>();
                    for (int i = 0; i < 5; i++)
                    {
                        var vm = new RepositoryExplorerViewModel();
                        
                        //TreeRepository treeRepository = new TreeRepository(Guid.NewGuid());
                        //Временно
                        DataTreeRepositoryService treeRepositoryService = new DataTreeRepositoryService();
                        TreeRepository treeRepository = treeRepositoryService.CreateSampleRepository();
                        //Временно
                        treeRepository.Name = $"TEST {i}";
                        vm.TreeRepository = treeRepository;
                        _treeRepositoriesVMs.Add(vm);
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
    }
}
