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
    public abstract class EfInfrastructureRepositoryBase<TContext> : IInfrastructureRepository
        where TContext : DbContext
    {
        protected readonly ILogger _logger;
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
                catch (Exception ex)
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
                    catch (Exception ex)
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
                return default;

            using (var context = GetNewContext())
            {
                var dbSet = GetDbSet<TEntity>(context);
                return action(context, dbSet);
            }
        }

        protected IEnumerable<TEntity> Select<TEntity>(Guid[] ownUuids = null, Guid[] owningTreesUuids = null) where TEntity : class, IMainEntity
        {
            return ExecuteWithContext<TEntity, IEnumerable<TEntity>>((context, dbSet) =>
            {
                var query = dbSet.Where(x => x.AuditInfo.IsDeleted == false);

                if (ownUuids?.Any() ?? false)
                    query = query.Where(x => ownUuids.Contains(x.Uuid));

                if (typeof(TEntity).IsAssignableTo(typeof(WorkingTreeMemberBase))
                && (owningTreesUuids?.Any() ?? false))
                    query = query.Where(x => owningTreesUuids.Contains((x as WorkingTreeMemberBase).OwningWorkingTreeUuid));

                return query.ToList();
            });
        }

        protected long Insert<TEntity>(IEnumerable<TEntity> items) where TEntity : class, IMainEntity
        {
            return ExecuteWithContext<TEntity, long>((context, dbSet) =>
            {
                // Предварительная обработка перед сохранением в БД
                var treesDictionary = GetNavigationEntitiesByUuid<WorkingTree>(
                    context,
                    items.Cast<IMainEntity>(),
                    item =>
                    {
                        if (item is WorkingTreeMemberBase wtm)
                        {
                            return wtm.OwningWorkingTreeUuid;
                        }
                        return null;
                    });

                foreach (var item in items)
                {
                    // Поля аудита
                    item.AuditInfo.CreatedBy = Environment.UserName;
                    item.AuditInfo.CreatedAt = DateTime.UtcNow;

                    // Навигационные свойства
                    SetNavigationProperties(item, treesDictionary);
                }

                // Сохранение
                dbSet.AddRange(items);
                MakeNavigationEntitiesUnchanged(context, items, typeof(AuditInfo));
                return context.SaveChanges();
            });
        }

        protected long Update<TEntity>(IEnumerable<TEntity> items) where TEntity : class, IMainEntity
        {
            return ExecuteWithContext<TEntity, long>((context, dbSet) =>
            {
                // Предварительная обработка перед сохранением в БД
                var treesDictionary = GetNavigationEntitiesByUuid<WorkingTree>(
                    context,
                    items.Cast<IMainEntity>(),
                    item =>
                    {
                        if (item is WorkingTreeMemberBase wtm)
                        {
                            return wtm.OwningWorkingTreeUuid;
                        }
                        return null;
                    });

                foreach (var item in items)
                {
                    // Поля аудита
                    item.AuditInfo.UpdatedBy = Environment.UserName;
                    item.AuditInfo.UpdatedAt = DateTime.UtcNow;

                    // Навигационные свойства
                    SetNavigationProperties(item, treesDictionary);
                }

                // Сохранение
                dbSet.UpdateRange(items);
                MakeNavigationEntitiesUnchanged(context, items, typeof(AuditInfo));
                return context.SaveChanges();
            });
        }

        protected long SoftDelete<TEntity>(IEnumerable<TEntity> items) where TEntity : class, IMainEntity
        {
            return ExecuteWithContext<TEntity, long>((context, dbSet) =>
            {
                // Предварительная обработка перед сохранением в БД
                var treesDictionary = GetNavigationEntitiesByUuid<WorkingTree>(
                    context,
                    items.Cast<IMainEntity>(),
                    item =>
                    {
                        if (item is WorkingTreeMemberBase wtm)
                        {
                            return wtm.OwningWorkingTreeUuid;
                        }
                        return null;
                    });

                foreach (var item in items)
                {
                    // Поля аудита
                    item.AuditInfo.IsDeleted = true;
                    item.AuditInfo.DeletedBy = Environment.UserName;
                    item.AuditInfo.DeletedAt = DateTime.UtcNow;

                    // Навигационные свойства
                    SetNavigationProperties(item, treesDictionary);
                }

                // Сохранение
                dbSet.UpdateRange(items);
                MakeNavigationEntitiesUnchanged(context, items, typeof(AuditInfo));
                return context.SaveChanges();
            });
        }

        internal static void MakeNavigationEntitiesUnchanged<TEntity>(DbContext context, IEnumerable<TEntity> entities, params Type[] skipTypes)
            where TEntity : class, IMainEntity
        {
            foreach (var entry in context.ChangeTracker.Entries<TEntity>().Where(x => entities.Any(e => e.Uuid == x.Entity.Uuid)))
            {
                foreach (var navEntry in entry.Navigations)
                {
                    if (navEntry.Metadata.IsCollection)
                    {
                        var collection = navEntry.CurrentValue as IEnumerable<object>;
                        if (collection != null)
                        {
                            foreach (var navEntity in collection)
                            {
                                ProcessNavigationEntity(context, navEntity, skipTypes);
                            }
                        }
                    }
                    else
                    {
                        var navEntity = navEntry.CurrentValue as object;
                        if (navEntity != null)
                        {
                            ProcessNavigationEntity(context, navEntity, skipTypes);
                        }
                    }
                }
            }
        }

        private static void ProcessNavigationEntity(DbContext context, object entity, params Type[] skipTypes)
        {
            if (skipTypes?.Contains(entity.GetType()) == true)
                return;

            var navEntryInternal = context.Entry(entity);
            if (navEntryInternal is { State: EntityState.Added })
            {
                navEntryInternal.State = EntityState.Unchanged;
            }
        }

        /// <summary>
        /// Возвращает словарь Guid → TNav для всех навигационных свойств TNav, встречающихся в items.
        /// TNav - тип навигационной сущности (например, WorkingTree, TreeNode, TreeRoot и т.п.)
        /// </summary>
        public static Dictionary<Guid, TNav> GetNavigationEntitiesByUuid<TNav>(
            DbContext context,
            IEnumerable<IMainEntity> items,
            Func<IMainEntity, Guid?> getEntityUuid)
            where TNav : class, IMainEntity
        {
            // Собираем все Uuid, которые участвуют как навигация
            var uuids = items
                .Select(getEntityUuid)
                .Where(uuid => uuid != null && uuid != Guid.Empty)
                .Distinct()
                .ToList();

            if (uuids.Count == 0)
            {
                return new Dictionary<Guid, TNav>();
            }

            return context
                .Set<TNav>()
                .AsNoTracking()
                .Where(n => uuids.Contains(n.Uuid))
                .ToDictionary(n => n.Uuid, n => n);
        }

        protected abstract TContext GetNewContext();

        protected abstract DbSet<TEntity> GetDbSet<TEntity>(TContext context)
            where TEntity : class, IMainEntity;

        protected abstract void SetNavigationProperties<TEntity, TNav>(TEntity item, Dictionary<Guid, TNav> navigationEntities)
            where TEntity : IMainEntity
            where TNav : IMainEntity;
    }
}