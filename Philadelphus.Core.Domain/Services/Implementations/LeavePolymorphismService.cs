using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Services.Implementations;

/// <summary>
/// Вычисляет полиморфного родителя по значениям эффективных атрибутов
/// прямого родительского узла.
/// </summary>
public sealed class LeavePolymorphismService : ILeavePolymorphismService
{
    private readonly ILeaveAttributeValueService _attributeValueService;

    /// <summary>
    /// Инициализирует сервис поиска полиморфных родителей.
    /// </summary>
    /// <param name="attributeValueService">Переиспользуемый сервис сравнения значений.</param>
    public LeavePolymorphismService(
        ILeaveAttributeValueService attributeValueService)
    {
        ArgumentNullException.ThrowIfNull(attributeValueService);

        _attributeValueService = attributeValueService;
    }

    /// <inheritdoc />
    public LeavePolymorphismResolution ResolveParent(TreeLeaveModel childLeave)
    {
        ArgumentNullException.ThrowIfNull(childLeave);

        var parentNode = childLeave.ParentNode.ParentNode;
        if (parentNode == null || IsDeleted(childLeave))
            return Complete(childLeave, LeavePolymorphismStatus.Invalid, []);

        var declaringUuids = parentNode.Attributes
            .Select(x => x.DeclaringUuid)
            .ToHashSet();
        var expectedAttributes = childLeave.Attributes
            .Where(x => declaringUuids.Contains(x.DeclaringUuid));
        var candidates = parentNode.ChildLeaves
            .Where(x => IsDeleted(x) == false);

        var matchResult = _attributeValueService.FindMatches(expectedAttributes, candidates);
        if (matchResult.IsValid == false)
            return Complete(childLeave, LeavePolymorphismStatus.Invalid, []);

        var status = matchResult.Matches.Count switch
        {
            0 => LeavePolymorphismStatus.NotFound,
            1 => LeavePolymorphismStatus.Resolved,
            _ => LeavePolymorphismStatus.Ambiguous
        };
        return Complete(childLeave, status, matchResult.Matches);
    }

    /// <inheritdoc />
    public IReadOnlyList<LeavePolymorphismResolution> RefreshLinks(
        IEnumerable<TreeLeaveModel> leaves)
    {
        ArgumentNullException.ThrowIfNull(leaves);

        return leaves.Select(ResolveParent).ToList();
    }

    /// <summary>
    /// Применяет итог поиска к runtime-связи и формирует неизменяемый результат.
    /// </summary>
    /// <param name="childLeave">Разрешаемый дочерний лист.</param>
    /// <param name="status">Рассчитанный статус.</param>
    /// <param name="candidates">Совпавшие кандидаты.</param>
    /// <returns>Снимок применённого результата.</returns>
    private static LeavePolymorphismResolution Complete(
        TreeLeaveModel childLeave,
        LeavePolymorphismStatus status,
        IReadOnlyList<TreeLeaveModel> candidates)
    {
        var parentLeave = status == LeavePolymorphismStatus.Resolved
            ? candidates.Single()
            : null;

        // SetPolymorphicParentLeave атомарно удаляет старую обратную связь
        // и добавляет новую только для однозначно разрешённого результата.
        childLeave.SetPolymorphicParentLeave(parentLeave);
        return new(childLeave, status, parentLeave, candidates);
    }

    /// <summary>
    /// Проверяет как сохранённый признак удаления, так и несохранённые состояния модели.
    /// </summary>
    private static bool IsDeleted(TreeLeaveModel leave) =>
        leave.AuditInfo.IsDeleted
        || leave.State is State.ForSoftDelete or State.ForHardDelete or State.SoftDeleted;
}
