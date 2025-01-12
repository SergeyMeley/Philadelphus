using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WpfApplication.ViewModels
{
    public class ApplicationViewModel
    {
        public List<string> RepositoryList { get => TreeRepositories.Select(x => x.Name).ToList(); private set => RepositoryList = value; }
        private List<TreeRepository> _treeRepositories;
        public List<TreeRepository> TreeRepositories { 
            get
            {
                var service = new DataTreeRepositoryService();
                return service.GetRepositoryList();
            }
            private set
            {
                _treeRepositories = value;
            }
        }
        


    }
}
