using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Domain.Fakes.Entities;
using Philadelphus.Tests.Domain.Fakes.PoliciesAndRules;
using Philadelphus.Tests.Domain.Fakes.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Helpers
{
    internal static class EntitiesCreationHelper
    {
        public static ElementAttributeModel CreateAttribute(
            IPropertiesPolicy<ElementAttributeModel>? policy = null)
        {
            var notification = new FakeNotificationService();
            var owner = new FakeWorkingTreeModel();

            return new ElementAttributeModel(
                Guid.NewGuid(),
                owner,
                Guid.NewGuid(),
                owner,
                new FakeWorkingTreeModel(),
                notification,
                policy ?? new FakeAllowAllPolicy<ElementAttributeModel>()
            );
        }
    }
}
