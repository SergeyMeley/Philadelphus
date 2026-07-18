using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Presentation.Services.Interfaces;

namespace Philadelphus.Tests.Presentation.Fakes.LeavePolymorphism;

/// <summary>
/// Возвращает заданный ответ на запрос подтверждения каскда.
/// </summary>
internal sealed class FakeLeavePolymorphismConfirmationService
    : ILeavePolymorphismConfirmationService
{
    private readonly bool _propagationConfirmed;

    /// <summary>
    /// Инициализирует fake-сервис ответом пользователя.
    /// </summary>
    internal FakeLeavePolymorphismConfirmationService(bool propagationConfirmed) =>
        _propagationConfirmed = propagationConfirmed;

    /// <summary>Количество запросов подтверждения каскада.</summary>
    internal int PropagationCallCount { get; private set; }

    /// <inheritdoc />
    public Task<bool> ConfirmManualFillAsync(
        TreeLeaveModel recipientLeave,
        int changedAttributeCount) => Task.FromResult(true);

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
