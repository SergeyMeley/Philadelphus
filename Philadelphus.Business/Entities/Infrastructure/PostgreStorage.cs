using Philadelphus.InfrastructureEntities.Enums;
using Philadelphus.InfrastructureEntities.Interfaces;
using Philadelphus.PostgreInfrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Philadelphus.Business.Entities.Infrastructure
{
    internal class PostgreStorage : IDataStorage
    {
        public Guid Guid { get; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public InfrastructureTypes InfrastructureType { get => InfrastructureTypes.PostgreSql; }
        public IInfrastructureRepository InfrastructureRepository { get; } = new PostgreMainEntityInfrastructure();
        public string ConnectionString { get; }
        public bool IsDefaultStorage { get; set; }
        public PostgreStorage(Guid guid, string name, string connectionString)
        {
            Guid = guid;
            ConnectionString = connectionString;
        }
    }
}
