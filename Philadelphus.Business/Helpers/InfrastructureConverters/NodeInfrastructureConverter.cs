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
        internal override IMainEntity BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeNode)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeNode());
            return result;
        }
        internal override IEnumerable<IMainEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeNode>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (TreeNode)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeNode());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntityModel DbToBusinessEntity(IMainEntity dbEntity)
        {
            if (dbEntity == null)
                return null;
            //var result = new TreeNode(new Guid(dbEntity.ParentGuid));
            //result = (TreeNode)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Node));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntityModel> DbToBusinessEntityCollection(IEnumerable<IMainEntity> dbEntityCollection)
        {
            if (dbEntityCollection == null)
                return null;
            var result = new List<TreeNodeModel>();
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
