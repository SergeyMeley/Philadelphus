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
        internal override DbTreeLeave BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            var result = (DbTreeLeave)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new DbTreeLeave());
            return result;
        }
        internal override List<DbTreeLeave> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            var result = new List<DbTreeLeave>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbTreeLeave)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new DbTreeLeave());
                result.Add(entity);
            }
            return result;
        }
        internal override TreeLeaveModel DbToBusinessEntity(IDbEntity dbEntity)
        {
            //var result = new TreeLeave(new Guid(dbEntity.ParentGuid));
            //result = (TreeLeave)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Leave));
            //return result;
            return null;
        }
        internal override List<TreeLeaveModel> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<TreeLeaveModel>();
            //foreach (var dbEntity in dbEntityCollection)
            //{
            //    var entity = new TreeLeave(new Guid(dbEntity.ParentGuid));
            //    entity = (TreeLeave)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Leave));
            //    result.Add(entity);
            //}
            return result;
        }
    }
}
