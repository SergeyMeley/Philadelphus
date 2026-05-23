using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Attributes.Builders;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Policies.Rules;
using Philadelphus.Tests.Domain.Fakes.PoliciesAndRules;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using Philadelphus.Tests.Domain.Fakes.Entities;
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

        [Fact]
        public void RequiredOverride_Should_Block_Adding_Value_To_Values()
        {
            var policy = AttributePolicyBuilder.CreateDefault(new FakeNotificationService());
            var owner = new FakeWorkingTreeModel();
            var model = CreateOwnAttribute(owner, policy);
            var value = CreateLeave(owner);

            model.IsCollectionValue = true;
            model.Override = OverrideType.Abstract;

            var result = model.TryAddValueToValuesCollection(value);

            Assert.False(result);
            Assert.Empty(model.Values);
        }

        [Fact]
        public void InheritedAttributeValue_Should_Follow_Parent_Until_Overridden()
        {
            var owner = new FakeWorkingTreeModel();
            var parentAttribute = CreateOwnAttribute(owner);
            var firstValue = CreateLeave(owner);
            var secondValue = CreateLeave(owner);

            parentAttribute.Value = firstValue;
            var inheritedAttribute = parentAttribute.CloneForChild(owner);

            parentAttribute.Value = secondValue;

            Assert.Same(secondValue, inheritedAttribute.Value);

            inheritedAttribute.Value = firstValue;
            parentAttribute.Value = secondValue;

            Assert.Same(firstValue, inheritedAttribute.Value);
        }

        [Fact]
        public void InheritedAttributeValues_Should_Follow_Parent_Until_Overridden()
        {
            var owner = new FakeWorkingTreeModel();
            var parentAttribute = CreateOwnAttribute(owner);
            var firstValue = CreateLeave(owner);
            var secondValue = CreateLeave(owner);

            parentAttribute.IsCollectionValue = true;
            parentAttribute.TryAddValueToValuesCollection(firstValue);
            var inheritedAttribute = parentAttribute.CloneForChild(owner);

            parentAttribute.TryAddValueToValuesCollection(secondValue);

            Assert.Contains(secondValue, inheritedAttribute.Values);

            inheritedAttribute.TryRemoveValueFromValuesCollection(secondValue);
            parentAttribute.ClearValuesCollection();

            Assert.Contains(firstValue, inheritedAttribute.Values);
        }

        [Fact]
        public void InheritedAttributeValue_Should_Return_Parent_Value_When_Parent_Override_Is_Forbidden()
        {
            var owner = new FakeWorkingTreeModel();
            var parentAttribute = CreateOwnAttribute(owner);
            var firstValue = CreateLeave(owner);
            var secondValue = CreateLeave(owner);

            parentAttribute.Value = firstValue;
            var inheritedAttribute = parentAttribute.CloneForChild(owner);
            inheritedAttribute.Value = secondValue;

            Assert.Same(secondValue, inheritedAttribute.Value);

            parentAttribute.Override = OverrideType.Sealed;

            Assert.Same(firstValue, inheritedAttribute.Value);
            Assert.False(inheritedAttribute.IsValueOverridden);
        }

        [Fact]
        public void InheritedAttributeValues_Should_Return_Parent_Values_When_Parent_Override_Is_Forbidden()
        {
            var owner = new FakeWorkingTreeModel();
            var parentAttribute = CreateOwnAttribute(owner);
            var firstValue = CreateLeave(owner);
            var secondValue = CreateLeave(owner);

            parentAttribute.IsCollectionValue = true;
            parentAttribute.TryAddValueToValuesCollection(firstValue);
            var inheritedAttribute = parentAttribute.CloneForChild(owner);
            inheritedAttribute.TryRemoveValueFromValuesCollection(firstValue);
            parentAttribute.TryAddValueToValuesCollection(secondValue);

            Assert.Empty(inheritedAttribute.Values);

            parentAttribute.Override = OverrideType.Sealed;

            Assert.Contains(firstValue, inheritedAttribute.Values);
            Assert.Contains(secondValue, inheritedAttribute.Values);
            Assert.False(inheritedAttribute.AreValuesOverridden);
        }

        [Fact]
        public void ReservedName_Should_Block_ElementAttribute_Property_Name()
        {
            var rule = new ValidNamePropertiesRule<ElementAttributeModel>(new FakeNotificationService(), NameUniquenessStrategy.ElementAttribute());
            var model = CreateOwnAttribute(new FakeWorkingTreeModel());

            var result = rule.CanWrite(model, nameof(ElementAttributeModel.Name), nameof(ElementAttributeModel.Value));

            Assert.False(result);
        }

        [Fact]
        public void ReservedName_Should_Block_WorkingTreeMember_Property_Display_Name()
        {
            var rule = new ValidNamePropertiesRule<ElementAttributeModel>(new FakeNotificationService(), NameUniquenessStrategy.ElementAttribute());
            var model = CreateOwnAttribute(new FakeWorkingTreeModel());

            var result = rule.CanWrite(model, nameof(ElementAttributeModel.Name), "\u041a\u043e\u0434");

            Assert.False(result);
        }

        [Fact]
        public void ReservedName_Should_Block_WorkingTreeMember_Property_Name()
        {
            var rule = new ValidNamePropertiesRule<ElementAttributeModel>(new FakeNotificationService(), NameUniquenessStrategy.ElementAttribute());
            var model = CreateOwnAttribute(new FakeWorkingTreeModel());

            var result = rule.CanWrite(model, nameof(ElementAttributeModel.Name), nameof(WorkingTreeMemberBaseModel<ElementAttributeModel>.CustomCode));

            Assert.False(result);
        }

        [Fact]
        public void ReservedName_Should_Block_Other_Attribute_Name_With_Same_Owner()
        {
            var rule = new ValidNamePropertiesRule<ElementAttributeModel>(new FakeNotificationService(), NameUniquenessStrategy.ElementAttribute());
            var owner = new FakeWorkingTreeModel();
            var existingAttribute = CreateOwnAttribute(owner);
            var model = CreateOwnAttribute(owner);

            existingAttribute.Name = "Existing";

            var result = rule.CanWrite(model, nameof(ElementAttributeModel.Name), "Existing");

            Assert.False(result);
        }

        [Fact]
        public void ReservedName_Should_Allow_Unique_Attribute_Name()
        {
            var rule = new ValidNamePropertiesRule<ElementAttributeModel>(new FakeNotificationService(), NameUniquenessStrategy.ElementAttribute());
            var owner = new FakeWorkingTreeModel();
            var existingAttribute = CreateOwnAttribute(owner);
            var model = CreateOwnAttribute(owner);

            existingAttribute.Name = "Existing";

            var result = rule.CanWrite(model, nameof(ElementAttributeModel.Name), "Unique");

            Assert.True(result);
        }

        [Fact]
        public void RemoveAttribute_Should_Remove_Existing_Attribute()
        {
            var owner = new FakeWorkingTreeModel();
            var attribute = CreateOwnAttribute(owner);

            var result = owner.RemoveAttribute(attribute);

            Assert.True(result);
            Assert.DoesNotContain(owner.Attributes, x => x.Uuid == attribute.Uuid);
        }

        private static ElementAttributeModel CreateOwnAttribute(
            FakeWorkingTreeModel owner,
            IPropertiesPolicy<ElementAttributeModel>? propertiesPolicy = null)
        {
            var uuid = Guid.NewGuid();

            return new ElementAttributeModel(
                uuid,
                owner,
                uuid,
                owner,
                owner,
                new FakeNotificationService(),
                propertiesPolicy ?? new EmptyPropertiesPolicy<ElementAttributeModel>());
        }

        private static TreeLeaveModel CreateLeave(FakeWorkingTreeModel tree)
        {
            var root = new TreeRootModel(
                Guid.NewGuid(),
                tree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<TreeRootModel>());
            var node = new TreeNodeModel(
                Guid.NewGuid(),
                root,
                tree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<TreeNodeModel>());

            return new TreeLeaveModel(
                Guid.NewGuid(),
                node,
                tree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<TreeLeaveModel>());
        }
    }
}
