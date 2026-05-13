using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;

namespace Philadelphus.Core.Domain.Handlers
{
    /// <summary>
    /// Обработчик уведомления
    /// </summary>
    /// <param name="notification">Уведомление.</param>
    public delegate bool NotificationHandler(Notification notification);
}
