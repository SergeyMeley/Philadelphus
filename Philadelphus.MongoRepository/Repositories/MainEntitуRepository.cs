using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.MongoRepository.Repositories
{
    public class MainEntitуRepository : IMainEntitiesRepository
    {
        MongoClient _client;
        IMongoDatabase _database;
        public MainEntitуRepository()
        {
            _client = Context.CreateConnection();
            _database = _client.GetDatabase("kubstu_database");
        }
        public DbMainEntitiesCollection GetMainEntitiesCollection()
        {
            var result = new DbMainEntitiesCollection();
            return result;
        }
        # region [ Select ]
        public IEnumerable<DbTreeRepository> SelectRepositories(string configPath)
        {
            var result = new List<DbTreeRepository>();
            return result;
        }
        public IEnumerable<DbTreeRoot> SelectRoots(DbTreeRepository dbTreeRepository)
        {
            var result = new List<DbTreeRoot>();
            var roots = _database.GetCollection<BsonDocument>("roots");
            //result = BsonSerializer.Deserialize<IEnumerable<DbTreeRoot>>(roots);
            //for (int i = 0; i < roots.Coun; i++)
            //{

            //}
            //foreach (var item in roots)
            //{
            //    result.Add(BsonSerializer.Deserialize<DbTreeRoot>(item));
                
            //}
            return result;
        }
        public IEnumerable<DbTreeNode> SelectNodes(DbTreeRepository dbTreeRepository)
        {
            var result = new List<DbTreeNode>();
            var repositories = _database.GetCollection<BsonDocument>("nodes");

            return result;
        }
        public IEnumerable<DbTreeLeave> SelectLeaves(DbTreeRepository dbTreeRepository)
        {
            var result = new List<DbTreeLeave>();
            var repositories = _database.GetCollection<BsonDocument>("leaves");

            return result;
        }
        public IEnumerable<DbAttribute> SelectAttributes(DbTreeRepository dbTreeRepository)
        {
            var result = new List<DbAttribute>();
            var repositories = _database.GetCollection<BsonDocument>("attributes");

            return result;
        }
        public IEnumerable<DbAttributeEntry> SelectAttributeEntries(DbTreeRepository dbTreeRepository)
        {
            var result = new List<DbAttributeEntry>();
            var repositories = _database.GetCollection<BsonDocument>("attribute_entries");

            return result;
        }
        public IEnumerable<DbAttributeValue> SelectAttributeValues(DbTreeRepository dbTreeRepository)
        {
            var result = new List<DbAttributeValue>();
            var repositories = _database.GetCollection<BsonDocument>("attribute_values");

            return result;
        }
        #endregion
        #region [ Insert ]
        public long InsertRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            long result = new long();
            return result;
        }
        public long InsertRoots(IEnumerable<DbTreeRoot> roots)
        {
            var collection = _database.GetCollection<BsonDocument>("roots");
            if (roots != null)
            {
                foreach (var item in roots)
                {
                    if (item != null)
                    {
                        var bson = item.ToBsonDocument();
                        collection.InsertOne(bson);
                    }
                }
            }
            return roots.Count();
        }
        public long InsertNodes(IEnumerable<DbTreeNode> nodes)
        {
            long result = new long();
            return result;
        }
        public long InsertLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            long result = new long();
            return result;
        }
        public long InsertAttributes(IEnumerable<DbAttribute> attributes)
        {
            long result = new long();
            return result;
        }
        public long InsertAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            long result = new long();
            return result;
        }
        public long InsertAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            long result = new long();
            return result;
        }
        #endregion
        #region [ Delete ]
        public long DeleteRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            long result = new long();
            return result;
        }
        public long DeleteRoots(IEnumerable<DbTreeRoot> roots)
        {
            long result = new long();
            return result;
        }
        public long DeleteNodes(IEnumerable<DbTreeNode> nodes)
        {
            long result = new long();
            return result;
        }
        public long DeleteLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            long result = new long();
            return result;
        }
        public long DeleteAttributes(IEnumerable<DbAttribute> attributes)
        {
            long result = new long();
            return result;
        }
        public long DeleteAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            long result = new long();
            return result;
        }
        public long DeleteAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            long result = new long();
            return result;
        }
        #endregion
        #region [ Update ]
        public long UpdateRepositories(IEnumerable<DbTreeRepository> repositories)
        {
            long result = new long();
            return result;
        }
        public long UpdateRoots(IEnumerable<DbTreeRoot> roots)
        {
            long result = new long();
            return result;
        }
        public long UpdateNodes(IEnumerable<DbTreeNode> nodes)
        {
            long result = new long();
            return result;
        }
        public long UpdateLeaves(IEnumerable<DbTreeLeave> leaves)
        {
            long result = new long();
            return result;
        }
        public long UpdateAttributes(IEnumerable<DbAttribute> attributes)
        {
            long result = new long();
            return result;
        }
        public long UpdateAttributeEntries(IEnumerable<DbAttributeEntry> attributeEntries)
        {
            long result = new long();
            return result;
        }
        public long UpdateAttributeValues(IEnumerable<DbAttributeValue> attributeValues)
        {
            long result = new long();
            return result;
        }
        #endregion
    }
}
