using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers
{
    /// <summary>
    /// Участник репозитория Чубушника
    /// </summary>
    public abstract class ShrubMemberBaseModel : PhiladelphusRepositoryMemberBaseModel, IShrubMemberModel, IPhiladelphusRepositoryMemberModel, IAttributeOwnerModel, IOwnerModel, IContentModel, ISequencableModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый участник кустарника";

        private long _sequence;
        private List<ElementAttributeModel> _parentElementAttributes;

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Порядковый номер
        /// </summary>
        public long Sequence
        {
            get
            {
                return _sequence;
            }
            set
            {
                if (_sequence != value)
                {
                    _sequence = value;
                    UpdateStateAfterChange();
                }
            }
        }

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Кустарник рабочих деревьев
        /// </summary>
        public ShrubModel OwningShrub { get; }

        /// <summary>
        /// Владелец
        /// </summary>
        public IOwnerModel Owner { get; }

        /// <summary>
        /// Содержимое
        /// </summary>
        public abstract ReadOnlyDictionary<Guid, IContentModel> Content { get; }

        /// <summary>
        /// Все содержимое (рекурсивно)
        /// </summary>
        public virtual ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive 
        { 
            get => throw new NotImplementedException(); 
        }

        /// <summary>
        /// Атрибуты (собственные и унаследованные)
        /// </summary>
        public List<ElementAttributeModel> Attributes
        {
            get
            {
                return PersonalAttributes.Concat(ParentElementAttributes).ToList();
            }
        }

        /// <summary>
        /// Собственные атрибуты
        /// </summary>
        public List<ElementAttributeModel> PersonalAttributes { get; } = new List<ElementAttributeModel>();

        /// <summary>
        /// Унаследованные атрибуты
        /// </summary>
        public List<ElementAttributeModel> ParentElementAttributes
        {
            get
            {
                return _parentElementAttributes;
            }
            set
            {
                if (_parentElementAttributes != value)
                {
                    _parentElementAttributes = value;
                    UpdateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Имеет атрибуты
        /// </summary>
        public bool HasAttributes
        {
            get
            {
                if (Attributes?.Count > 0)
                    return true;
                if (PersonalAttributes?.Count > 0)
                    return true;
                if (ParentElementAttributes?.Count > 0)
                    return true;
                return false;
            }
        }


        #endregion

        #region [ Infrastructure Properties ]



        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Участник репозитория Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="dbEntity">Сущность БД</param>
        internal ShrubMemberBaseModel(
            Guid uuid,
            ShrubModel owner,
            IMainEntity dbEntity)
            : base(uuid, dbEntity, owner.OwningRepository)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            OwningShrub = owner;

            ParentElementAttributes = new List<ElementAttributeModel>();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Добавить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        public void AddAttribute(ElementAttributeModel attribute)
        {
            Attributes.Add(attribute);
         }

        /// <summary>
        /// Удалить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        public void RemoveAttribute(ElementAttributeModel attribute)
        {
            Attributes.Remove(attribute);
        }

        public IEnumerable<ElementAttributeModel> GetVisibleAttributes(IWorkingTreeMemberModel? viewer)
        {
            if (viewer == null) yield break;

            foreach (var attr in Attributes)
            {
                var res = attr.Visibility switch
                {
                    VisibilityScope.Public => true,
                    VisibilityScope.Private => IsSameNodeOrLeaf(viewer),
                    VisibilityScope.Internal => IsSameRoot(viewer),
                    VisibilityScope.Protected => IsDescendantOrSelf(viewer),
                    VisibilityScope.InternalProtected => IsSameRoot(viewer) || IsDescendantOrSelf(viewer),
                    _ => false
                };

                if (res)
                    yield return attr;
            }
        }

        private bool IsSameNodeOrLeaf(IWorkingTreeMemberModel viewer)
        {
            if (viewer is TreeNodeModel n
                && n.Uuid == Uuid)
            {
                return true;
            }
            if (viewer is TreeLeaveModel l
                && l.ParentNode.Uuid == Uuid)
            {
                return true;
            }
            return false;
        }

        private bool IsSameRoot(IWorkingTreeMemberModel viewer)
        {
            if (this is IWorkingTreeMemberModel wtm)
            {
                if (viewer.OwningWorkingTree.Uuid == wtm.Uuid)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsDescendantOrSelf(IWorkingTreeMemberModel viewer)
        {
            //var current = this;
            //while (current != null)
            //{
            //    if (current.Id == viewer.Id) return true;
            //    // Подняться к родителю (требует ссылки на родителя или рекурсия по дереву)
            //    current = OwningShrub.ContentTrees.SelectMany(x => x.AllContentRecursive);
            //}
            return false;
        }

        #endregion
    }
}
