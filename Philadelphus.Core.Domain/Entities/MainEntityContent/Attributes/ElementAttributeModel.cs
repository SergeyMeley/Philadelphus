using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes
{
    /// <summary>
    /// Атрибут элемента Чубушника
    /// </summary>
    public class ElementAttributeModel : MainEntityBaseModel, ITreeElementContentModel
    {
        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string DefaultFixedPartOfName { get => "Новый атрибут"; }

        /// <summary>
        /// Тип сущности (устар.)
        /// </summary>
        public override EntityTypesModel EntityType { get => EntityTypesModel.Attribute; }

        /// <summary>
        /// Владелец атрибута
        /// </summary>
        public IAttributeOwnerModel Owner { get; set; }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public override IDataStorageModel DataStorage { get => Owner.DataStorage; }

        /// <summary>
        /// Тип данных (узел дерева репозитория Чубушника)
        /// </summary>
        public TreeNodeModel ValueType { get; set; }

        /// <summary>
        /// Коллекция типов данных (узлы дерева репозитория Чубушника)
        /// </summary>
        public IEnumerable<TreeNodeModel>? ValueTypesList { get; set; }

        /// <summary>
        /// Значение (лист выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public TreeLeaveModel Value { get; set; }

        /// <summary>
        /// Коллекция допустимых значений (листы выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public IEnumerable<TreeLeaveModel>? ValuesList { get; set; }

        /// <summary>
        /// Атрибут элемента Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="owner">Владелец атрибута</param>
        /// <param name="dbEntity">Сущность БД</param>
        public ElementAttributeModel(Guid uuid, IAttributeOwnerModel owner, IMainEntity dbEntity) : base(uuid, dbEntity)
        {
            Owner = owner;
            Initialize();
        }

        /// <summary>
        /// Инициализировать
        /// </summary>
        private void Initialize()
        {
            Name = NamingHelper.GetNewName(new List<string>(), DefaultFixedPartOfName);
        }
    }
}
