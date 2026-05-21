using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Policies.Builders;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Policies.Attributes.Rules
{
    /// <summary>
    /// Проверяет совместимость значения системного листа с системным типом атрибута.
    /// </summary>
    /// <remarks>
    /// Сам <see cref="SystemBaseTreeLeaveModel" /> уже гарантирует корректность значения для собственного
    /// <see cref="SystemBaseTreeLeaveModel.SystemBaseType" />. Это правило остается отдельным, потому что
    /// атрибут может ожидать другой системный тип, и тогда строковое значение листа нужно проверить уже
    /// относительно типа атрибута.
    /// </remarks>
    public class SystemBaseAttributeValuePropertiesRule : IAttributePropertiesRule<ElementAttributeModel>
    {
        private readonly INotificationService _notificationService;

        public SystemBaseAttributeValuePropertiesRule(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Проверяет возможность чтения свойства атрибута.
        /// </summary>
        /// <param name="model">Проверяемый атрибут.</param>
        /// <param name="prop">Имя свойства.</param>
        /// <returns>Всегда true: правило ограничивает только запись.</returns>
        public bool CanRead(ElementAttributeModel model, string prop)
        {
            return true;
        }

        /// <summary>
        /// Проверяет возможность записи значения или типа данных атрибута.
        /// </summary>
        /// <param name="model">Проверяемый атрибут.</param>
        /// <param name="prop">Имя записываемого свойства.</param>
        /// <param name="value">Новое значение свойства.</param>
        /// <returns>true, если запись допустима; иначе false.</returns>
        public bool CanWrite(ElementAttributeModel model, string prop, object value)
        {
            if (prop == nameof(ElementAttributeModel.Value))
            {
                return CanWriteValue(model, value as TreeLeaveModel);
            }

            if (prop == nameof(ElementAttributeModel.ValueType))
            {
                return CanWriteValueType(model, value as TreeNodeModel);
            }

            return true;
        }

        /// <summary>
        /// Проверяет, соответствует ли присваиваемый системный лист текущему типу данных атрибута.
        /// </summary>
        /// <param name="model">Проверяемый атрибут.</param>
        /// <param name="value">Присваиваемый лист-значение.</param>
        /// <returns>true, если значение совместимо с типом данных атрибута; иначе false.</returns>
        private bool CanWriteValue(ElementAttributeModel model, TreeLeaveModel? value)
        {
            if (SystemBaseAttributeValueCompatibilityValidator.IsCompatible(
                model.ValueType,
                value,
                out var systemBaseType,
                out var stringValue,
                out var expectedFormat))
            {
                return true;
            }

            _notificationService.SendTextMessage<SystemBaseAttributeValuePropertiesRule>(
                $"Для атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}] " +
                $"значение '{stringValue ?? "<null>"}' не соответствует системному типу '{systemBaseType}'. " +
                $"Ожидаемый формат: {expectedFormat}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);

            return false;
        }

        /// <summary>
        /// Проверяет, можно ли сменить тип данных атрибута без потери согласованности уже заданных значений.
        /// </summary>
        /// <remarks>
        /// Если значение уже заполнено системным листом, при смене системного типа оно не должно оставаться
        /// ссылкой на лист старого узла. Сначала проверяется, что строковое значение можно представить в новом
        /// типе; затем <see cref="OnWrite" /> заменяет ссылку на лист из нового базового узла.
        /// </remarks>
        /// <param name="model">Проверяемый атрибут.</param>
        /// <param name="valueType">Новый тип данных атрибута.</param>
        /// <returns>true, если текущие значения можно перенести на новый тип; иначе false.</returns>
        private bool CanWriteValueType(ElementAttributeModel model, TreeNodeModel? valueType)
        {
            if (model.IsCollectionValue)
            {
                foreach (var value in model.Values)
                {
                    if (TryResolveCompatibleValue(
                        valueType,
                        value,
                        out _,
                        out var systemBaseType,
                        out var stringValue,
                        out var expectedFormat) == false)
                    {
                        SendValueTypeChangeError(model, stringValue, systemBaseType, expectedFormat);
                        return false;
                    }
                }

                return true;
            }

            if (TryResolveCompatibleValue(
                valueType,
                model.Value,
                out _,
                out var singleSystemBaseType,
                out var singleStringValue,
                out var singleExpectedFormat))
            {
                return true;
            }

            SendValueTypeChangeError(model, singleStringValue, singleSystemBaseType, singleExpectedFormat);
            return false;
        }

        /// <summary>
        /// Отправляет диагностическое сообщение о невозможности сменить тип данных атрибута.
        /// </summary>
        /// <param name="model">Атрибут, у которого меняется тип данных.</param>
        /// <param name="stringValue">Строковое значение текущего системного листа.</param>
        /// <param name="systemBaseType">Системный тип, ожидаемый новым типом данных атрибута.</param>
        /// <param name="expectedFormat">Описание ожидаемого формата значения.</param>
        private void SendValueTypeChangeError(
            ElementAttributeModel model,
            string? stringValue,
            SystemBaseType? systemBaseType,
            string expectedFormat)
        {
            _notificationService.SendTextMessage<SystemBaseAttributeValuePropertiesRule>(
                $"Нельзя сменить тип атрибута '{model.Name}' [{model.Uuid}] элемента '{(model.Owner as IMainEntityModel)?.Name}' [{(model.Owner as IMainEntityModel)?.Uuid}]: " +
                $"уже заданное значение '{stringValue ?? "<null>"}' не соответствует системному типу '{systemBaseType}'. " +
                $"Ожидаемый формат: {expectedFormat}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);
        }

        /// <summary>
        /// Возвращает значение свойства без преобразований.
        /// </summary>
        /// <param name="model">Атрибут, у которого читается свойство.</param>
        /// <param name="prop">Имя читаемого свойства.</param>
        /// <param name="value">Исходное значение свойства.</param>
        /// <returns>Исходное значение свойства.</returns>
        public object OnRead(ElementAttributeModel model, string prop, object value)
        {
            return value;
        }

        /// <summary>
        /// Выполняет перенос уже заданных системных значений после успешной смены типа данных атрибута.
        /// </summary>
        /// <param name="model">Атрибут, у которого изменилось свойство.</param>
        /// <param name="prop">Имя измененного свойства.</param>
        /// <param name="oldValue">Старое значение свойства.</param>
        /// <param name="newValue">Новое значение свойства.</param>
        public void OnWrite(ElementAttributeModel model, string prop, object oldValue, object newValue)
        {
            if (prop != nameof(ElementAttributeModel.ValueType)
                || newValue is not TreeNodeModel valueType)
            {
                return;
            }

            if (model.IsCollectionValue)
            {
                var values = model.Values
                    .Select(value => ResolveCompatibleValueOrCurrent(valueType, value))
                    .GroupBy(value => value.Uuid)
                    .Select(x => x.First())
                    .ToList();

                model.ClearValuesCollection();
                foreach (var value in values)
                {
                    model.TryAddValueToValuesCollection(value);
                }

                return;
            }

            model.Value = ResolveCompatibleValueOrCurrent(valueType, model.Value);
        }

        /// <summary>
        /// Возвращает совместимый лист для нового типа данных или исходное значение, если перенос не требуется.
        /// </summary>
        /// <param name="valueType">Новый тип данных атрибута.</param>
        /// <param name="value">Текущее значение атрибута.</param>
        /// <returns>Лист, который должен стать новым значением атрибута.</returns>
        private TreeLeaveModel ResolveCompatibleValueOrCurrent(TreeNodeModel valueType, TreeLeaveModel value)
        {
            return TryResolveCompatibleValue(
                valueType,
                value,
                out var resolvedValue,
                out _,
                out _,
                out _)
                ? resolvedValue ?? value
                : value;
        }

        /// <summary>
        /// Проверяет совместимость текущего значения с новым типом данных и подготавливает лист для переноса.
        /// </summary>
        /// <param name="valueType">Новый тип данных атрибута.</param>
        /// <param name="value">Текущее значение атрибута.</param>
        /// <param name="resolvedValue">Совместимый лист в новом базовом узле или исходное значение.</param>
        /// <param name="systemBaseType">Системный тип, по которому выполнялась проверка.</param>
        /// <param name="stringValue">Строковое значение проверяемого системного листа.</param>
        /// <param name="expectedFormat">Описание ожидаемого формата значения.</param>
        /// <returns>true, если значение можно использовать с новым типом данных; иначе false.</returns>
        private bool TryResolveCompatibleValue(
            TreeNodeModel? valueType,
            TreeLeaveModel? value,
            out TreeLeaveModel? resolvedValue,
            out SystemBaseType? systemBaseType,
            out string? stringValue,
            out string expectedFormat)
        {
            resolvedValue = value;
            systemBaseType = null;
            stringValue = null;
            expectedFormat = string.Empty;

            if (valueType is not SystemBaseTreeNodeModel systemBaseNode
                || value is not SystemBaseTreeLeaveModel systemBaseValue)
            {
                return true;
            }

            systemBaseType = systemBaseNode.SystemBaseType;
            stringValue = systemBaseValue.StringValue;
            if (SystemBaseStringValueValidator.IsValid(systemBaseType.Value, stringValue, out expectedFormat) == false)
            {
                return false;
            }

            if (systemBaseValue.ParentNode?.Uuid == systemBaseNode.Uuid)
            {
                return true;
            }

            resolvedValue = GetOrCreateCompatibleSystemBaseLeave(systemBaseNode, systemBaseValue);
            return resolvedValue != null;
        }

        /// <summary>
        /// Находит или создает системный лист в целевом базовом узле с тем же строковым значением.
        /// </summary>
        /// <remarks>
        /// При смене ValueType нельзя переиспользовать лист из старого узла даже при совпадающем формате:
        /// доменно это значение другого базового типа. Поэтому ссылка переносится на аналогичный лист под
        /// новым системным узлом.
        /// </remarks>
        /// <param name="targetNode">Целевой системный узел типа данных.</param>
        /// <param name="sourceValue">Исходный системный лист.</param>
        /// <returns>Совместимый лист целевого узла или null, если его нельзя получить.</returns>
        private SystemBaseTreeLeaveModel? GetOrCreateCompatibleSystemBaseLeave(
            SystemBaseTreeNodeModel targetNode,
            SystemBaseTreeLeaveModel sourceValue)
        {
            if (targetNode.SystemBaseType == SystemBaseType.BOOL)
            {
                return ResolveBoolLeave(targetNode, sourceValue.StringValue);
            }

            var existing = targetNode.ChildLeaves
                .OfType<SystemBaseTreeLeaveModel>()
                .FirstOrDefault(x => string.Equals(x.StringValue, sourceValue.StringValue, StringComparison.Ordinal));
            if (existing != null)
            {
                return existing;
            }

            var result = new SystemBaseTreeLeaveModel(
                Guid.CreateVersion7(),
                targetNode,
                targetNode.OwningWorkingTree,
                targetNode.SystemBaseType,
                _notificationService,
                PropertiesPolicyBuilder.CreateTreeLeaveDefault(_notificationService));

            result.StringValue = sourceValue.StringValue;
            return result;
        }

        /// <summary>
        /// Возвращает предопределенный BOOL-лист, соответствующий строковому значению.
        /// </summary>
        /// <remarks>
        /// Для BOOL новые листья не создаются: у системного типа допустимы только предопределенные значения.
        /// </remarks>
        /// <param name="targetNode">Системный узел BOOL.</param>
        /// <param name="stringValue">Строковое значение, которое нужно сопоставить с предопределенным листом.</param>
        /// <returns>Предопределенный BOOL-лист или null, если значение не распознано.</returns>
        private static SystemBaseTreeLeaveModel? ResolveBoolLeave(
            SystemBaseTreeNodeModel targetNode,
            string stringValue)
        {
            if (SystemBaseStringValueValidator.TryParse(SystemBaseType.BOOL, stringValue, out var typedValue, out _) == false
                || typedValue is not bool boolValue)
            {
                return null;
            }

            return targetNode.ChildLeaves
                .OfType<SystemBaseTreeLeaveModel>()
                .FirstOrDefault(x => x.TypedValue is bool existingBoolValue && existingBoolValue == boolValue);
        }
    }
}
