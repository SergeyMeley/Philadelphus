using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Запрещает изменение предопределенных системных листов логического типа.
    /// </summary>
    /// <remarks>
    /// Логический системный тип представлен фиксированным набором значений: <c>Истина</c> и <c>Ложь</c>.
    /// Эти листья используются как справочник допустимых значений, поэтому их нельзя редактировать через
    /// обычные свойства листа. Добавление и удаление таких листов дополнительно блокируется на уровне сервиса.
    /// </remarks>
    internal class SystemBaseBoolTreeLeaveReadOnlyPropertiesRule : IPropertiesRule<TreeLeaveModel>
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Создает правило запрета изменения логических системных листов.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений для диагностических сообщений.</param>
        public SystemBaseBoolTreeLeaveReadOnlyPropertiesRule(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Разрешает чтение всех свойств листа.
        /// </summary>
        /// <param name="model">Проверяемый лист.</param>
        /// <param name="prop">Имя читаемого свойства.</param>
        /// <returns>Всегда true.</returns>
        public bool CanRead(TreeLeaveModel model, string prop)
        {
            return true;
        }

        /// <summary>
        /// Запрещает запись любого свойства системного листа типа BOOL.
        /// </summary>
        /// <param name="model">Проверяемый лист.</param>
        /// <param name="prop">Имя записываемого свойства.</param>
        /// <param name="value">Новое значение свойства.</param>
        /// <returns>false для системных листов BOOL; иначе true.</returns>
        public bool CanWrite(TreeLeaveModel model, string prop, object value)
        {
            if (model is not SystemBaseTreeLeaveModel systemBaseLeave
                || systemBaseLeave.SystemBaseType != SystemBaseType.BOOL)
            {
                return true;
            }

            _notificationService.SendTextMessage<SystemBaseBoolTreeLeaveReadOnlyPropertiesRule>(
                $"Изменение системного логического значения '{model.Name}' [{model.Uuid}] запрещено. " +
                $"Для типа BOOL допустимы только предопределенные значения 'Истина' и 'Ложь'.",
                criticalLevel: NotificationCriticalLevelModel.Warning);

            return false;
        }

        /// <summary>
        /// Возвращает прочитанное значение без преобразований.
        /// </summary>
        /// <param name="model">Лист.</param>
        /// <param name="prop">Имя свойства.</param>
        /// <param name="value">Исходное значение свойства.</param>
        /// <returns>Исходное значение свойства.</returns>
        public object OnRead(TreeLeaveModel model, string prop, object value)
        {
            return value;
        }

        /// <summary>
        /// Не выполняет дополнительных действий после успешной записи.
        /// </summary>
        /// <param name="model">Лист.</param>
        /// <param name="prop">Имя свойства.</param>
        /// <param name="oldValue">Предыдущее значение.</param>
        /// <param name="newValue">Новое значение.</param>
        public void OnWrite(TreeLeaveModel model, string prop, object oldValue, object newValue)
        {
        }
    }
}
