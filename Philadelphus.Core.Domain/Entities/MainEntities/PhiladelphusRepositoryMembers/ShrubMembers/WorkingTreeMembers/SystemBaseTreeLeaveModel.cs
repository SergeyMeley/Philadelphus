using Philadelphus.Core.Domain.Entities.Enums;
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
        private string _stringValue = string.Empty;

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
        [Display(Name = "Системный тип", Description = "Системный базовый тип")]
        public override SystemBaseType SystemBaseType { get; }

        /// <summary>
        /// Строковое значение
        /// </summary>
        [Display(Name = "Значение", Description = "Строковое значение")]
        public string StringValue 
        { 
            get
            {
                return _stringValue; 
            }
            set
            {
                if (SetValue(ref _stringValue, value))
                {
                    Name = value;
                    Description = value;
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
    }
}
