using Philadelphus.Infrastructure.Persistence.Common.Enums;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    public interface IInfrastructureRepository
    {
        //public InfrastructureTypes InfrastructureRepositoryTypes { get; }
        public InfrastructureEntityGroups EntityGroup { get; }
        public bool CheckAvailability();
    }
}
