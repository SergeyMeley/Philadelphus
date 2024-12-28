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
        IEnumerable<DbLayer> SelectProjects();
        IEnumerable<DbLayer> SelectLayers();
        IEnumerable<DbNode> SelectNodes();
        IEnumerable<DbNode> SelectElements();
        #endregion
        # region [ Insert ]
        IEnumerable<DbLayer> InsertProjects(IEnumerable<DbProject> projects);
        IEnumerable<DbLayer> InsertLayers(IEnumerable<DbLayer> layers);
        IEnumerable<DbNode> InsertNodes(IEnumerable<DbNode> nodes);
        IEnumerable<DbNode> InsertElements(IEnumerable<DbElement> elements);
        #endregion
        # region [ Delete ]
        IEnumerable<DbLayer> DeletetProjects(int[] ids);
        IEnumerable<DbLayer> DeleteLayers(int[] ids);
        IEnumerable<DbNode> DeleteNodes(int[] ids);
        IEnumerable<DbNode> DeleteElements(int[] ids);
        #endregion
        # region [ Update ]
        IEnumerable<DbLayer> UpdateProjects(IEnumerable<DbProject> projects);
        IEnumerable<DbLayer> UpdateLayers(IEnumerable<DbLayer> layers);
        IEnumerable<DbNode> UpdateNodes(IEnumerable<DbNode> nodes);
        IEnumerable<DbNode> UpdateElements(IEnumerable<DbElement> elements);
        #endregion
    }
}
