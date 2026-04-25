using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Builders;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Tests.Domain.Fakes.Entities;
using Philadelphus.Tests.Domain.Fakes.Services;
using Philadelphus.Tests.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Entities.MainEntities.Attributes
{
    public class ElementAttributePolicyTests
    {
        [Fact]
        public void Should_Block_When_Any_Rule_Blocks()
        {
            var rules = new IAttributePropertiesRule<ElementAttributeModel>[]
            {
                new RequiredOverrideValuePropertiesRule(new FakeNotificationService()),
                new FakeAlwaysDenyRule()
            };

            var policy = new CompositeAttributePropertiesPolicy(new FakeNotificationService(), rules);

            var model = EntitiesCreationHelper.CreateAttribute();

            var result = policy.CanWrite(model, nameof(ElementAttributeModel.Value), new object());

            Assert.False(result);
        }

        [Fact]
        public void DefaultPolicy_Should_Block_Invalid_Changes()
        {
            var policy = AttributePolicyBuilder.CreateDefault(new FakeNotificationService());

            var model = EntitiesCreationHelper.CreateAttribute(policy);

            model.Override = OverrideType.Abstract;

            var result = policy.CanWrite(model, nameof(ElementAttributeModel.Value), new object());

            Assert.False(result);
        }
    }
}
