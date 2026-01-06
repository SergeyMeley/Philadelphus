using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;

namespace Philadelphus.Core.Domain.Entities.MainEntities.TreeRepositoryMembers.TreeRootMembers
{
    /// <summary>
    /// Участник корня репозитория Чубушника
    /// </summary>
    public abstract class TreeRootMemberBaseModel : TreeRepositoryMemberBaseModel, ITreeRootMemberModel
    {
        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string DefaultFixedPartOfName { get => "Новый член узла"; }

        /// <summary>
        /// Родительский корень репозитория Чубушника
        /// </summary>
        public TreeRootModel ParentRoot { get; protected set; }

        /// <summary>
        /// Тип сущности (устар.)
        /// </summary>
        public override EntityTypesModel EntityType => throw new NotImplementedException();

        /// <summary>
        /// Участник корня репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="parent">Родительский элемент Чубушника</param>
        /// <param name="dbEntity">Сущность БД</param>
        public TreeRootMemberBaseModel(Guid uuid, IParentModel parent, IMainEntity dbEntity) : base(uuid, parent, dbEntity)
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
            if (base.SetParents(parent) == false)
                return false;
            if (parent is TreeRootModel)
            {
                ParentRoot = (TreeRootModel)parent;
                return true;
            }
            else if (parent is TreeRootMemberBaseModel)
            {
                ParentRoot = ((TreeRootMemberBaseModel)parent).ParentRoot;
                return true;
            }
            return false;
        }
    }
}
