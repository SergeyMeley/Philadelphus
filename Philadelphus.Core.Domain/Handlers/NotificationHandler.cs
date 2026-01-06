using Philadelphus.Core.Domain.Entities.OtherEntities;

namespace Philadelphus.Core.Domain.Handlers
{
    /// <summary>
    /// Обработчик уведомления
    /// </summary>
    /// <param name="notification"></param>
    /// <returns></returns>
    public delegate bool NotificationHandler(NotificationModel notification);
}
