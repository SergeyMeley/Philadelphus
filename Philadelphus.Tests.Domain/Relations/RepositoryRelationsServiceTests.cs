using FluentAssertions;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Relations;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;
using Philadelphus.Tests.Domain.Fakes.PoliciesAndRules;

namespace Philadelphus.Tests.Domain.Relations;

/// <summary>
/// Проверяет вычисление связей объектов репозитория.
/// </summary>
public sealed class RepositoryRelationsServiceTests
{
    /// <summary>
    /// Проверяет, что владелец навигации задается только для атрибута и соответствует его фактическому владельцу.
    /// </summary>
    [Fact]
    public void GetDirectRelations_Content_SetsNavigationOwnerOnlyForAttribute()
    {
        var notificationService = new FakeNotificationService();
        var repository = new FakePhiladelphusRepositoryModel();
        var tree = new WorkingTreeModel(
            Guid.NewGuid(),
            new FakeDataStorageModel(),
            repository.ContentShrub,
            notificationService,
            new EmptyPropertiesPolicy<WorkingTreeModel>());
        repository.ContentShrub.AddContent(tree);
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var attributeUuid = Guid.NewGuid();
        var attribute = new ElementAttributeModel(
            attributeUuid,
            root,
            attributeUuid,
            root,
            tree,
            notificationService,
            new FakeAllowAllPolicy<ElementAttributeModel>());
        var service = new RepositoryRelationsService();

        var shrubRelations = service.GetDirectRelations(repository, repository.ContentShrub);
        var treeRelations = service.GetDirectRelations(repository, tree);

        shrubRelations.Should().ContainSingle(x =>
            x.Target.Uuid == tree.Uuid
            && x.Type == RepositoryRelationType.Content
            && x.NavigationOwnerUuid == null);
        treeRelations.Should().ContainSingle(x =>
            x.Target.Uuid == root.Uuid
            && x.Type == RepositoryRelationType.Content
            && x.NavigationOwnerUuid == null);
        treeRelations.Should().ContainSingle(x =>
            x.Target.Uuid == attribute.Uuid
            && x.Type == RepositoryRelationType.Content
            && x.NavigationOwnerUuid == root.Uuid);
    }

    /// <summary>
    /// Проверяет обратную связь узла с атрибутом, использующим узел как тип данных.
    /// </summary>
    [Fact]
    public void GetDirectRelations_DataTypeNode_ReturnsReferencingAttribute()
    {
        var notificationService = new FakeNotificationService();
        var repository = new FakePhiladelphusRepositoryModel();
        var tree = new WorkingTreeModel(
            Guid.NewGuid(),
            new FakeDataStorageModel(),
            repository.ContentShrub,
            notificationService,
            new EmptyPropertiesPolicy<WorkingTreeModel>());
        repository.ContentShrub.AddContent(tree);
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var dataType = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var attributeOwner = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var attributeUuid = Guid.NewGuid();
        var attribute = new ElementAttributeModel(
            attributeUuid,
            attributeOwner,
            attributeUuid,
            attributeOwner,
            tree,
            notificationService,
            new FakeAllowAllPolicy<ElementAttributeModel>())
        {
            ValueType = dataType
        };

        var service = new RepositoryRelationsService();

        attribute.ValueType.Should().BeSameAs(dataType);
        tree.ContentAttributes.Should().Contain(attribute);

        var relations = service.GetDirectRelations(
            repository,
            dataType);

        relations.Should().ContainSingle(x =>
            x.Target.Uuid == attribute.Uuid
            && x.Type == RepositoryRelationType.AttributeDataType
            && x.BlocksSourceDeletion);
    }
}
