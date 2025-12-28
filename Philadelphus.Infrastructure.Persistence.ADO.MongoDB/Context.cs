using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
