using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Attributes.Builders;
using Philadelphus.Core.Domain.Policies.Attributes.Rules;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntityContent.Attributes;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes
{
    /// <summary>
    /// Атрибут элемента Чубушника
    /// </summary>
    public class ElementAttributeModel : WorkingTreeMemberBaseModel<ElementAttributeModel>, IContentModel
    {
        #region [ Fields ]

        /// <summary>
        /// Фиксированная часть наименования по умолчанию
        /// </summary>
        protected override string _defaultFixedPartOfName => "Новый атрибут";

        private readonly IAttributeOwnerModel _attributeOwner;
        private readonly Guid _declaringUuid;
        private readonly IAttributeOwnerModel _declaringAttributeOwner;
        private readonly bool _isOwn;
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
            get => GetValue(_valueType);
            set => SetValue(ref _valueType, value);
        }

        /// <summary>
        /// Коллекция типов данных (узлы дерева репозитория Чубушника)
        /// </summary>
        public IEnumerable<TreeNodeModel>? ValueTypesList 
        { 
            get
            {
                if (Owner is IShrubMemberModel sm)
                {
                    return sm.OwningShrub.ContentWorkingTrees
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
            get => GetValue(_isCollectionValue);
            set => SetValue(ref _isCollectionValue, value);
        }

        /// <summary>
        /// Значение (лист выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public TreeLeaveModel Value
        {
            get => GetValue(_value);
            set => SetValue(ref _value, value);
        }

        /// <summary>
        /// Значения (листы выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public IReadOnlyList<TreeLeaveModel> Values
        {
            get => GetValue(_values.AsReadOnly());
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
            get => GetValue(_visibility);
            set => SetValue(ref _visibility, value);
        }

        /// <summary>
        /// Возможность переопределения
        /// </summary>
        public OverrideType Override
        {
            get => GetValue(_override);
            set => SetValue(ref _override, value);
        }

        #endregion

        #region [ Hierarchy Properties ]



        #endregion

        #region [ Ownership Properties ]

        /// <summary>
        /// Объявивший владелец, с которого унаследован атрибут
        /// </summary>
        public IOwnerModel DeclaringOwner { get => _declaringAttributeOwner; }

        /// <summary>
        /// Уникальный идентификатор в рамках текущего владельца
        /// </summary>
        public Guid LocalUuid => Uuid;

        /// <summary>
        /// Уникальный идентификатор в рамках объяввшего владельца
        /// </summary>
        public Guid DeclaringUuid
        {
            get => GetValue(_declaringUuid);
        }

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
                            var originalAttribute = ao.Attributes?.SingleOrDefault(x => x.DeclaringUuid == DeclaringUuid);
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
            get => GetValue(_isOwn);
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
            IAttributeOwnerModel localOwner, 
            Guid declaringUuid,
            IAttributeOwnerModel declaringOwner,
            WorkingTreeModel owningWorkingTree,
            INotificationService notificationService,
            IPropertiesPolicy<ElementAttributeModel> propertiesPolicy) 
            : base(localUuid, localOwner, notificationService, propertiesPolicy)
        {
            ArgumentNullException.ThrowIfNull(localOwner, nameof(localOwner));
            ArgumentNullException.ThrowIfNull(declaringOwner, nameof(declaringOwner));

            if (declaringUuid == Guid.Empty)
                throw new ArgumentException(nameof(declaringUuid));

            _attributeOwner = localOwner;
            _declaringUuid = declaringUuid;
            _declaringAttributeOwner = declaringOwner;

            _isOwn = localUuid == _declaringUuid
                && _attributeOwner.Uuid == _declaringAttributeOwner.Uuid;
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
        public ElementAttributeModel CloneForChild(IAttributeOwnerModel newOwner)
        {
            var result = new ElementAttributeModel(
                localUuid: Guid.CreateVersion7(),
                localOwner: newOwner,
                declaringUuid: this.DeclaringUuid,
                declaringOwner: this._declaringAttributeOwner,
                owningWorkingTree: this.OwningWorkingTree,
                notificationService: _notificationService,
                propertiesPolicy: _propertiesPolicy)
            {
                _name = this._name,
                _description = this._description,
                _valueType = this._valueType,
                _isCollectionValue = this._isCollectionValue,
                _value = this._value, 
                _values = new List<TreeLeaveModel>(this._values),
                _visibility = this._visibility,
                Override = this._override
            };

            newOwner.AddAttribute(result);

            return result;
        }

        protected override bool AddContentDetailed(IContentModel content)
        {
            throw new NotImplementedException();
        }

        protected override bool RemoveContentDetailed(IContentModel content)
        {
            throw new NotImplementedException();
        }

        protected override bool ClearContentDetailed()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получить унаследованный атрибут с родителя
        /// </summary>
        /// <param name="elevationLevel">Уровень поднятия. Величина обратная к глубине наследования. 
        ///     elevationLevel = this.InheritanceDepth - targetParent.InheritanceDepth 
        ///     (0 - текущий владелец, 1 - ближайший родитель, int.Max - изначальный владелец)</param>
        /// <returns></returns>
        public ElementAttributeModel GetInheritedAttributeFromParent(int elevationLevel = 1)
        {
            if (elevationLevel == 0 || IsOwn)
                return this;

            if (Owner is IChildrenModel c)
            {
                if (c.Parent is IAttributeOwnerModel nextParent)
                {
                    var inheritedAttribute = nextParent.Attributes.SingleOrDefault(x => x.DeclaringUuid == DeclaringUuid);
                     
                    if (inheritedAttribute != null)
                        return inheritedAttribute.GetInheritedAttributeFromParent(elevationLevel - 1); ;

                    _notificationService.SendTextMessage<ElementAttributeModel>($"Ошибка поиска унаследованного атрибута '{Name}' - родитель владельца '{(c.Parent as IMainEntity)?.Name}' не содержит такого атрибута. Обратитесь к разработчику.");
                    throw new Exception();
                }

                _notificationService.SendTextMessage<ElementAttributeModel>($"Ошибка поиска унаследованного атрибута '{Name}' - родитель владельца '{(c.Parent as IMainEntity)?.Name}' не содержит атрибутов. Обратитесь к разработчику.");
                throw new Exception();
            }

            _notificationService.SendTextMessage<ElementAttributeModel>($"Ошибка поиска унаследованного атрибута '{Name}' - владелец атрибута '{(Owner as IMainEntity)?.Name}' не имеет родителей. Обратитесь к разработчику.");
            throw new Exception();
        }

        #endregion
    }
}
