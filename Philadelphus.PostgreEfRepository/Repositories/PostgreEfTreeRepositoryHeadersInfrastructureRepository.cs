using Microsoft.EntityFrameworkCore;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.PostgreEfRepository.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreEfRepository.Repositories
{
    public class PostgreEfTreeRepositoryHeadersInfrastructureRepository : ITreeRepositoryHeadersInfrastructureRepository
    {
        private TreeRepositoryHeadersPhiladelphusContext _context;
        public PostgreEfTreeRepositoryHeadersInfrastructureRepository(string connectionString)
        {
            //var optionsBuilder = new DbContextOptionsBuilder<TreeRepositoryHeadersPhiladelphusContext>();
            //optionsBuilder.UseNpgsql(connectionString);
            //_context = new TreeRepositoryHeadersPhiladelphusContext(optionsBuilder.Options);
            _context = new TreeRepositoryHeadersPhiladelphusContext(connectionString);
        }
        public bool CheckAvailability()
        {
            return _context.Database.CanConnect();
        }
        public IEnumerable<TreeRepository> SelectRepositories()
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
