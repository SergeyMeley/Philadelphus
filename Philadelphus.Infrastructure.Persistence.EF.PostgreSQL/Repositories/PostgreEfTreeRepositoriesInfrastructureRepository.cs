using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    public class PostgreEfPhiladelphusRepositoriesInfrastructureRepository : IPhiladelphusRepositoriesInfrastructureRepository
    {
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.PhiladelphusRepositories; }

        private string _connectionString;   //TODO: Заменить на использование контекста на сессию с ленивой загрузкой

        private PhiladelphusRepositoriesPhiladelphusContext _context;
        public PostgreEfPhiladelphusRepositoriesInfrastructureRepository(string connectionString, bool needEnsureDeleted = false)
        {
            _connectionString = connectionString;   //TODO: Заменить на использование контекста на сессию с ленивой загрузкой   
            _context = new PhiladelphusRepositoriesPhiladelphusContext(connectionString);

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
        public bool CheckAvailability()
        {
            using (var context = GetNewContext())
            {
                if (context.Database.CanConnect() == false)
                     return false;
                try
                {
                    if (context.Database.GetService<IRelationalDatabaseCreator>().Exists() == false)
                        return false;
                    context.Database.ExecuteSqlRaw($"SELECT {0}", 1);
                    context.Database.OpenConnection();
                    context.Database.CloseConnection();
                }
                catch (Exception ex)
                {
                    return false;
                }
                return true;
            }
        }
        private PhiladelphusRepositoriesPhiladelphusContext GetNewContext() => new PhiladelphusRepositoriesPhiladelphusContext(_connectionString);
        public IEnumerable<PhiladelphusRepository> SelectRepositories()
        {
            if (CheckAvailability() == false)
                return null;

            List<PhiladelphusRepository> result = null;

            using (var context = GetNewContext())
            {
                result = context.Repositories.Where(x => x.AuditInfo.IsDeleted == false).ToList();
            }

            return result;
        }
        public IEnumerable<PhiladelphusRepository> SelectRepositories(Guid[] uuids)
        {
            if (CheckAvailability() == false)
                return null;

            List<PhiladelphusRepository> result = null;

            using (var context = GetNewContext())
            {
                result = context.Repositories.Where(x =>
                x.AuditInfo.IsDeleted == false
                && uuids.Contains(x.Uuid)
                ).ToList();
            }

            return result;
        }

        public long InsertRepository(PhiladelphusRepository item)
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
        public long UpdateRepository(PhiladelphusRepository item)
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
        public long DeleteRepository(PhiladelphusRepository item)
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
