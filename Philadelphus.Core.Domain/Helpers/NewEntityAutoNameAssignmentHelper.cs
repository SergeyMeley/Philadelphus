using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Helpers;

/// <summary>
/// Повторно запускает автоматическое именование только для новых пользовательских элементов.
/// </summary>
internal static class NewEntityAutoNameAssignmentHelper
{
    /// <summary>
    /// Подбирает непустое имя и завершает работу после достижения защитного лимита.
    /// </summary>
    /// <typeparam name="T">Тип создаваемой доменной сущности.</typeparam>
    /// <param name="model">Новая сущность с ещё не назначенным именем.</param>
    /// <param name="notificationService">Сервис пользовательских уведомлений.</param>
    /// <returns><see langword="true" />, если имя было назначено.</returns>
    internal static bool TryAssign<T>(
        MainEntityBaseModel<T> model,
        INotificationService notificationService)
        where T : MainEntityBaseModel<T>
    {
        const int maxAttempts = 1000;

        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(notificationService);

        for (var attempt = 0; attempt < maxAttempts; attempt++)
        {
            model.Name = NamingHelper.GetNewName(model.AutoNameFixedPart);
            if (string.IsNullOrWhiteSpace(model.Name) == false)
            {
                return true;
            }
        }

        notificationService.SendTextMessage<MainEntityBaseModel<T>>(
            $"Не удалось автоматически подобрать наименование для элемента типа '{model.Type}' [{model.Uuid}] " +
            $"за {maxAttempts} попыток.",
            criticalLevel: NotificationCriticalLevelModel.Warning);
        return false;
    }
}
