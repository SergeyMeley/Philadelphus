using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Policies.Attributes
{
    /// <summary>
    /// Политика свойств атрибутов.
    /// </summary>
    internal class CompositeAttributePropertiesPolicy : IAttributePropertiesPolicy
    {
        private readonly INotificationService _notificationService;
        private readonly List<IAttributePropertiesRule<ElementAttributeModel>> _rules;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="CompositeAttributePropertiesPolicy" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="rules">Набор правил.</param>
        public CompositeAttributePropertiesPolicy(
            INotificationService notificationService,
            IEnumerable<IAttributePropertiesRule<ElementAttributeModel>> rules)
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
        public bool CanRead(ElementAttributeModel model, string prop)
        {
            var result = _rules.All(r => r.CanRead(model, prop));

            if (result == false)
            {
                _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                    $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                    $"не пройдены проверки политик на чтение данных",
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
        public bool CanWrite(ElementAttributeModel model, string prop, object value)
        {
            var result = _rules.All(r => r.CanWrite(model, prop, value));

            if (result == false)
            {
                _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                    $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                    $"не пройдены проверки политик на запись данных",
                    criticalLevel: NotificationCriticalLevelModel.Warning);
            }

            return result;
        }

        /// <summary>
        /// Выполняет операцию OnRead.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <param name="value">Значение.</param>
        /// <returns>Результат выполнения операции.</returns>
        public object OnRead(ElementAttributeModel model, string prop, object value)
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
        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
            foreach (var r in _rules)
                r.OnWrite(model, prop, oldValue, newValue);
        }
    }
}
