using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Contexts;
using Philadelphus.Infrastructure.Persistence.EF.Repositories;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Properties;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Infrastructure.Persistence.EF.PostgreSQL.Repositories
{
    public abstract class PostgreEfInfrastructureRepositoryBase<TContext> : EfInfrastructureRepositoryBase<TContext>
        where TContext : DbContext
    {
        public PostgreEfInfrastructureRepositoryBase(
            ILogger logger,
            string connectionString)
            : base(logger, connectionString)
        {
        }
        protected override bool IsDuplicateTableException(Exception ex)
        {
            return ex is PostgresException pgEx && pgEx.SqlState == "42P07";
        }
    }
}
