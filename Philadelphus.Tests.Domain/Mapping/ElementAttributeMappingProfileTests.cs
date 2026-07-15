using AutoMapper;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

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
    public void Map_MissingValueTypeAndValue_SetsErrorsAndPreservesReferences()
    {
        // Arrange
        var tree = new FakeWorkingTreeModel();
        var valueTypeUuid = Guid.CreateVersion7();
        var valueUuid = Guid.CreateVersion7();
        var entity = CreateAttribute(tree, valueTypeUuid, valueUuid);
        var mapper = CreateMapper();

        // Act
        var model = MapAttribute(mapper, entity, tree);
        var mappedBackEntity = mapper.Map<ElementAttribute>(model);

        // Assert
        model.ValueType.Should().BeNull();
        model.Value.Should().BeNull();
        model.ValueTypeReferenceErrorCode.Should().Be(AttributeReferenceErrorCodes.ValueTypeNotFound);
        model.ValueReferenceErrorCode.Should().Be(AttributeReferenceErrorCodes.ValueNotFound);
        model.UnresolvedValueUuid.Should().Be(valueUuid);
        mappedBackEntity.ValueTypeUuid.Should().Be(valueTypeUuid);
        mappedBackEntity.ValueUuid.Should().Be(valueUuid);
    }

    [Fact]
    public void Map_MissingCollectionValues_SetsErrorAndPreservesReferences()
    {
        // Arrange
        var tree = new FakeWorkingTreeModel();
        var valueTypeUuid = Guid.CreateVersion7();
        var firstValueUuid = Guid.CreateVersion7();
        var secondValueUuid = Guid.CreateVersion7();
        var entity = CreateAttribute(tree, valueTypeUuid, valueUuid: null);
        entity.IsCollectionValue = true;
        entity.ValuesUuids = [firstValueUuid, secondValueUuid];
        var mapper = CreateMapper();

        // Act
        var model = MapAttribute(mapper, entity, tree);
        var mappedBackEntity = mapper.Map<ElementAttribute>(model);

        // Assert
        model.Values.Should().BeEmpty();
        model.ValueReferenceErrorCode.Should().Be(AttributeReferenceErrorCodes.ValueNotFound);
        mappedBackEntity.ValuesUuids.Should().Equal(firstValueUuid, secondValueUuid);
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
        FakeWorkingTreeModel tree)
    {
        return mapper.MapAttributes(
                [entity],
                [tree],
                new Dictionary<Guid, TreeNodeModel>(),
                new Dictionary<Guid, TreeLeaveModel>(),
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
