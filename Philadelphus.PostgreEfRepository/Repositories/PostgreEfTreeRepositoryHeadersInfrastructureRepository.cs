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
        private string _connectionString;   //TODO: Заменить на использование контекста на сессию с ленивой загрузкой
        private TreeRepositoriesPhiladelphusContext GetNewContext() => new TreeRepositoriesPhiladelphusContext(_connectionString);

        private TreeRepositoriesPhiladelphusContext _context;
        public PostgreEfTreeRepositoryHeadersInfrastructureRepository(string connectionString, bool needEnsureDeleted = false)
        {
            _connectionString = connectionString;   //TODO: Заменить на использование контекста на сессию с ленивой загрузкой   
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

            List<TreeRepository> result = null;

            using (var context = GetNewContext())
            {
                result = context.Repositories.Where(x => x.AuditInfo.IsDeleted == false).ToList();
            }

            return result;
        }
        public long InsertRepository(TreeRepository item)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                item.AuditInfo.CreatedAt = DateTime.UtcNow;
                item.AuditInfo.CreatedBy = Environment.UserName;
                context.Repositories.Add(item);
                result = context.SaveChanges();
            }

            return result;
        }
        public long UpdateRepository(TreeRepository item)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                item.AuditInfo.UpdatedBy = Environment.UserName;
                context.Update(item);
                result = context.SaveChanges();
            }

            return result;
        }
        public long DeleteRepository(TreeRepository item)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                item.AuditInfo.IsDeleted = true;
                item.AuditInfo.DeletedAt = DateTime.UtcNow;
                item.AuditInfo.DeletedBy = Environment.UserName;
                context.Update(item);
                result = context.SaveChanges();
            }

            return result;
        }


    }
}
