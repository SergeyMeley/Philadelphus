using Philadelphus.Core.Domain.Infrastructure.Messaging.Messages;

namespace Philadelphus.Core.Domain.Handlers
{
    /// <summary>
    /// Обработчик уведомления
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    public delegate bool NotificationHandler(Notification notification);
}
