using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Infrastructure.Persistence.ADO.MongoDB.Repositories
{
    public class MongoMainEntitуInfrastructure : IPhiladelphusRepositoriesMembersInfrastructureRepository
    {
        MongoClient _client;
        IMongoDatabase _database;

        public InfrastructureEntityGroups EntityGroup => throw new NotImplementedException();

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
                BsonClassMap.RegisterClassMap<PhiladelphusRepository>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Uuid).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<TreeRoot>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Uuid).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<TreeNode>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Uuid).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
            try
            {
                BsonClassMap.RegisterClassMap<TreeLeave>(c => {
                    c.AutoMap();
                    c.MapIdProperty(s => s.Uuid).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                });
            }
            catch (Exception)
            {
            }
        }
        # region [ Select ]
        public IEnumerable<PhiladelphusRepository> SelectRepositories(List<string> pathes)
        {
            var collection = _database.GetCollection<PhiladelphusRepository>("repositories");
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
        #endregion
        #region [ Insert ]
        public long InsertRepositories(IEnumerable<PhiladelphusRepository> repositories)
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
        #endregion
        #region [ Delete ]
        public long DeleteRepositories(IEnumerable<PhiladelphusRepository> repositories)
        {
            long result = new long();
            return result;
        }
        public long SoftDeleteRoots(IEnumerable<TreeRoot> roots)
        {
            long result = new long();
            return result;
        }
        public long SoftDeleteNodes(IEnumerable<TreeNode> nodes)
        {
            long result = new long();
            return result;
        }
        public long SoftDeleteLeaves(IEnumerable<TreeLeave> leaves)
        {
            long result = new long();
            return result;
        }
        public long SoftDeleteAttributes(IEnumerable<ElementAttribute> attributes)
        {
            long result = new long();
            return result;
        }
        #endregion
        #region [ Update ]
        public long UpdateRepositories(IEnumerable<PhiladelphusRepository> repositories)
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
        #endregion


        public bool CheckAvailability()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeRoot> SelectRoots(Guid[] uuids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeNode> SelectNodes(Guid[] parentRootUuids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreeLeave> SelectLeaves(Guid[] parentRootUuids)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ElementAttribute> SelectAttributes()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WorkingTree> SelectTrees()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<WorkingTree> SelectTrees(Guid[] uuids)
        {
            throw new NotImplementedException();
        }

        public long InsertTrees(IEnumerable<WorkingTree> items)
        {
            throw new NotImplementedException();
        }

        public long UpdateTrees(IEnumerable<WorkingTree> items)
        {
            throw new NotImplementedException();
        }

        public long SoftDeleteTrees(IEnumerable<WorkingTree> items)
        {
            throw new NotImplementedException();
        }
    }
}
