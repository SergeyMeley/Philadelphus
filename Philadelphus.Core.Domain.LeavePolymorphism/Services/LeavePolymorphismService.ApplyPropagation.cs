using System.Threading;

using Philadelphus.Core.Domain.Contracts.LeavePolymorphism;

namespace Philadelphus.Core.Domain.LeavePolymorphism.Services;

public sealed partial class LeavePolymorphismService
{
    /// <summary>
    /// Потокобезопасный признак выполняющегося каскадного обновления.
    /// </summary>
    private int _propagationInProgress;

    /// <inheritdoc />
    public bool IsPropagationInProgress =>
        Volatile.Read(ref _propagationInProgress) != 0;

    /// <inheritdoc />
    public void ApplyPropagation(LeavePolymorphismPropagationPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);

        BeginPropagation();

        try
        {
            ValidatePropagationPlan(plan);

            foreach (var item in plan.Items)
            {
                _attributeValueService.FillFromLeave(
                    item.TargetLeave,
                    item.SourceLeave,
                    item.DeclaringUuids);
            }

            // Связи пересчитываются после всех переносов значений, чтобы промежуточное
            // состояние одного уровня не изменило последовательность текущего плана.
            RefreshLinks(plan.Items.Select(x => x.TargetLeave));
        }
        finally
        {
            EndPropagation();
        }
    }

    /// <summary>
    /// Атомарно начинает операцию над снимком полиморфных связей.
    /// </summary>
    private void BeginPropagation()
    {
        if (Interlocked.CompareExchange(ref _propagationInProgress, 1, 0) != 0)
        {
            throw new InvalidOperationException(
                "Операция над полиморфными наследниками уже выполняется.");
        }
    }

    /// <summary>
    /// Снимает защиту после завершения или прерывания операции.
    /// </summary>
    private void EndPropagation() =>
        Volatile.Write(ref _propagationInProgress, 0);

    /// <summary>
    /// Проверяет, что сохранённая runtime-топология не изменилась после расчёта плана.
    /// </summary>
    private static void ValidatePropagationPlan(
        LeavePolymorphismPropagationPlan plan)
    {
        var targetUuids = new HashSet<Guid>();
        foreach (var item in plan.Items)
        {
            if (ReferenceEquals(item.TargetLeave.PolymorphicParentLeave, item.SourceLeave) == false)
            {
                throw new InvalidOperationException(
                    $"Связь листа '{item.TargetLeave.Name}' [{item.TargetLeave.Uuid}] "
                    + "изменилась после расчёта каскадного обновления.");
            }

            if (targetUuids.Add(item.TargetLeave.Uuid) == false)
            {
                throw new InvalidOperationException(
                    $"Лист '{item.TargetLeave.Name}' [{item.TargetLeave.Uuid}] "
                    + "повторяется в плане каскадного обновления.");
            }
        }
    }
}
