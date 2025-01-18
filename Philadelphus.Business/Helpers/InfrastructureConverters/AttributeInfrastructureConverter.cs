using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    internal class AttributeInfrastructureConverter : InfrastructureConverterBase
    {
        internal override IDbEntity BusinessToDbEntity(IMainEntity businessEntity)
        {
            var result = (DbAttribute)BusinessToDbMainProperties(businessEntity);
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbAttribute>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbAttribute)BusinessToDbMainProperties(businessEntity);
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntity DbToBusinessEntity(IDbEntity dbEntity)
        {
            var result = (EntityAttribute)DbToBusinessMainProperties(dbEntity);
            return result;
        }
        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<EntityAttribute>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var entity = (EntityAttribute)DbToBusinessMainProperties(dbEntity);
                result.Add(entity);
            }
            return result;
        }
    }
}
