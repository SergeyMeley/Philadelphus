using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Services;
using Philadelphus.InfrastructureEntities.Enums;
using System.Collections.ObjectModel;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationViewModel : ViewModelBase
    {
        private List<TreeRepository> _treeRepositories = new List<TreeRepository>();
        public ObservableCollection<TreeRepository> TreeRepositories { get => new ObservableCollection<TreeRepository>(_treeRepositories); private set => _treeRepositories = value.ToList(); }

        private RepositoryExplorerViewModel _repositoryViewModel = new RepositoryExplorerViewModel();
        public RepositoryExplorerViewModel RepositoryExplorerViewModel { get { return _repositoryViewModel; } }
    }
}
