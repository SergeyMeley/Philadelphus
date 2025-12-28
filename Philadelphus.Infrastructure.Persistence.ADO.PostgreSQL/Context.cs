using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.ADO.PostgreSQL
{
    internal class Context
    {
        public NpgsqlDataSource CreateConnection(string connectionString)
        {
            var dataSource = NpgsqlDataSource.Create(connectionString);
            return dataSource;
        }
        public NpgsqlDataSource CreateConnection()
        {
            //var connectionString = $"Host=@host;Username=@username;password=@password;database=@database";
            var connectionString = $"Host=localhost;Username=postgres;password=Dniwe2002;database=Philadelphus";  //TEMP
            NpgsqlParameter[] parameters =
                {
                    new NpgsqlParameter("host", "localhost"),
                    new NpgsqlParameter("port", "5432"),
                    new NpgsqlParameter("username", "postgres"),
                    new NpgsqlParameter("password", "Dniwe2002"),
                    new NpgsqlParameter("database", "Philadelphus"),
                };
            var dataSource = NpgsqlDataSource.Create(connectionString);
            return dataSource;
        }
    }
}
