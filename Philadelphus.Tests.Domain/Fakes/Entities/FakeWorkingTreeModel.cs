using Moq;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Domain.Fakes.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Fakes.Entities
{
    public class FakeWorkingTreeModel : WorkingTreeModel
    {
        public FakeWorkingTreeModel()
            : base(Guid.NewGuid(), new Mock<IDataStorageModel>().Object, new FakeShrubModel(), new FakeNotificationService(), new EmptyPropertiesPolicy<WorkingTreeModel>())
        {
        }
    }
}
