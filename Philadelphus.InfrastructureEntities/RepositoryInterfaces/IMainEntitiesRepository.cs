using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.InfrastructureEntities.RepositoryInterfaces
{
    public interface IMainEntitiesRepository
    {
        DbMainEntitiesCollection GetMainEntitiesCollection();        //удалить?
        # region [ Select ]
        IEnumerable<DbTreeRepository> SelectProjects();
        IEnumerable<DbTreeRoot> SelectLayers();
        IEnumerable<DbTreeNode> SelectNodes();
        IEnumerable<DbTreeLeave> SelectElements();
        #endregion
        # region [ Insert ]
        int InsertProjects(IEnumerable<DbTreeRepository> projects);
        int InsertLayers(IEnumerable<DbTreeRoot> layers);
        int InsertNodes(IEnumerable<DbTreeNode> nodes);
        int InsertElements(IEnumerable<DbTreeLeave> elements);
        #endregion
        # region [ Delete ]
        int DeleteProjects(int[] ids);
        int DeleteLayers(int[] ids);
        int DeleteNodes(int[] ids);
        int DeleteElements(int[] ids);
        #endregion
        # region [ Update ]
        int UpdateProjects(IEnumerable<DbTreeRepository> projects);
        int UpdateLayers(IEnumerable<DbTreeRoot> layers);
        int UpdateNodes(IEnumerable<DbTreeNode> nodes);
        int UpdateElements(IEnumerable<DbTreeLeave> elements);
        #endregion
    }
}
