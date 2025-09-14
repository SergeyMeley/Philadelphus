using Microsoft.EntityFrameworkCore;
using Npgsql;
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
    public class PostgreEfTreeRepositoryHeadersInfrastructureRepository : ITreeRepositoriesInfrastructureRepository
    {
        private TreeRepositoriesPhiladelphusContext _context;
        public PostgreEfTreeRepositoryHeadersInfrastructureRepository(string connectionString)
        {
            //var optionsBuilder = new DbContextOptionsBuilder<TreeRepositoriesPhiladelphusContext>();
            //optionsBuilder
            //    .UseNpgsql(connectionString)
            //    .UseLazyLoadingProxies();
            //_context = new TreeRepositoriesPhiladelphusContext(optionsBuilder.Options);
            _context = new TreeRepositoriesPhiladelphusContext(connectionString);
            if(CheckAvailability())
            {
                if (_context.Database.GetPendingMigrations().ToList().Any())
                {
                    try
                    {
                        _context.Database.Migrate();
                    }
                    catch (PostgresException ex) when (ex.SqlState == "42P07") { }
                    catch (Exception ex) { throw; }
                }
            }
        }
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.TreeRepositoriesInfrastructureRepository; }
        public bool CheckAvailability()
        {
            return _context.Database.CanConnect();
        }
        public IEnumerable<TreeRepository> SelectRepositories()
        {
            if (CheckAvailability() == false)
                return null;
            return _context.Repositories.ToList();
        }
        public long DeleteRepository(TreeRepository repository)
        {
            throw new NotImplementedException();
        }

        public long UpdateRepository(TreeRepository repository)
        {
            throw new NotImplementedException();
        }

        public long InsertRepository(TreeRepository repository)
        {
            if (CheckAvailability() == false)
                return 0;
            repository.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            repository.AuditInfo.CreatedOn = DateTime.Now.ToString();
            repository.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            repository.AuditInfo.UpdatedOn = DateTime.Now.ToString();
            _context.Repositories.Add(repository);
            return _context.SaveChanges();
        }
    }
}
