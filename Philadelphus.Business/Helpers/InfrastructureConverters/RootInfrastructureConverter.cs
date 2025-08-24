using Philadelphus.Business.Entities.Enums;
using Philadelphus.Business.Entities.RepositoryElements.Interfaces;
using Philadelphus.Business.Entities.RepositoryElements;
using Philadelphus.Business.Factories;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    internal class RootInfrastructureConverter :InfrastructureConverterBase
    {
        internal override IEntity BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            var result = (TreeRoot)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeRoot());
            return result;
        }
        internal override IEnumerable<IEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            var result = new List<TreeRoot>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (TreeRoot)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeRoot());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntityModel DbToBusinessEntity(IEntity dbEntity)
        {
            //var result = new TreeRoot(new Guid(dbEntity.ParentGuid));
            //result = (TreeRoot)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Root));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntityModel> DbToBusinessEntityCollection(IEnumerable<IEntity> dbEntityCollection)
        {
            var result = new List<TreeRootModel>();
            //foreach (var dbEntity in dbEntityCollection)
            //{
            //    var entity = new TreeRoot(new Guid(dbEntity.ParentGuid));
            //    entity = (TreeRoot)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Root));
            //    result.Add(entity);
            //}
            return result;
        }
    }
}
