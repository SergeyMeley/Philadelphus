using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using System.ComponentModel.DataAnnotations;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Доменная модель системного узла рабочего дерева.
    /// </summary>
    public class SystemBaseTreeNodeModel : TreeNodeModel
    {
        /// <summary>
        /// Предопределенные идентификаторы системных базовых типов.
        /// </summary>
        private static Dictionary<Guid, SystemBaseType> _baseUuids = new Dictionary<Guid, SystemBaseType>()
        {
            { Guid.Parse("00000000-0000-0000-0000-00015b10ec20"), SystemBaseType.OBJECT },
            { Guid.Parse("00000000-0000-0000-0000-192018091407"), SystemBaseType.STRING },
            { Guid.Parse("00000000-0000-0000-0000-142113e1809c"), SystemBaseType.NUMERIC },
            { Guid.Parse("00000000-0000-0000-0000-914200507e18"), SystemBaseType.INTEGER },
            { Guid.Parse("00000000-0000-0000-0000-0000f1215a20"), SystemBaseType.FLOAT },
            { Guid.Parse("00000000-0000-0000-0000-0b151512ea14"), SystemBaseType.BOOL },
            { Guid.Parse("00000000-0000-0000-0000-da20e200913e"), SystemBaseType.DATETIME },
            { Guid.Parse("00000000-0000-0000-0000-0000000da20e"), SystemBaseType.DATE },
            { Guid.Parse("00000000-0000-0000-0000-00000200913e"), SystemBaseType.TIME },
            { Guid.Parse("00000000-0000-0000-0000-000000f0912e"), SystemBaseType.FILE },
            { Guid.Parse("00000000-0000-0000-0000-001315141e25"), SystemBaseType.MONEY },
        };

        /// <summary>
        /// Тип.
        /// </summary>
        [Display(Name = "[Системный тип]", Description = "Системный базовый тип")]
        public override SystemBaseType SystemBaseType { get; }

        /// <summary>
        /// Создать системный узел по системному базовому типу.
        /// </summary>
        /// <param name="parent">Родительский элемент рабочего дерева.</param>
        /// <param name="owner">Рабочее дерево, которому принадлежит узел.</param>
        /// <param name="type">Системный базовый тип.</param>
        /// <param name="notificationService">Сервис уведомлений об изменениях модели.</param>
        /// <param name="propertiesPolicy">Политика доступности свойств узла.</param>
        internal SystemBaseTreeNodeModel(
            IParentModel parent, 
            WorkingTreeModel owner, 
            SystemBaseType type,
            INotificationService notificationService,
            IPropertiesPolicy<TreeNodeModel> propertiesPolicy) 
            : base(GetUuidByType(type), parent, owner, notificationService, propertiesPolicy)
        {
            SystemBaseType = type;
            InitProperties(type);
        }

        /// <summary>
        /// Восстановить системный узел по предопределенному идентификатору.
        /// </summary>
        /// <param name="uuid">Идентификатор системного узла.</param>
        /// <param name="parent">Родительский элемент рабочего дерева.</param>
        /// <param name="owner">Рабочее дерево, которому принадлежит узел.</param>
        /// <param name="notificationService">Сервис уведомлений об изменениях модели.</param>
        /// <param name="propertiesPolicy">Политика доступности свойств узла.</param>
        internal SystemBaseTreeNodeModel(
            Guid uuid,
            IParentModel parent,
            WorkingTreeModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<TreeNodeModel> propertiesPolicy)
            : base(uuid, parent, owner, notificationService, propertiesPolicy)
        {
            SystemBaseType = GetTypeByUuid(uuid);
            InitProperties(SystemBaseType);
        }

        /// <summary>
        /// Инициализировать отображаемые свойства системного узла.
        /// </summary>
        /// <param name="type">Системный базовый тип.</param>
        private void InitProperties(SystemBaseType type)
        {
            switch (type)
            {
                case SystemBaseType.OBJECT:
                    Name = "Объект";
                    Description = "object, variant";
                    CustomCode = "OBJ0";
                    Alias = "obj";
                    break;
                case SystemBaseType.STRING:
                    Name = "Текст";
                    Description = "string, text";
                    CustomCode = "TEXT";
                    Alias = "txt";
                    break;
                case SystemBaseType.NUMERIC:
                    Name = "Число";
                    Description = "numeric";
                    CustomCode = "NUM0";
                    Alias = "num";
                    break;
                case SystemBaseType.INTEGER:
                    Name = "Целое число";
                    Description = "integer";
                    CustomCode = "INT0";
                    Alias = "int";
                    break;
                case SystemBaseType.FLOAT:
                    Name = "Дробное число";
                    Description = "float";
                    CustomCode = "FLT0";
                    Alias = "flt";
                    break;
                case SystemBaseType.BOOL:
                    Name = "Логическое значение";
                    Description = "bool, boolean";
                    CustomCode = "BOOL";
                    Alias = "bol";
                    break;
                case SystemBaseType.DATETIME:
                    Name = "Дата и время";
                    Description = "datetime";
                    CustomCode = "DTIM";
                    Alias = "dtm";
                    break;
                case SystemBaseType.DATE:
                    Name = "Дата";
                    Description = "date";
                    CustomCode = "DATE";
                    Alias = "dat";
                    break;
                case SystemBaseType.TIME:
                    Name = "Время";
                    Description = "time";
                    CustomCode = "TIME";
                    Alias = "tim";
                    break;
                case SystemBaseType.FILE:
                    Name = "Файл";
                    Description = "file, binary";
                    CustomCode = "FILE";
                    Alias = "fil";
                    break;
                case SystemBaseType.MONEY:
                    Name = "Деньги";
                    Description = "money, currency";
                    CustomCode = "MNY0";
                    Alias = "mny";
                    break;
                default:
                    throw new Exception();
            }
        }

        /// <summary>
        /// Получить предопределенный идентификатор системного узла по типу.
        /// </summary>
        /// <param name="type">Системный базовый тип.</param>
        /// <returns>Идентификатор системного узла.</returns>
        internal static Guid GetUuidByType(SystemBaseType type)
        {
            if (_baseUuids.ContainsValue(type))
            {
                return _baseUuids.SingleOrDefault(x => x.Value == type).Key;
            }
            throw new Exception();
        }

        /// <summary>
        /// Получить системный базовый тип по предопределенному идентификатору узла.
        /// </summary>
        /// <param name="uuid">Идентификатор системного узла.</param>
        /// <returns>Системный базовый тип.</returns>
        internal static SystemBaseType GetTypeByUuid(Guid uuid)
        {
            if (_baseUuids.ContainsKey(uuid))
            {
                return _baseUuids[uuid];
            }
            throw new Exception();
        }

        /// <summary>
        /// Проверить, относится ли идентификатор к предопределенным системным узлам.
        /// </summary>
        /// <param name="uuid">Проверяемый идентификатор.</param>
        /// <returns>Признак системного узла.</returns>
        internal static bool IsSystemBaseType(Guid uuid)
        {
            return _baseUuids.ContainsKey(uuid);
        }

        /// <summary>
        /// Запрещает пользовательское добавление атрибутов к системному базовому узлу.
        /// </summary>
        /// <param name="attribute">Атрибут.</param>
        /// <returns>false.</returns>
        /// <remarks>Implements requirement R-5.05.</remarks>
        public override bool AddAttribute(global::Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes.ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            SendAttributeCollectionRestriction("R-5.05", "добавление атрибута");
            return false;
        }

        /// <summary>
        /// Запрещает пользовательское удаление атрибутов системного базового узла.
        /// </summary>
        /// <param name="attribute">Атрибут.</param>
        /// <returns>false.</returns>
        /// <remarks>Implements requirement R-5.05.</remarks>
        public override bool RemoveAttribute(global::Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes.ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            SendAttributeCollectionRestriction("R-5.05", "удаление атрибута");
            return false;
        }

        /// <summary>
        /// Запрещает пользовательскую очистку атрибутов системного базового узла.
        /// </summary>
        /// <returns>false.</returns>
        /// <remarks>Implements requirement R-5.05.</remarks>
        public override bool ClearAttributes()
        {
            SendAttributeCollectionRestriction("R-5.05", "очистка атрибутов");
            return false;
        }

        private void SendAttributeCollectionRestriction(string requirementCode, string operation)
        {
            _notificationService.SendTextMessage<SystemBaseTreeNodeModel>(
                $"{requirementCode}: Для системного базового узла '{Name}' [{Uuid}] операция '{operation}' запрещена. " +
                "Атрибуты элементов системного рабочего дерева не редактируются пользователем.",
                criticalLevel: NotificationCriticalLevelModel.Warning);
        }
    }
}
