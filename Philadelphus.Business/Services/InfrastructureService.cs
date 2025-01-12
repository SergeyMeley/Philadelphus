using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Services
{
    public class InfrastructureService
    {
        #region [Database to business entity]
        public TreeRepository DbToBusinessRepository(IDbEntity repository)
        {
            TreeRepository result  = new TreeRepository();
            if (repository.GetType() == typeof(DbTreeRepository))
            {
                
            }
            else if (true)
            {

            }
            

            return result;
        }
        #endregion
        #region [Business to database entity]
        public DbTreeRepository BusinessToDbRepository(TreeRepository repository)
        {
            var result = new DbTreeRepository(repository.Id, repository.Name);

            return result;
        }
        #endregion
    }
}
