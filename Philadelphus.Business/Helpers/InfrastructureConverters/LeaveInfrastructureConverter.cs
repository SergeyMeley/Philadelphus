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
    internal class LeaveInfrastructureConverter : InfrastructureConverterBase
    {
        internal override DbTreeLeave BusinessToDbEntity(IMainEntity businessEntity)
        {
            var result = (DbTreeLeave)BusinessToDbMainProperties((RepositoryElementBase)businessEntity, new DbTreeLeave());
            return result;
        }
        internal override List<DbTreeLeave> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbTreeLeave>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbTreeLeave)BusinessToDbMainProperties((RepositoryElementBase)businessEntity, new DbTreeLeave());
                result.Add(entity);
            }
            return result;
        }
        internal override TreeLeave DbToBusinessEntity(IDbEntity dbEntity)
        {
            //var result = new TreeLeave(new Guid(dbEntity.ParentGuid));
            //result = (TreeLeave)DbToBusinessMainProperties(dbEntity, (RepositoryElementBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Leave));
            //return result;
            return null;
        }
        internal override List<TreeLeave> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<TreeLeave>();
            //foreach (var dbEntity in dbEntityCollection)
            //{
            //    var entity = new TreeLeave(new Guid(dbEntity.ParentGuid));
            //    entity = (TreeLeave)DbToBusinessMainProperties(dbEntity, (RepositoryElementBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Leave));
            //    result.Add(entity);
            //}
            return result;
        }
    }
}
