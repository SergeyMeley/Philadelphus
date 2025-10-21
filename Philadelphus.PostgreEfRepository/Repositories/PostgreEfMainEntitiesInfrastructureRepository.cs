using Castle.Components.DictionaryAdapter.Xml;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
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
    public class PostgreEfMainEntitiesInfrastructureRepository : IMainEntitiesInfrastructureRepository
    {
        private readonly MainEntitiesPhiladelphusContext _context;
        public PostgreEfMainEntitiesInfrastructureRepository(string connectionString)
        {
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
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.MainEntities; }
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

        #region [ Select ]

        public IEnumerable<TreeRoot> SelectRoots()
        {
            if (CheckAvailability() == false)
                return null;
            return _context.TreeRoots.ToList();
        }
        public IEnumerable<TreeRoot> SelectRoots(Guid[] guids)
        {
            if (CheckAvailability() == false)
                return null;
            if (guids == null || guids.Any() == false)
                return new List<TreeRoot>();
            return _context.TreeRoots.Where(x => guids.Contains(x.Guid)).ToList();
        }
        public IEnumerable<TreeNode> SelectNodes()
        {
            if (CheckAvailability() == false)
                return null;
            return _context.TreeNodes.ToList();
        }
        public IEnumerable<TreeNode> SelectNodes(Guid parentRootGuid)
        {
            if (CheckAvailability() == false)
                return null;
            return _context.TreeNodes.Where(x => x.ParentTreeRoot.Guid == parentRootGuid).ToList();
        }
        public IEnumerable<TreeLeave> SelectLeaves()
        {
            if (CheckAvailability() == false)
                return null;
            return _context.TreeLeaves.ToList();
        }
        public IEnumerable<TreeLeave> SelectLeaves(Guid parentRootGuid)
        {
            if (CheckAvailability() == false)
                return null;
            return _context.TreeLeaves.Where(x => x.ParentTreeRoot.Guid == parentRootGuid).ToList();
        }
        public IEnumerable<ElementAttribute> SelectAttributes()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<TreeElementAttributeValue> SelectAttributeValues()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region [ Insert ]

        public long InsertRoots(IEnumerable<TreeRoot> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                //item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                //item.AuditInfo.CreatedAt = DateTime.UtcNow;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                _context.TreeRoots.Add(item);
            }
            return _context.SaveChanges();
        }
        public long InsertNodes(IEnumerable<TreeNode> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                if (_context.TreeRoots.FirstOrDefault(x => x.Guid == item.ParentTreeRootGuid) != null)
                {
                    item.ParentTreeRoot = _context.TreeRoots.FirstOrDefault(x => x.Guid == item.ParentTreeRootGuid);
                }
                //item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                //item.AuditInfo.CreatedAt = DateTime.UtcNow;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                _context.TreeNodes.Add(item);
            }
            return _context.SaveChanges();
        }
        public long InsertLeaves(IEnumerable<TreeLeave> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                if (_context.TreeRoots.FirstOrDefault(x => x.Guid == item.ParentTreeRootGuid) != null)
                {
                    item.ParentTreeRoot = _context.TreeRoots.FirstOrDefault(x => x.Guid == item.ParentTreeRootGuid);
                }
                //item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                //item.AuditInfo.CreatedAt = DateTime.UtcNow;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                _context.TreeLeaves.Add(item);
            }
            return _context.SaveChanges();
        }
        public long InsertAttributes(IEnumerable<ElementAttribute> items)
        {
            throw new NotImplementedException();
        }
        public long InsertAttributeValues(IEnumerable<TreeElementAttributeValue> items)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region [ Update ]

        public long UpdateRoots(IEnumerable<TreeRoot> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                _context.TreeRoots.Update(item);
            }
            return _context.SaveChanges();
        }
        public long UpdateNodes(IEnumerable<TreeNode> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                _context.TreeNodes.Update(item);
            }
            return _context.SaveChanges();
        }
        public long UpdateLeaves(IEnumerable<TreeLeave> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                _context.TreeLeaves.Update(item);
            }
            return _context.SaveChanges();
        }
        public long UpdateAttributes(IEnumerable<ElementAttribute> items)
        {
            throw new NotImplementedException();
        }
        public long UpdateAttributeValues(IEnumerable<TreeElementAttributeValue> items)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region [ Delete ]

        public long DeleteRoots(IEnumerable<TreeRoot> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.IsDeleted = true;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                _context.TreeRoots.Update(item);
            }
            return _context.SaveChanges();
        }
        public long DeleteNodes(IEnumerable<TreeNode> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.IsDeleted = true;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                _context.TreeNodes.Update(item);
            }
            return _context.SaveChanges();
        }
        public long DeleteLeaves(IEnumerable<TreeLeave> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.IsDeleted = true;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedAt = DateTime.UtcNow;
                _context.TreeLeaves.Update(item);
            }
            return _context.SaveChanges();
        }
        public long DeleteAttributes(IEnumerable<ElementAttribute> items)
        {
            throw new NotImplementedException();
        }
        public long DeleteAttributeValues(IEnumerable<TreeElementAttributeValue> items)
        {
            throw new NotImplementedException();
        }

        #endregion
        
    }
}
