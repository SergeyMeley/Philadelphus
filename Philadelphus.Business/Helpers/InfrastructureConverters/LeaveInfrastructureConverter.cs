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
        internal override TreeLeave BusinessToDbEntity(IMainEntityModel businessEntity)
        {
            if (businessEntity == null)
                return null;
            var result = (TreeLeave)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeLeave());
            return result;
        }
        internal override List<TreeLeave> BusinessToDbEntityCollection(IEnumerable<IMainEntityModel> businessEntityCollection)
        {
            if (businessEntityCollection == null)
                return null;
            var result = new List<TreeLeave>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (TreeLeave)BusinessToDbMainProperties((TreeRepositoryMemberBaseModel)businessEntity, new TreeLeave());
                result.Add(entity);
            }
            return result;
        }
        internal override TreeLeaveModel DbToBusinessEntity(IEntity dbEntity)
        {
            if (dbEntity == null)
                return null;
            //var result = new TreeLeave(new Guid(dbEntity.ParentGuid));
            //result = (TreeLeave)DbToBusinessMainProperties(dbEntity, (TreeRepositoryMemberBase)MainEntityFactory.CreateMainEntitiesRepositoriesFactory(EntityTypes.Leave));
            //return result;
            return null;
        }
        internal override List<TreeLeaveModel> DbToBusinessEntityCollection(IEnumerable<IEntity> dbEntityCollection)
        {
            if (dbEntityCollection == null)
                return null;
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
