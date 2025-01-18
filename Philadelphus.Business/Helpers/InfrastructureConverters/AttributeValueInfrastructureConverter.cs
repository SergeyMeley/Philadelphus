using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    internal class AttributeValueInfrastructureConverter : InfrastructureConverterBase
    {
        internal override IDbEntity BusinessToDbEntity(IMainEntity businessEntity)
        {
            var result = (DbAttributeValue)BusinessToDbMainProperties(businessEntity);
            return result;
        }
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntityCollection)
        {
            var result = new List<DbAttributeValue>();
            foreach (var businessEntity in businessEntityCollection)
            {
                var entity = (DbAttributeValue)BusinessToDbMainProperties(businessEntity);
                result.Add(entity);
            }
            return result;
        }
        internal override IMainEntity DbToBusinessEntity(IDbEntity dbEntity)
        {
            var result = (AttributeValue)DbToBusinessMainProperties(dbEntity);
            return result;
        }
        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntityCollection)
        {
            var result = new List<AttributeValue>();
            foreach (var dbEntity in dbEntityCollection)
            {
                var entity = (AttributeValue)DbToBusinessMainProperties(dbEntity);
                result.Add(entity);
            }
            return result;
        }






        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntities)
        {
            var result = new List<DbAttributeValue>();
            foreach (var businessEntity in businessEntities)
            {
                var entity = (DbAttributeValue)BusinessToDbMainProperties(businessEntity);
                result.Add(entity);
            }
            return result;
        }

        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntities)
        {
            var result = new List<AttributeValue>();
            foreach (var dbEntity in dbEntities)
            {
                var entity = (AttributeValue)DbToBusinessMainProperties(dbEntity);
                result.Add(entity);
            }
            return result;
        }
    }
}
