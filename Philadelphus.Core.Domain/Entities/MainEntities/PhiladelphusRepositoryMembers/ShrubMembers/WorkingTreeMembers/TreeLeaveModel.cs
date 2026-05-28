using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers
{
    /// <summary>
    /// Лист дерева участников репозитория Чубушника.
    /// </summary>
    public class TreeLeaveModel : WorkingTreeMemberBaseModel<TreeLeaveModel>, IWorkingTreeMemberModel, IChildrenModel, IOwnerModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию.
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый лист";

        private string _stringValue = EmptyStringValue;

        /// <summary>
        /// Техническое значение пустого листа для хранения в не-null поле.
        /// </summary>
        public const string EmptyStringValue = "<empty>";

        private static readonly JsonSerializerOptions _stringValueJsonOptions = new()
        {
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            Converters = { new JsonStringEnumConverter() },
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        #endregion

        #region [ Properties ]

        #region [ General Properties ]

        /// <summary>
        /// Тип.
        /// </summary>
        [Display(Name = "Системный тип", Description = "Системный базовый тип")]
        public virtual SystemBaseType SystemBaseType => SystemBaseType.USER_DEFINED;

        /// <summary>
        /// Строковое значение листа.
        /// </summary>
        /// <remarks>
        /// Для пользовательских листьев значение вычисляется из текущей коллекции атрибутов и не вводится
        /// вручную. Системные листы переопределяют это свойство и хранят валидируемое значение базового типа.
        /// </remarks>
        [Display(Name = "Значение", Description = "Строковое значение листа")]
        public virtual string StringValue
        {
            get => BuildAttributesStringValue();
            set => SetStoredStringValue(value);
        }

        #endregion

        #region [ Hierarchy Properties ]

        /// <summary>
        /// Родительский узел.
        /// </summary>
        [Display(Name = "Родитель", Description = "Родительский узел")]
        public TreeNodeModel ParentNode { get; }

        /// <summary>
        /// Родитель.
        /// </summary>
        [Display(Name = "Родитель", Description = "Родитель")]
        public IParentModel Parent => ParentNode;

        /// <summary>
        /// Все родители рекурсивно.
        /// </summary>
        [Display(Name = "Родители", Description = "Все родители рекурсивно")]
        public ReadOnlyDictionary<Guid, IOwnerModel> AllParentsRecursive
        {
            get => throw new NotImplementedException();
        }

        #endregion

        #region [ Ownership Properties ]

        #endregion

        #region [ Infrastructure Properties ]



        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="TreeLeaveModel" />.
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор.</param>
        /// <param name="parent">Родительский узел.</param>
        /// <param name="owner">Рабочее дерево, которому принадлежит лист.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="propertiesPolicy">Политика свойств.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        internal TreeLeaveModel(
            Guid uuid,
            TreeNodeModel parent,
            WorkingTreeModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<TreeLeaveModel> propertiesPolicy)
            : base(uuid, owner, notificationService, propertiesPolicy)
        {
            ArgumentNullException.ThrowIfNull(parent);

            ParentNode = parent;

            Parent.AddChild(this);
            OwningWorkingTree.ContentLeaves.Add(this);
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Сменить родителя.
        /// </summary>
        /// <param name="newParent">Новый родительский элемент.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool ChangeParent(IParentModel newParent)
        {
            ArgumentNullException.ThrowIfNull(newParent);

            return false;
        }

        /// <summary>
        /// Запрещает пользовательское добавление атрибутов к листу.
        /// </summary>
        /// <param name="attribute">Атрибут.</param>
        /// <returns>false.</returns>
        /// <remarks>Implements requirement R-6.01.</remarks>
        public override bool AddAttribute(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            SendAttributeCollectionRestriction("R-6.01", "добавление атрибута");
            return false;
        }

        /// <summary>
        /// Разрешает восстановление и распространение унаследованных атрибутов на лист.
        /// </summary>
        /// <param name="attribute">Унаследованный атрибут.</param>
        /// <returns>true, если унаследованный атрибут добавлен; иначе false.</returns>
        public override bool AddInheritedAttribute(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            return base.AddInheritedAttribute(attribute);
        }

        /// <summary>
        /// Запрещает пользовательское удаление атрибутов листа.
        /// </summary>
        /// <param name="attribute">Атрибут.</param>
        /// <returns>false.</returns>
        /// <remarks>Implements requirement R-6.01.</remarks>
        public override bool RemoveAttribute(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            SendAttributeCollectionRestriction("R-6.01", "удаление атрибута");
            return false;
        }

        /// <summary>
        /// Запрещает пользовательскую очистку атрибутов листа.
        /// </summary>
        /// <returns>false.</returns>
        /// <remarks>Implements requirement R-6.01.</remarks>
        public override bool ClearAttributes()
        {
            SendAttributeCollectionRestriction("R-6.01", "очистка атрибутов");
            return false;
        }

        /// <summary>
        /// Добавить содержимое.
        /// </summary>
        /// <param name="content">Содержимое.</param>
        /// <returns>true, если содержимое добавлено; иначе false.</returns>
        protected override bool AddContentDetailed(IContentModel content)
        {
            return true;
        }

        /// <summary>
        /// Удалить содержимое.
        /// </summary>
        /// <param name="content">Содержимое.</param>
        /// <returns>true, если содержимое удалено; иначе false.</returns>
        protected override bool RemoveContentDetailed(IContentModel content)
        {
            return true;
        }

        /// <summary>
        /// Очистить содержимое.
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        protected override bool ClearContentDetailed()
        {
            return true;
        }

        /// <summary>
        /// Приводит строковое значение к не-null и не-empty представлению для хранения.
        /// </summary>
        /// <param name="value">Исходное значение.</param>
        /// <returns>Нормализованное значение.</returns>
        protected static string NormalizeStringValue(string? value)
        {
            return string.IsNullOrEmpty(value)
                ? EmptyStringValue
                : value;
        }

        /// <summary>
        /// Возвращает явно сохраненное строковое значение.
        /// </summary>
        /// <remarks>
        /// Метод нужен наследникам, которые используют persisted-значение напрямую, а не вычисляют его из
        /// атрибутов пользовательского листа.
        /// </remarks>
        /// <returns>Сохраненное строковое значение.</returns>
        protected string GetStoredStringValue()
        {
            return GetValue(_stringValue);
        }

        /// <summary>
        /// Записывает строковое значение в backing field через общую политику свойств.
        /// </summary>
        /// <param name="value">Новое строковое значение.</param>
        /// <returns>true, если значение было изменено; иначе false.</returns>
        protected bool SetStoredStringValue(string? value)
        {
            return SetValue(ref _stringValue, NormalizeStringValue(value), nameof(StringValue));
        }

        /// <summary>
        /// Формирует строковое значение пользовательского листа из текущей коллекции атрибутов.
        /// </summary>
        /// <remarks>
        /// Пользовательский лист не редактирует <see cref="StringValue" /> напрямую: значение является JSON-словарем
        /// пар "название атрибута - значение атрибута". Системные листья переопределяют это поведение и хранят
        /// валидируемое значение базового типа.
        /// </remarks>
        /// <returns>JSON-представление атрибутов листа.</returns>
        private string BuildAttributesStringValue()
        {
            var attributes = Attributes?
                .ToDictionary(x => x.Name, GetAttributeStringValue)
                ?? new Dictionary<string, string>();

            return JsonSerializer.Serialize(attributes, _stringValueJsonOptions);
        }

        private static string GetAttributeStringValue(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (attribute.IsCollectionValue)
            {
                return string.Join(", ", attribute.Values.Select(x => x.Name));
            }

            return attribute.Value?.Name ?? string.Empty;
        }

        private void SendAttributeCollectionRestriction(string requirementCode, string operation)
        {
            _notificationService.SendTextMessage<TreeLeaveModel>(
                $"{requirementCode}: Для листа '{Name}' [{Uuid}] операция '{operation}' запрещена. " +
                "Список атрибутов листа не редактируется пользователем и наследуется с узла.",
                criticalLevel: NotificationCriticalLevelModel.Warning);
        }

        #endregion
    }
}
