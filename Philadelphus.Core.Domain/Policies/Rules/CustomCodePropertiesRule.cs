using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Rules
{
    /// <summary>
    /// Правило, ограничивающее значения свойства CustomCode.
    /// </summary>
    /// <remarks>Implements requirements R-3.01, R-3.02, R-3.03 and R-3.04.</remarks>
    internal class CustomCodePropertiesRule<T> : IPropertiesRule<T>, IPrepareWriteValuePropertiesRule<T>, IAttributePropertiesRule<ElementAttributeModel>
        where T : WorkingTreeMemberBaseModel<T>
    {
        private readonly INotificationService _notificationService;
        private readonly ICustomCodeUniquenessStrategy<T> _strategy;

        /// <summary>
        /// Инициализирует правило проверки свойства <c>CustomCode</c>.
        /// </summary>
        /// <param name="notificationService">Сервис пользовательских уведомлений.</param>
        /// <param name="strategy">
        /// Стратегия, определяющая область уникальности кода:
        /// для элементов дерева это все рабочее дерево, для атрибутов - атрибуты одного владельца.
        /// </param>
        public CustomCodePropertiesRule(
            INotificationService notificationService,
            ICustomCodeUniquenessStrategy<T> strategy)
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
            if (prop != nameof(WorkingTreeMemberBaseModel<T>.CustomCode)
                || value is not string newCustomCode)
            {
                return true;
            }

            // Пустой код запрещен после предварительной нормализации.
            // Если пользователь ввел только недопустимые символы, PrepareWriteValue вернет пустую строку,
            // и именно эта проверка заблокирует изменение.
            if (string.IsNullOrWhiteSpace(newCustomCode))
            {
                SendRestrictionNotification(model, prop, "значение не может быть пустым");
                return false;
            }

            // Не сравниваем объект с самим собой: повторное присвоение текущего значения не должно считаться дублем.
            if (_strategy.GetCustomCodeItems(model).Any(x => x.Uuid != model.Uuid && x.CustomCode == newCustomCode))
            {
                SendRestrictionNotification(model, prop, $"уже есть другой элемент с CustomCode = '{newCustomCode}'");
                return false;
            }

            return true;
        }

        public object PrepareWriteValue(T model, string prop, object value)
        {
            if (prop != nameof(WorkingTreeMemberBaseModel<T>.CustomCode)
                || value is not string newCustomCode)
            {
                return value;
            }

            // CustomCode хранится в более строгом формате, чем Name:
            // допускаются только латинские буквы и цифры, остальные символы удаляются без блокировки записи.
            return NormalizeCustomCode(newCustomCode);
        }

        public object OnRead(T model, string prop, object value)
        {
            return value;
        }

        public void OnWrite(T model, string prop, object oldValue, object newValue)
        {
        }

        /// <summary>
        /// Удаляет из пользовательского кода все символы, кроме латинских букв и цифр.
        /// </summary>
        /// <param name="value">Исходное значение CustomCode.</param>
        /// <returns>Строка, содержащая только символы <c>A-Z</c>, <c>a-z</c> и <c>0-9</c>.</returns>
        internal static string NormalizeCustomCode(string? value)
        {
            if (value == null)
                return string.Empty;

            return new string(value.Where(IsAllowedCustomCodeCharacter).ToArray());
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

        private static bool IsAllowedCustomCodeCharacter(char value)
        {
            return value is >= 'A' and <= 'Z'
                || value is >= 'a' and <= 'z'
                || value is >= '0' and <= '9';
        }

        private void SendRestrictionNotification(T model, string prop, string reason)
        {
            _notificationService.SendTextMessage<CustomCodePropertiesRule<T>>(
                $"Для элемента '{model.Name}' [{model.Uuid}] изменение значения свойства '{prop}' ограничено, т.к. {reason}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);
        }
    }
}
