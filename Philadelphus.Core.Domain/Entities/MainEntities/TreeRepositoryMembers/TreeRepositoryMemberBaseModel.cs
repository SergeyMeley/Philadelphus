using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers
{
    /// <summary>
    /// Участник репозитория Чубушника
    /// </summary>
    public abstract class TreeRepositoryMemberBaseModel : MainEntityBaseModel, ITreeRepositoryMemberModel, IAttributeOwnerModel, IChildrenModel, ISequencableModel
    {
        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string DefaultFixedPartOfName { get => "Новый член репозитория"; }

        /// <summary>
        /// Родитель элемента
        /// </summary>
        public IParentModel Parent { get; protected set; }

        /// <summary>
        /// Родительский репозиторий
        /// </summary>
        public TreeRepositoryModel ParentRepository { get; protected set; }

        /// <summary>
        /// Порядковый номер
        /// </summary>
        public long Sequence { get; set; }

        /// <summary>
        /// Атрибуты (собственные и унаследованные)
        /// </summary>
        public List<ElementAttributeModel> Attributes 
        { get
            {
                return PersonalAttributes.Concat(ParentElementAttributes).ToList();
            }
        }

        /// <summary>
        /// Собственные атрибуты
        /// </summary>
        public List<ElementAttributeModel> PersonalAttributes { get; set; } = new List<ElementAttributeModel>();

        /// <summary>
        /// Унаследованные атрибуты
        /// </summary>
        public List<ElementAttributeModel> ParentElementAttributes { get; set; } = new List<ElementAttributeModel>();

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public override IDataStorageModel DataStorage { get; }

        /// <summary>
        /// Участник репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="parent">Родительский элемент Чубушника</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal TreeRepositoryMemberBaseModel(Guid uuid, IParentModel parent, IMainEntity dbEntity) : base(uuid, dbEntity)
        {
            SetParents(parent);
        }

        /// <summary>
        /// Назначить родителей
        /// </summary>
        /// <param name="parent">Непосредственный родитель</param>
        /// <returns></returns>
        protected bool SetParents(IParentModel parent)
        {
            if (parent == null)
                return false;

            Parent = parent;

            if (parent is TreeRepositoryModel)
            {
                ParentRepository = (TreeRepositoryModel)parent;
                return true;
            }
            else if (parent is TreeRepositoryMemberBaseModel)
            {
                ParentRepository = ((TreeRepositoryMemberBaseModel)parent).ParentRepository;
                return true;
            }

            return false;
        }
    }
}
