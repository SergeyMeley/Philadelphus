using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System.ComponentModel.DataAnnotations;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Доменная модель системного листа рабочего дерева.
    /// </summary>
    public class SystemBaseTreeLeaveModel : TreeLeaveModel
    {
        private object? _typedValue;

        /// <summary>
        /// Предопределенные идентификаторы и строковые значения системных листьев.
        /// </summary>
        private static Dictionary<Guid, (SystemBaseType Type, string Value)> _baseUuids = new Dictionary<Guid, (SystemBaseType Type, string Value)>()
        {
            { Guid.Parse("00000000-0000-0000-0000-00000201821e"), (SystemBaseType.BOOL, "Истина") },
            { Guid.Parse("00000000-0000-0000-0000-00000fa1219e"), (SystemBaseType.BOOL, "Ложь") },
        };

        /// <summary>
        /// Тип.
        /// </summary>
        [Display(Name = "[Системный тип]", Description = "Системный базовый тип")]
        public override SystemBaseType SystemBaseType { get; }

        /// <summary>
        /// Типизированное представление <see cref="StringValue" /> согласно <see cref="SystemBaseType" />.
        /// </summary>
        /// <remarks>
        /// Наружный контракт системного листа остается строковым для совместимости с хранением, маппингом и
        /// существующим UI. Это свойство фиксирует доменный тип значения внутри модели, чтобы проверка
        /// корректности была инвариантом самого системного листа, а не только внешним правилом политики.
        /// </remarks>
        [Display(Name = "[Типизированное значение]", Description = "Значение системного листа, приведенное к системному базовому типу")]
        public object? TypedValue => _typedValue;

        /// <summary>
        /// Отображаемое имя системного листа.
        /// </summary>
        /// <remarks>
        /// Для системных листьев имя является производным от <see cref="StringValue" />. Внешняя запись в
        /// Name игнорируется, чтобы наименование не стало отдельным источником состояния и не расходилось с
        /// persisted-значением базового типа.
        /// </remarks>
        public override string Name
        {
            get => StringValue;
            set
            {
            }
        }

        /// <summary>
        /// Строковое значение
        /// </summary>
        [Display(Name = "[Значение]", Description = "Строковое значение")]
        public override string StringValue 
        { 
            get
            {
                return GetStoredStringValue(); 
            }
            set
            {
                var normalizedValue = NormalizeStringValue(value);
                if (TryParseStringValue(normalizedValue, out var typedValue) == false)
                {
                    return;
                }

                var oldValue = GetStoredStringValue();
                var descriptionWasSynchronized = string.IsNullOrEmpty(Description)
                    || string.Equals(Description, oldValue, StringComparison.Ordinal);
                var isSameValue = string.Equals(oldValue, normalizedValue, StringComparison.Ordinal);
                var changed = SetStoredStringValue(normalizedValue);
                if (changed
                    || (isSameValue
                        && (_typedValue is null
                            || string.Equals(Name, normalizedValue, StringComparison.Ordinal) == false)))
                {
                    ApplyStringValue(normalizedValue, typedValue, descriptionWasSynchronized);
                }
            }
        }

        /// <summary>
        /// Создать системный лист по родительскому системному типу и строковому значению.
        /// </summary>
        /// <param name="parent">Родительский системный узел.</param>
        /// <param name="owner">Рабочее дерево, которому принадлежит лист.</param>
        /// <param name="value">Строковое значение системного листа.</param>
        /// <param name="notificationService">Сервис уведомлений об изменениях модели.</param>
        /// <param name="propertiesPolicy">Политика доступности свойств листа.</param>
        internal SystemBaseTreeLeaveModel(
            SystemBaseTreeNodeModel parent,
            WorkingTreeModel owner,
            string value,
            INotificationService notificationService,
            IPropertiesPolicy<TreeLeaveModel> propertiesPolicy)
            : base(GetUuidByValue(parent.SystemBaseType, value), parent, owner, notificationService, propertiesPolicy)
        {
            SystemBaseType = parent.SystemBaseType;
            parent.AddPredefinedSystemBaseBoolLeave(this);
            InitProperties(Uuid);
        }

        /// <summary>
        /// Восстановить системный лист по предопределенному идентификатору.
        /// </summary>
        /// <param name="uuid">Идентификатор системного листа.</param>
        /// <param name="parent">Родительский системный узел.</param>
        /// <param name="owner">Рабочее дерево, которому принадлежит лист.</param>
        /// <param name="type">Тип листа для случаев, когда идентификатор еще не зарегистрирован как системный.</param>
        /// <param name="notificationService">Сервис уведомлений об изменениях модели.</param>
        /// <param name="propertiesPolicy">Политика доступности свойств листа.</param>
        internal SystemBaseTreeLeaveModel(
            Guid uuid, 
            SystemBaseTreeNodeModel parent, 
            WorkingTreeModel owner,
            SystemBaseType type,
            INotificationService notificationService,
            IPropertiesPolicy<TreeLeaveModel> propertiesPolicy) 
            : base(uuid, parent, owner, notificationService, propertiesPolicy)
        {
            SystemBaseType = IsSystemBaseValue(uuid)
                ? GetTypeByUuid(uuid)
                : type;

            parent.AddPredefinedSystemBaseBoolLeave(this);

            if (IsSystemBaseValue(uuid))
            {
                InitProperties(uuid);
            }
        }

        /// <summary>
        /// Получить все предопределенные строковые значения для системного типа.
        /// </summary>
        /// <param name="type">Системный тип.</param>
        /// <returns>Строковые значения системных листьев указанного типа.</returns>
        internal static IEnumerable<string> GetValuesByType(SystemBaseType type)
        {
            return _baseUuids
                .Where(x => x.Value.Type == type)
                .Select(x => x.Value.Value);
        }

        /// <summary>
        /// Получить предопределенный идентификатор системного листа по типу и значению.
        /// </summary>
        /// <param name="type">Системный тип.</param>
        /// <param name="value">Строковое значение системного листа.</param>
        /// <returns>Идентификатор системного листа.</returns>
        internal static Guid GetUuidByValue(SystemBaseType type, string value)
        {
            var item = _baseUuids.SingleOrDefault(x =>
                x.Value.Type == type
                && string.Equals(x.Value.Value, value, StringComparison.OrdinalIgnoreCase));

            if (item.Key != Guid.Empty)
            {
                return item.Key;
            }

            throw new Exception();
        }

        /// <summary>
        /// Получить системный тип по предопределенному идентификатору листа.
        /// </summary>
        /// <param name="uuid">Идентификатор системного листа.</param>
        /// <returns>Системный тип листа.</returns>
        internal static SystemBaseType GetTypeByUuid(Guid uuid)
        {
            if (_baseUuids.ContainsKey(uuid))
            {
                return _baseUuids[uuid].Type;
            }
            throw new Exception();
        }

        /// <summary>
        /// Проверить, относится ли идентификатор к предопределенным системным листьям.
        /// </summary>
        /// <param name="uuid">Проверяемый идентификатор.</param>
        /// <returns>Признак системного листа.</returns>
        internal static bool IsSystemBaseValue(Guid uuid)
        {
            return _baseUuids.ContainsKey(uuid);
        }

        /// <summary>
        /// Инициализировать свойства системного листа по предопределенному идентификатору.
        /// </summary>
        /// <param name="uuid">Идентификатор системного листа.</param>
        private void InitProperties(Guid uuid)
        {
            if (_baseUuids.ContainsKey(uuid) == false)
            {
                throw new Exception();
            }

            var props = _baseUuids[uuid];
            InitProperties(props.Type, props.Value);
        }

        /// <summary>
        /// Инициализировать отображаемое значение, код и псевдоним системного листа.
        /// </summary>
        /// <param name="type">Системный тип листа.</param>
        /// <param name="value">Строковое значение системного листа.</param>
        private void InitProperties(SystemBaseType type, string value)
        {
            switch (type)
            {
                case SystemBaseType.BOOL:
                    switch (value)
                    {
                        case "Истина":
                            StringValue = "Истина";
                            CustomCode = "TRUE";
                            Alias = "tru";
                            break;
                        case "Ложь":
                            StringValue = "Ложь";
                            CustomCode = "FALS";
                            Alias = "fls";
                            break;
                        default:
                            throw new Exception();
                    }
                    break;
                default:
                    throw new Exception();
            }
        }

        /// <summary>
        /// Проверяет строковое значение и подготавливает его типизированное представление.
        /// </summary>
        /// <param name="value">Новое строковое значение системного листа.</param>
        /// <param name="typedValue">Типизированное значение, соответствующее <see cref="SystemBaseType" />.</param>
        /// <returns>true, если строка допустима для типа листа; иначе false.</returns>
        private bool TryParseStringValue(string value, out object? typedValue)
        {
            if (SystemBaseStringValueValidator.TryParse(SystemBaseType, value, out typedValue, out var expectedFormat))
            {
                return true;
            }

            _notificationService.SendTextMessage<SystemBaseTreeLeaveModel>(
                $"Для системного листа '{Name}' [{Uuid}] значение '{value ?? "<null>"}' " +
                $"не соответствует системному типу '{SystemBaseType}'. Ожидаемый формат: {expectedFormat}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);

            return false;
        }

        /// <summary>
        /// Синхронизирует строковое значение с типизированным представлением и отображаемыми полями листа.
        /// </summary>
        /// <remarks>
        /// Метод вызывается даже когда новое значение равно текущему. Это важно для дефолтных значений вроде
        /// пустой строки: поле <see cref="_stringValue" /> уже равно string.Empty, но <see cref="TypedValue" />,
        /// Name и Description еще должны быть инициализированы согласованным состоянием.
        /// </remarks>
        /// <param name="value">Строковое значение системного листа.</param>
        /// <param name="typedValue">Типизированное представление строкового значения.</param>
        /// <param name="descriptionWasSynchronized">true, если описание до изменения совпадало со старым значением и может быть обновлено автоматически.</param>
        private void ApplyStringValue(string value, object? typedValue, bool descriptionWasSynchronized)
        {
            _typedValue = typedValue;

            // Name остается отображаемым представлением системного значения, но больше не является
            // источником истины для сохранения: persistence пишет отдельное поле StringValue.
            _name = value;
            if (descriptionWasSynchronized)
            {
                _description = value;
            }
        }

    }
}
