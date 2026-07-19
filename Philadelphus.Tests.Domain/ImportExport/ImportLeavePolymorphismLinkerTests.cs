using FluentAssertions;
using Moq;

using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.ImportExport.Mapping;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.ImportExport;

/// <summary>
/// Проверяет заполнение полиморфных родителей по временным корреляциям строк импорта.
/// </summary>
public class ImportLeavePolymorphismLinkerTests
{
    /// <summary>
    /// Вложенные связи должны заполняться сверху вниз, даже если корреляции получены в другом порядке.
    /// </summary>
    [Fact]
    public void Fill_NestedLinks_FillsParentsBeforeDescendants()
    {
        var (parentLeave, childLeave, grandChildLeave) = CreateLeaves();
        var parentCorrelationId = Guid.NewGuid();
        var childCorrelationId = Guid.NewGuid();
        var fills = new List<(TreeLeaveModel Child, TreeLeaveModel Parent)>();
        var progress = new List<(int Current, int Total)>();
        var service = CreateService(fills);
        var linker = new ImportLeavePolymorphismLinker(service.Object);

        linker.Fill(
            new Dictionary<Guid, TreeLeaveModel>
            {
                [parentCorrelationId] = parentLeave,
                [childCorrelationId] = childLeave
            },
            new Dictionary<TreeLeaveModel, Guid>
            {
                [grandChildLeave] = childCorrelationId,
                [childLeave] = parentCorrelationId
            },
            (current, total) => progress.Add((current, total)));

        fills.Should().Equal(
            (childLeave, parentLeave),
            (grandChildLeave, childLeave));
        progress.Should().Equal((0, 2), (1, 2), (2, 2));
        service.Verify(x => x.ResolveParent(childLeave), Times.Once);
        service.Verify(x => x.ResolveParent(grandChildLeave), Times.Once);
    }

    /// <summary>
    /// Пустой набор FK-связей должен завершать этап импорта с допустимым диапазоном прогресса.
    /// </summary>
    [Fact]
    public void Fill_EmptyLinks_ReportsCompletedProgress()
    {
        var service = new Mock<ILeavePolymorphismService>();
        var progress = new List<(int Current, int Total)>();
        var linker = new ImportLeavePolymorphismLinker(service.Object);

        linker.Fill(
            new Dictionary<Guid, TreeLeaveModel>(),
            new Dictionary<TreeLeaveModel, Guid>(),
            (current, total) => progress.Add((current, total)));

        progress.Should().Equal((1, 1));
        service.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Потерянная родительская корреляция должна завершать импорт понятной ошибкой до изменения листа.
    /// </summary>
    [Fact]
    public void Fill_MissingParentCorrelation_ThrowsBeforeFill()
    {
        var (_, childLeave, _) = CreateLeaves();
        var missingCorrelationId = Guid.NewGuid();
        var service = new Mock<ILeavePolymorphismService>();
        var linker = new ImportLeavePolymorphismLinker(service.Object);

        var action = () => linker.Fill(
            new Dictionary<Guid, TreeLeaveModel>(),
            new Dictionary<TreeLeaveModel, Guid> { [childLeave] = missingCorrelationId },
            (_, _) => { });

        action.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{childLeave.Name}*{missingCorrelationId}*");
        service.VerifyNoOtherCalls();
    }

    /// <summary>
    /// Создаёт mock сервиса и сохраняет фактический порядок заполнения листов.
    /// </summary>
    private static Mock<ILeavePolymorphismService> CreateService(
        ICollection<(TreeLeaveModel Child, TreeLeaveModel Parent)> fills)
    {
        var service = new Mock<ILeavePolymorphismService>();
        service
            .Setup(x => x.FillFromParent(
                It.IsAny<IAttributeOwnerModel>(),
                It.IsAny<TreeLeaveModel>()))
            .Callback<IAttributeOwnerModel, TreeLeaveModel>((child, parent) =>
                fills.Add(((TreeLeaveModel)child, parent)))
            .Returns(new LeaveAttributeFillResult([]));
        service
            .Setup(x => x.ResolveParent(It.IsAny<TreeLeaveModel>()))
            .Returns((TreeLeaveModel child) => new LeavePolymorphismResolution(
                child,
                LeavePolymorphismStatus.NotFound,
                null,
                []));
        return service;
    }

    /// <summary>
    /// Создаёт три листа на последовательных уровнях дерева.
    /// </summary>
    private static (TreeLeaveModel Parent, TreeLeaveModel Child, TreeLeaveModel GrandChild) CreateLeaves()
    {
        var notifications = new FakeNotificationService();
        var tree = new FakeWorkingTreeModel();
        var root = new TreeRootModel(Guid.NewGuid(), tree, notifications,
            new EmptyPropertiesPolicy<TreeRootModel>());
        var parentNode = CreateNode(root, tree, notifications);
        var childNode = CreateNode(parentNode, tree, notifications);
        var grandChildNode = CreateNode(childNode, tree, notifications);

        return (
            CreateLeave(parentNode, tree, notifications, "Родитель"),
            CreateLeave(childNode, tree, notifications, "Наследник"),
            CreateLeave(grandChildNode, tree, notifications, "Внук"));
    }

    /// <summary>
    /// Создаёт узел тестового дерева.
    /// </summary>
    private static TreeNodeModel CreateNode(
        IParentModel parent,
        WorkingTreeModel tree,
        FakeNotificationService notifications) =>
        new(Guid.NewGuid(), parent, tree, notifications,
            new EmptyPropertiesPolicy<TreeNodeModel>());

    /// <summary>
    /// Создаёт именованный лист тестового дерева.
    /// </summary>
    private static TreeLeaveModel CreateLeave(
        TreeNodeModel parent,
        WorkingTreeModel tree,
        FakeNotificationService notifications,
        string name) =>
        new TreeLeaveModel(Guid.NewGuid(), parent, tree, notifications,
            new EmptyPropertiesPolicy<TreeLeaveModel>())
        {
            Name = name
        };
}
