using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Builders
{
    public static class AttributePolicyBuilder
    {
        public static IAttributePropertiesPolicy CreateDefault(INotificationService notificationService)
        {
            return new CompositeAttributePropertiesPolicy(notificationService, new IAttributePropertiesRule<ElementAttributeModel>[]
            {
                new NonOwnAttributePropertiesRule(notificationService),
                new ParentOverrideForbiddenPropertiesRule(notificationService),
                new OverrideVisibilityPropertiesRule(notificationService),
                new RequiredOverrideValuePropertiesRule(notificationService),
            });
        }
    }
}
