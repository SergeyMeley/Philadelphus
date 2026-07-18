using FluentAssertions;
using Philadelphus.Presentation.Models.LeavePolymorphism;

namespace Philadelphus.Tests.Presentation.Services;

/// <summary>
/// Проверяет правила хранения автоподтверждений в рамках одной сессии приложения.
/// </summary>
public sealed class LeavePolymorphismConfirmationSessionStateTests
{
    [Fact]
    public void ManualFillDecision_IsRememberedOnlyWithConfirmation()
    {
        var state = new LeavePolymorphismConfirmationSessionState();

        state.RememberManualFillDecision(confirmed: false, rememberForSession: true);
        state.IsManualFillAutoConfirmed.Should().BeFalse();

        state.RememberManualFillDecision(confirmed: true, rememberForSession: true);
        state.IsManualFillAutoConfirmed.Should().BeTrue();
    }

    [Fact]
    public void PropagationDecision_IsRememberedOnlyWithConfirmation()
    {
        var state = new LeavePolymorphismConfirmationSessionState();

        state.RememberPropagationDecision(confirmed: true, rememberForSession: false);
        state.IsPropagationAutoConfirmed.Should().BeFalse();

        state.RememberPropagationDecision(confirmed: true, rememberForSession: true);
        state.IsPropagationAutoConfirmed.Should().BeTrue();
    }

    [Fact]
    public void ConfirmationFlags_AreIndependentAndNewSessionStartsEmpty()
    {
        var state = new LeavePolymorphismConfirmationSessionState();
        state.RememberManualFillDecision(confirmed: true, rememberForSession: true);

        state.IsPropagationAutoConfirmed.Should().BeFalse();
        new LeavePolymorphismConfirmationSessionState()
            .IsManualFillAutoConfirmed.Should().BeFalse();
    }
}
