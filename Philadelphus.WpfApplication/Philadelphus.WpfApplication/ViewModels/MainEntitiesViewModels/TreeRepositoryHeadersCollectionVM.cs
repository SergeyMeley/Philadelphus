using Philadelphus.Business.Services;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;

namespace Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels
{
    public class TreeRepositoryHeadersCollectionVM : ViewModelBase
    {
        private TreeRepositoryCollectionService _service;

        private List<TreeRepositoryHeaderVM> _treeRepositoryHeadersVMs;
        public List<TreeRepositoryHeaderVM> TreeRepositoryHeadersVMs
        {
            get
            {
                if (_treeRepositoryHeadersVMs == null)
                {
                    _treeRepositoryHeadersVMs = new List<TreeRepositoryHeaderVM>();
                }
                _treeRepositoryHeadersVMs.Clear();
                var headers = _service.GetTreeRepositoryHeadersCollection().OrderByDescending(x => x.LastOpening);
                Action updateTreeRepositoryHeaders = () =>
                {
                    OnPropertyChanged(nameof(TreeRepositoryHeadersVMs));
                    OnPropertyChanged(nameof(FavoriteTreeRepositoryHeadersVMs));
                    OnPropertyChanged(nameof(LastTreeRepositoryHeadersVMs));
                };
                foreach (var header in headers)
                {
                    _treeRepositoryHeadersVMs.Add(new TreeRepositoryHeaderVM(_service, header, updateTreeRepositoryHeaders));
                }
                return _treeRepositoryHeadersVMs;
            }
        }
        public List<TreeRepositoryHeaderVM> FavoriteTreeRepositoryHeadersVMs
        {
            get
            {
                return TreeRepositoryHeadersVMs.Where(x => x.IsFavorite).ToList();
            }
        }
        public List<TreeRepositoryHeaderVM> LastTreeRepositoryHeadersVMs
        {
            get
            {
                return TreeRepositoryHeadersVMs.OrderByDescending(x => x.LastOpening).Where(x => DateTime.UtcNow - x.LastOpening <= TimeSpan.FromDays(30)).ToList();
            }
        }

        private TreeRepositoryHeaderVM _selectedTreeRepositoryHeaderVM;
        public TreeRepositoryHeaderVM SelectedTreeRepositoryHeaderVM 
        {
            get
            { 
                return _selectedTreeRepositoryHeaderVM; 
            }
            set
            {
                _selectedTreeRepositoryHeaderVM = value;
            }
        }

        public TreeRepositoryHeadersCollectionVM(TreeRepositoryCollectionService service)
        {
            _service = service;
        }

    }
}
