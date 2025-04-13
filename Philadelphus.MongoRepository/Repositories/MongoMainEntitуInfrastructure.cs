using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.MainEntities;
using Philadelphus.InfrastructureEntities.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.MongoRepository.Repositories
{
    public class MongoMainEntitуInfrastructure : IMainEntitiesInfrastructure
    {
        public InfrastructureTypes InftastructureRepositoryTypes { get; } = InfrastructureTypes.MongoDb;
        MongoClient _client;
        IMongoDatabase _database;
        public MongoMainEntitуInfrastructure()
        {
            _client = Context.CreateConnection();
            _database = _client.GetDatabase("kubstu_database");
            Map();
        }
        public void Map()
        {
            try
            {
                BsonClassMap.RegisterClassMap<DbTreeRepository>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<DbTreeRoot>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<DbTreeNode>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<DbTreeLeave>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<DbTestEntitie>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
        }
        public DbMainEntitiesCollection GetMainEntitiesCollection()
        {
            var result = new DbMainEntitiesCollection();
            result.DbTreeRoots = SelectRoots();
            result.DbTreeNodes = SelectNodes();
            result.DbTreeLeaves = SelectLeaves();
            result.DbAttributes = SelectAttributes();
            result.DbAttributeEntries = SelectAttributeEntries();
            result.DbAttributeValues = SelectAttributeValues();
            return result;
        }
        # region [ Select ]
        public IEnumerable<DbTreeRepository> SelectRepositories(List<string> pathes)
        {
            var collection = _database.GetCollection<DbTreeRepository>("repositories");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<DbTreeRoot> SelectRoots()
        {
            var collection = _database.GetCollection<DbTreeRoot>("roots");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<DbTreeNode> SelectNodes()
        {
            var collection = _database.GetCollection<DbTreeNode>("nodes");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<DbTreeLeave> SelectLeaves()
        {
            var collection = _database.GetCollection<DbTreeLeave>("leaves");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<DbEntityAttribute> SelectAttributes()
        {
            var collection = _database.GetCollection<DbEntityAttribute>("attributes");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<DbAttributeEntry> SelectAttributeEntries()
        {
            var collection = _database.GetCollection<DbAttributeEntry>("attribute_entries");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<DbAttributeValue> SelectAttributeValues()
        {
            var collection = _database.GetCollection<DbAttributeValue>("attribute_values");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
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
            var collection = _database.GetCollection<DbTreeRoot>("roots");
            foreach (var item in roots)
            {
                collection.InsertOne(item);
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
        public long InsertAttributes(IEnumerable<DbEntityAttribute> attributes)
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
        public long DeleteAttributes(IEnumerable<DbEntityAttribute> attributes)
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
        public long UpdateAttributes(IEnumerable<DbEntityAttribute> attributes)
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


        public List<DbTestEntitie> SelectTest()
        {
            var collection = _database.GetCollection<DbTestEntitie>("test");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public long InsertTest(IEnumerable<DbTestEntitie> entitie)
        {
            
            var collection = _database.GetCollection<DbTestEntitie>("test");
            foreach (var item in entitie)
            {
                collection.InsertOne(item);
            }
            return entitie.Count();
        }
    }
}
