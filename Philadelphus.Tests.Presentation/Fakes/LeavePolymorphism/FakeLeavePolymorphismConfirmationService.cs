using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Tests.Presentation.Fakes.LeavePolymorphism;

/// <summary>
/// Возвращает заданный ответ на запрос подтверждения каскада.
/// </summary>
internal sealed class FakeLeavePolymorphismConfirmationService
    : ILeavePolymorphismConfirmationService
{
    private readonly bool _propagationConfirmed;
    private readonly bool _manualFillConfirmed;

    /// <summary>
    /// Инициализирует fake-сервис ответом пользователя.
    /// </summary>
    internal FakeLeavePolymorphismConfirmationService(
        bool propagationConfirmed,
        bool manualFillConfirmed = true)
    {
        _propagationConfirmed = propagationConfirmed;
        _manualFillConfirmed = manualFillConfirmed;
    }

    /// <summary>Количество запросов подтверждения каскада.</summary>
    internal int PropagationCallCount { get; private set; }

    /// <summary>Количество запросов подтверждения ручного заполнения.</summary>
    internal int ManualFillCallCount { get; private set; }

    /// <summary>Последнее показанное число изменяемых атрибутов.</summary>
    internal int LastManualFillChangedAttributeCount { get; private set; }

    /// <inheritdoc />
    public Task<bool> ConfirmManualFillAsync(
        IAttributeOwnerModel recipient,
        int changedAttributeCount)
    {
        ManualFillCallCount++;
        LastManualFillChangedAttributeCount = changedAttributeCount;
        return Task.FromResult(_manualFillConfirmed);
    }

    /// <inheritdoc />
    public Task<bool> ConfirmPropagationAsync(
        TreeLeaveModel changedParentLeave,
        int affectedLeaveCount,
        int changedAttributeCount)
    {
        PropagationCallCount++;
        return Task.FromResult(_propagationConfirmed);
    }
}
