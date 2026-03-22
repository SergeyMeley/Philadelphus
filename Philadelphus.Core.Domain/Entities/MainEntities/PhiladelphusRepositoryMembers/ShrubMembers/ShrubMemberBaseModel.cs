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

        private List<ElementAttributeModel> _attributes = new List<ElementAttributeModel>();
        private IReadOnlyList<ElementAttributeModel> _cachedAttributes;

        private long _sequence;

        private object _lockObject = new();
        private int _version = 0;
        private int _cachedVersion = -1;

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
                if (_cachedVersion != _version)
                {
                    lock (_lockObject)
                    {
                        var result = GetActualAttributesList();
                        _cachedAttributes = result;
                        _cachedVersion = _version;
                    }
                }
                return _cachedAttributes;
            }
        }

        private IReadOnlyList<ElementAttributeModel> GetActualAttributesList()
        {
            lock (_lockObject)
            {
                var attributes = _attributes?.Where(x => x.IsOwn).ToList();

                var oldParentAttributes = _attributes?.Where(x => x.IsOwn == false).ToList();

                var newParentAttributes = new List<ElementAttributeModel>();
                if (this is IChildrenModel c)
                {
                    if (c.Parent is IAttributeOwnerModel ao)
                    {
                        if (ao is IWorkingTreeMemberModel wtm)
                        {
                            var allCurrentParentAttributes = ao.GetVisibleAttributesRecursive(wtm).ToList();
                            foreach (var attribute in allCurrentParentAttributes)
                            {
                                if (newParentAttributes.Any(x => x.DeclaringUuid == attribute.DeclaringUuid) == false)   // Пропускаем атрибуты, которые уже унаследованы с ближайшегго родителя
                                {
                                    var oldParentAttribute = oldParentAttributes.SingleOrDefault(x => x.DeclaringUuid == attribute.DeclaringUuid);
                                    if (oldParentAttribute != null)
                                    {
                                        newParentAttributes.Add(oldParentAttribute);
                                    }
                                    else
                                    {
                                        newParentAttributes.Add(attribute.CloneForChild(this));
                                    }
                                }
                            }
                        }
                    }
                }

                attributes.AddRange(newParentAttributes);

                return attributes.AsReadOnly();
            }
        }

        /// <summary>
        /// Собственные атрибуты
        /// </summary>
        public IReadOnlyList<ElementAttributeModel> PersonalAttributes 
        { 
            get => Attributes?.Where(x => x.IsOwn).ToList(); 
        }

        /// <summary>
        /// Унаследованные атрибуты
        /// </summary>
        public IReadOnlyList<ElementAttributeModel> ParentElementAttributes
        {
            get => Attributes?.Where(x => x.IsOwn == false).ToList();
        }

        /// <summary>
        /// Имеет атрибуты
        /// </summary>
        public bool HasAttributes
        {
            get
            {
                return _attributes.Any();
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
            ShrubModel owner)
            : base(uuid, owner.OwningRepository)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));

            OwningShrub = owner;
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Добавить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        public bool AddAttribute(ElementAttributeModel attribute)
        {
            lock (_lockObject)
            {
                if (_attributes.Any(x => x.Uuid == attribute.Uuid))
                    return false;

                _attributes.Add(attribute);
                _version++;
                return true;
            }
         }

        /// <summary>
        /// Удалить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        public bool RemoveAttribute(ElementAttributeModel attribute)
        {
            lock (_lockObject)
            {
                if (_attributes.Any(x => x.Uuid == attribute.Uuid))
                    return false;

                _attributes.Remove(attribute);
                _version++;
                return true;
            }
        }

        /// <summary>
        /// Очистить атрибуты
        /// </summary>
        public bool ClearAttributes()
        {
            lock (_lockObject)
            {
                _attributes.Clear();
                _version++;
                return true;
            }
        }

        /// <summary>
        /// Получить видимые атрибуты родителей
        /// </summary>
        /// <param name="viewer">Текущий элемент</param>
        /// <returns></returns>
        public IEnumerable<ElementAttributeModel> GetVisibleAttributesRecursive(IWorkingTreeMemberModel? viewer)
        {
            if (viewer == null) yield break;

            var allAttributes = Attributes; // Получаем один раз
            foreach (var attr in allAttributes)
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
