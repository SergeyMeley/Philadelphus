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
            return _context.Repositories.ToList();
        }
        public long DeleteRepository(TreeRepository item)
        {
            if (CheckAvailability() == false)
                return -1;
            var repository = _context.Repositories.FirstOrDefault(x => x.Guid == item.Guid);
            repository.AuditInfo.IsDeleted = true;
            repository.AuditInfo.DeletedBy = Environment.UserName;
            repository.AuditInfo.DeletedAt = DateTime.UtcNow;
            return _context.SaveChanges();
        }

        public long UpdateRepository(TreeRepository item)
        {
            long result = 0;

            if (CheckAvailability() == false)
                return -1;
            //var repository = _context.Repositories.FirstOrDefault(x => x.Guid == item.Guid);
            //_context.Update(item);
            //item.AuditInfo.UpdatedBy = Environment.UserName;
            //item.AuditInfo.UpdatedAt = DateTime.UtcNow;
            //return _context.SaveChanges();
            using (var context = new TreeRepositoriesPhiladelphusContext(_connectionString))
            {
                item.AuditInfo.UpdatedBy = Environment.UserName;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                context.Update(item);
                result = context.SaveChanges();
            }

            return result;
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
