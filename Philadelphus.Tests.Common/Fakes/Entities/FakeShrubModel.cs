using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Common.Fakes.Entities
{
    public class FakeShrubModel : ShrubModel
    {
        public FakeShrubModel()
            : base(
                Guid.NewGuid(),
                new FakePhiladelphusRepositoryModel(),
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<ShrubModel>())
        {
        }
    }
}
