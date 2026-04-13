using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Core.Domain.Policies.Attributes.Builders
{
    public static class AttributePolicyBuilder
    {
        public static IAttributePropertiesPolicy CreateDefault()
        {
            return new CompositeAttributePropertiesPolicy(new IAttributePropertiesRule<ElementAttributeModel>[]
            {
                new NonOwnAttributePropertiesRule(),
                new ParentOverrideForbiddenPropertiesRule(),
                new OverrideVisibilityPropertiesRule(),
                new RequiredOverrideValuePropertiesRule(),
            });
        }
    }
}
