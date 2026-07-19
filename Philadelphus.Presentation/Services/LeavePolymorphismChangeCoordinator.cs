using System.Threading;

using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Models.LeavePolymorphism;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Presentation.Services;

/// <summary>
/// Связывает доменный план полиморфного обновления с интерактивным
/// подтверждением в presentation-слое.
/// </summary>
public sealed class LeavePolymorphismChangeCoordinator
    : ILeavePolymorphismChangeCoordinator
{
    private readonly ILeavePolymorphismService _leavePolymorphismService;
    private readonly ILeavePolymorphismConfirmationService _confirmationService;
    private int _isProcessing;

    /// <summary>
    /// Инициализирует координатор доменным сервисом и сервисом диалогов.
    /// </summary>
    /// <param name="leavePolymorphismService">Сервис полиморфных связей.</param>
    /// <param name="confirmationService">Сервис подтверждения каскадного обновления.</param>
    public LeavePolymorphismChangeCoordinator(
        ILeavePolymorphismService leavePolymorphismService,
        ILeavePolymorphismConfirmationService confirmationService)
    {
        ArgumentNullException.ThrowIfNull(leavePolymorphismService);
        ArgumentNullException.ThrowIfNull(confirmationService);

        _leavePolymorphismService = leavePolymorphismService;
        _confirmationService = confirmationService;
    }

    /// <inheritdoc />
    public async Task<LeavePolymorphismManualFillResult> FillFromParentAsync(
        TreeLeaveModel recipientLeave,
        TreeLeaveModel parentLeave)
    {
        ArgumentNullException.ThrowIfNull(recipientLeave);
        ArgumentNullException.ThrowIfNull(parentLeave);

        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            return new(false, []);

        try
        {
            var changedAttributeCount = _leavePolymorphismService
                .CountFillFromParentChanges(recipientLeave, parentLeave);
            if (changedAttributeCount > 0
                && await _confirmationService.ConfirmManualFillAsync(
                    recipientLeave,
                    changedAttributeCount) == false)
            {
                return new(false, []);
            }

            var fillResult = _leavePolymorphismService.FillFromParent(
                recipientLeave,
                parentLeave);
            _leavePolymorphismService.ResolveParent(recipientLeave);
            return new(true, fillResult.ChangedAttributes);
        }
        finally
        {
            Volatile.Write(ref _isProcessing, 0);
        }
    }

    /// <inheritdoc />
    public LeavePolymorphismChangeResult CreateParentChain(TreeLeaveModel childLeave)
    {
        ArgumentNullException.ThrowIfNull(childLeave);

        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            return new(false, []);

        try
        {
            var createdLeaves = _leavePolymorphismService.CreateParentChain(childLeave);
            _leavePolymorphismService.RefreshLinks(createdLeaves.Prepend(childLeave));
            return new(false, createdLeaves);
        }
        finally
        {
            Volatile.Write(ref _isProcessing, 0);
        }
    }

    /// <inheritdoc />
    public async Task<LeavePolymorphismChangeResult> HandleChangedLeaveAsync(
        TreeLeaveModel changedLeave)
    {
        ArgumentNullException.ThrowIfNull(changedLeave);

        // Модальный диалог оставляет async-операцию незавершённой. Защита не даёт
        // программным уведомлениям открыть второй каскад до ответа пользователя.
        if (Interlocked.CompareExchange(ref _isProcessing, 1, 0) != 0)
            return new(false, []);

        try
        {
            var plan = _leavePolymorphismService.BuildPropagationPlan(changedLeave);
            IReadOnlyList<TreeLeaveModel> createdLeaves = [];
            if (plan.AffectedLeaveCount > 0)
            {
                var confirmed = await _confirmationService.ConfirmPropagationAsync(
                    changedLeave,
                    plan.AffectedLeaveCount,
                    plan.ChangedAttributeCount);

                if (confirmed)
                    _leavePolymorphismService.ApplyPropagation(plan);
                else
                    createdLeaves = _leavePolymorphismService
                        .PreserveChildrenAndResolveReplacement(plan)
                        .CreatedLeaves;
            }

            // Изменённый лист сам может быть наследником другого уровня.
            _leavePolymorphismService.ResolveParent(changedLeave);
            return new(plan.AffectedLeaveCount > 0, createdLeaves);
        }
        finally
        {
            Volatile.Write(ref _isProcessing, 0);
        }
    }
}
