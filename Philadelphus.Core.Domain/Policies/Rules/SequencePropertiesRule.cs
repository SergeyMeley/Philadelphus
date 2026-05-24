using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Правило, ограничивающее значения свойства Sequence.
    /// </summary>
    /// <remarks>Implements requirements R-2.01, R-2.02 and R-2.03.</remarks>
    internal class SequencePropertiesRule<T> : IPropertiesRule<T>, IAttributePropertiesRule<ElementAttributeModel>
        where T : MainEntityBaseModel<T>, ISequencableModel
    {
        private readonly INotificationService _notificationService;
        private readonly ISequenceUniquenessStrategy<T> _strategy;

        /// <summary>
        /// Инициализирует правило проверки свойства <c>Sequence</c>.
        /// </summary>
        /// <param name="notificationService">Сервис пользовательских уведомлений.</param>
        /// <param name="strategy">
        /// Стратегия, возвращающая элементы той коллекции, внутри которой значение <c>Sequence</c>
        /// должно быть уникальным.
        /// </param>
        public SequencePropertiesRule(
            INotificationService notificationService,
            ISequenceUniquenessStrategy<T> strategy)
        {
            _notificationService = notificationService;
            _strategy = strategy;
        }

        public bool CanRead(T model, string prop)
        {
            return true;
        }

        public bool CanWrite(T model, string prop, object value)
        {
            if (prop != nameof(ISequencableModel.Sequence)
                || value is not long newSequence)
            {
                return true;
            }

            // Sequence используется для пользовательского порядка в коллекциях.
            // Ноль и отрицательные значения не допускаются, чтобы не смешивать реальные значения
            // с дефолтным значением поля long.
            if (newSequence <= 0)
            {
                SendRestrictionNotification(model, prop, "значение должно быть больше 0");
                return false;
            }

            // Область уникальности зависит от типа модели:
            // узлы сравниваются с узлами своего родителя, листья - с листьями родительского узла,
            // атрибуты - с другими непосредственными атрибутами владельца.
            if (_strategy.GetSequencedItems(model).Any(x => x.Uuid != model.Uuid && x.Sequence == newSequence))
            {
                SendRestrictionNotification(model, prop, $"в этой коллекции уже есть элемент с Sequence = {newSequence}");
                return false;
            }

            return true;
        }

        public object OnRead(T model, string prop, object value)
        {
            return value;
        }

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

        private void SendRestrictionNotification(T model, string prop, string reason)
        {
            _notificationService.SendTextMessage<SequencePropertiesRule<T>>(
                $"Для элемента '{model.Name}' [{model.Uuid}] изменение значения свойства '{prop}' ограничено, т.к. {reason}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);
        }
    }
}
