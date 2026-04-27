using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Builders;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Tests.Domain.Fakes.Entities;
using Philadelphus.Tests.Domain.Fakes.PoliciesAndRules;
using Philadelphus.Tests.Domain.Fakes.Services;
using Philadelphus.Tests.Domain.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Philadelphus.Tests.Domain.Entities.MainEntities.Attributes
{
    public class NonOwnAttributeRuleTests
    {
        [Fact]
        public void NonOwn_Should_Block_Core_Properties()
        {
            var policy = AttributePolicyBuilder.CreateDefault(new FakeNotificationService());

            var model = EntitiesCreationHelper.CreateAttribute();

            var result = policy.CanWrite(model, nameof(ElementAttributeModel.Name), "Test");

            Assert.False(result);
        }

        [Fact]
        public void Should_Block_Value_When_Own_And_Abstract()
        {
            var rule = new RequiredOverrideValuePropertiesRule(new FakeNotificationService());

            var model = EntitiesCreationHelper.CreateAttribute();

            var result = rule.CanWrite(model, nameof(ElementAttributeModel.Value), new object());

            Assert.False(result);
        }

        [Fact]
        public void Should_Allow_Value_When_Not_Abstract()
        {
            var rule = new RequiredOverrideValuePropertiesRule(new FakeNotificationService());

            var model = EntitiesCreationHelper.CreateAttribute();

            var result = rule.CanWrite(model, nameof(ElementAttributeModel.Value), new object());

            Assert.True(result);
        }

        [Fact]
        public void Should_Block_Override_When_Parent_Disabled()
        {
            var model = EntitiesCreationHelper.CreateAttribute();

            model.Override = OverrideType.Sealed;

            var policy = AttributePolicyBuilder.CreateDefault(new FakeNotificationService());

            var result = policy.CanWrite(model, nameof(ElementAttributeModel.Name), "Test");

            Assert.False(result);
        }

        [Fact]
        public void Should_Block_Value_When_Override_Required()
        {
            var model = EntitiesCreationHelper.CreateAttribute();

            var policy = AttributePolicyBuilder.CreateDefault(new FakeNotificationService());

            var result = policy.CanWrite(model, nameof(ElementAttributeModel.Value), new object());

            Assert.False(result);
        }

        [Fact]
        public void Should_Not_StackOverflow_On_Circular_Read()
        {
            var policy = new FakeRecursivePolicy<TestAttribute>(); // специально вызывает другой prop

            var model = new TestAttribute(policy);

            var value = model.Name;

            Assert.NotNull(value);
        }

        [Fact]
        public void RequiredOverride_Should_Block_Value()
        {
            var rule = new RequiredOverrideValuePropertiesRule(new FakeNotificationService());

            var model = EntitiesCreationHelper.CreateAttribute();

            model.Override = OverrideType.Abstract;

            var result = rule.CanWrite(model, nameof(ElementAttributeModel.Value), new object());

            Assert.False(result);
        }
    }
}
