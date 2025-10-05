using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
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
        public PostgreEfTreeRepositoryHeadersInfrastructureRepository(string connectionString, bool needEnsureDeleted = false)
        {
            _context = new TreeRepositoriesPhiladelphusContext(connectionString);

            if (CheckAvailability())
            {
                //_context.Database.EnsureDeleted();
                _context.Database.EnsureCreated();

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
            if (_context.Database.CanConnect() == false)
                return false;
            try
            {
                if (_context.Database.GetService<IRelationalDatabaseCreator>().Exists() == false)
                    return false;
                _context.Database.ExecuteSqlRaw($"SELECT {0}", 1);
                _context.Database.OpenConnection();
                _context.Database.CloseConnection();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public IEnumerable<TreeRepository> SelectRepositories()
        {
            if (CheckAvailability() == false)
                return null;
            return _context.Repositories.ToList();
        }
        public long DeleteRepository(TreeRepository item)
        {
            if (CheckAvailability() == false)
                return -1;
            var repository = _context.Repositories.FirstOrDefault(x => x.Guid == item.Guid);
            repository.AuditInfo.IsDeleted = true;
            repository.AuditInfo.DeletedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            repository.AuditInfo.DeletedAt = DateTime.Now;
            return _context.SaveChanges();
        }

        public long UpdateRepository(TreeRepository item)
        {
            if (CheckAvailability() == false)
                return -1;
            //var repository = _context.Repositories.FirstOrDefault(x => x.Guid == item.Guid);
            //repository.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //repository.AuditInfo.UpdatedAt = DateTime.Now;
            return _context.SaveChanges();
        }

        public long InsertRepository(TreeRepository item)
        {
            if (CheckAvailability() == false)
                return -1;
            _context.Repositories.Add(item);
            return _context.SaveChanges();
        }
    }
}
