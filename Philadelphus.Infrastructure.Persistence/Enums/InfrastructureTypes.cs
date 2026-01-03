using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.Enums
{
    public enum InfrastructureTypes
    {
        Empty,
        WindowsDirectory,
        PostgreSqlAdo,
        PostgreSqlEf,
        MongoDbAdo,
        MongoDbEf,
        MsSqlServerEf,
        SQLite,
        JsonDocument,
        XmlDocument
    }
}
