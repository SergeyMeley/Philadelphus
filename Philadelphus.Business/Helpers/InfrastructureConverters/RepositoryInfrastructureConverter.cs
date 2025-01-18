using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Helpers.InfrastructureConverters
{
    internal class RepositoryInfrastructureConverter : InfrastructureConverterBase
    {
        internal override IEnumerable<IDbEntity> BusinessToDbEntityCollection(IEnumerable<IMainEntity> businessEntities)
        {
            var result = new List<DbTreeRepository>();
            foreach (var businessEntity in businessEntities)
            {
                var entity = (DbTreeRepository)BusinessToDbMainProperties(businessEntity);
                result.Add(entity);
            }
            return result;
        }

        internal override IEnumerable<IMainEntity> DbToBusinessEntityCollection(IEnumerable<IDbEntity> dbEntities)
        {
            var result = new List<TreeRepository>();
            foreach (var dbEntity in dbEntities)
            {
                var entity = (TreeRepository)DbToBusinessMainProperties(dbEntity);
                result.Add(entity);
            }
            return result;
        }
    }
}
