using MongoDB.Driver;

namespace Philadelphus.Infrastructure.Persistence.ADO.MongoDB
{
    internal static class Context
    {
        internal static MongoClient CreateConnection()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            return client;
        }
    }
}
