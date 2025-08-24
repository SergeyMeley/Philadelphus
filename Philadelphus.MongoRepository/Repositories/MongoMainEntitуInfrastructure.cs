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
        public InfrastructureTypes InfrastructureRepositoryTypes { get; } = InfrastructureTypes.MongoDb;
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
                BsonClassMap.RegisterClassMap<TreeRepository>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<TreeRoot>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<TreeNode>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<TreeLeave>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<TestEntitie>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Guid).SetSerializer(new MongoDB.Bson.Serialization.Serializers.GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
        }
        public MainEntitiesCollection GetMainEntitiesCollection()
        {
            var result = new MainEntitiesCollection();
            result.DbTreeRoots = SelectRoots();
            result.DbTreeNodes = SelectNodes();
            result.DbTreeLeaves = SelectLeaves();
            result.DbAttributes = SelectAttributes();
            result.DbAttributeEntries = SelectAttributeEntries();
            result.DbAttributeValues = SelectAttributeValues();
            return result;
        }
        # region [ Select ]
        public IEnumerable<TreeRepository> SelectRepositories(List<string> pathes)
        {
            var collection = _database.GetCollection<TreeRepository>("repositories");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<TreeRoot> SelectRoots()
        {
            var collection = _database.GetCollection<TreeRoot>("roots");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<TreeNode> SelectNodes()
        {
            var collection = _database.GetCollection<TreeNode>("nodes");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<TreeLeave> SelectLeaves()
        {
            var collection = _database.GetCollection<TreeLeave>("leaves");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<ElementAttribute> SelectAttributes()
        {
            var collection = _database.GetCollection<ElementAttribute>("attributes");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<AttributeEntry> SelectAttributeEntries()
        {
            var collection = _database.GetCollection<AttributeEntry>("attribute_entries");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public IEnumerable<AttributeValue> SelectAttributeValues()
        {
            var collection = _database.GetCollection<AttributeValue>("attribute_values");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        #endregion
        #region [ Insert ]
        public long InsertRepositories(IEnumerable<TreeRepository> repositories)
        {
            long result = new long();
            return result;
        }
        public long InsertRoots(IEnumerable<TreeRoot> roots)
        {
            var collection = _database.GetCollection<TreeRoot>("roots");
            foreach (var item in roots)
            {
                collection.InsertOne(item);
            }
            return roots.Count();
        }
        public long InsertNodes(IEnumerable<TreeNode> nodes)
        {
            long result = new long();
            return result;
        }
        public long InsertLeaves(IEnumerable<TreeLeave> leaves)
        {
            long result = new long();
            return result;
        }
        public long InsertAttributes(IEnumerable<ElementAttribute> attributes)
        {
            long result = new long();
            return result;
        }
        public long InsertAttributeEntries(IEnumerable<AttributeEntry> attributeEntries)
        {
            long result = new long();
            return result;
        }
        public long InsertAttributeValues(IEnumerable<AttributeValue> attributeValues)
        {
            long result = new long();
            return result;
        }
        #endregion
        #region [ Delete ]
        public long DeleteRepositories(IEnumerable<TreeRepository> repositories)
        {
            long result = new long();
            return result;
        }
        public long DeleteRoots(IEnumerable<TreeRoot> roots)
        {
            long result = new long();
            return result;
        }
        public long DeleteNodes(IEnumerable<TreeNode> nodes)
        {
            long result = new long();
            return result;
        }
        public long DeleteLeaves(IEnumerable<TreeLeave> leaves)
        {
            long result = new long();
            return result;
        }
        public long DeleteAttributes(IEnumerable<ElementAttribute> attributes)
        {
            long result = new long();
            return result;
        }
        public long DeleteAttributeEntries(IEnumerable<AttributeEntry> attributeEntries)
        {
            long result = new long();
            return result;
        }
        public long DeleteAttributeValues(IEnumerable<AttributeValue> attributeValues)
        {
            long result = new long();
            return result;
        }
        #endregion
        #region [ Update ]
        public long UpdateRepositories(IEnumerable<TreeRepository> repositories)
        {
            long result = new long();
            return result;
        }
        public long UpdateRoots(IEnumerable<TreeRoot> roots)
        {
            long result = new long();
            return result;
        }
        public long UpdateNodes(IEnumerable<TreeNode> nodes)
        {
            long result = new long();
            return result;
        }
        public long UpdateLeaves(IEnumerable<TreeLeave> leaves)
        {
            long result = new long();
            return result;
        }
        public long UpdateAttributes(IEnumerable<ElementAttribute> attributes)
        {
            long result = new long();
            return result;
        }
        public long UpdateAttributeEntries(IEnumerable<AttributeEntry> attributeEntries)
        {
            long result = new long();
            return result;
        }
        public long UpdateAttributeValues(IEnumerable<AttributeValue> attributeValues)
        {
            long result = new long();
            return result;
        }
        #endregion


        public List<TestEntitie> SelectTest()
        {
            var collection = _database.GetCollection<TestEntitie>("test");
            var documents = collection.Find(new BsonDocument()).ToList();
            return documents;
        }
        public long InsertTest(IEnumerable<TestEntitie> entitie)
        {
            
            var collection = _database.GetCollection<TestEntitie>("test");
            foreach (var item in entitie)
            {
                collection.InsertOne(item);
            }
            return entitie.Count();
        }
    }
}
