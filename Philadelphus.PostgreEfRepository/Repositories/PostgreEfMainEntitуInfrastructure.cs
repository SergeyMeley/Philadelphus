using Microsoft.EntityFrameworkCore.Query.Internal;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.PostgreEfRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreEfRepository.Repositories
{
    public class PostgreEfMainEntityInfrastructure : IMainEntitiesInfrastructureRepository
    {
        public bool CanConnect()
        {
            bool result;
            using (var context = new PhiladelphusContext())
            {
                result = context.Database.CanConnect();
            }
            return result;
        }
        public InfrastructureTypes InfrastructureRepositoryTypes { get; } = InfrastructureTypes.PostgreSql;
        private readonly PhiladelphusContext _context = new PhiladelphusContext();
        

        public long DeleteAttributeEntries(IEnumerable<AttributeEntry> attributeEntries)
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributes(IEnumerable<ElementAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributeValues(IEnumerable<AttributeValue> attributeValues)
        {
            throw new NotImplementedException();
        }

        public long DeleteLeaves(IEnumerable<TreeLeave> leaves)
        {
            throw new NotImplementedException();
        }

        public long DeleteNodes(IEnumerable<TreeNode> nodes)
        {
            throw new NotImplementedException();
        }

        public long DeleteRepositories(IEnumerable<TreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

        public long DeleteRoots(IEnumerable<TreeRoot> roots)
        {
            throw new NotImplementedException();
        }

        public MainEntitiesCollection GetMainEntitiesCollection()
        {
            throw new NotImplementedException();
        }

        public long InsertAttributeEntries(IEnumerable<AttributeEntry> attributeEntries)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributes(IEnumerable<ElementAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributeValues(IEnumerable<AttributeValue> attributeValues)
        {
            throw new NotImplementedException();
        }

        public long InsertLeaves(IEnumerable<TreeLeave> leaves)
        {
            throw new NotImplementedException();
        }

        public long InsertNodes(IEnumerable<TreeNode> nodes)
        {
            throw new NotImplementedException();
        }

        public long InsertRepositories(IEnumerable<TreeRepository> repositories)
        {
            using(_context)
            {
                if (CanConnect() == false)
                    return 0;
                foreach (var item in repositories)
                {
                    item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    item.AuditInfo.CreatedOn = DateTime.Now.ToString();
                    item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    item.AuditInfo.UpdatedOn = DateTime.Now.ToString();
                    _context.Repositories.Add(item);
                }
                return _context.SaveChanges();
            }
        }

        public long InsertRoots(IEnumerable<TreeRoot> roots)
        {
            using (_context)
            {
                if (CanConnect() == false)
                    return 0;
                foreach (var item in roots)
                {
                    item.AuditInfo.CreatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    item.AuditInfo.CreatedOn = DateTime.Now.ToString();
                    item.AuditInfo.UpdatedBy = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                    item.AuditInfo.UpdatedOn = DateTime.Now.ToString();
                    _context.RootDetails.Add(item);
                }
                return _context.SaveChanges();
            }
        }

        public IEnumerable<AttributeEntry> SelectAttributeEntries()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ElementAttribute> SelectAttributes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<AttributeValue> SelectAttributeValues()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeLeave> SelectLeaves()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeNode> SelectNodes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRepository> SelectRepositories(List<string> pathes)
        {
            if (CanConnect() == false)
                return null;
            var result = new List<TreeRepository>();
            using (_context)
            {
                result = _context.Repositories.ToList();
            }
            return result;
        }

        public IEnumerable<TreeRoot> SelectRoots()
        {
            if (CanConnect() == false)
                return null;
            var result = new List<TreeRoot>();
            using (_context)
            {
                result = _context.RootDetails.ToList();
            }
            return result;
        }

        public long UpdateAttributeEntries(IEnumerable<AttributeEntry> attributeEntries)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributes(IEnumerable<ElementAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributeValues(IEnumerable<AttributeValue> attributeValues)
        {
            throw new NotImplementedException();
        }

        public long UpdateLeaves(IEnumerable<TreeLeave> leaves)
        {
            throw new NotImplementedException();
        }

        public long UpdateNodes(IEnumerable<TreeNode> nodes)
        {
            throw new NotImplementedException();
        }

        public long UpdateRepositories(IEnumerable<TreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

        public long UpdateRoots(IEnumerable<TreeRoot> roots)
        {
            throw new NotImplementedException();
        }
    }
}
