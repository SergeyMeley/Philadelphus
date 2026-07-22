using AutoMapper;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Mapping;
using Philadelphus.Core.Domain.Mapping.MainEntitiesMapping;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.Mapping;

public class ElementAttributeMappingProfileTests
{
    [Fact]
    public void Map_FormulaValue_IgnoresMaterializedValueFromDatabase()
    {
        // Arrange
        var tree = new FakeWorkingTreeModel();
        var notificationService = new FakeNotificationService();
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
        var materializedValue = new TreeLeaveModel(
            Guid.CreateVersion7(),
            valueType,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var entity = CreateAttribute(tree, valueType.Uuid, materializedValue.Uuid);
        entity.ValueFormula = $"=[{materializedValue.Uuid}]";
        var mapper = CreateMapper();

        // Act
        var model = MapAttribute(
            mapper,
            entity,
            tree,
            new Dictionary<Guid, TreeNodeModel> { [valueType.Uuid] = valueType });

        // Assert
        model.ValueType.Should().BeSameAs(valueType);
        model.Value.Should().BeNull();
        model.ValueFormula.Should().Be(entity.ValueFormula);

        // Имитируем вычисление той же формулы после завершения загрузки. Совпавший с сохраненным
        // материализованный результат не должен делать только что загруженный атрибут измененным.
        ((IMainEntityWritableModel)model).SetState(State.SavedOrLoaded);
        model.Value = materializedValue;
        var mappedBackEntity = mapper.Map<ElementAttribute>(model);

        model.State.Should().Be(State.SavedOrLoaded);
        mappedBackEntity.ValueTypeUuid.Should().Be(valueType.Uuid);
        mappedBackEntity.ValueUuid.Should().Be(materializedValue.Uuid);
        mappedBackEntity.ValueFormula.Should().Be(entity.ValueFormula);
    }

    [Fact]
    public void Map_ValueWithoutFormula_DoesNotUseMaterializedValueAsSource()
    {
        // Arrange
        var tree = new FakeWorkingTreeModel();
        var valueUuid = Guid.CreateVersion7();
        var entity = CreateAttribute(tree, Guid.CreateVersion7(), valueUuid);
        var mapper = CreateMapper();

        // Act
        var model = MapAttribute(mapper, entity, tree);

        // Assert
        model.Value.Should().BeNull();
        model.ValueFormula.Should().BeEmpty();
    }

    [Fact]
    public void Map_MissingValueType_SetsErrorAndPreservesReference()
    {
        // Arrange
        var tree = new FakeWorkingTreeModel();
        var valueTypeUuid = Guid.CreateVersion7();
        var entity = CreateAttribute(tree, valueTypeUuid, valueUuid: null);
        var mapper = CreateMapper();

        // Act
        var model = MapAttribute(mapper, entity, tree);
        var mappedBackEntity = mapper.Map<ElementAttribute>(model);

        // Assert
        model.ValueType.Should().BeNull();
        model.ValueTypeReferenceErrorCode.Should().Be(AttributeReferenceErrorCodes.ValueTypeNotFound);
        mappedBackEntity.ValueTypeUuid.Should().Be(valueTypeUuid);
    }

    [Fact]
    public void Map_CollectionValues_IgnoresMaterializedValuesFromDatabase()
    {
        // Arrange
        var tree = new FakeWorkingTreeModel();
        var valueTypeUuid = Guid.CreateVersion7();
        var firstValueUuid = Guid.CreateVersion7();
        var secondValueUuid = Guid.CreateVersion7();
        var entity = CreateAttribute(tree, valueTypeUuid, valueUuid: null);
        entity.IsCollectionValue = true;
        entity.ValuesUuids = [firstValueUuid, secondValueUuid];
        entity.ValueFormula = $"={{[{firstValueUuid}];[{secondValueUuid}]}}";
        var mapper = CreateMapper();

        // Act
        var model = MapAttribute(mapper, entity, tree);
        var mappedBackEntity = mapper.Map<ElementAttribute>(model);

        // Assert
        model.Values.Should().BeEmpty();
        model.ValueFormula.Should().Be(entity.ValueFormula);
        mappedBackEntity.ValuesUuids.Should().BeEmpty();
    }

    private static ElementAttribute CreateAttribute(
        FakeWorkingTreeModel tree,
        Guid? valueTypeUuid,
        Guid? valueUuid)
    {
        var attributeUuid = Guid.CreateVersion7();

        return new ElementAttribute
        {
            Uuid = attributeUuid,
            DeclaringUuid = attributeUuid,
            OwnerUuid = tree.Uuid,
            DeclaringOwnerUuid = tree.Uuid,
            OwningWorkingTreeUuid = tree.Uuid,
            ValueTypeUuid = valueTypeUuid,
            ValueUuid = valueUuid
        };
    }

    private static ElementAttributeModel MapAttribute(
        IMapper mapper,
        ElementAttribute entity,
        FakeWorkingTreeModel tree,
        IReadOnlyDictionary<Guid, TreeNodeModel>? valueTypesByUuid = null)
    {
        return mapper.MapAttributes(
                [entity],
                [tree],
                valueTypesByUuid ?? new Dictionary<Guid, TreeNodeModel>(),
                tree,
                new FakeNotificationService(),
                new EmptyPropertiesPolicy<ElementAttributeModel>())
            .Single();
    }

    private static IMapper CreateMapper()
    {
        var configuration = new MapperConfiguration(
            cfg =>
            {
                cfg.AddProfile<AuditInfoMappingProfile>();
                cfg.AddProfile<ElementAttributeMappingProfile>();
            },
            NullLoggerFactory.Instance);

        return configuration.CreateMapper();
    }
}
