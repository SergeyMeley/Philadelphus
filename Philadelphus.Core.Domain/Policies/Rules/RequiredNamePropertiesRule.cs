using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Правило, запрещающее пустое значение свойства Name.
    /// </summary>
    internal class RequiredNamePropertiesRule<T> : IPropertiesRule<T>, IAttributePropertiesRule<ElementAttributeModel>
        where T : MainEntityBaseModel<T>
    {
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RequiredNamePropertiesRule{T}" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        public RequiredNamePropertiesRule(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Признак доступности чтения.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CanRead(T model, string prop)
        {
            return true;
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
            if (prop == nameof(MainEntityBaseModel<T>.Name)
                && value is string newName
                && string.IsNullOrWhiteSpace(newName))
            {
                _notificationService.SendTextMessage<RequiredNamePropertiesRule<T>>(
                    $"Для элемента '{model.Name}' [{model.Uuid}] изменение значения свойства '{prop}' ограничено, т.к. наименование не может быть пустым.",
                    criticalLevel: NotificationCriticalLevelModel.Warning);

                return false;
            }

            return true;
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
        }

        bool IAttributePropertiesRule<ElementAttributeModel>.CanRead(ElementAttributeModel model, string prop)
        {
            return true;
        }

        bool IAttributePropertiesRule<ElementAttributeModel>.CanWrite(ElementAttributeModel model, string prop, object value)
        {
            return model is T typedModel
                ? CanWrite(typedModel, prop, value)
                : true;
        }

        object IAttributePropertiesRule<ElementAttributeModel>.OnRead(ElementAttributeModel model, string prop, object value)
        {
            return value;
        }

        void IAttributePropertiesRule<ElementAttributeModel>.OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
        }
    }
}
