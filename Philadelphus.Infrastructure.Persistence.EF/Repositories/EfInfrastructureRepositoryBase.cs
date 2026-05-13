using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Philadelphus.Infrastructure.Persistence.EF.Repositories
{
    /// <summary>
    /// Представляет объект базового репозитория БД.
    /// </summary>
    public abstract class EfInfrastructureRepositoryBase<TContext> : IInfrastructureRepository
        where TContext : DbContext
    {
        private enum AuditOperation
        {
            Insert,
            Update,
            SoftDelete
        }

        protected readonly ILogger _logger;
        
        /// <summary>
        /// Группа инфраструктурных сущностей.
        /// </summary>
        public abstract InfrastructureEntityGroups EntityGroup { get; }
        protected string _connectionString { get; }     //TODO: Заменить на использование контекста на сессию с ленивой загрузкой
        protected TContext _context { get; init; }
        protected EfInfrastructureRepositoryBase(
            ILogger logger, 
            string connectionString)
        {
            _logger = logger;
            _connectionString = connectionString;       //TODO: Заменить на использование контекста на сессию с ленивой загрузкой
            _context = GetNewContext();

            InitDb();
        }

        /// <summary>
        /// Проверить доступность.
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public virtual bool CheckAvailability()
        {
            var sw = new Stopwatch();
            sw.Start();
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
                catch (Exception)
                {
                    return false;
                }

                sw.Stop();
                _logger.Information($"Task '{Task.CurrentId}'. Репозиторий БД '{this.GetType().Name}'. t = {sw.ElapsedMilliseconds} мс.");

                return true;
            }
        }

        protected virtual void InitDb()
        {
            if (CheckAvailability() == false)
                _context.Database.EnsureCreated();

            if (CheckAvailability())
            {
                if (_context.Database.GetPendingMigrations().ToList().Any())
                {
                    try
                    {
                        _context.Database.Migrate();
                    }
                    catch (Exception ex) when (IsDuplicateTableException(ex))
                    {
                        // Пропускаем ошибку дублирования таблицы
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }
        }

        protected abstract bool IsDuplicateTableException(Exception ex);

        private TResult ExecuteWithContext<TEntity, TResult>(
            Func<TContext, DbSet<TEntity>, TResult> action)
            where TEntity : class, IMainEntity
        {
            if (CheckAvailability() == false)
                return default!;

            using (var context = GetNewContext())
            {
                var dbSet = GetDbSet<TEntity>(context);
                return action(context, dbSet);
            }
        }

        protected IEnumerable<TEntity> Select<TEntity>(Guid[]? ownUuids = null, Guid[]? owningTreesUuids = null) where TEntity : class, IMainEntity
        {
            return ExecuteWithContext<TEntity, IEnumerable<TEntity>>((context, dbSet) =>
            {
                var query = dbSet.Where(x => x.AuditInfo.IsDeleted == false);

                if (ownUuids?.Any() ?? false)
                    query = query.Where(x => ownUuids.Contains(x.Uuid));

                if (typeof(TEntity).IsAssignableTo(typeof(WorkingTreeMemberBase))
                && (owningTreesUuids?.Any() ?? false))
                    query = query.Where(x => owningTreesUuids.Contains((x as WorkingTreeMemberBase)!.OwningWorkingTreeUuid));

                return query.ToList();
            });
        }

        protected IEnumerable<WorkingTree> SelectWorkingTreeAggregates(Guid[]? uuids = null)
        {
            return ExecuteWithContext<WorkingTree, IEnumerable<WorkingTree>>((context, dbSet) =>
            {
                var query = dbSet
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(x => x.AuditInfo.IsDeleted == false);

                if (uuids?.Any() ?? false)
                    query = query.Where(x => uuids.Contains(x.Uuid));

                var trees = query
                    .Include(x => x.ContentRoot)
                    .Include(x => x.ContentNodes.Where(n => n.AuditInfo.IsDeleted == false))
                    .Include(x => x.ContentLeaves.Where(l => l.AuditInfo.IsDeleted == false))
                    .Include(x => x.ContentAttributes.Where(a => a.AuditInfo.IsDeleted == false))
                    .ToList();

                foreach (var tree in trees)
                {
                    if (tree.ContentRoot?.AuditInfo.IsDeleted == true)
                    {
                        tree.ContentRoot = null!;
                    }

                    tree.ContentNodes = tree.ContentNodes
                        .Where(x => x.AuditInfo.IsDeleted == false)
                        .ToList();

                    tree.ContentLeaves = tree.ContentLeaves
                        .Where(x => x.AuditInfo.IsDeleted == false)
                        .ToList();

                    tree.ContentAttributes = tree.ContentAttributes
                        .Where(x => x.AuditInfo.IsDeleted == false)
                        .ToList();
                }

                return trees;
            });
        }

        protected long Insert<TEntity>(IEnumerable<TEntity> items) where TEntity : class, IMainEntity
        {
            return ExecuteWithContext<TEntity, long>((context, dbSet) =>
            {
                dbSet.AddRange(items);
                AssignAuditInfoToTrackedGraph(context, AuditOperation.Insert);
                return context.SaveChanges();
            });
        }

        protected long Update<TEntity>(IEnumerable<TEntity> items) where TEntity : class, IMainEntity
        {
            return ExecuteWithContext<TEntity, long>((context, dbSet) =>
            {
                dbSet.UpdateRange(items);
                AssignAuditInfoToTrackedGraph(context, AuditOperation.Update);
                return context.SaveChanges();
            });
        }

        protected long SoftDelete<TEntity>(IEnumerable<TEntity> items) where TEntity : class, IMainEntity
        {
            return ExecuteWithContext<TEntity, long>((context, dbSet) =>
            {
                dbSet.UpdateRange(items);
                AssignAuditInfoToTrackedGraph(context, AuditOperation.SoftDelete);
                return context.SaveChanges();
            });
        }

        private static void AssignAuditInfoToTrackedGraph(DbContext context, AuditOperation operation)
        {
            var now = DateTime.UtcNow;
            var userName = Environment.UserName;

            foreach (var entry in context.ChangeTracker.Entries<IMainEntity>())
            {
                if (entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                    continue;

                var auditInfo = entry.Entity.AuditInfo ??= new AuditInfo();
                EnsureCreatedAuditInfo(auditInfo, userName, now);

                switch (operation)
                {
                    case AuditOperation.Insert:
                        if (entry.State == EntityState.Added)
                        {
                            EnsureCreatedAuditInfo(auditInfo, userName, now);
                        }
                        break;

                    case AuditOperation.Update:
                        if (entry.State == EntityState.Added)
                        {
                            EnsureCreatedAuditInfo(auditInfo, userName, now);
                        }
                        else if (entry.State == EntityState.Modified)
                        {
                            auditInfo.UpdatedBy = userName;
                            auditInfo.UpdatedAt = now;
                        }
                        break;

                    case AuditOperation.SoftDelete:
                        auditInfo.IsDeleted = true;
                        auditInfo.DeletedBy = userName;
                        auditInfo.DeletedAt = now;
                        break;
                }
            }
        }

        private static void EnsureCreatedAuditInfo(AuditInfo auditInfo, string userName, DateTime now)
        {
            if (string.IsNullOrWhiteSpace(auditInfo.CreatedBy))
                auditInfo.CreatedBy = userName;

            if (auditInfo.CreatedAt == default)
                auditInfo.CreatedAt = now;
        }

        protected abstract TContext GetNewContext();

        protected abstract DbSet<TEntity> GetDbSet<TEntity>(TContext context)
            where TEntity : class, IMainEntity;
    }
}
