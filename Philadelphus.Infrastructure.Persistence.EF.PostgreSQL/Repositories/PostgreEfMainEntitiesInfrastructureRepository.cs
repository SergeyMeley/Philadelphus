using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    public class PostgreEfMainEntitiesInfrastructureRepository : IPhiladelphusRepositoriesMembersInfrastructureRepository
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

        public IEnumerable<WorkingTree> SelectTrees()
        {
            if (CheckAvailability() == false)
                return null;

            List<WorkingTree> result = null;

            using (var context = GetNewContext())
            {
                result = context.WorkingTrees.Where(x => x.AuditInfo.IsDeleted == false).ToList();
            }

            return result;
        }

        public IEnumerable<WorkingTree> SelectTrees(Guid[] uuids)
        {
            if (CheckAvailability() == false)
                return null;

            List<WorkingTree> result = null;

            using (var context = GetNewContext())
            {
                result = context.WorkingTrees.Where(x =>
                x.AuditInfo.IsDeleted == false
                && uuids.Contains(x.Uuid)
                ).ToList();
            }

            return result;
        }

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
        public IEnumerable<TreeRoot> SelectRoots(Guid[] owningTreesUuids)
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeRoot> result = null;

            using (var context = GetNewContext())
            {
                result = context.TreeRoots.Where(x =>
                x.AuditInfo.IsDeleted == false
                && (owningTreesUuids.Contains(x.OwningWorkingTreeUuid)
                || owningTreesUuids.Contains(x.OwningWorkingTree.Uuid))
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
        public IEnumerable<TreeNode> SelectNodes(Guid[] owningTreesUuids)
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeNode> result = null;

            using (var context = GetNewContext())
            {
                result = context.TreeNodes.Where(x =>
                x.AuditInfo.IsDeleted == false
                && (owningTreesUuids.Contains(x.OwningWorkingTreeUuid)
                || owningTreesUuids.Contains(x.OwningWorkingTree.Uuid))
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
        public IEnumerable<TreeLeave> SelectLeaves(Guid[] owningTreesUuids)
        {
            if (CheckAvailability() == false)
                return null;

            List<TreeLeave> result = null;

            using (var context = GetNewContext())
            {
                result = context.TreeLeaves.Where(x =>
                x.AuditInfo.IsDeleted == false
                && (owningTreesUuids.Contains(x.OwningWorkingTreeUuid)
                || owningTreesUuids.Contains(x.OwningWorkingTree.Uuid))
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

        public long InsertTrees(IEnumerable<WorkingTree> items)
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
                }

                context.WorkingTrees.AddRange(items);
                result = context.SaveChanges();
            }

            return result;
        }

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

                    item.OwningWorkingTree = context.WorkingTrees.SingleOrDefault(x => x.Uuid == item.OwningWorkingTreeUuid);
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

                    item.OwningWorkingTree = context.WorkingTrees.SingleOrDefault(x => x.Uuid == item.OwningWorkingTreeUuid);
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

                    item.OwningWorkingTree = context.WorkingTrees.SingleOrDefault(x => x.Uuid == item.OwningWorkingTreeUuid);
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
                }

                context.ElementAttributes.AddRange(items);
                result = context.SaveChanges();
            }

            return result;
        }

        #endregion

        #region [ Update ]

        public long UpdateTrees(IEnumerable<WorkingTree> items)
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

                context.WorkingTrees.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }

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

                    item.OwningWorkingTree = context.WorkingTrees.SingleOrDefault(x => x.Uuid == item.OwningWorkingTreeUuid);
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

                    item.OwningWorkingTree = context.WorkingTrees.SingleOrDefault(x => x.Uuid == item.OwningWorkingTreeUuid);
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

                    item.OwningWorkingTree = context.WorkingTrees.SingleOrDefault(x => x.Uuid == item.OwningWorkingTreeUuid);
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

        public long SoftDeleteTrees(IEnumerable<WorkingTree> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.DeletedBy = Environment.UserName;
                    item.AuditInfo.DeletedAt = DateTime.UtcNow;
                }

                context.WorkingTrees.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }

        public long SoftDeleteRoots(IEnumerable<TreeRoot> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.DeletedBy = Environment.UserName;
                    item.AuditInfo.DeletedAt = DateTime.UtcNow;

                    item.OwningWorkingTree = context.WorkingTrees.SingleOrDefault(x => x.Uuid == item.OwningWorkingTreeUuid);
                }

                context.TreeRoots.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long SoftDeleteNodes(IEnumerable<TreeNode> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.DeletedBy = Environment.UserName;
                    item.AuditInfo.DeletedAt = DateTime.UtcNow;

                    item.OwningWorkingTree = context.WorkingTrees.SingleOrDefault(x => x.Uuid == item.OwningWorkingTreeUuid);
                }

                context.TreeNodes.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long SoftDeleteLeaves(IEnumerable<TreeLeave> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.DeletedBy = Environment.UserName;
                    item.AuditInfo.DeletedAt = DateTime.UtcNow;

                    item.OwningWorkingTree = context.WorkingTrees.SingleOrDefault(x => x.Uuid == item.OwningWorkingTreeUuid);
                }

                context.TreeLeaves.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }
        public long SoftDeleteAttributes(IEnumerable<ElementAttribute> items)
        {
            if (CheckAvailability() == false)
                return -1;

            long result = 0;

            using (var context = GetNewContext())
            {
                foreach (var item in items)
                {
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.DeletedBy = Environment.UserName;
                    item.AuditInfo.DeletedAt = DateTime.UtcNow;
                }

                context.ElementAttributes.UpdateRange(items);
                result = context.SaveChanges();
            }

            return result;
        }

        #endregion

    }
}
