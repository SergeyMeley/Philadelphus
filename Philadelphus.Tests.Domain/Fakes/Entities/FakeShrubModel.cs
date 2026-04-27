using Moq;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Domain.Fakes.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Fakes.Entities
{
    internal class FakeShrubModel : ShrubModel
    {
        public FakeShrubModel()
            : base(Guid.NewGuid(), new FakePhiladelphusRepositoryModel(), new FakeNotificationService(), new EmptyPropertiesPolicy<ShrubModel>())
        {

        }
    }
}
