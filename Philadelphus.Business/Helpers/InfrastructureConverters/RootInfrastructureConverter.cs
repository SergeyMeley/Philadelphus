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
        internal override IDbEntity BusinessToDbEntity(IMainEntity businessEntity)
        {
            var result = (DbTreeRoot)BusinessToDbMainProperties((TreeRepositoryMemberBase)businessEntity, new DbTreeRoot());
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbTreeRoot>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbTreeRoot)BusinessToDbMainProperties((TreeRepositoryMemberBase)businessEntity, new DbTreeRoot());
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntity DbToBusinessEntity(IDbEntity dbEntity)
        {
            //var result = new TreeRoot(new Guid(dbEntity.ParentGuid));
            //result = (TreeRoot)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Root));
            //return result;
            return null;
        }
        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<TreeRoot>();
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
