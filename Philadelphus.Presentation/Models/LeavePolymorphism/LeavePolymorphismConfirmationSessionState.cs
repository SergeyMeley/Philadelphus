namespace Philadelphus.Presentation.Models.LeavePolymorphism;

/// <summary>
/// Хранит настройки автоподтверждения полиморфных операций до завершения приложения.
/// </summary>
public sealed class LeavePolymorphismConfirmationSessionState
{
    /// <summary>
    /// Признак автоподтверждения ручного заполнения по выбранному листу.
    /// </summary>
    public bool IsManualFillAutoConfirmed { get; private set; }

    /// <summary>
    /// Признак автоподтверждения каскадного обновления наследников.
    /// </summary>
    public bool IsPropagationAutoConfirmed { get; private set; }

    /// <summary>
    /// Запоминает решение по ручному заполнению только после положительного ответа.
    /// </summary>
    /// <param name="confirmed">Пользователь подтвердил операцию.</param>
    /// <param name="rememberForSession">Отмечен чек-бокс запоминания.</param>
    public void RememberManualFillDecision(bool confirmed, bool rememberForSession)
    {
        if (confirmed && rememberForSession)
            IsManualFillAutoConfirmed = true;
    }

    /// <summary>
    /// Запоминает решение по каскадному обновлению только после положительного ответа.
    /// </summary>
    /// <param name="confirmed">Пользователь подтвердил операцию.</param>
    /// <param name="rememberForSession">Отмечен чек-бокс запоминания.</param>
    public void RememberPropagationDecision(bool confirmed, bool rememberForSession)
    {
        if (confirmed && rememberForSession)
            IsPropagationAutoConfirmed = true;
    }
}
