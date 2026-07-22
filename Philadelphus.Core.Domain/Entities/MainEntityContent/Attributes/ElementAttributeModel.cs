using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Policies;
using Philadelphus.Core.Domain.Policies.Builders;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.MainEntities;
using System.Collections.ObjectModel;

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
        private Guid? _unresolvedValueTypeUuid;
        private TreeLeaveModel _value;
        private Guid? _persistedMaterializedValueUuid;
        private string _valueFormula = string.Empty;
        private string _valueFormulaErrorCode = string.Empty;
        private List<TreeLeaveModel> _values = new List<TreeLeaveModel>();
        private List<Guid> _unresolvedValuesUuids = new List<Guid>();
        private IReadOnlyList<Guid>? _persistedMaterializedValuesUuids;
        private bool _isValueOverridden;    // Признак того, что одиночное значение унаследованного атрибута было переопределено локально.
        private bool _areValuesOverridden;  // Признак того, что коллекция значений унаследованного атрибута была переопределена локально.
        private VisibilityScope _visibility;
        private OverrideType _override;
        private ElementAttributeModel? _inheritedAttributeFromParent;

        #endregion

        #region [ Properties ] 

        #region [ General Properties ]

        /// <summary>
        /// Тип данных (узел дерева репозитория Чубушника)
        /// </summary>
        public virtual TreeNodeModel ValueType
        {
            get => GetValue(_valueType);
            set
            {
                SetValue(ref _valueType, value);

                if (ReferenceEquals(_valueType, value))
                {
                    _unresolvedValueTypeUuid = null;
                }
            }
        }

        /// <summary>
        /// Код ошибки привязки типа данных.
        /// </summary>
        public string ValueTypeReferenceErrorCode => _unresolvedValueTypeUuid.HasValue
            ? AttributeReferenceErrorCodes.ValueTypeNotFound
            : string.Empty;

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
                        .SelectMany(x => x.ContentNodes);
                }
                throw new Exception("Владелец атрибута не является элементом репозитория. Непредвиденная ситуация, обратитесь к разработчику.");
            }
        }

        /// <summary>
        /// Допускается коллекция значений
        /// </summary>
        public virtual bool IsCollectionValue
        {
            get => GetValue(_isCollectionValue);
            set => SetValue(ref _isCollectionValue, value);
        }

        /// <summary>
        /// Значение (лист выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public virtual TreeLeaveModel Value
        {
            get => GetValue(GetEffectiveValue());
            set
            {
                var inheritedValue = _inheritedAttributeFromParent?.Value;
                var alreadyLocalValue = SameValue(value, _value);
                bool valueChanged;

                // ValueUuid в БД — лишь материализованный результат для отчетов, поэтому при загрузке
                // сам лист в Value не восстанавливается. Если вычисленная по формуле ссылка совпала с
                // сохраненным результатом, заполняем runtime-значение без перевода атрибута в Changed.
                if (_value == null
                    && value?.Uuid == _persistedMaterializedValueUuid
                    && State == State.SavedOrLoaded)
                {
                    _value = value;
                    _persistedMaterializedValueUuid = null;
                    valueChanged = false;
                }
                else
                {
                    valueChanged = SetValue(ref _value, value);

                    if (ReferenceEquals(_value, value))
                    {
                        _persistedMaterializedValueUuid = null;
                    }
                }

                if (_isOwn == false
                    && (valueChanged || alreadyLocalValue))
                {
                    _isValueOverridden = SameValue(value, inheritedValue) == false;
                }
            }
        }

        /// <summary>
        /// Формула одиночного значения атрибута.
        /// </summary>
        public string ValueFormula
        {
            get => GetValue(GetEffectiveValueFormula());
            set
            {
                value ??= string.Empty;

                var inheritedFormula = _inheritedAttributeFromParent?.ValueFormula ?? string.Empty;
                var alreadyLocalFormula = string.Equals(value, _valueFormula, StringComparison.Ordinal);
                var formulaChanged = SetValue(ref _valueFormula, value);

                if (_isOwn == false
                    && (formulaChanged || alreadyLocalFormula))
                {
                    _isValueOverridden = string.Equals(value, inheritedFormula, StringComparison.Ordinal) == false;
                }
            }
        }

        /// <summary>
        /// Код ошибки последнего вычисления формулы значения.
        /// </summary>
        /// <remarks>
        /// Runtime-диагностика не сохраняется в БД, поэтому ее изменение не должно переводить
        /// загруженный атрибут в состояние <see cref="State.Changed" />.
        /// </remarks>
        public string ValueFormulaErrorCode
        {
            get => GetValue(GetEffectiveValueFormulaErrorCode());
            set => _valueFormulaErrorCode = value ?? string.Empty;
        }

        /// <summary>
        /// Код ошибки привязки значения коллекционного атрибута.
        /// </summary>
        public string ValuesReferenceErrorCode => _isCollectionValue
            && _unresolvedValuesUuids.Count > 0
                ? AttributeReferenceErrorCodes.ValueNotFound
                : string.Empty;

        /// <summary>
        /// Значения (листы выбранного узла дерева репозитория Чубушника)
        /// </summary>
        public IReadOnlyList<TreeLeaveModel> Values
        {
            get => GetValue(GetEffectiveValues());
        }

        /// <summary>
        /// Признак того, что одиночное значение унаследованного атрибута отличается от родительского.
        /// </summary>
        public bool IsValueOverridden => _isOwn == false
            && IsParentOverrideForbidden() == false
            && _isValueOverridden;

        /// <summary>
        /// Признак того, что коллекция значений унаследованного атрибута отличается от родительской.
        /// </summary>
        public bool AreValuesOverridden => _isOwn == false
            && IsParentOverrideForbidden() == false
            && _areValuesOverridden;

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
        public virtual VisibilityScope Visibility
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

        /// <summary>
        /// Унаследованный атрибут родителя
        /// </summary>
        public ElementAttributeModel? InheritedAttributeFromParent 
        {
            get => GetValue(_inheritedAttributeFromParent); 
        }

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
        /// Признак вычисляемого атрибута, который существует только во время работы приложения.
        /// </summary>
        public virtual bool IsRuntime => false;

        /// <summary>
        /// Все владельцы (рекурсивно)
        /// </summary>
        public override ReadOnlyDictionary<Guid, IOwnerModel> AllOwnersRecursive 
        { 
            get => RecursiveRelationshipHelper.ToReadOnlyDictionary(
                RecursiveRelationshipHelper.EnumerateOwnersRecursive(this)); 
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
        /// <param name="localUuid">Локальный уникальный идентификатор.</param>
        /// <param name="localOwner">Локальный владелец.</param>
        /// <param name="declaringUuid">Уникальный идентификатор объявления.</param>
        /// <param name="declaringOwner">Владелец объявления.</param>
        /// <param name="owningWorkingTree">Владеющее рабочее дерево.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="propertiesPolicy">Политика свойств.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
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

            ArgumentOutOfRangeException.ThrowIfEqual(declaringUuid, Guid.Empty);

            _attributeOwner = localOwner;
            _declaringUuid = declaringUuid;
            _declaringAttributeOwner = declaringOwner;

            _isOwn = localUuid == _declaringUuid
                && _attributeOwner.Uuid == _declaringAttributeOwner.Uuid;

            if (_isOwn)
            {
                _attributeOwner.AddContent(this);
            }
            else
            {
                _attributeOwner.AddInheritedAttribute(this);
            }

            OwningWorkingTree.ContentAttributes.Add(this);

            _inheritedAttributeFromParent = TryGetInheritedAttributeFromParent();

            _visibility = VisibilityScope.Public;

            if (_attributeOwner is TreeRootModel)
            {
                _override = OverrideType.Abstract;
            }
            else if (_attributeOwner is TreeNodeModel)
            {
                _override = OverrideType.Virtual;
            }
            else if (_attributeOwner is TreeLeaveModel)
            {
                _override = OverrideType.NotApplicable;
            }
        }

        #endregion

        #region [ Methods ]

        /// <summary>
        /// Сменить владельца
        /// </summary>
        /// <param name="newOwner">Новый владелец.</param>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        public bool ChangeOwner(IOwnerModel newOwner)
        {
            ArgumentNullException.ThrowIfNull(newOwner);

            throw new NotImplementedException();
        }

        /// <summary>
        /// Добавить значение атрибута в коллекцию
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool TryAddValueToValuesCollection(TreeLeaveModel value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (CanWriteValuesCollection() == false)
                return false;
            if (_isCollectionValue == false)
                return false; 
            PrepareValuesCollectionForOverride();
            if (_values != null && _values.Any(x => x.Uuid == value.Uuid))
                return false;
            if (IsCompatibleCollectionValue(value) == false)
                return false;

            _values?.Add(value);
            _persistedMaterializedValuesUuids = null;
            MarkValuesOverriddenIfNeeded();
            UpdateStateStateAfterChange();
            return true;
        }

        /// <summary>
        /// Атомарно заменить материализованный результат формулы коллекционного атрибута.
        /// </summary>
        public bool TrySetValuesFromFormula(IEnumerable<TreeLeaveModel> values)
        {
            ArgumentNullException.ThrowIfNull(values);

            if (CanWriteValuesCollection() == false || _isCollectionValue == false)
                return false;

            var materializedValues = values.DistinctBy(x => x.Uuid).ToList();
            if (materializedValues.Any(x => IsCompatibleCollectionValue(x) == false))
                return false;

            var materializedUuids = materializedValues.Select(x => x.Uuid).ToArray();
            var previousUuids = _persistedMaterializedValuesUuids
                ?? _values.Select(x => x.Uuid).ToArray();
            var valuesChanged = previousUuids.SequenceEqual(materializedUuids) == false;

            _values = materializedValues;
            _unresolvedValuesUuids.Clear();
            _persistedMaterializedValuesUuids = null;
            MarkValuesOverriddenIfNeeded();

            if (valuesChanged)
            {
                UpdateStateStateAfterChange();
            }

            return true;
        }

        /// <summary>
        /// Очистить runtime-результат коллекционной формулы без изменения состояния модели.
        /// </summary>
        public bool ClearValuesFromFormula()
        {
            if (CanWriteValuesCollection() == false || _isCollectionValue == false)
                return false;

            var valuesChanged = _values.Count > 0 || _unresolvedValuesUuids.Count > 0;
            _values.Clear();
            _unresolvedValuesUuids.Clear();
            _persistedMaterializedValuesUuids = null;
            MarkValuesOverriddenIfNeeded();

            if (valuesChanged)
            {
                UpdateStateStateAfterChange();
            }

            return true;
        }

        /// <summary>
        /// Назначить одиночное значение системного атрибута по строковому представлению.
        /// </summary>
        /// <param name="stringValue">Строковое значение базового системного типа.</param>
        /// <returns>true, если значение найдено или создано и присвоено; иначе false.</returns>
        public bool TrySetSystemBaseValueFromString(string? stringValue)
        {
            if (_isCollectionValue
                || ValueType is not SystemBaseTreeNodeModel systemBaseNode
                || string.IsNullOrWhiteSpace(stringValue))
            {
                return false;
            }

            if (TryGetOrCreateSystemBaseLeave(
                    systemBaseNode,
                    stringValue,
                    out var systemBaseLeave) == false
                || systemBaseLeave == null)
            {
                return false;
            }

            Value = systemBaseLeave;
            return SameValue(Value, systemBaseLeave);
        }

        /// <summary>
        /// Исключить значение атрибута из коллекции
        /// </summary>
        /// <param name="value">Значение</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public bool TryRemoveValueFromValuesCollection(TreeLeaveModel value)
        {
            ArgumentNullException.ThrowIfNull(value);

            if (CanWriteValuesCollection() == false)
                return false;
            if (_isCollectionValue == false)
                return false;
            PrepareValuesCollectionForOverride();
            if (_values != null && _values.Any(x => x == value) == false)
                return false;
            _values?.Remove(value);
            _persistedMaterializedValuesUuids = null;
            MarkValuesOverriddenIfNeeded();
            UpdateStateStateAfterChange(); 
            return true;
        }

        /// <summary>
        /// Очистить коллекцию значений атрибута
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        public bool ClearValuesCollection()
        {
            if (CanWriteValuesCollection() == false)
                return false;
            if (_isCollectionValue == false)
                return false;
            PrepareValuesCollectionForOverride();
            if (_values == null)
                return false;
            _values?.Clear();
            _unresolvedValuesUuids.Clear();
            _persistedMaterializedValuesUuids = null;
            MarkValuesOverriddenIfNeeded();
            UpdateStateStateAfterChange(); 
            return true;
        }

        /// <summary>
        /// Получить копию для наследника
        /// </summary>
        /// <param name="newOwner">Новый владелец.</param>
        /// <returns>Результат выполнения операции.</returns>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public virtual ElementAttributeModel CloneForChild(IAttributeOwnerModel newOwner)
        {
            ArgumentNullException.ThrowIfNull(newOwner);

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
                _unresolvedValueTypeUuid = this._unresolvedValueTypeUuid,
                _isCollectionValue = this._isCollectionValue,
                _value = this._value,
                _persistedMaterializedValueUuid = this._persistedMaterializedValueUuid,
                _valueFormula = this._valueFormula,
                _valueFormulaErrorCode = this._valueFormulaErrorCode,
                _values = new List<TreeLeaveModel>(this._values),
                _unresolvedValuesUuids = new List<Guid>(this._unresolvedValuesUuids),
                _persistedMaterializedValuesUuids = this._persistedMaterializedValuesUuids?.ToArray(),
                _visibility = this._visibility,
                _inheritedAttributeFromParent = this,
            };

            if (newOwner is not TreeLeaveModel)
            {
                result._override = this._override;
            }

            return result;
        }

        internal long AssignInheritedAutoSequence(IEnumerable<long>? existSequences = null)
        {
            _sequence = SequenceHelper.GetNewSequence(existSequences);
            return _sequence;
        }

        /// <summary>
        /// Загрузить формулу значения из хранилища без проверки политик записи.
        /// </summary>
        internal void LoadValueFormula(string? valueFormula)
        {
            _valueFormula = valueFormula ?? string.Empty;

            if (_isOwn == false
                && _inheritedAttributeFromParent != null
                && IsParentOverrideForbidden() == false
                && string.IsNullOrWhiteSpace(_valueFormula) == false)
            {
                _isValueOverridden = string.Equals(
                    _valueFormula,
                    _inheritedAttributeFromParent.ValueFormula,
                    StringComparison.Ordinal) == false;
            }
        }

        /// <summary>
        /// Загрузить тип данных и сохранить исходную ссылку, если тип не найден.
        /// </summary>
        internal void LoadValueType(TreeNodeModel? valueType, Guid? valueTypeUuid)
        {
            _valueType = valueType!;
            _unresolvedValueTypeUuid = valueType == null ? valueTypeUuid : null;
        }

        /// <summary>
        /// Запомнить идентификатор материализованного результата, не загружая его в runtime-значение.
        /// </summary>
        /// <remarks>
        /// Идентификатор используется только для сравнения результата последующего вычисления формулы,
        /// чтобы неизменившийся результат не помечал только что загруженный атрибут как Changed.
        /// Источником Value он не является.
        /// </remarks>
        internal void LoadPersistedMaterializedValueUuid(Guid? valueUuid)
        {
            _value = null!;
            _persistedMaterializedValueUuid = valueUuid;
        }

        /// <summary>
        /// Запомнить идентификаторы материализованного результата, не загружая их в runtime-коллекцию.
        /// </summary>
        internal void LoadPersistedMaterializedValuesUuids(IEnumerable<Guid>? valuesUuids)
        {
            _values.Clear();
            _unresolvedValuesUuids.Clear();
            _persistedMaterializedValuesUuids = valuesUuids?.ToArray() ?? Array.Empty<Guid>();
        }

        /// <summary>
        /// Загрузить коллекцию значений и сохранить ссылки на отсутствующие значения.
        /// </summary>
        internal void LoadValues(
            IEnumerable<TreeLeaveModel> values,
            IEnumerable<Guid> unresolvedValuesUuids)
        {
            _values = new List<TreeLeaveModel>(values);
            _unresolvedValuesUuids = new List<Guid>(unresolvedValuesUuids);
            _persistedMaterializedValuesUuids = null;
        }

        /// <summary>
        /// Получить ссылку на тип данных, включая отсутствующий тип.
        /// </summary>
        internal Guid? ValueTypeReferenceUuid => _valueType?.Uuid ?? _unresolvedValueTypeUuid;

        /// <summary>
        /// Получить ссылки на значения коллекции, включая отсутствующие значения.
        /// </summary>
        internal IReadOnlyList<Guid> ValuesReferenceUuids => _values
            .Select(x => x.Uuid)
            .Concat(_unresolvedValuesUuids)
            .ToArray();

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
        /// <returns>Результат выполнения операции.</returns>
        public ElementAttributeModel GetInheritedAttributeFromParent(int elevationLevel = 1)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(elevationLevel);

            if (elevationLevel == 0 || IsOwn)
                return this;

            if (elevationLevel == 1 && _inheritedAttributeFromParent != null)
                return _inheritedAttributeFromParent;

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

        /// <summary>
        /// Попробовать получить унаследованный атрибут с родителя в процессе формирования дерева, пока у владельца еще нет родителя (без выбрасывания исключений)
        /// </summary>
        /// <returns>Результат выполнения операции.</returns>
        private ElementAttributeModel? TryGetInheritedAttributeFromParent()
        {
            if (IsOwn)
                return this;

            if (Owner is IChildrenModel c
                && c.Parent is IAttributeOwnerModel nextParent)
            {
                return nextParent.Attributes.SingleOrDefault(x => x.DeclaringUuid == DeclaringUuid);
            }

            return null;
        }

        /// <summary>
        /// Проверить, разрешает ли политика изменение коллекции значений.
        /// </summary>
        /// <returns>true, если коллекцию значений можно изменить; иначе false.</returns>
        private bool CanWriteValuesCollection()
        {
            return _propertiesPolicy?.CanWrite(this, nameof(Values), _values.AsReadOnly()) != false;
        }

        private bool IsCompatibleCollectionValue(TreeLeaveModel value)
        {
            if (SystemBaseAttributeValueCompatibilityValidator.IsCompatible(
                    ValueType,
                    value,
                    out var systemBaseType,
                    out var stringValue,
                    out var expectedFormat))
            {
                return true;
            }

            _notificationService.SendTextMessage<ElementAttributeModel>(
                $"Для коллекционного атрибута '{Name}' [{Uuid}] элемента '{(Owner as IMainEntityModel)?.Name}' [{(Owner as IMainEntityModel)?.Uuid}] " +
                $"значение '{stringValue ?? "<null>"}' не соответствует системному типу '{systemBaseType}'. " +
                $"Ожидаемый формат: {expectedFormat}.",
                criticalLevel: NotificationCriticalLevelModel.Warning);

            return false;
        }

        /// <summary>
        /// Получить актуальное одиночное значение с учетом наследования.
        /// </summary>
        /// <returns>Локальное значение или значение родительского атрибута, если оно еще не переопределено.</returns>
        private TreeLeaveModel GetEffectiveValue()
        {
            if (_isOwn == false
                && (IsParentOverrideForbidden() || _isValueOverridden == false)
                && _inheritedAttributeFromParent != null)
            {
                return _inheritedAttributeFromParent.Value;
            }

            return _value;
        }

        /// <summary>
        /// Получить актуальную формулу одиночного значения с учетом наследования.
        /// </summary>
        private string GetEffectiveValueFormula()
        {
            if (_isOwn == false
                && (IsParentOverrideForbidden() || _isValueOverridden == false)
                && _inheritedAttributeFromParent != null)
            {
                return _inheritedAttributeFromParent.ValueFormula;
            }

            return _valueFormula;
        }

        /// <summary>
        /// Получить актуальный код ошибки формулы с учетом наследования.
        /// </summary>
        private string GetEffectiveValueFormulaErrorCode()
        {
            if (_isOwn == false
                && (IsParentOverrideForbidden() || _isValueOverridden == false)
                && _inheritedAttributeFromParent != null)
            {
                return _inheritedAttributeFromParent.ValueFormulaErrorCode;
            }

            return _valueFormulaErrorCode;
        }

        /// <summary>
        /// Получить актуальную коллекцию значений с учетом наследования.
        /// </summary>
        /// <returns>Локальная коллекция или коллекция родительского атрибута, если она еще не переопределена.</returns>
        private IReadOnlyList<TreeLeaveModel> GetEffectiveValues()
        {
            if (_isOwn == false
                && (IsParentOverrideForbidden() || _areValuesOverridden == false)
                && _inheritedAttributeFromParent != null)
            {
                return _inheritedAttributeFromParent.Values;
            }

            return _values.AsReadOnly();
        }

        /// <summary>
        /// Проверить, запрещает ли родительский атрибут локальное переопределение у наследника.
        /// </summary>
        /// <returns>true, если родительский атрибут запрещает переопределение; иначе false.</returns>
        private bool IsParentOverrideForbidden()
        {
            return _inheritedAttributeFromParent?.Override == OverrideType.Sealed;
        }

        /// <summary>
        /// Подготовить локальную копию унаследованной коллекции перед ее изменением.
        /// </summary>
        private void PrepareValuesCollectionForOverride()
        {
            if (_isOwn == false
                && _areValuesOverridden == false
                && _inheritedAttributeFromParent != null)
            {
                _values = new List<TreeLeaveModel>(_inheritedAttributeFromParent.Values);
            }
        }

        /// <summary>
        /// Зафиксировать факт локального переопределения коллекции значений, если она отличается от родительской.
        /// </summary>
        private void MarkValuesOverriddenIfNeeded()
        {
            if (_isOwn && _inheritedAttributeFromParent == null)
                return;

            _areValuesOverridden = ValuesMatchInherited() == false;
        }

        /// <summary>
        /// Проверить, совпадает ли локальная коллекция значений с унаследованной.
        /// </summary>
        /// <returns>true, если набор значений совпадает с родительским; иначе false.</returns>
        private bool ValuesMatchInherited()
        {
            return _inheritedAttributeFromParent?.Values.Select(x => x.Uuid).SequenceEqual(_values.Select(x => x.Uuid)) == true;
        }

        /// <summary>
        /// Проверить, совпадают ли одиночные значения атрибута.
        /// </summary>
        /// <param name="value">Локальное значение.</param>
        /// <param name="inheritedValue">Унаследованное значение.</param>
        /// <returns>true, если значения совпадают; иначе false.</returns>
        private static bool SameValue(TreeLeaveModel value, TreeLeaveModel inheritedValue)
        {
            return value?.Uuid == inheritedValue?.Uuid;
        }

        /// <summary>
        /// Найти существующий системный лист в узле типа данных или создать новый допустимый лист.
        /// </summary>
        private bool TryGetOrCreateSystemBaseLeave(
            SystemBaseTreeNodeModel systemBaseNode,
            string stringValue,
            out SystemBaseTreeLeaveModel? systemBaseLeave)
        {
            systemBaseLeave = systemBaseNode.ChildLeaves
                .OfType<SystemBaseTreeLeaveModel>()
                .FirstOrDefault(x => string.Equals(x.StringValue, stringValue, StringComparison.Ordinal));

            if (systemBaseLeave != null)
            {
                return true;
            }

            if (SystemBaseStringValueValidator.IsValid(systemBaseNode.SystemBaseType, stringValue, out var expectedFormat) == false)
            {
                _notificationService.SendTextMessage<ElementAttributeModel>(
                    $"Для атрибута '{Name}' [{Uuid}] элемента '{(Owner as IMainEntityModel)?.Name}' [{(Owner as IMainEntityModel)?.Uuid}] " +
                    $"значение '{stringValue}' не соответствует системному типу '{systemBaseNode.SystemBaseType}'. " +
                    $"Ожидаемый формат: {expectedFormat}.",
                    criticalLevel: NotificationCriticalLevelModel.Warning);

                return false;
            }

            if (systemBaseNode.SystemBaseType == SystemBaseType.BOOL)
            {
                systemBaseLeave = ResolveBoolSystemBaseLeave(systemBaseNode, stringValue);
                return systemBaseLeave != null;
            }

            systemBaseLeave = new SystemBaseTreeLeaveModel(
                Guid.CreateVersion7(),
                systemBaseNode,
                systemBaseNode.OwningWorkingTree,
                systemBaseNode.SystemBaseType,
                _notificationService,
                PropertiesPolicyBuilder.CreateTreeLeaveDefault(_notificationService));

            systemBaseLeave.StringValue = stringValue;
            return true;
        }

        /// <summary>
        /// Получить предопределенный BOOL-лист по введенному пользователем строковому значению.
        /// </summary>
        private static SystemBaseTreeLeaveModel? ResolveBoolSystemBaseLeave(
            SystemBaseTreeNodeModel systemBaseNode,
            string stringValue)
        {
            if (SystemBaseStringValueValidator.TryParse(SystemBaseType.BOOL, stringValue, out var typedValue, out _) == false
                || typedValue is not bool boolValue)
            {
                return null;
            }

            return systemBaseNode.ChildLeaves
                .OfType<SystemBaseTreeLeaveModel>()
                .FirstOrDefault(x => x.TypedValue is bool existingBoolValue && existingBoolValue == boolValue);
        }

        #endregion
    }
}
