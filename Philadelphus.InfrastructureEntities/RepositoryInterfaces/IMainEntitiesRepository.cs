using Philadelphus.InfrastructureEntities.MainEntities;

namespace Philadelphus.InfrastructureEntities.RepositoryInterfaces
{
    public interface IMainEntitiesRepository : IInfrastructureRepository
    {
        DbMainEntitiesCollection GetMainEntitiesCollection();        //удалить?
        # region [ Select ]
        public IEnumerable<DbTreeRepository> SelectRepositories(List<string> pathes);
        public IEnumerable<DbTreeRoot> SelectRoots();
        public IEnumerable<DbTreeNode> SelectNodes();
        public IEnumerable<DbTreeLeave> SelectLeaves();
        public IEnumerable<DbEntityAttribute> SelectAttributes();
        public IEnumerable<DbAttributeEntry> SelectAttributeEntries();
        public IEnumerable<DbAttributeValue> SelectAttributeValues();
        #endregion
        #region [ Insert ]
        public long InsertRepositories(IEnumerable<DbTreeRepository> repositories);
        public long InsertRoots(IEnumerable<DbTreeRoot> roots);
        public long InsertNodes(IEnumerable<DbTreeNode> nodes);
        public long InsertLeaves(IEnumerable<DbTreeLeave> leaves);
        public long InsertAttributes(IEnumerable<DbEntityAttribute> attributes);
        public long InsertAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries);
        public long InsertAttributeValues(IEnumerable<DbAttributeValue> attributeValues);
        #endregion
        #region [ Delete ]
        public long DeleteRepositories(IEnumerable<DbTreeRepository> repositories);
        public long DeleteRoots(IEnumerable<DbTreeRoot> roots);
        public long DeleteNodes(IEnumerable<DbTreeNode> nodes);
        public long DeleteLeaves(IEnumerable<DbTreeLeave> leaves);
        public long DeleteAttributes(IEnumerable<DbEntityAttribute> attributes);
        public long DeleteAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries);
        public long DeleteAttributeValues(IEnumerable<DbAttributeValue> attributeValues);
        #endregion
        #region [ Update ]
        public long UpdateRepositories(IEnumerable<DbTreeRepository> repositories);
        public long UpdateRoots(IEnumerable<DbTreeRoot> roots);
        public long UpdateNodes(IEnumerable<DbTreeNode> nodes);
        public long UpdateLeaves(IEnumerable<DbTreeLeave> leaves);
        public long UpdateAttributes(IEnumerable<DbEntityAttribute> attributes);
        public long UpdateAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries);
        public long UpdateAttributeValues(IEnumerable<DbAttributeValue> attributeValues);
        #endregion
    }
} 
