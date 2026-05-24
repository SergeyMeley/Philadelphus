using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies
{
    /// <summary>
    /// Композитная политика свойств.
    /// </summary>
    /// <remarks>Implements requirements R-0.01 and R-0.02.</remarks>
    internal class CompositePropertiesPolicy<T> : IPropertiesPolicy<T>
        where T : MainEntityBaseModel<T>
    {
        private readonly INotificationService _notificationService;
        private readonly List<IPropertiesRule<T>> _rules;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CompositePropertiesPolicy{T}" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="rules">Набор правил.</param>
        public CompositePropertiesPolicy(
            INotificationService notificationService,
            IEnumerable<IPropertiesRule<T>> rules)
        {
            _notificationService = notificationService;
            _rules = rules.ToList();
        }

        /// <summary>
        /// Признак доступности чтения.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CanRead(T model, string prop)
        {
            var result = _rules.All(r => r.CanRead(model, prop));

            if (result == false)
            {
                _notificationService.SendTextMessage<CompositePropertiesPolicy<T>>(
                    $"Для элемента '{model.Name}' [{model.Uuid}] не пройдены проверки политик на чтение данных",
                    criticalLevel: NotificationCriticalLevelModel.Warning);
            }

            return result;
        }

        /// <summary>
        /// Признак доступности записи.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <param name="value">Значение.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CanWrite(T model, string prop, object value)
        {
            var result = _rules.All(r => r.CanWrite(model, prop, value));

            if (result == false)
            {
                _notificationService.SendTextMessage<CompositePropertiesPolicy<T>>(
                    $"Для элемента '{model.Name}' [{model.Uuid}] не пройдены проверки политик на запись данных",
                    criticalLevel: NotificationCriticalLevelModel.Warning);
            }

            return result;
        }

        /// <summary>
        /// Выполняет предварительную подготовку значения перед записью.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <param name="value">Значение.</param>
        /// <returns>Результат выполнения операции.</returns>
        public object PrepareWriteValue(T model, string prop, object value)
        {
            foreach (var r in _rules.OfType<IPrepareWriteValuePropertiesRule<T>>())
                value = r.PrepareWriteValue(model, prop, value);

            return value;
        }

        /// <summary>
        /// Выполняет операцию OnRead.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <param name="value">Значение.</param>
        /// <returns>Результат выполнения операции.</returns>
        public object OnRead(T model, string prop, object value)
        {
            foreach (var r in _rules)
                value = r.OnRead(model, prop, value);

            return value;
        }

        /// <summary>
        /// Выполняет операцию OnWrite.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <param name="oldValue">Предыдущее значение.</param>
        /// <param name="newValue">Новое значение.</param>
        public void OnWrite(T model, string prop, object oldValue, object newValue)
        {
            foreach (var r in _rules)
                r.OnWrite(model, prop, oldValue, newValue);
        }
    }
}
