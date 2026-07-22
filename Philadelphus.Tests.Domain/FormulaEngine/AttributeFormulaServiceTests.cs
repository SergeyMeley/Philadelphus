using FluentAssertions;

using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.FormulaEngine.Evaluation;
using Philadelphus.Core.Domain.FormulaEngine.Execution;
using Philadelphus.Core.Domain.FormulaEngine.Extensions;
using Philadelphus.Core.Domain.FormulaEngine.Formatting;
using Philadelphus.Core.Domain.FormulaEngine.Registry;
using Philadelphus.Core.Domain.FormulaEngine.Services;
using Philadelphus.Core.Domain.FormulaEngine.SystemFormulas;
using Philadelphus.Core.Domain.FormulaEngine.TreeLeaves;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.FormulaEngine;

public class AttributeFormulaServiceTests
{
    [Fact]
    public void TryCreateValueFormulaFromText_TreeLeaveReference_ReturnsFormula()
    {
        var fixture = CreateFixture();

        var result = fixture.Attribute.TryCreateValueFormulaFromText(
            $"  [{fixture.Value.Uuid}]  ",
            out var formula);

        result.Should().BeTrue();
        formula.Should().Be($"=[{fixture.Value.Uuid}]");
    }

    [Fact]
    public void TryCreateValueFormulaFromText_TreeLeaveName_ReturnsFormula()
    {
        var fixture = CreateFixture();
        fixture.Value.Name = "Допустимое значение";

        var result = fixture.Attribute.TryCreateValueFormulaFromText(fixture.Value.Name, out var formula);

        result.Should().BeTrue();
        formula.Should().Be($"=[{fixture.Value.Uuid}]");
    }

    [Fact]
    public void TryCreateValueFormulaFromText_UnknownValue_ReturnsFalse()
    {
        var fixture = CreateFixture();

        var result = fixture.Attribute.TryCreateValueFormulaFromText("Неизвестное значение", out var formula);

        result.Should().BeFalse();
        formula.Should().BeEmpty();
    }

    [Fact]
    public void SetFormulaText_Formula_SetsTrimmedFormulaAndClearsError()
    {
        var fixture = CreateFixture();
        fixture.Attribute.ValueFormulaErrorCode = "#ERROR!";

        fixture.Attribute.SetFormulaText("  =[019f6c74-6a97-7ea8-a24c-24f284b636c9]  ");

        fixture.Attribute.ValueFormula.Should().Be("=[019f6c74-6a97-7ea8-a24c-24f284b636c9]");
        fixture.Attribute.ValueFormulaErrorCode.Should().BeEmpty();
    }

    [Fact]
    public void SetFormulaText_TreeLeaveReference_AssignsReferencedValue()
    {
        var fixture = CreateFixture();

        fixture.Attribute.SetFormulaText($"[{fixture.Value.Uuid}]");

        fixture.Attribute.ValueFormula.Should().Be($"=[{fixture.Value.Uuid}]");
        fixture.Attribute.Value.Should().BeSameAs(fixture.Value);
    }

    [Fact]
    public void CanUseRelativeAttributeReference_SameOwner_ReturnsTrue()
    {
        var fixture = CreateFixture();
        var owner = (IAttributeOwnerModel)fixture.Attribute.Owner;
        var referencedUuid = Guid.CreateVersion7();
        var referencedAttribute = new ElementAttributeModel(
            referencedUuid,
            owner,
            referencedUuid,
            owner,
            fixture.Attribute.OwningWorkingTree,
            new FakeNotificationService(),
            new EmptyPropertiesPolicy<ElementAttributeModel>());

        FormulaReferenceRules.CanUseRelativeAttributeReference(fixture.Attribute, referencedAttribute)
            .Should().BeTrue();
    }

    [Fact]
    public void CanUseRelativeAttributeReference_DifferentOwners_ReturnsFalse()
    {
        var targetAttribute = CreateFixture().Attribute;
        var referencedAttribute = CreateFixture().Attribute;

        FormulaReferenceRules.CanUseRelativeAttributeReference(targetAttribute, referencedAttribute)
            .Should().BeFalse();
    }

    [Fact]
    public void AssignValueAsFormula_SetsFormulaAndMaterializedValue()
    {
        var fixture = CreateFixture();
        fixture.Attribute.ValueFormulaErrorCode = "#ERROR!";

        fixture.Attribute.AssignValueAsFormula(fixture.Value);

        fixture.Attribute.ValueFormula.Should().Be($"=[{fixture.Value.Uuid}]");
        fixture.Attribute.ValueFormulaErrorCode.Should().BeEmpty();
        fixture.Attribute.Value.Should().BeSameAs(fixture.Value);
    }

    [Fact]
    public void AssignValuesAsFormula_SetsShortCollectionFormula()
    {
        var fixture = CreateFixture();
        fixture.Attribute.IsCollectionValue = true;
        fixture.Attribute.TryAddValueToValuesCollection(fixture.Value).Should().BeTrue();

        fixture.Attribute.AssignValuesAsFormula();

        fixture.Attribute.ValueFormula.Should().Be($"={{[{fixture.Value.Uuid}]}}");
        fixture.Attribute.ValueFormulaErrorCode.Should().BeEmpty();
    }

    [Fact]
    public void SetFormulaText_CollectionFormula_SetsTrimmedFormula()
    {
        var fixture = CreateFixture();
        fixture.Attribute.IsCollectionValue = true;

        fixture.Attribute.SetFormulaText($"  ={{[{fixture.Value.Uuid}]}}  ");

        fixture.Attribute.ValueFormula.Should().Be($"={{[{fixture.Value.Uuid}]}}");
    }

    [Fact]
    public void ClearFormulaValue_ClearsFormulaErrorAndMaterializedValue()
    {
        var fixture = CreateFixture();
        fixture.Attribute.AssignValueAsFormula(fixture.Value);
        fixture.Attribute.ValueFormulaErrorCode = "#ERROR!";

        fixture.Attribute.ClearFormulaValue();

        fixture.Attribute.ValueFormula.Should().BeEmpty();
        fixture.Attribute.ValueFormulaErrorCode.Should().BeEmpty();
        fixture.Attribute.Value.Should().BeNull();
    }

    [Fact]
    public void RecalculateAttribute_MaterializesValueFromFormulaReference()
    {
        // Arrange
        var fixture = CreateFixture();

        // Act
        var result = Recalculate(fixture.Attribute, $"=[{fixture.Value.Uuid}]");

        // Assert
        result.Should().BeTrue();
        fixture.Attribute.ValueFormula.Should().Be($"=[{fixture.Value.Uuid}]");
        fixture.Attribute.ValueFormulaErrorCode.Should().BeEmpty();
        fixture.Attribute.Value.Should().BeSameAs(fixture.Value);
    }

    [Fact]
    public void RecalculateAttribute_MaterializesCollectionFromShortFormula()
    {
        var fixture = CreateFixture();
        fixture.Attribute.IsCollectionValue = true;
        var formula = $"={{[{fixture.Value.Uuid}]}}";

        var result = Recalculate(fixture.Attribute, formula);

        result.Should().BeTrue();
        fixture.Attribute.ValueFormula.Should().Be(formula);
        fixture.Attribute.ValueFormulaErrorCode.Should().BeEmpty();
        fixture.Attribute.Values.Should().ContainSingle().Which.Should().BeSameAs(fixture.Value);
    }

    [Fact]
    public void RecalculateAttribute_InvalidCollectionFormula_ClearsMaterializedValues()
    {
        var fixture = CreateFixture();
        fixture.Attribute.IsCollectionValue = true;
        fixture.Attribute.TryAddValueToValuesCollection(fixture.Value).Should().BeTrue();
        var formula = $"={{[{Guid.CreateVersion7()}]}}";

        var result = Recalculate(fixture.Attribute, formula);

        result.Should().BeTrue();
        fixture.Attribute.Values.Should().BeEmpty();
        fixture.Attribute.ValueFormula.Should().Be(formula);
        fixture.Attribute.ValueFormulaErrorCode.Should().NotBeEmpty();
    }

    [Fact]
    public void RecalculateAttribute_MissingReference_ClearsPreviousMaterializedValue()
    {
        // Arrange
        var fixture = CreateFixture();
        fixture.Attribute.Value = fixture.Value;
        var formula = $"=[{Guid.CreateVersion7()}]";

        // Act
        var result = Recalculate(fixture.Attribute, formula);

        // Assert
        result.Should().BeTrue();
        fixture.Attribute.Value.Should().BeNull();
        fixture.Attribute.ValueFormula.Should().Be(formula);
        fixture.Attribute.ValueFormulaErrorCode.Should().NotBeEmpty();
    }

    [Fact]
    public void RecalculateAttribute_MissingReference_DoesNotChangeLoadedAttributeState()
    {
        // Arrange
        var fixture = CreateFixture();
        var formula = $"=[{Guid.CreateVersion7()}]";
        fixture.Attribute.LoadPersistedMaterializedValueUuid(fixture.Value.Uuid);
        fixture.Attribute.LoadValueFormula(formula);
        ((IMainEntityWritableModel)fixture.Attribute).SetState(State.SavedOrLoaded);

        // Act
        var result = Recalculate(fixture.Attribute, formula);

        // Assert
        result.Should().BeTrue();
        fixture.Attribute.Value.Should().BeNull();
        fixture.Attribute.ValueFormulaErrorCode.Should().NotBeEmpty();
        fixture.Attribute.State.Should().Be(State.SavedOrLoaded);
    }

    [Fact]
    public void RecalculateAttribute_ResolvesValueFromValueTypeChildLeaves()
    {
        // Arrange
        var fixture = CreateFixtureWithValueInAnotherWorkingTree();
        var resolver = new TreeNodeTreeLeaveResolver(fixture.Attribute.ValueType);

        // Act
        var result = Recalculate(fixture.Attribute, $"=[{fixture.Value.Uuid}]", resolver);

        // Assert
        result.Should().BeTrue();
        fixture.Attribute.ValueFormulaErrorCode.Should().BeEmpty();
        fixture.Attribute.Value.Should().BeSameAs(fixture.Value);
    }

    private static bool Recalculate(
        ElementAttributeModel attribute,
        string formula,
        ITreeLeaveResolver? treeLeaveResolver = null)
    {
        var registry = new FormulaRegistry();
        registry.RegisterProvider(new TreeLeaveFormulaProvider());
        registry.RegisterProvider(new CollectionFormulaProvider());
        var evaluator = new FormulaAstEvaluator(registry);
        var service = new AttributeFormulaService(evaluator, new FakeNotificationService());

        return service.RecalculateAttribute(
            attribute,
            formula,
            new HashSet<Guid>(),
            new HashSet<Guid>(),
            target => new FormulaExecutionContext
            {
                WorkingTree = target.OwningWorkingTree,
                TreeLeaveResolver = treeLeaveResolver
                    ?? new WorkingTreeTreeLeaveResolver(target.OwningWorkingTree),
                CurrentAttributeOwner = target.Owner as IAttributeOwnerModel
            },
            _ => { });
    }

    private static (ElementAttributeModel Attribute, TreeLeaveModel Value) CreateFixture()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.CreateVersion7(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var valueType = new TreeNodeModel(
            Guid.CreateVersion7(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var value = new TreeLeaveModel(
            Guid.CreateVersion7(),
            valueType,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var attributeUuid = Guid.CreateVersion7();
        var attribute = new ElementAttributeModel(
            attributeUuid,
            root,
            attributeUuid,
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<ElementAttributeModel>())
        {
            ValueType = valueType
        };

        return (attribute, value);
    }

    private static (ElementAttributeModel Attribute, TreeLeaveModel Value) CreateFixtureWithValueInAnotherWorkingTree()
    {
        var notificationService = new FakeNotificationService();
        var ownerTree = new FakeWorkingTreeModel();
        var ownerRoot = new TreeRootModel(
            Guid.CreateVersion7(),
            ownerTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var valueTree = new FakeWorkingTreeModel();
        var valueRoot = new TreeRootModel(
            Guid.CreateVersion7(),
            valueTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var valueType = new TreeNodeModel(
            Guid.CreateVersion7(),
            valueRoot,
            valueTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var value = new TreeLeaveModel(
            Guid.CreateVersion7(),
            valueType,
            valueTree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var attributeUuid = Guid.CreateVersion7();
        var attribute = new ElementAttributeModel(
            attributeUuid,
            ownerRoot,
            attributeUuid,
            ownerRoot,
            ownerTree,
            notificationService,
            new EmptyPropertiesPolicy<ElementAttributeModel>())
        {
            ValueType = valueType
        };

        return (attribute, value);
    }
}
