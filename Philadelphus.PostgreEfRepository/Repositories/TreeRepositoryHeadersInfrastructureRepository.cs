using Microsoft.EntityFrameworkCore;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreEfRepository.Repositories
{
    public class TreeRepositoryHeadersInfrastructureRepository : ITreeRepositoryHeadersInfrastructureRepository
    {
        public bool CheckAvailability()
        {
            bool result;
            using (var context = new PhiladelphusContext())
            {
                result = context.Database.CanConnect();
            }
            return result;
        }
        public InfrastructureTypes InfrastructureRepositoryTypes { get; } = InfrastructureTypes.PostgreSql;
        private readonly PhiladelphusContext _context = new PhiladelphusContext();
        public IEnumerable<TreeRepository> SelectRepositories(List<string> pathes)
        {
            if (CheckAvailability() == false)
                return null;
            var result = new List<TreeRepository>();
            using (_context)
            {
                result = _context.Repositories.ToList();
            }
            return result;
        }
        public long DeleteRepositories(IEnumerable<TreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

        public long UpdateRepositories(IEnumerable<TreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

        public long InsertRepositories(IEnumerable<TreeRepository> repositories)
        {
            using (_context)
            {
                if (CheckAvailability() == false)
                    return 0;
                foreach (var item in repositories)
                {
                    item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    item.AuditInfo.CreatedOn = DateTime.Now.ToString();
                    item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    item.AuditInfo.UpdatedOn = DateTime.Now.ToString();
                    _context.Repositories.Add(item);
                }
                return _context.SaveChanges();
            }
        }
    }
}
