using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.WindowsFileSystemRepository.Repositories
{
    public class MainEntityRepository : IMainEntitiesRepository
    {
        public DbMainEntitiesCollection GetMainEntitiesCollection()
        {
            return null;
        }
        # region [ Select ]
        public IEnumerable<DbTreeRepository> SelectRepositories(string configPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(configPath);
            return null;
        }
        public IEnumerable<DbTreeRoot> SelectRoots(DbTreeRepository dbTreeRepository)
        {
            return null;
        }
        public IEnumerable<DbTreeNode> SelectNodes(DbTreeRepository dbTreeRepository)
        {
            return null;
        }
        public IEnumerable<DbTreeLeave> SelectLeaves(DbTreeRepository dbTreeRepository)
        {
            return null;
        }
        public IEnumerable<DbAttribute> SelectAttributes(DbTreeRepository dbTreeRepository)
        {
            return null;
        }
        public IEnumerable<DbAttributeEntry> SelectAttributeEntries(DbTreeRepository dbTreeRepository)
        {
            return null;
        }
        public IEnumerable<DbAttributeValue> SelectAttributeValues(DbTreeRepository dbTreeRepository)
        {
            return null;
        }
        #endregion
        #region [ Insert ]
        public long InsertRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            return 0;
        }
        public long InsertRoots(IEnumerable<DbTreeRoot> roots)
        {
            return 0;
        }
        public long InsertNodes(IEnumerable<DbTreeNode> nodes)
        {
            return 0;
        }
        public long InsertLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            return 0;
        }
        public long InsertAttributes(IEnumerable<DbAttribute> attributes)
        {
            return 0;
        }
        public long InsertAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            return 0;
        }
        public long InsertAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            return 0;
        }
        #endregion
        #region [ Delete ]
        public long DeleteRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            return 0;
        }
        public long DeleteRoots(IEnumerable<DbTreeRoot> roots)
        {
            return 0;
        }
        public long DeleteNodes(IEnumerable<DbTreeNode> nodes)
        {
            return 0;
        }
        public long DeleteLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            return 0;
        }
        public long DeleteAttributes(IEnumerable<DbAttribute> attributes)
        {
            return 0;
        }
        public long DeleteAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            return 0;
        }
        public long DeleteAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            return 0;
        }
        #endregion
        #region [ Update ]
        public long UpdateRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            return 0;
        }
        public long UpdateRoots(IEnumerable<DbTreeRoot> roots)
        {
            return 0;
        }
        public long UpdateNodes(IEnumerable<DbTreeNode> nodes)
        {
            return 0;
        }
        public long UpdateLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            return 0;
        }
        public long UpdateAttributes(IEnumerable<DbAttribute> attributes)
        {
            return 0;
        }
        public long UpdateAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            return 0;
        }
        public long UpdateAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            return 0;
        }
        #endregion
    }
}
