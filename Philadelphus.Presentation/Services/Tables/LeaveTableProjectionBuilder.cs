using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Presentation.Models.Tables;

namespace Philadelphus.Presentation.Services.Tables;

/// <summary>
/// Строит readonly-проекцию листьев для динамических таблиц.
/// </summary>
public static class LeaveTableProjectionBuilder
{
    /// <summary>
    /// Формирует системные, атрибутные и audit-колонки листьев.
    /// </summary>
    public static IReadOnlyList<ChildCollectionTableColumn> buildLeaveTableColumns(
        IEnumerable<TreeLeaveModel>? leaves,
        int startOrder = 0)
    {
        var leavesList = leaves?.ToList() ?? [];
        var result = new List<ChildCollectionTableColumn>
        {
            ChildCollectionTableBuilder.CreateColumn(
                nameof(ISequencableModel.Sequence),
                startOrder,
                child => child is TreeLeaveModel leave ? leave.Sequence : null,
                typeof(ISequencableModel),
                nameof(ISequencableModel.Sequence)),
            ChildCollectionTableBuilder.CreateColumn(
                nameof(ILinkableByUuidModel.Uuid),
                startOrder + 1,
                child => child.Uuid,
                typeof(ILinkableByUuidModel),
                nameof(ILinkableByUuidModel.Uuid)),
            ChildCollectionTableBuilder.CreateColumn(
                nameof(IMainEntityModel.Name),
                startOrder + 2,
                child => child is IMainEntityModel entity ? entity.Name : null,
                typeof(IMainEntityModel),
                nameof(IMainEntityModel.Name)),
            ChildCollectionTableBuilder.CreateColumn(
                nameof(IMainEntityModel.Description),
                startOrder + 3,
                child => child is IMainEntityModel entity ? entity.Description : null,
                typeof(IMainEntityModel),
                nameof(IMainEntityModel.Description)),
        };

        var attributes = GetAttributes(leavesList).ToList();
        var reservedBindingKeys = new HashSet<string>(
            result.Select(x => x.Key).Concat(attributes.SelectMany(x => new[]
            {
                x.DeclaringUuid.ToString(),
                string.IsNullOrWhiteSpace(x.Name) ? x.DeclaringUuid.ToString() : x.Name,
            })),
            StringComparer.Ordinal);
        var order = startOrder + result.Count;

        foreach (var attribute in attributes)
        {
            result.Add(ChildCollectionTableBuilder.CreateAttributeColumn(
                attribute,
                order++,
                reservedBindingKeys,
                isReadOnly: true));
        }

        var auditPrefix = $"{nameof(IMainEntityModel.AuditInfo)}.";
        var auditColumns = ChildCollectionTableBuilder
            .buildChildCollectionTableColumns(null, leavesList)
            .Where(x => x.Key.StartsWith(auditPrefix, StringComparison.Ordinal));

        result.AddRange(auditColumns.Select(x => CopyReadOnly(x, order++)));
        return result;
    }

    /// <summary>
    /// Формирует readonly-строки и связывает их с исходными листьями.
    /// </summary>
    public static IReadOnlyList<ChildCollectionTableRow> buildLeaveTableRows(
        IEnumerable<TreeLeaveModel>? leaves,
        IEnumerable<ChildCollectionTableColumn>? columns)
        => ChildCollectionTableBuilder.buildChildCollectionTableRows(leaves, columns);

    private static IEnumerable<ElementAttributeModel> GetAttributes(
        IEnumerable<TreeLeaveModel> leaves)
        => leaves
            .SelectMany(x => x.Attributes ?? [])
            .Where(x => x.DeclaringUuid != Guid.Empty)
            .GroupBy(x => x.DeclaringUuid)
            .Select(x => x.First())
            .OrderBy(x => x.DeclaringOwner is ISequencableModel owner ? owner.Sequence : long.MaxValue)
            .ThenBy(x => x.InheritanceDepth)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.DeclaringUuid);

    private static ChildCollectionTableColumn CopyReadOnly(
        ChildCollectionTableColumn source,
        int order)
        => new(
            source.Key,
            source.Header,
            order,
            source.GetValue,
            bindingKey: source.BindingKey,
            headerToolTip: source.HeaderToolTip);
}
