using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.PostgreRepository.Repositories
{
    public class MainEntityRepository : IMainEntitiesRepository
    {
        public InfrastructureRepositoryTypes InftastructureRepositoryTypes { get; } = InfrastructureRepositoryTypes.PostgreSql;
        private readonly Context _context;
        public MainEntityRepository()
        {
            _context = new Context();
        }
        public DbMainEntitiesCollection GetMainEntitiesCollection()
        {
            DbMainEntitiesCollection collection = new DbMainEntitiesCollection();
            //collection.DbTreeRoots = (List<DbTreeRoot>)SelectRoots();
            collection.DbTreeNodes = (List<DbTreeNode>)SelectNodes();
            return collection;
        }
        #region [Select]
        IEnumerable<DbTreeRepository> SelectProjects()
        {
            var dataCollection = new List<DbTreeRepository>();
            using (var cmd = _context.CreateConnection().CreateCommand(""))
            using (var reader = cmd.ExecuteReaderAsync().Result)
            {
                while (reader.Read())
                {
                    var record = new DbTreeRepository();
                    record.Name = reader.GetString(1);
                    if (!reader.IsDBNull(3))
                    {
                        record.Description = reader.GetString(3);
                    }
                    dataCollection.Add(record);
                }
            }
            return dataCollection;
        }
        public IEnumerable<DbTreeRoot> SelectRoots()
        {
            var dataCollection = new List<DbTreeRoot>();
            using (var cmd = _context.CreateConnection().CreateCommand(""))
            using (var reader = cmd.ExecuteReaderAsync().Result)
            {
                while (reader.Read())
                {
                    var record = new DbTreeRoot();
                    record.Name = reader.GetString(1);
                    if (!reader.IsDBNull(2))
                    {
                        record.Description = reader.GetString(2);
                    }
                    dataCollection.Add(record);
                }
            }
            return dataCollection;
        }
        public IEnumerable<DbTreeNode> SelectNodes()
        {
            throw new NotImplementedException();
        }
        public IEnumerable<DbTreeLeave> SelectLeaves()
        {
            throw new NotImplementedException();
        }
        #endregion
        #region [Insert]
        public int InsertRepositories(IEnumerable<DbTreeRepository> projects)
        {
            try
            {
                using (var cmd = _context.CreateConnection().CreateCommand($""))
                {
                    cmd.ExecuteNonQuery();
                }
                return 0;
            }
            catch (Exception)
            {
                return -1;
            }
        }
        public int InsertRoots(IEnumerable<DbTreeRoot> layers)
        {
            throw new NotImplementedException();
        }
        public int InsertNodes(IEnumerable<DbTreeNode> nodes)
        {
            throw new NotImplementedException();
        }
        public int InsertLeaves(IEnumerable<DbTreeLeave> elements)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region [Delete]
        public int DeleteRepositories(int[] ids)
        {
            throw new NotImplementedException();
        }
        public int DeleteRoots(int[] ids)
        {
            throw new NotImplementedException();
        }
        public int DeleteNodes(int[] ids)
        {
            throw new NotImplementedException();
        }
        public int DeleteLeaves(int[] ids)
        {
            throw new NotImplementedException();
        }
        #endregion
        #region [Update]
        public int UpdateRepositories(IEnumerable<DbTreeRepository> projects)
        {
            throw new NotImplementedException();
        }
        public int UpdateRoots(IEnumerable<DbTreeRoot> layers)
        {
            throw new NotImplementedException();
        }
        public int UpdateNodes(IEnumerable<DbTreeNode> nodes)
        {
            throw new NotImplementedException();
        }
        public int UpdateLeaves(IEnumerable<DbTreeLeave> elements)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbTreeRepository> SelectRepositories(string configPath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbTreeRoot> SelectRoots(DbTreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbTreeNode> SelectNodes(DbTreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbTreeLeave> SelectLeaves(DbTreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbEntityAttribute> SelectAttributes(DbTreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbAttributeEntry> SelectAttributeEntries(DbTreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<DbAttributeValue> SelectAttributeValues(DbTreeRepository dbTreeRepository)
        {
            throw new NotImplementedException();
        }

        long IMainEntitiesRepository.InsertRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

        long IMainEntitiesRepository.InsertRoots(IEnumerable<DbTreeRoot> roots)
        {
            throw new NotImplementedException();
        }

        long IMainEntitiesRepository.InsertNodes(IEnumerable<DbTreeNode> nodes)
        {
            throw new NotImplementedException();
        }

        long IMainEntitiesRepository.InsertLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributes(IEnumerable<DbEntityAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            throw new NotImplementedException();
        }

        public long InsertAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            throw new NotImplementedException();
        }

        public long DeleteRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

        public long DeleteRoots(IEnumerable<DbTreeRoot> roots)
        {
            throw new NotImplementedException();
        }

        public long DeleteNodes(IEnumerable<DbTreeNode> nodes)
        {
            throw new NotImplementedException();
        }

        public long DeleteLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributes(IEnumerable<DbEntityAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            throw new NotImplementedException();
        }

        public long DeleteAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            throw new NotImplementedException();
        }

        long IMainEntitiesRepository.UpdateRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            throw new NotImplementedException();
        }

        long IMainEntitiesRepository.UpdateRoots(IEnumerable<DbTreeRoot> roots)
        {
            throw new NotImplementedException();
        }

        long IMainEntitiesRepository.UpdateNodes(IEnumerable<DbTreeNode> nodes)
        {
            throw new NotImplementedException();
        }

        long IMainEntitiesRepository.UpdateLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributes(IEnumerable<DbEntityAttribute> attributes)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            throw new NotImplementedException();
        }

        public long UpdateAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
