using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Common.Enums;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;

namespace Philadelphus.Tests.Common.Fakes.Entities
{
    public class FakeDataStorageModel : IDataStorageModel
    {
        public Guid Uuid { get; set; } = Guid.CreateVersion7();

        public string Name { get; set; } = "TestStorage";

        public string Description { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public InfrastructureTypes InfrastructureType => throw new NotImplementedException();

        public Dictionary<InfrastructureEntityGroups, IInfrastructureRepository> InfrastructureRepositories => throw new NotImplementedException();

        public IPhiladelphusRepositoriesInfrastructureRepository PhiladelphusRepositoriesInfrastructureRepository => throw new NotImplementedException();

        public IShrubMembersInfrastructureRepository ShrubMembersInfrastructureRepository => throw new NotImplementedException();

        public bool IsAvailable => throw new NotImplementedException();

        public bool IsDisabled { get; set; } = false;

        public bool IsHidden { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DateTime LastCheckTime => throw new NotImplementedException();

        public bool HasPhiladelphusRepositoriesInfrastructureRepository => throw new NotImplementedException();

        public bool HasShrubMembersInfrastructureRepository => throw new NotImplementedException();

        public IReportsInfrastructureRepository ReportsInfrastructureRepository => throw new NotImplementedException();

        public bool HasReportsInfrastructureRepository => throw new NotImplementedException();

        public bool CheckAvailable()
        {
            throw new NotImplementedException();
        }

        public Task<bool> CheckAvailableAsync()
        {
            throw new NotImplementedException();
        }

        public bool StartAvailableAutoChecking(int interval)
        {
            throw new NotImplementedException();
        }

        public bool StopAvailableAutoChecking()
        {
            throw new NotImplementedException();
        }
    }
}
