using System.Collections.ObjectModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Factories.Implementations;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;

public class MainEntityBaseVMTests
{
    [Fact]
    public void AttributeValuesCollectionVM_RefreshesAssignedValuesInAttributeVM()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var valueType = new TreeNodeModel(Guid.NewGuid(), root, tree, notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var replacementType = new TreeNodeModel(Guid.NewGuid(), root, tree, notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var value = new TreeLeaveModel(Guid.NewGuid(), valueType, tree, notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>())
        {
            Name = "Значение",
        };
        var attribute = CreateAttribute(root, tree, notificationService);
        attribute.ValueType = valueType;
        attribute.IsCollectionValue = true;
        var rootVM = new TreeRootVM(
            root,
            CreateDataStoragesCollectionVM(tree.DataStorage),
            DispatchProxy.Create<IPhiladelphusRepositoryService, DefaultDispatchProxy>(),
            DispatchProxy.Create<IFileDialogService, DefaultDispatchProxy>(),
            notificationService);
        var attributeVM = rootVM.AttributesVMs.Single(x => x.Model == attribute);
        attributeVM.SelectedValueType = replacementType;
        attributeVM.SelectedValueType = valueType;
        var changedProperties = new List<string?>();
        attributeVM.PropertyChanged += (_, args) => changedProperties.Add(args.PropertyName);
        var sut = new AttributeValuesCollectionVM(attributeVM);

        sut.Rows.Single(x => x.SourceUuid == value.Uuid)["IsSelected"] = true;

        attributeVM.AssignedValuesString.Should().Be("Значение");
        attributeVM.AssignedValues.Should().Equal(value);
        changedProperties.Should().Contain(nameof(attributeVM.AssignedValuesString));
    }

    [Fact]
    public void AttributeValuesCollectionVM_RemainsBoundToOpenedAttributeAfterSelectionChanges()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var valueType = new TreeNodeModel(Guid.NewGuid(), root, tree, notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var value = new TreeLeaveModel(Guid.NewGuid(), valueType, tree, notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var openedAttribute = CreateAttribute(root, tree, notificationService);
        openedAttribute.ValueType = valueType;
        openedAttribute.IsCollectionValue = true;
        var selectedAttribute = CreateAttribute(root, tree, notificationService);
        selectedAttribute.ValueType = valueType;
        selectedAttribute.IsCollectionValue = true;
        var rootVM = new TreeRootVM(
            root,
            CreateDataStoragesCollectionVM(tree.DataStorage),
            DispatchProxy.Create<IPhiladelphusRepositoryService, DefaultDispatchProxy>(),
            DispatchProxy.Create<IFileDialogService, DefaultDispatchProxy>(),
            notificationService);
        var openedAttributeVM = rootVM.AttributesVMs.Single(x => x.Model == openedAttribute);
        var sut = new AttributeValuesCollectionVM(openedAttributeVM);

        rootVM.SelectedAttributeVM = rootVM.AttributesVMs.Single(x => x.Model == selectedAttribute);
        sut.Rows.Single(x => x.SourceUuid == value.Uuid)["IsSelected"] = true;

        sut.Attribute.Should().BeSameAs(openedAttribute);
        openedAttribute.Values.Should().Equal(value);
        selectedAttribute.Values.Should().BeEmpty();
    }

    [Fact]
    public void AttributeValuesCollectionVM_TracksTypeAndStopsAfterDispose()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var initialType = new TreeNodeModel(Guid.NewGuid(), root, tree, notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        _ = new TreeLeaveModel(Guid.NewGuid(), initialType, tree, notificationService,
            new EmptyPropertiesPolicy<TreeLeaveModel>());
        var replacementType = new TreeNodeModel(Guid.NewGuid(), root, tree, notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var replacementValue = new TreeLeaveModel(Guid.NewGuid(), replacementType, tree,
            notificationService, new EmptyPropertiesPolicy<TreeLeaveModel>());
        var attribute = CreateAttribute(root, tree, notificationService);
        attribute.ValueType = initialType;
        attribute.IsCollectionValue = true;
        var rootVM = new TreeRootVM(
            root,
            CreateDataStoragesCollectionVM(tree.DataStorage),
            DispatchProxy.Create<IPhiladelphusRepositoryService, DefaultDispatchProxy>(),
            DispatchProxy.Create<IFileDialogService, DefaultDispatchProxy>(),
            notificationService);
        var attributeVM = rootVM.AttributesVMs.Single(x => x.Model == attribute);
        var factory = new AttributeValuesCollectionVMFactory(
            DispatchProxy.Create<ILeaveAttributeValueService, DefaultDispatchProxy>(),
            new DefaultRelayCommandFactory(),
            DispatchProxy.Create<IAttributeValueCreationConfirmationService, DefaultDispatchProxy>());
        var sut = factory.Create(attributeVM);
        var closeRequests = 0;
        sut.CloseRequested += (_, _) => closeRequests++;

        attributeVM.SelectedValueType = replacementType;

        sut.Values.Select(x => x.Value).Should().Equal(replacementValue);
        replacementValue.AuditInfo.IsDeleted = true;
        factory.RefreshOpenEditors();
        sut.Values.Should().BeEmpty();

        factory.CloseOpenEditors();
        closeRequests.Should().Be(1);

        sut.Dispose();
        attributeVM.SelectedValueType = initialType;
        attributeVM.IsCollectionValue = false;

        sut.Values.Should().BeEmpty();
        closeRequests.Should().Be(1);
    }

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

    /// <summary>
    /// Собственный атрибут должен разрешать переключение между одиночным и коллекционным значением.
    /// </summary>
    [Fact]
    public void OwnAttributeVM_CollectionMode_CanBeEnabled()
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

        attributeVM.IsCollectionValue = true;

        attributeVM.CanChangeCollectionMode.Should().BeTrue();
        attributeVM.IsCollectionValue.Should().BeTrue();
        attribute.IsCollectionValue.Should().BeTrue();
    }

    /// <summary>
    /// Вид значения унаследованного атрибута изменяется только через его объявление.
    /// </summary>
    [Fact]
    public void InheritedAttributeVM_CollectionMode_IsUnavailable()
    {
        var notificationService = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(
            Guid.NewGuid(),
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeRootModel>());
        _ = CreateAttribute(root, tree, notificationService);
        var child = new TreeNodeModel(
            Guid.NewGuid(),
            root,
            tree,
            notificationService,
            new EmptyPropertiesPolicy<TreeNodeModel>());
        var childVM = CreateNodeVM(child, tree, notificationService);
        var attributeVM = childVM.AttributesVMs.Single();

        attributeVM.CanChangeCollectionMode.Should().BeFalse();
        attributeVM.CollectionModeToolTip.Should().Contain("где он объявлен");
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
