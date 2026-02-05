using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Properties;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers
{
    /// <summary>
    /// Лист дерева участников репозитория Чубушника (аналог объекта в ООП)
    /// </summary>
    public class TreeLeaveModel : TreeRootMemberBaseModel, IChildrenModel, ITreeRootMemberModel
    {
        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string DefaultFixedPartOfName { get => "Новый лист"; }

        /// <summary>
        /// Тип сущности (устар.)
        /// </summary>
        public override EntityTypesModel EntityType { get => EntityTypesModel.Leave; }

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public override IDataStorageModel DataStorage { get => ParentRoot.OwnDataStorage; }

        /// <summary>
        /// Лист репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="parent">Родительский узел Чубушника</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal TreeLeaveModel(Guid uuid, TreeNodeModel parent, IMainEntity dbEntity) : base(uuid, parent, dbEntity)
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
            //foreach (var child in Parent.Childs)
            //{
            //    existNames.Add(((IMainEntity)child).Name);
            //}
            Name = NamingHelper.GetNewName(existNames, DefaultFixedPartOfName);
            //Childs = new ObservableCollection<IChildren>();
        }
    }
}
