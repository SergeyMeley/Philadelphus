using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Domain.Fakes.PoliciesAndRules;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Helpers
{
    internal static class EntitiesCreationHelper
    {
        public static ElementAttributeModel CreateAttribute(
            IPropertiesPolicy<ElementAttributeModel>? policy = null,
            bool isOwn = false)
        {
            var notification = new FakeNotificationService();
            var owner = new FakeWorkingTreeModel();
            var localUuid = Guid.NewGuid();

            return new ElementAttributeModel(
                localUuid,
                owner,
                isOwn ? localUuid : Guid.NewGuid(),
                owner,
                owner,
                notification,
                policy ?? new FakeAllowAllPolicy<ElementAttributeModel>()
            );
        }
    }
}
