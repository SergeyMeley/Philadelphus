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
        public InfrastructureEntityGroups EntityGroup { get => InfrastructureEntityGroups.MainEntitiesInfrastructureRepository; }
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
        public MainEntitiesCollection GetMainEntitiesCollection()
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributeEntries(IEnumerable<TreeElementAttribute> items)
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributes(IEnumerable<ElementAttribute> items)
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributeValues(IEnumerable<TreeElementAttributeValue> items)
        {
            throw new NotImplementedException();
        }

        public long DeleteLeaves(IEnumerable<TreeLeave> items)
        {
            throw new NotImplementedException();
        }

        public long DeleteNodes(IEnumerable<TreeNode> items)
        {
            throw new NotImplementedException();
        }

        public long DeleteRoots(IEnumerable<TreeRoot> items)
        {
            throw new NotImplementedException();
        }


        public long InsertAttributeEntries(IEnumerable<TreeElementAttribute> items)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributes(IEnumerable<ElementAttribute> items)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributeValues(IEnumerable<TreeElementAttributeValue> items)
        {
            throw new NotImplementedException();
        }

        public long InsertLeaves(IEnumerable<TreeLeave> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.CreatedOn = DateTime.Now;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedOn = DateTime.Now;
                _context.TreeLeaves.Add(item);
            }
            return _context.SaveChanges();

        }

        public long InsertNodes(IEnumerable<TreeNode> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.CreatedOn = DateTime.Now;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedOn = DateTime.Now;
                _context.TreeNodes.Add(item);
            }
            return _context.SaveChanges();
        }

        public long InsertRoots(IEnumerable<TreeRoot> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.CreatedOn = DateTime.Now;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedOn = DateTime.Now;
                _context.TreeRoots.Add(item);
            }
            return _context.SaveChanges();
        }

        public IEnumerable<TreeElementAttribute> SelectAttributeEntries()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ElementAttribute> SelectAttributes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeElementAttributeValue> SelectAttributeValues()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeLeave> SelectLeaves()
        {
            if (CheckAvailability() == false)
                return null;
            return _context.TreeLeaves.ToList();
        }

        public IEnumerable<TreeNode> SelectNodes()
        {
            if (CheckAvailability() == false)
                return null;
            return _context.TreeNodes.ToList();
        }

        public IEnumerable<TreeRoot> SelectRoots()
        {
            if (CheckAvailability() == false)
                return null;
            return _context.TreeRoots.ToList();
        }

        public long UpdateAttributeEntries(IEnumerable<TreeElementAttribute> items)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributes(IEnumerable<ElementAttribute> items)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributeValues(IEnumerable<TreeElementAttributeValue> items)
        {
            throw new NotImplementedException();
        }

        public long UpdateLeaves(IEnumerable<TreeLeave> items)
        {
            if (CheckAvailability() == false)
                return 0;
            foreach (var item in items)
            {
                item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.CreatedOn = DateTime.Now;
                item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                item.AuditInfo.UpdatedOn = DateTime.Now;
                _context.TreeRoots.Add(item);
            }
            return _context.SaveChanges();
        }

        public long UpdateNodes(IEnumerable<TreeNode> items)
        {
            throw new NotImplementedException();
        }

        public long UpdateRoots(IEnumerable<TreeRoot> items)
        {
            throw new NotImplementedException();
        }

    }
}
