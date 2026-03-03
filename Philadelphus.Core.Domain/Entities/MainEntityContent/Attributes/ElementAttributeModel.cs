using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;

namespace Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes
{
    /// <summary>
    /// Атрибут элемента Чубушника
    /// </summary>
    public class ElementAttributeModel : MainEntityBaseModel, IContentModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый атрибут";

        private bool _isCollectionValue = false;
        private TreeNodeModel _valueType;
        private TreeLeaveModel _value;
        private List<TreeLeaveModel> _values = new List<TreeLeaveModel>();
        private VisibilityScope _visibility = VisibilityScope.Public;
        private OverrideType _override = OverrideType.Virtual;

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Тип данных (узел дерева репозитория Чубушника)
        /// </summary>
        public TreeNodeModel ValueType
        {
            get
            {
                return _valueType;
            }
            set
            {
                if (_valueType != value)
                {
                    _valueType = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Коллекция типов данных (узлы дерева репозитория Чубушника)
        /// </summary>
        public IEnumerable<TreeNodeModel>? ValueTypesList 
        { 
            get
            {
                if (Owner is ShrubMemberBaseModel sm)
                {
                    return sm.OwningShrub.ContentTrees
                        .SelectMany(x => x.GetAllNodesRecursive());
                }
                throw new Exception("Владелец атрибута не является элементом репозитория. Непредвиденная ситуация, обратитесь к разработчику.");
            }
        }

        /// <summary>
        /// Допускается коллекция значений
        /// </summary>
        public bool IsCollectionValue 
        { 
            get
            {
                return _isCollectionValue; 
            }
            set
            {
                if (_isCollectionValue != value)
                {
                    _isCollectionValue = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Значение (лист выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public TreeLeaveModel Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Значения (листы выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public IReadOnlyList<TreeLeaveModel> Values 
        { 
            get
            {
                return _values.AsReadOnly();
            }
        }

        /// <summary>
        /// Коллекция допустимых значений (листы выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public IEnumerable<TreeLeaveModel>? ValuesList
        {
            get
            {
                return ValueType?.ChildLeaves;
                
                // TODO: Подумать про использование листов наследников
                //if (Owner is ShrubMemberBaseModel sm)
                //{
                //    return sm.OwningShrub.ContentTrees
                //        .SelectMany(x => x.GetAllLeavesRecursive())
                //        .Where(x => x.ParentNode.Uuid == ValueType?.Uuid);
                //}
                //throw new Exception("Владелец атрибута не является элементом репозитория. Непредвиденная ситуация, обратитесь к разработчику.");
            }
        }

        /// <summary>
        /// Область видимости
        /// </summary>
        public VisibilityScope Visibility
        {
            get
            {
                return _visibility;
            }
            set
            {
                if (_visibility != value)
                {
                    _visibility = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        /// <summary>
        /// Возможность переопределения
        /// </summary>
        public OverrideType Override
        {
            get
            {
                return _override;
            }
            set
            {
                if (_override != value)
                {
                    _override = value;
                    UpdateStateStateAfterChange();
                }
            }
        }

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Владелец
        /// </summary>
        public IOwnerModel Owner { get; }

        /// <summary>
        /// Объявивший владелец, с которого унаследован атрибут
        /// </summary>
        public IOwnerModel DeclaringOwner { get; }

        /// <summary>
        /// Уникальный идентификатор в рамках текущего владельца
        /// </summary>
        public Guid LocalUuid => Uuid;

        /// <summary>
        /// Уникальный идентификатор в рамках объяввшего владельца
        /// </summary>
        public Guid DeclaringUuid { get; }

        /// <summary>
        /// Глубина наследования
        /// </summary>
        public int InheritanceDepth 
        { 
            get
            {
                int depth = -1;
                if (IsOwn)
                {
                    depth = 0;
                }
                else
                {
                    if (Owner is IChildrenModel c)
                    {
                        if (c.Parent is IAttributeOwnerModel ao)
                        {
                            var originalAttribute = ao.Attributes.SingleOrDefault(x => x.DeclaringUuid == DeclaringUuid);
                            if (originalAttribute != null)
                            {
                                depth = originalAttribute.InheritanceDepth + 1;
                            }
                        }
                    }
                }
                return depth;
            }
        }

        /// <summary>
        /// Персональный атрибут, созданный в рамках текущего владельца, а не унаследованный
        /// </summary>
        public bool IsOwn 
        { 
            get => LocalUuid == DeclaringUuid
                && Owner.Uuid == DeclaringOwner.Uuid;
        }

        /// <summary>
        /// Все владельцы (рекурсивно)
        /// </summary>
        public ReadOnlyDictionary<Guid, IOwnerModel> AllOwnersRecursive 
        { 
            get => throw new NotImplementedException(); 
        }

        #endregion

        #region [ Infrastructure Properties ]

        /// <summary>
        /// Хранилище данных
        /// </summary>
        public override IDataStorageModel DataStorage
        {
            get
            {
                if (Owner is IMainEntityModel m)
                {
                    return m.DataStorage;
                }
                return null;
            }
        }

        #endregion

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Атрибут элемента Чубушника
        /// </summary>
        /// <param name="uuid">Уникальный идентификатор</param>
        /// <param name="owner">Владелец атрибута</param>
        /// <param name="dbEntity">Сущность БД</param>
        public ElementAttributeModel(
            Guid localUuid, 
            IOwnerModel localOwner, 
            Guid declaringUuid,
            IOwnerModel declaringOwner,
            IMainEntity dbEntity) 
            : base(localUuid, dbEntity)
        {
            ArgumentNullException.ThrowIfNull(localOwner, nameof(localOwner));
            ArgumentNullException.ThrowIfNull(declaringOwner, nameof(declaringOwner));

            if (declaringUuid == Guid.Empty)
                throw new ArgumentException(nameof(declaringUuid));

            Owner = localOwner;
            DeclaringUuid = declaringUuid;
            DeclaringOwner = declaringOwner;

            if (Owner is IWorkingTreeMemberModel wtm)
            {
                Name = NamingHelper.GetNewName(wtm.OwningWorkingTree.UnavailableNames, _defaultFixedPartOfName);
                wtm.OwningWorkingTree.UnavailableNames.Add(Name);
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Сменить владельца
        /// </summary>
        public bool ChangeOwner(IOwnerModel newOwner)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Добавить значение атрибута в коллекцию
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns></returns>
        public bool TryAddValueToValuesCollection(TreeLeaveModel value)
        {
            if (_isCollectionValue == false)
                return false; 
            if (_values != null && _values.Any(x => x.Uuid == value.Uuid))
                return false;
            _values?.Add(value);
            UpdateStateStateAfterChange();
            return true;
        }

        /// <summary>
        /// Исключить значение атрибута из коллекции
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns></returns>
        public bool TryRemoveValueFromValuesCollection(TreeLeaveModel value)
        {
            if (_isCollectionValue == false)
                return false;
            if (_values != null && _values.Any(x => x == value) == false)
                return false;
            _values?.Remove(value);
            UpdateStateStateAfterChange(); 
            return true;
        }

        /// <summary>
        /// Очистить коллекцию значений атрибута
        /// </summary>
        /// <returns></returns>
        public bool ClearValuesCollection()
        {
            if (_isCollectionValue == false)
                return false;
            if (_values != null)
                return false;
            _values?.Clear();
            UpdateStateStateAfterChange(); 
            return true;
        }

        /// <summary>
        /// Получить копию для наследника
        /// </summary>
        /// <param name="newOwner"></param>
        /// <returns></returns>
        public ElementAttributeModel CloneForChild(IOwnerModel newOwner)
        {
            return new ElementAttributeModel(
                localUuid: Guid.NewGuid(),
                localOwner: newOwner,
                declaringUuid: this.DeclaringUuid,
                declaringOwner: this.DeclaringOwner,
                dbEntity: this.DbEntity
            )
            {
                Name = this.Name,
                Description = this.Description,
                ValueType = this.ValueType,
                IsCollectionValue = this.IsCollectionValue, 
                Value = this.Value, 
                _values = new List<TreeLeaveModel>(this._values), 
                Visibility = this.Visibility,
                Override = this.Override
            };
        }

        #endregion
    }
}
