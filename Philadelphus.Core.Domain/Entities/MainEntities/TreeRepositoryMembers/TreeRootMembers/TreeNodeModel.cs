using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers
{
    /// <summary>
    /// Узел дерева участников репозитория Чубушника (аналог классов и интерфейсов в ООП)
    /// </summary>
    public class TreeNodeModel : TreeRootMemberBaseModel, IParentModel, ITreeRootMemberModel
    {
        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string DefaultFixedPartOfName { get => "Новый узел"; }

        /// <summary>
        /// Тип сущности (устар.)
        /// </summary>
        public override EntityTypesModel EntityType { get => EntityTypesModel.Node; }

        /// <summary>
        /// Дочерние узлы
        /// </summary>
        public List<TreeNodeModel> ChildTreeNodes { get => Childs.Where(x => x.GetType() == typeof(TreeNodeModel)).Cast<TreeNodeModel>().ToList(); }

        /// <summary>
        /// Дочерние листы
        /// </summary>
        public List<TreeLeaveModel> ChildTreeLeaves { get => Childs.Where(x => x.GetType() == typeof(TreeLeaveModel)).Cast<TreeLeaveModel>().ToList(); }

        /// <summary>
        /// Дочерние элементы (узлы и листы)
        /// </summary>
        public List<IChildrenModel> Childs { get; set; } = new List<IChildrenModel>();

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public override IDataStorageModel DataStorage { get => ParentRoot.OwnDataStorage; }

        /// <summary>
        /// Узел репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="parent">Родительский узел или корень Чубушника</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal TreeNodeModel(Guid uuid, IParentModel parent, IMainEntity dbEntity) : base(uuid, parent, dbEntity)
        {
            if (SetParents(parent))
            {
                Initialize();
            }
        }

        /// <summary>
        /// Инициализировать
        /// </summary>
        private void Initialize()
        {
            List<string> existNames = new List<string>();
            foreach (var item in ParentRepository.ElementsCollection)
            {
                existNames.Add(item.Name);
            }
            Name = NamingHelper.GetNewName(existNames, DefaultFixedPartOfName);
            Childs = new List<IChildrenModel>();
        }
    }
}
