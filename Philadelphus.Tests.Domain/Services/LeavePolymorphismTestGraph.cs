using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Tests.Common.Fakes.Entities;
using Philadelphus.Tests.Common.Fakes.Services;

namespace Philadelphus.Tests.Domain.Services;

/// <summary>
/// Содержит модели тестового дерева для сценариев полиморфных связей.
/// </summary>
internal sealed record LeavePolymorphismTestGraph(
    WorkingTreeModel Tree,
    FakeNotificationService Notifications,
    TreeNodeModel GrandParentNode,
    TreeNodeModel ParentNode,
    TreeNodeModel ChildNode,
    Guid DeclarationUuid,
    TreeLeaveModel FirstParent,
    TreeLeaveModel SecondParent,
    TreeLeaveModel FirstChild,
    TreeLeaveModel SecondChild,
    TreeLeaveModel FirstValue,
    TreeLeaveModel SecondValue);
