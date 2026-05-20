using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Правило, ограничивающее наименование атрибута системными свойствами и атрибутами того же владельца.
    /// </summary>
    public class ReservedAttributeNamePropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        private readonly INotificationService _notificationService;

        private static readonly HashSet<string> _reservedNames = typeof(WorkingTreeMemberBaseModel<ElementAttributeModel>)
            .GetProperties()
            .Select(x => x.Name)
            .Concat(typeof(ElementAttributeModel).GetProperties().Select(x => x.Name))
            .ToHashSet(StringComparer.Ordinal);

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ReservedAttributeNamePropertiesRule" />.
        /// </summary>
        /// <param name="notificationService">Сервис уведомлений.</param>
        public ReservedAttributeNamePropertiesRule(
            INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Признак доступности чтения.
        /// </summary>
        /// <param name="model">Модель.</param>
        /// <param name="prop">Свойство.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool CanRead(ElementAttributeModel model, string prop)
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
        public bool CanWrite(ElementAttributeModel model, string prop, object value)
        {
            if (prop != nameof(ElementAttributeModel.Name)
                || value is not string newName)
            {
                return true;
            }

            if (_reservedNames.Contains(newName))
            {
                _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                    $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                    $"изменение значения свойства '{prop}' ограничено, т.к. наименование '{newName}' совпадает с наименованием системного свойства.",
                    criticalLevel: NotificationCriticalLevelModel.Warning);

                return false;
            }

            if (model.Owner is IAttributeOwnerModel attributeOwner
                && attributeOwner.Attributes.Any(x => x.Uuid != model.Uuid && x.Name == newName))
            {
                _notificationService.SendTextMessage<CompositeAttributePropertiesPolicy>(
                    $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                    $"изменение значения свойства '{prop}' ограничено, т.к. у текущего владельца уже есть другой атрибут с наименованием '{newName}'.",
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
        public object OnRead(ElementAttributeModel model, string prop, object value)
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
        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
        }
    }
}
