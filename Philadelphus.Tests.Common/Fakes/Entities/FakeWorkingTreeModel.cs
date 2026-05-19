using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Common.Fakes.Entities
{
    public class FakeWorkingTreeModel : WorkingTreeModel
    {
        public FakeWorkingTreeModel()
            : base(
                Guid.NewGuid(),
                new FakeDataStorageModel(),
                new FakeShrubModel(),
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<WorkingTreeModel>())
        {
        }
    }
}
