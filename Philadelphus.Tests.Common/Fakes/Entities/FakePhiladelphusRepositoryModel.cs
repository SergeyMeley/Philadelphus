using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Common.Fakes.Entities
{
    public class FakePhiladelphusRepositoryModel : PhiladelphusRepositoryModel
    {
        public FakePhiladelphusRepositoryModel()
            : base(
                Guid.NewGuid(),
                new FakeDataStorageModel(),
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(),
                new EmptyPropertiesPolicy<ShrubModel>())
        {
        }
    }
}
