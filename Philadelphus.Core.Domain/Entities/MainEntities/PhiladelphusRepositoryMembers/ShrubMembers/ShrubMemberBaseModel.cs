using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.LeavePolymorphism;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers
{
    /// <summary>
    /// Участник репозитория Чубушника
    /// </summary>
    public abstract class ShrubMemberBaseModel<T> : PhiladelphusRepositoryMemberBaseModel<T>, IShrubMemberModel, IPhiladelphusRepositoryMemberModel, IAttributeOwnerModel, IOwnerModel, IContentModel, ISequencableModel
        where T : ShrubMemberBaseModel<T>
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый участник кустарника";

        private List<ElementAttributeModel> _attributes = new List<ElementAttributeModel>();
        private IReadOnlyList<ElementAttributeModel> _cachedAttributes;

        protected long _sequence;

        private object _lockObject = new();
        private int _version = 0;
        private int _cachedVersion = -1;
        private int _attributesListRecalculationSuspendCount;

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Порядковый номер
        /// </summary>
        [Display(Name = "[№]", Description = "Порядковый номер")]
        public long Sequence
        {
            get => GetValue(_sequence);
            set => SetValue(ref _sequence, value);
        }

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Кустарник рабочих деревьев
        /// </summary>
        [Display(Name = "[Кустарник]", Description = "Владеющий кустарник")]
        public ShrubModel OwningShrub { get; }

        /// <summary>
        /// Содержимое
        /// </summary>
        [Display(Name = "[Содержимое]", Description = "Содержимое")]
        public abstract ReadOnlyDictionary<Guid, IContentModel> Content { get; }

        /// <summary>
        /// Все содержимое (рекурсивно)
        /// </summary>
        [Display(Name = "[Все содержимое]", Description = "Все содержимое")]
        public virtual ReadOnlyDictionary<Guid, IContentModel> AllContentRecursive 
        { 
            get => RecursiveRelationshipHelper.ToReadOnlyDictionary(
                RecursiveRelationshipHelper.EnumerateContentRecursive(this)); 
        }

        /// <summary>
        /// Атрибуты (собственные и унаследованные)
        /// </summary>
        [Display(Name = "[Атрибуты]", Description = "Атрибуты (собственные и унаследованные)")]
        public IReadOnlyList<ElementAttributeModel> Attributes 
        {
            get
            {
                if (_attributesListRecalculationSuspendCount > 0)
                {
                    return _attributes.AsReadOnly();
                }

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
                EnsureRuntimeAttributes();

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
                                // Системные узлы и их листья не участвуют в пользовательском полиморфизме.
                                if (attribute is LeavePolymorphismAttributeModel
                                    && (this is SystemBaseTreeNodeModel
                                        || this is TreeLeaveModel
                                        {
                                            ParentNode: SystemBaseTreeNodeModel
                                        }))
                                {
                                    continue;
                                }

                                if (newParentAttributes.Any(x => x.DeclaringUuid == attribute.DeclaringUuid) == false)   // Пропускаем атрибуты, которые уже унаследованы с ближайшегго родителя
                                {
                                    var oldParentAttribute = oldParentAttributes.SingleOrDefault(x => x.DeclaringUuid == attribute.DeclaringUuid);
                                    if (oldParentAttribute != null)
                                    {
                                        newParentAttributes.Add(oldParentAttribute);
                                    }
                                    else
                                    {
                                        var inheritedAttribute = attribute.CloneForChild(this);
                                        inheritedAttribute.AssignInheritedAutoSequence(
                                            attributes
                                                .Concat(oldParentAttributes)
                                                .Concat(newParentAttributes)
                                                .Select(x => x.Sequence));
                                        newParentAttributes.Add(inheritedAttribute);
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
        /// Создаёт отсутствующие вычисляемые атрибуты перед обновлением кеша.
        /// </summary>
        /// <remarks>
        /// Базовые сущности не имеют runtime-атрибутов; специализированные владельцы
        /// переопределяют метод и добавляют их в общую коллекцию атрибутов.
        /// </remarks>
        protected virtual void EnsureRuntimeAttributes()
        {
        }

        /// <summary>
        /// Собственные атрибуты
        /// </summary>
        [Display(Name = "[Собственные атрибуты]", Description = "Собственные атрибуты")]
        public IReadOnlyList<ElementAttributeModel> PersonalAttributes 
        { 
            get => Attributes?.Where(x => x.IsOwn).ToList(); 
        }

        /// <summary>
        /// Унаследованные атрибуты
        /// </summary>
        [Display(Name = "[Унаследованные атрибуты]", Description = "Унаследованные атрибуты")]
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
        internal ShrubMemberBaseModel(
            Guid uuid,
            IOwnerModel owner,
            INotificationService notificationService,
            IPropertiesPolicy<T> propertiesPolicy)
            : base(uuid, owner, notificationService, propertiesPolicy)
        {
            ArgumentNullException.ThrowIfNull(owner);

            if (owner is ShrubModel sh)
            {
                OwningShrub = sh;
            }
            else if (owner is IShrubMemberModel shm)
            {
                OwningShrub = shm.OwningShrub;
            }
            else 
            {
                throw new ArgumentException();
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Добавить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public virtual bool AddAttribute(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (attribute.IsOwn == false)
                return false;

            return AddAttributeCore(attribute);
        }

        /// <summary>
        /// Добавить унаследованный атрибут.
        /// </summary>
        /// <param name="attribute">Унаследованный атрибут.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public virtual bool AddInheritedAttribute(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            if (attribute.IsOwn)
                return false;

            return AddAttributeCore(attribute);
        }

        private bool AddAttributeCore(ElementAttributeModel attribute)
        {
            lock (_lockObject)
            {
                if (_attributes.Any(x => x.Uuid == attribute.Uuid))
                    return false;

                _attributes.Add(attribute);
                ((IAttributeOwnerModel)this).MarkAsNeedRecalculateAttributesList();
                return true;
            }
        }

        void IAttributeOwnerModel.MarkAsNeedRecalculateAttributesList()
        {
            _version++;
            if (_attributesListRecalculationSuspendCount > 0)
            {
                return;
            }

            if (this is IParentModel p)
            {
                foreach (var c in p.Childs)
                {
                    if (c.Value is IAttributeOwnerModel ao)
                    {
                        ao.MarkAsNeedRecalculateAttributesList();
                    }
                }
            }
        }

        void IAttributeOwnerModel.SuspendAttributesListRecalculation()
        {
            _attributesListRecalculationSuspendCount++;
        }

        void IAttributeOwnerModel.ResumeAttributesListRecalculation()
        {
            if (_attributesListRecalculationSuspendCount <= 0)
            {
                return;
            }

            _attributesListRecalculationSuspendCount--;
            if (_attributesListRecalculationSuspendCount == 0)
            {
                ((IAttributeOwnerModel)this).MarkAsNeedRecalculateAttributesList();
            }
        }

        /// <summary>
        /// Удалить атрибут
        /// </summary>
        /// <param name="attribute">Атрибут</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public virtual bool RemoveAttribute(ElementAttributeModel attribute)
        {
            ArgumentNullException.ThrowIfNull(attribute);

            lock (_lockObject)
            {
                var remItem = _attributes.FirstOrDefault(x => x.Uuid == attribute.Uuid);
                if (remItem == null)
                    return false;

                _attributes.Remove(remItem);
                ((IAttributeOwnerModel)this).MarkAsNeedRecalculateAttributesList();
                return true;
            }
        }

        /// <summary>
        /// Очистить атрибуты
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public virtual bool ClearAttributes()
        {
            lock (_lockObject)
            {
                _attributes.Clear();
                ((IAttributeOwnerModel)this).MarkAsNeedRecalculateAttributesList();
                return true;
            }
        }

        /// <summary>
        /// Присвоить автоматически сгенерированный порядковый номер.
        /// </summary>
        /// <param name="existSequences">Занятые порядковые номера.</param>
        /// <returns>Присвоенный порядковый номер.</returns>
        public long AssignAutoSequence(IEnumerable<long>? existSequences = null)
        {
            foreach (var sequence in SequenceHelper.GetNewSequences(existSequences ?? Enumerable.Empty<long>()))
            {
                Sequence = sequence;
                if (Sequence == sequence)
                {
                    return Sequence;
                }
            }

            throw new InvalidOperationException("Не удалось присвоить свободный порядковый номер.");
        }

        /// <summary>
        /// Получить видимые атрибуты родителей
        /// </summary>
        /// <param name="viewer">Текущий элемент</param>
        /// <returns>Результат выполнения операции.</returns>
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

        /// <summary>
        /// Добавить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool AddContent(IContentModel content)
        {
            ArgumentNullException.ThrowIfNull(content);

            if (content is ElementAttributeModel a)
            {
                return AddAttribute(a);
            }
            return AddContentDetailed(content);
        }

        /// <summary>
        /// Добавить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        protected abstract bool AddContentDetailed(IContentModel content);

        /// <summary>
        /// Удалить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool RemoveContent(IContentModel content)
        {
            ArgumentNullException.ThrowIfNull(content);

            if (content is ElementAttributeModel a)
            {
                return RemoveAttribute(a);
            }
            return RemoveContentDetailed(content);
        }

        /// <summary>
        /// Удалить содержимое
        /// </summary>
        /// <param name="content">Содержимое</param>
        protected abstract bool RemoveContentDetailed(IContentModel content);

        /// <summary>
        /// Очистить содержимое
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool ClearContent()
        {
            ClearAttributes();
            ClearContentDetailed();
            return true;
        }

        /// <summary>
        /// Очистить содержимое
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        protected abstract bool ClearContentDetailed();

        #endregion
    }
}
