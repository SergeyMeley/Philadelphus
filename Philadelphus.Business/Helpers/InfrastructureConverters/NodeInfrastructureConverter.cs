using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    internal class NodeInfrastructureConverter : InfrastructureConverterBase
    {
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntities)
        {
            var result = new List<DbTreeNode>();
            foreach (var businessEntity in businessEntities)
            {
                var entity = (DbTreeNode)BusinessToDbMainProperties(businessEntity);
                result.Add(entity);
            }
            return result;
        }

        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntities)
        {
            var result = new List<TreeNode>();
            foreach (var dbEntity in dbEntities)
            {
                var entity = (TreeNode)DbToBusinessMainProperties(dbEntity);
                result.Add(entity);
            }
            return result;
        }
    }
}
