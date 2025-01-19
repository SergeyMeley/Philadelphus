using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.Business.Factories;
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
        internal override IDbEntity BusinessToDbEntity(IMainEntity businessEntity)
        {
            var result = (DbTreeNode)BusinessToDbMainProperties(businessEntity, new DbTreeNode());
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbTreeNode>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbTreeNode)BusinessToDbMainProperties(businessEntity, new DbTreeNode());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntity DbToBusinessEntity(IDbEntity dbEntity)
        {
            var result = (TreeNode)DbToBusinessMainProperties(dbEntity, MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Node));
            return result;
        }
        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<TreeNode>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var entity = (TreeNode)DbToBusinessMainProperties(dbEntity, MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Node));
                result.Add(entity);
            }
            return result;
        }
    }
}
