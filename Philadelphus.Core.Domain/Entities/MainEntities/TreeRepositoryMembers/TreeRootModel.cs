using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers
{
    /// <summary>
    /// Корень дерева участников репозитория Чубушника (аналог проекта / библиотеки в .NET)
    /// </summary>
    public class TreeRootModel : TreeRepositoryMemberBaseModel, IHavingOwnDataStorageModel, IChildrenModel, IParentModel
    {
        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string DefaultFixedPartOfName { get => "Новый корень"; }

        /// <summary>
        /// Тип сущности (устар.)
        /// </summary>
        public override EntityTypesModel EntityType { get => EntityTypesModel.Root; }

        /// <summary>
        /// Собственное хранилище данных
        /// </summary>
        private IDataStorageModel _ownDataStorage;

        /// <summary>
        /// Собственное хранилище данных
        /// </summary>
        public IDataStorageModel OwnDataStorage
        {
            get
            {
                return _ownDataStorage;
            }
            private set
            {
                _ownDataStorage = value;
                DataStorages.Add(value);
            }
        }

        /// <summary>
        /// Хранилище данных дочерних элементов
        /// </summary>
        public override IDataStorageModel DataStorage { get => OwnDataStorage; }

        /// <summary>
        /// Коллекция допустимых хранилищ данных дочерних элементов
        /// </summary>
        public List<IDataStorageModel> DataStorages { get; internal set; } = new List<IDataStorageModel>();

        /// <summary>
        /// Тип элемента (устар.)
        /// </summary>
        public EntityElementTypeModel ElementType { get; set; }

        /// <summary>
        /// Непосредственный родитель (репозиторий)
        /// </summary>
        public IParentModel Parent {  get; private set; }

        /// <summary>
        /// Дочерние узлы репозитория Чубушника
        /// </summary>
        public List<TreeNodeModel> ChildTreeNodes { get => Childs.Where(x => x.GetType() == typeof(TreeNodeModel)).Cast<TreeNodeModel>().ToList(); }

        /// <summary>
        /// Дочерние элементы корня Чубушника (узлы)
        /// </summary>
        public List<IChildrenModel> Childs { get; set; }

        /// <summary>
        /// Корень дерева участников репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="parent">Непосредственный родитель</param>
        /// <param name="dataStorage">Хранилище данных</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal TreeRootModel(Guid uuid, TreeRepositoryModel parent, IDataStorageModel dataStorage, IMainEntity dbEntity) : base(uuid, parent, dbEntity)
        {
            if (SetParents(parent))
            {
                OwnDataStorage = dataStorage;
                Initialize();
            }
        }

        /// <summary>
        /// Инициализировать
        /// </summary>
        private void Initialize()
        {
            List<string> existNames = new List<string>();
            //foreach (var item in ParentRepository.ElementsCollection)
            //{
            //    existNames.Add(item.Name);
            //}
            //foreach (var child in Parent.Childs)
            //{
            //    existNames.Add(((IMainEntity)child).Name);
            //}
            Name = NamingHelper.GetNewName(existNames, DefaultFixedPartOfName);
            Childs = new List<IChildrenModel>();
            ElementType = new EntityElementTypeModel(Guid.NewGuid(), this, null);
        }

        /// <summary>
        /// Изменить хранилище данных
        /// </summary>
        /// <param name="storage">Новое хранилище</param>
        /// <returns></returns>
        public bool ChangeDataStorage(IDataStorageModel storage)
        {
            OwnDataStorage = storage;
            return true;
        }
    }
}
