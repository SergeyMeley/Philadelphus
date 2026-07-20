using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;

public class MainEntityBaseVMTests
{
    /// <summary>
    /// Изменение выбранного атрибута должно уведомлять вложенные Avalonia-привязки.
    /// </summary>
    [Fact]
    public void SelectedAttributeVM_Changed_RaisesPropertyChanged()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var attribute = CreateAttribute(root, tree, notificationService);
        var rootVM = new TreeRootVM(
            root,
            CreateDataStoragesCollectionVM(tree.DataStorage),
            DispatchProxy.Create<IPhiladelphusRepositoryService, DefaultDispatchProxy>(),
            DispatchProxy.Create<IFileDialogService, DefaultDispatchProxy>(),
            notificationService);
        var attributeVM = rootVM.AttributesVMs.Single(x => x.Model == attribute);
        var changedProperties = new List<string?>();
        rootVM.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);

        rootVM.SelectedAttributeVM = attributeVM;

        rootVM.SelectedAttributeVM.Should().BeSameAs(attributeVM);
        changedProperties.Should().Contain(nameof(rootVM.SelectedAttributeVM));
    }

    [Fact]
    public void AttributesVMs_NewChildNode_InheritsExistingParentAttribute()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var firstAttribute = CreateAttribute(root, tree, notificationService);
        var child = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var childVM = CreateNodeVM(child, tree, notificationService);

        child.ParentElementAttributes.Should().ContainSingle()
            .Which.DeclaringUuid.Should().Be(firstAttribute.DeclaringUuid);
        childVM.AttributesVMs.Should().ContainSingle()
            .Which.Model.DeclaringUuid.Should().Be(firstAttribute.DeclaringUuid);
    }

    [Fact]
    public void AttributesVMs_ExistingChildNode_InheritsNewParentAttribute()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var firstAttribute = CreateAttribute(root, tree, notificationService);
        var child = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var childVM = CreateNodeVM(child, tree, notificationService);

        var attributesVMs = childVM.AttributesVMs;
        var firstAttributeVM = attributesVMs.Single();

        var secondAttribute = CreateAttribute(root, tree, notificationService);
        childVM.NotifyChildsPropertyChangedRecursive();

        child.Attributes.Should().HaveCount(2);
        attributesVMs.Should().HaveCount(2);
        childVM.AttributesVMs.Should().BeSameAs(attributesVMs);
        childVM.AttributesVMs.Select(x => x.Model.DeclaringUuid)
            .Should().BeEquivalentTo(new[] { firstAttribute.DeclaringUuid, secondAttribute.DeclaringUuid });
        childVM.AttributesVMs.Single(x => x.Model.DeclaringUuid == firstAttribute.DeclaringUuid)
            .Should().BeSameAs(firstAttributeVM);
    }

    [Fact]
    public void AttributesVMs_NewChildLeave_InheritsExistingParentAttribute()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var parent = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var attribute = CreateAttribute(parent, tree, notificationService);
        var leave = new TreeLeaveModel(
            Guid.NewGuid(),
            parent,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var parentVM = CreateNodeVM(parent, tree, notificationService);
        var leaveVM = parentVM.ChildLeaves.Single();

        leave.ParentElementAttributes.Should().ContainSingle()
            .Which.DeclaringUuid.Should().Be(attribute.DeclaringUuid);
        leaveVM.AttributesVMs.Should().ContainSingle()
            .Which.Model.DeclaringUuid.Should().Be(attribute.DeclaringUuid);
    }

    [Fact]
    public void AttributesVMs_ExistingChildLeave_InheritsNewParentAttribute()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var parent = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var firstAttribute = CreateAttribute(parent, tree, notificationService);
        _ = new TreeLeaveModel(
            Guid.NewGuid(),
            parent,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var parentVM = CreateNodeVM(parent, tree, notificationService);
        var leaveVM = parentVM.ChildLeaves.Single();
        var attributesVMs = leaveVM.AttributesVMs;
        var firstAttributeVM = attributesVMs.Single();

        var secondAttribute = CreateAttribute(parent, tree, notificationService);
        parentVM.NotifyChildsPropertyChangedRecursive();

        leaveVM.Model.ParentElementAttributes.Should().HaveCount(2);
        attributesVMs.Should().HaveCount(2);
        leaveVM.AttributesVMs.Should().BeSameAs(attributesVMs);
        leaveVM.AttributesVMs.Select(x => x.Model.DeclaringUuid)
            .Should().BeEquivalentTo(new[] { firstAttribute.DeclaringUuid, secondAttribute.DeclaringUuid });
        leaveVM.AttributesVMs.Single(x => x.Model.DeclaringUuid == firstAttribute.DeclaringUuid)
            .Should().BeSameAs(firstAttributeVM);
    }

    private static ElementAttributeModel CreateAttribute(
        IAttributeOwnerModel owner,
        FakeWorkingTreeModel tree,
        FakeNotificationService notificationService)
    {
        var uuid = Guid.NewGuid();
        return new ElementAttributeModel(
            uuid,
            owner,
            uuid,
            owner,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<ElementAttributeModel>());
    }

    private static TreeNodeVM CreateNodeVM(
        TreeNodeModel node,
        FakeWorkingTreeModel tree,
        FakeNotificationService notificationService)
    {
        return new TreeNodeVM(
            node,
            CreateDataStoragesCollectionVM(tree.DataStorage),
            DispatchProxy.Create<IPhiladelphusRepositoryService, DefaultDispatchProxy>(),
            DispatchProxy.Create<IFileDialogService, DefaultDispatchProxy>(),
            notificationService);
    }

    private static DataStoragesCollectionVM CreateDataStoragesCollectionVM(
        Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages.IDataStorageModel dataStorage)
    {
        var storageVM = (DataStorageVM)RuntimeHelpers.GetUninitializedObject(typeof(DataStorageVM));
        SetPrivateField(storageVM, "_model", dataStorage);

        var result = (DataStoragesCollectionVM)RuntimeHelpers.GetUninitializedObject(typeof(DataStoragesCollectionVM));
        SetPrivateField(
            result,
            "_dataStoragesVMs",
            new ObservableCollection<DataStorageVM> { storageVM });
        return result;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(target, value);
    }

    public class DefaultDispatchProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            var returnType = targetMethod?.ReturnType;
            return returnType?.IsValueType == true
                ? Activator.CreateInstance(returnType)
                : null;
        }
    }
}
