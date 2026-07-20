using Philadelphus.Core.Domain.Contracts.LeaveAttributeValues;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;

namespace Philadelphus.Core.Domain.LeaveAttributeValues.Services;

public sealed partial class LeaveAttributeValueService
{
    /// <summary>
    /// Находит активные системные листья с типизированным значением переданной строки.
    /// </summary>
    public LeaveAttributeMatchResult FindSystemValue(
        SystemBaseTreeNodeModel valueType,
        string? expectedValue)
    {
        ArgumentNullException.ThrowIfNull(valueType);

        if (SystemBaseStringValueValidator.TryParse(
                valueType.SystemBaseType,
                expectedValue,
                out var expectedTypedValue,
                out _) == false)
        {
            return new(LeaveAttributeMatchStatus.Invalid, []);
        }

        var matches = new List<TreeLeaveModel>();
        foreach (var candidate in valueType.ChildLeaves.Where(IsActive))
        {
            if (candidate is not SystemBaseTreeLeaveModel systemCandidate
                || systemCandidate.SystemBaseType != valueType.SystemBaseType
                || SystemBaseStringValueValidator.TryParse(
                    systemCandidate.SystemBaseType,
                    systemCandidate.StringValue,
                    out var candidateTypedValue,
                    out _) == false)
            {
                return new(LeaveAttributeMatchStatus.Invalid, []);
            }

            if (Equals(expectedTypedValue, candidateTypedValue))
                matches.Add(systemCandidate);
        }

        var status = matches.Count switch
        {
            0 => LeaveAttributeMatchStatus.NotFound,
            1 => LeaveAttributeMatchStatus.Resolved,
            _ => LeaveAttributeMatchStatus.Ambiguous
        };
        return new(status, matches);
    }

    /// <summary>
    /// Явно создаёт системный лист для валидного значения, отсутствующего в узле.
    /// </summary>
    public SystemBaseTreeLeaveModel CreateSystemValue(
        SystemBaseTreeNodeModel valueType,
        string value)
    {
        ArgumentNullException.ThrowIfNull(valueType);
        ArgumentNullException.ThrowIfNull(value);

        if (valueType.SystemBaseType == SystemBaseType.BOOL)
            throw new InvalidOperationException("Создание системных значений BOOL запрещено.");

        var matchResult = FindSystemValue(valueType, value);
        if (matchResult.Status == LeaveAttributeMatchStatus.Invalid)
        {
            throw new InvalidOperationException(
                $"Значение '{value}' не соответствует системному типу "
                + $"'{valueType.SystemBaseType}'. Ожидаемый формат: "
                + SystemBaseStringValueValidator.GetExpectedFormat(valueType.SystemBaseType) + ".");
        }

        if (matchResult.Status != LeaveAttributeMatchStatus.NotFound)
        {
            throw new InvalidOperationException(
                $"Системное значение '{value}' уже представлено "
                + $"{matchResult.Matches.Count} активными листьями.");
        }

        var createdLeave = _repositoryService.CreateTreeLeave(
            valueType,
            needAutoName: true,
            withoutInfoNotifications: true) as SystemBaseTreeLeaveModel
            ?? throw new InvalidOperationException(
                $"Не удалось создать системное значение в узле "
                + $"'{valueType.Name}' [{valueType.Uuid}].");
        createdLeave.StringValue = value;
        return createdLeave;
    }

    private static bool IsActive(TreeLeaveModel leave) =>
        leave.AuditInfo.IsDeleted == false
        && leave.State is not (State.ForSoftDelete or State.ForHardDelete or State.SoftDeleted);
}
