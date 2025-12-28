using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using Npgsql;
using Philadelphus.Infrastructure.Persistence.Enums;
using Philadelphus.Infrastructure.Persistence.Interfaces;
using Philadelphus.Infrastructure.Persistence.MainEntities;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    public class PostgreEfMainEntitiesInfrastructureRepository : IMainEntitiesInfrastructureRepository
    {
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.MainEntities; }

        private string _connectionString;   //TODO: Заменить на использование контекста на сессию с ленивой загрузкой

        private readonly MainEntitiesPhiladelphusContext _context;
        public PostgreEfMainEntitiesInfrastructureRepository(string connectionString)
        {
            _connectionString = connectionString;   //TODO: Заменить на использование контекста на сессию с ленивой загрузкой  
            _context = new MainEntitiesPhiladelphusContext(connectionString);

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
        private MainEntitiesPhiladelphusContext GetNewContext() => new MainEntitiesPhiladelphusContext(_connectionString);

        #region [ Select ]

        public IEnumerable<TreeRoot> SelectRoots()
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeRoot> result = null;

            using (var context = GetNewContext())
            {
                result = context.TreeRoots.Where(x => x.AuditInfo.IsDeleted == false).ToList();
            }

            return result;
        }
        public IEnumerable<TreeRoot> SelectRoots(Guid[] guids)
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeRoot> result = null;

            using (var context = GetNewContext())
            {
                result = context.TreeRoots.Where(x => 
                x.AuditInfo.IsDeleted == false
                && guids.Contains(x.Guid)
                ).ToList();
            }

            return result;
        }
        public IEnumerable<TreeNode> SelectNodes()
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeNode> result = null;

            using (var context = GetNewContext())
            {
                result = context.TreeNodes.Where(x => x.AuditInfo.IsDeleted == false).ToList();
            }

            return result;
        }
        public IEnumerable<TreeNode> SelectNodes(Guid[] parentRootGuids)
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeNode> result = null;

            using (var context = GetNewContext())
            {
                result = context.TreeNodes.Where(x =>
                x.AuditInfo.IsDeleted == false
                && (parentRootGuids.Contains(x.ParentTreeRootGuid ?? Guid.Empty) //TODO: Избавиться от костыля Guid.Empty
                || parentRootGuids.Contains(x.ParentTreeRoot.Guid))
                ).ToList();
            }

            return result;
        }
        public IEnumerable<TreeLeave> SelectLeaves()
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeLeave> result = null;

            using (var context = GetNewContext())
            {
                result = context.TreeLeaves.Where(x => x.AuditInfo.IsDeleted == false).ToList();
            }

            return result;
        }
        public IEnumerable<TreeLeave> SelectLeaves(Guid[] parentRootGuids)
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeLeave> result = null;

            using (var context = GetNewContext())
            {
                result = context.TreeLeaves.Where(x =>
                x.AuditInfo.IsDeleted == false
                && (parentRootGuids.Contains(x.ParentTreeRootGuid ?? Guid.Empty) //TODO: Избавиться от костыля Guid.Empty
                || parentRootGuids.Contains(x.ParentTreeRoot.Guid))
                ).ToList();
            }

            return result;
        }
        public IEnumerable<ElementAttribute> SelectAttributes()
        {
            if (CheckAvailability() == false)
                return null;

            List<ElementAttribute> result = null;

            using (var context = GetNewContext())
            {
                result = context.ElementAttributes.Where(x => x.AuditInfo.IsDeleted == false).ToList();
            }

            return result;
        }

        #endregion

        #region [ Insert ]

        public long InsertRoots(IEnumerable<TreeRoot> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.CreatedAt = DateTime.UtcNow;
                    item.AuditInfo.CreatedBy = Environment.UserName;
                }
                
                context.TreeRoots.AddRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long InsertNodes(IEnumerable<TreeNode> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.CreatedBy = Environment.UserName;
                    item.AuditInfo.CreatedAt = DateTime.UtcNow;
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.TreeNodes.AddRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long InsertLeaves(IEnumerable<TreeLeave> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.CreatedBy = Environment.UserName;
                    item.AuditInfo.CreatedAt = DateTime.UtcNow;
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.TreeLeaves.AddRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long InsertAttributes(IEnumerable<ElementAttribute> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.CreatedBy = Environment.UserName;
                    item.AuditInfo.CreatedAt = DateTime.UtcNow;
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.ElementAttributes.AddRange(items);
                result = context.SaveChanges();
            }

            return result;
        }

        #endregion

        #region [ Update ]

        public long UpdateRoots(IEnumerable<TreeRoot> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.TreeRoots.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long UpdateNodes(IEnumerable<TreeNode> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.TreeNodes.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long UpdateLeaves(IEnumerable<TreeLeave> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.TreeLeaves.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long UpdateAttributes(IEnumerable<ElementAttribute> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.ElementAttributes.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }

        #endregion

        #region [ Delete ]

        public long DeleteRoots(IEnumerable<TreeRoot> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.TreeRoots.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long DeleteNodes(IEnumerable<TreeNode> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.TreeNodes.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long DeleteLeaves(IEnumerable<TreeLeave> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.TreeLeaves.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long DeleteAttributes(IEnumerable<ElementAttribute> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                }

                context.ElementAttributes.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }

        #endregion
        
    }
}
