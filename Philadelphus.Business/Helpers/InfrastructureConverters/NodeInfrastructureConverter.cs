using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
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
            var result = (DbTreeNode)BusinessToDbMainProperties((TreeRepositoryMemberBase)businessEntity, new DbTreeNode());
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbTreeNode>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbTreeNode)BusinessToDbMainProperties((TreeRepositoryMemberBase)businessEntity, new DbTreeNode());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntity DbToBusinessEntity(IDbEntity dbEntity)
        {
            //var result = new TreeNode(new Guid(dbEntity.ParentGuid));
            //result = (TreeNode)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Node));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<TreeNode>();
            //foreach (var dbEntity in dbEntityCollection)
            //{
            //    var entity = new TreeNode(new Guid(dbEntity.ParentGuid));
            //    entity = (TreeNode)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Node));
            //    result.Add(entity);
            //}
            return result;
        }
    }
}
