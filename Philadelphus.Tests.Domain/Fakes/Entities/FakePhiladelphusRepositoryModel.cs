using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Tests.Domain.Fakes.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Fakes.Entities
{
    internal class FakePhiladelphusRepositoryModel : PhiladelphusRepositoryModel
    {
        internal FakePhiladelphusRepositoryModel()
            : base(Guid.NewGuid(), new FakeDataStorageModel(), new FakeNotificationService(), new EmptyPropertiesPolicy<PhiladelphusRepositoryModel>(), new EmptyPropertiesPolicy<ShrubModel>())
        {
        }
    }
}
