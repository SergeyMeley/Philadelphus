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
        private List<ElementAttributeModel> _attributes;

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
                    UpdateStateStateAfterChange();
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
        public IOwnerModel Owner { get => OwningShrub; }

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
        public IReadOnlyList<ElementAttributeModel> Attributes 
        { 
            get
            {
                var attributes = _attributes?.Where(x => x.IsOwn).ToList();
                if (this is IChildrenModel c)
                {
                    if (c.Parent is IAttributeOwnerModel ao)
                    {
                        if (ao is IWorkingTreeMemberModel wtm)
                        {
                            foreach (var attribute in ao.GetVisibleAttributesRecursive(wtm))
                            {
                                if (attributes.Any(x => x.DeclaringUuid == attribute.DeclaringUuid) == false)   // Пропускаем атрибуты, которые уже унаследованы с ближайшегго родителя
                                {
                                    attributes.Add(attribute.CloneForChild(this));
                                }
                            }
                        }
                    }
                }
                _attributes = attributes;
                return attributes;
            }
        }

        /// <summary>
        /// Собственные атрибуты
        /// </summary>
        public IReadOnlyList<ElementAttributeModel> PersonalAttributes 
        { 
            get => _attributes?.Where(x => x.IsOwn).ToList(); 
        }

        /// <summary>
        /// Унаследованные атрибуты
        /// </summary>
        public IReadOnlyList<ElementAttributeModel> ParentElementAttributes
        {
            get => _attributes?.Where(x => x.IsOwn == false).ToList();
        }

        /// <summary>
        /// Имеет атрибуты
        /// </summary>
        public bool HasAttributes
        {
            get
            {
                if (Attributes.Count() > 0)
                    return true;
                if (PersonalAttributes.Count() > 0)
                    return true;
                if (ParentElementAttributes.Count() > 0)
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
            _attributes = new List<ElementAttributeModel>();
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Добавить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        public void AddAttribute(ElementAttributeModel attribute)
        {
            _attributes.Add(attribute);
         }

        /// <summary>
        /// Удалить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        public void RemoveAttribute(ElementAttributeModel attribute)
        {
            _attributes.Remove(attribute);
        }

        /// <summary>
        /// Очистить атрибуты
        /// </summary>
        public void ClearAttributes()
        {
            _attributes.Clear();
        }

        /// <summary>
        /// Получить видимые атрибуты родителей
        /// </summary>
        /// <param name="viewer">Текущий элемент</param>
        /// <returns></returns>
        public IEnumerable<ElementAttributeModel> GetVisibleAttributesRecursive(IWorkingTreeMemberModel? viewer)
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
            
            if (this is IChildrenModel c)
            {
                if (c.Parent is IAttributeOwnerModel ao)
                {
                    foreach (var item in ao.GetVisibleAttributesRecursive(viewer))
                    {
                        yield return item;
                    }
                 }
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
                if (viewer.OwningWorkingTree.Uuid == wtm.OwningWorkingTree.Uuid)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsDescendantOrSelf(IWorkingTreeMemberModel viewer)
        {
            if (viewer is IChildrenModel c)
            {
                if (c.AllParentsRecursive.ContainsKey(Uuid))
                {
                    return true;
                }
            }
            return IsSameNodeOrLeaf(viewer);
        }

        #endregion
    }
}
