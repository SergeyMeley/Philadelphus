using Philadelphus.Business.Entities.MainEntities;
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
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntities)
        {
            var result = new List<DbTreeRoot>();
            foreach (var businessEntity in businessEntities)
            {
                var entity = (DbTreeRoot)BusinessToDbMainProperties(businessEntity);
                result.Add(entity);
            }
            return result;
        }

        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntities)
        {
            var result = new List<TreeRoot>();
            foreach (var dbEntity in dbEntities)
            {
                var entity = (TreeRoot)DbToBusinessMainProperties(dbEntity);
                result.Add(entity);
            }
            return result;
        }
    }
}
