using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.LeavePolymorphism.Services;

/// <summary>
/// Вычисляет полиморфного родителя по значениям эффективных атрибутов
/// прямого родительского узла.
/// </summary>
public sealed partial class LeavePolymorphismService : ILeavePolymorphismService
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

        var matchResult = FindParentCandidates(childLeave, parentNode);
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
        childLeave.Attributes
            .OfType<LeavePolymorphismAttributeModel>()
            .SingleOrDefault()
            ?.SetResolution(status, candidates);
        return new(childLeave, status, parentLeave, candidates);
    }

    /// <summary>
    /// Проверяет как сохранённый признак удаления, так и несохранённые состояния модели.
    /// </summary>
    private static bool IsDeleted(TreeLeaveModel leave) =>
        leave.AuditInfo.IsDeleted
        || leave.State is State.ForSoftDelete or State.ForHardDelete or State.SoftDeleted;

    /// <summary>
    /// Ищет активные листья заданного родительского узла по его полному набору атрибутов.
    /// </summary>
    private LeaveAttributeMatchResult FindParentCandidates(
        TreeLeaveModel sourceLeave,
        TreeNodeModel parentNode)
    {
        var declaringUuids = parentNode.Attributes
            .Where(x => IsPolymorphismAttribute(x) == false)
            .Select(x => x.DeclaringUuid)
            .ToHashSet();
        var expectedAttributes = GetExpectedParentAttributes(sourceLeave, declaringUuids);

        if (expectedAttributes.Count != declaringUuids.Count)
            return new(false, []);

        return _attributeValueService.FindMatches(
            expectedAttributes,
            parentNode.ChildLeaves.Where(x => IsDeleted(x) == false));
    }

    /// <summary>
    /// Выбирает из листа материализованные атрибуты указанного родительского уровня.
    /// </summary>
    private static IReadOnlyList<ElementAttributeModel> GetExpectedParentAttributes(
        TreeLeaveModel sourceLeave,
        IReadOnlySet<Guid> declaringUuids) =>
        sourceLeave.Attributes
            .Where(x => declaringUuids.Contains(x.DeclaringUuid))
            .ToList();

    /// <summary>
    /// Проверяет принадлежность атрибута к вычисляемой полиморфной связи.
    /// </summary>
    private static bool IsPolymorphismAttribute(ElementAttributeModel attribute) =>
        attribute is LeavePolymorphismAttributeModel;
}
