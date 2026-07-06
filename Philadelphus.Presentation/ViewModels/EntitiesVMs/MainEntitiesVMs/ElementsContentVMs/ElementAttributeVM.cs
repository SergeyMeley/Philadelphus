using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.Services.Tables;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using System.Collections.ObjectModel;

namespace Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs
{
    /// <summary>
    /// Модель представления для атрибута элемента.
    /// </summary>
    public class ElementAttributeVM : MainEntityBaseVM<ElementAttributeModel>
    {
        #region [ Props ]

        private readonly IPhiladelphusRepositoryService _service;

        private readonly ElementAttributeModel _model;
        private ObservableCollection<TreeLeaveModel> _assignedValues = new ObservableCollection<TreeLeaveModel>();

        /// <summary>
        /// Владелец.
        /// </summary>
        public IOwnerModel Owner { get => _model.Owner; }
        
        /// <summary>
        /// Хранилище данных.
        /// </summary>
        public IDataStorageModel DataStorage { get => _model.DataStorage; }
        public TreeNodeModel SelectedValueType 
        { 
            get
            {
                return _model.ValueType;
            }
            set
            {
                if (_model.ValueType != value)
                {
                    _model.ValueType = value;
                    OnPropertyChanged(nameof(SelectedValueType));
                    OnPropertyChanged(nameof(ValuesList));
                    SelectedValue = null;
                    OnPropertyChanged(nameof(SelectedValue));
                    AssignedValue = null;
                    OnPropertyChanged(nameof(AssignedValue)); 
                    _assignedValues = null;
                    OnPropertyChanged(nameof(AssignedValues));
                    OnPropertyChanged(nameof(IsValueOverridden));
                    OnPropertyChanged(nameof(ValueOverrideToolTip));
                    OnPropertyChanged(nameof(AreValuesOverridden));
                    OnPropertyChanged(nameof(ValuesOverrideToolTip));
                }
                
            }
        }
      
        /// <summary>
        /// Список типов значений.
        /// </summary>
        public IEnumerable<TreeNodeModel>? ValueTypesList { get => _model.ValueTypesList; }

        public bool IsCollectionValue
        {
            get
            {
                return _model.IsCollectionValue;
            }
            set
            {
                if (value == true)
                {
                    AssignedValue = null;
                    _assignedValues = new ObservableCollection<TreeLeaveModel>();
                    OnPropertyChanged(nameof(AssignedValue));
                }
                else
                {
                    _assignedValues = null;
                    OnPropertyChanged(nameof(AssignedValues));
                    OnPropertyChanged(nameof(AssignedValuesString));
                }
                _model.IsCollectionValue = value;
                OnPropertyChanged(nameof(IsCollectionValue));
                OnPropertyChanged(nameof(IsValueOverridden));
                OnPropertyChanged(nameof(ValueOverrideToolTip));
                OnPropertyChanged(nameof(AreValuesOverridden));
                OnPropertyChanged(nameof(ValuesOverrideToolTip));
            }
        }

        public TreeLeaveModel AssignedValue
        {
            get
            {
                return _model.Value;
            }
            set
            {
                if (IsCollectionValue == false)
                {
                    _model.ValueFormula = string.Empty;
                    _model.ValueFormulaErrorCode = string.Empty;
                    _model.Value = value;
                }
                NotifyStateVisibilityPropertiesChanged();
                OnPropertyChanged(nameof(AssignedValue));
                OnPropertyChanged(nameof(AssignedValueText));
                OnPropertyChanged(nameof(DisplayedValueText));
                OnPropertyChanged(nameof(FormulaValueText));
                OnPropertyChanged(nameof(IsValueOverridden));
                OnPropertyChanged(nameof(ValueOverrideToolTip));
            }
        }

        /// <summary>
        /// Текстовое представление одиночного значения для редактируемого ComboBox системных базовых типов.
        /// </summary>
        public string AssignedValueText
        {
            get => AssignedValue is SystemBaseTreeLeaveModel systemBaseValue
                ? systemBaseValue.StringValue
                : AssignedValue?.Name ?? string.Empty;
            set
            {
                if (_model.TrySetSystemBaseValueFromString(value))
                {
                    _model.ValueFormula = string.Empty;
                    _model.ValueFormulaErrorCode = string.Empty;
                    NotifyStateVisibilityPropertiesChanged();
                    OnPropertyChanged(nameof(AssignedValue));
                    OnPropertyChanged(nameof(AssignedValueText));
                    OnPropertyChanged(nameof(DisplayedValueText));
                    OnPropertyChanged(nameof(FormulaValueText));
                    OnPropertyChanged(nameof(ValuesList));
                    OnPropertyChanged(nameof(IsValueOverridden));
                    OnPropertyChanged(nameof(ValueOverrideToolTip));
                }
            }
        }

        /// <summary>
        /// Значение ячейки в режиме просмотра: результат формулы или код ошибки.
        /// </summary>
        public string DisplayedValueText
        {
            // Единая логика с таблицей наследников — см. AttributeValueText.
            get => AttributeValueText.GetDisplayText(_model);
        }

        /// <summary>
        /// Значение ячейки в режиме редактирования: формула или ссылка на лист.
        /// </summary>
        public string FormulaValueText
        {
            // Единая логика с таблицей наследников — см. AttributeValueText.
            get => AttributeValueText.GetFormulaText(_model);
            set
            {
                AttributeValueText.SetFormulaText(_model, value);
                NotifyStateVisibilityPropertiesChanged();
                OnPropertyChanged(nameof(AssignedValue));
                OnPropertyChanged(nameof(AssignedValueText));
                OnPropertyChanged(nameof(DisplayedValueText));
                OnPropertyChanged(nameof(FormulaValueText));
                OnPropertyChanged(nameof(ValuesList));
                OnPropertyChanged(nameof(IsValueOverridden));
                OnPropertyChanged(nameof(ValueOverrideToolTip));
            }
        }

        /// <summary>
        /// Признак переопределения одиночного значения относительно родительского атрибута.
        /// </summary>
        public bool IsValueOverridden => _model.IsValueOverridden;

        /// <summary>
        /// Подсказка переопределения одиночного значения.
        /// </summary>
        public string ValueOverrideToolTip => _model.IsValueOverridden
            ? $"Значение переопределено относительно родительского атрибута. У родителя: {FormatValue(_model.InheritedAttributeFromParent?.Value)}"
            : "Значение наследуется от родительского атрибута";

        public ReadOnlyObservableCollection<TreeLeaveModel> AssignedValues 
        {
            get
            {
                if (IsCollectionValue)
                {
                    if (_assignedValues != null)
                    {
                        return new ReadOnlyObservableCollection<TreeLeaveModel>(_assignedValues);
                    }
                }
                return null;
            }
        }
        public string AssignedValuesString
        {
            get => string.Join("; ", _assignedValues?.Select(x => x.Name) ?? new List<string>());
        }

        /// <summary>
        /// Признак переопределения коллекции значений относительно родительского атрибута.
        /// </summary>
        public bool AreValuesOverridden => _model.AreValuesOverridden;

        /// <summary>
        /// Подсказка переопределения коллекции значений.
        /// </summary>
        public string ValuesOverrideToolTip => _model.AreValuesOverridden
            ? $"Коллекция значений переопределена относительно родительского атрибута. У родителя: {FormatValues(_model.InheritedAttributeFromParent?.Values)}"
            : "Коллекция значений наследуется от родительского атрибута";
       
        /// <summary>
        /// Выбранное значение.
        /// </summary>
        public TreeLeaveModel SelectedValue { get; set; }
        public IEnumerable<TreeLeaveModel>? ValuesList 
        { 
            get
            {
                return _model.ValuesList;
            }
        }

        /// <summary>
        /// Выбранный элемент области видимости (единственная точка выбора из UI).
        /// Привязывается к <see cref="VisibilityScopesList" /> по ССЫЛКЕ через ComboBox.SelectedItem.
        /// Выбор-по-значению (SelectedValue+SelectedValueBinding) в Avalonia терял выбор при
        /// рециклинге ячеек DataGrid — значение «слетало» при добавлении/прокрутке строк (см. тех-долг E);
        /// item-based SelectedItem сравнивает по ссылке и переживает подмену DataContext.
        /// Геттер всегда отдаёт актуальный элемент из списка → выбор корректно восстанавливается.
        /// </summary>
        public VisibilityScopeItem? SelectedVisibilityScope
        {
            get
            {
                return VisibilityScopesList.FirstOrDefault(x => x.Value == _model.Visibility);
            }
            set
            {
                // Паразитный null при рециклинге ячейки игнорируем — иначе затрём модель.
                if (value == null)
                {
                    return;
                }

                _model.Visibility = value.Value;
                OnPropertyChanged(nameof(SelectedVisibilityScope));
            }
        }

        /// <summary>
        /// Список областей видимости.
        /// </summary>
        public List<VisibilityScopeItem> VisibilityScopesList { get; }

        /// <summary>
        /// Выбранный элемент режима переопределения (единственная точка выбора из UI).
        /// Привязка по ССЫЛКЕ к <see cref="OverrideTypesList" /> через ComboBox.SelectedItem —
        /// аналогично <see cref="SelectedVisibilityScope" /> (см. тех-долг E).
        /// </summary>
        public OverrideTypeItem? SelectedOverrideType
        {
            get
            {
                return OverrideTypesList.FirstOrDefault(x => x.Value == _model.Override);
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _model.Override = value.Value;
                OnPropertyChanged(nameof(SelectedOverrideType));
            }
        }

        /// <summary>
        /// Список режимов переопределения.
        /// </summary>
        public List<OverrideTypeItem> OverrideTypesList { get; }

        /// <summary>
        /// Глубина наследования.
        /// </summary>
        public int InheritanceDepth => _model.InheritanceDepth;

        /// <summary>
        /// Владелец объявления.
        /// </summary>
        public IOwnerModel DeclaringOwner => _model.DeclaringOwner;

        #endregion

        #region [ Construct ]

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ElementAttributeVM" />.
        /// </summary>
        /// <param name="elementAttribute">Параметр elementAttribute.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="service">Доменный сервис.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ElementAttributeVM(
            ElementAttributeModel elementAttribute,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IPhiladelphusRepositoryService service,
            IFileDialogService fileDialogService,
            INotificationService? notificationService)
            : base(elementAttribute, dataStoragesCollectionVM, service, fileDialogService, notificationService)
        {
            ArgumentNullException.ThrowIfNull(elementAttribute);
            ArgumentNullException.ThrowIfNull(elementAttribute.Values);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(fileDialogService);

            _service = service;

            _model = elementAttribute;

            foreach (var item in _model.Values)
            {
                _assignedValues.Add(item);
            }

            VisibilityScopesList = new List<VisibilityScopeItem>(
            Enum.GetValues<VisibilityScope>()
                .Select(e => new VisibilityScopeItem
                {
                    Value = e,
                    DisplayName = e.GetDisplayName()
                }));

            OverrideTypesList = new List<OverrideTypeItem>(
            Enum.GetValues<OverrideType>()
                .Select(o => new OverrideTypeItem
                {
                    Value = o,
                    DisplayName = o.GetDisplayName()
                }));
        }

        #endregion

        #region [Commands]



        #endregion

        #region [ Methods ]

        public void NotifyChildsPropertyChangedRecursive()
        {
            NotifyStateVisibilityPropertiesChanged();
        }

        /// <summary>
        /// Добавляет данные AddSelectedValue.
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool AddSelectedValue()
        {
            if (IsCollectionValue == false)
                return false;
            if (SelectedValue == null)
                return false;
            if (_assignedValues != null && _assignedValues.Any(x => x.Uuid == SelectedValue.Uuid))
                return false;

            if (_model.TryAddValueToValuesCollection(SelectedValue))
            {
                _assignedValues?.Add(SelectedValue);
                OnPropertyChanged(nameof(AssignedValues)); 
                OnPropertyChanged(nameof(AssignedValuesString));
                OnPropertyChanged(nameof(AreValuesOverridden));
                OnPropertyChanged(nameof(ValuesOverrideToolTip));
                NotifyStateVisibilityPropertiesChanged();
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Удаляет данные RemoveSelectedValue.
        /// </summary>
        /// <returns>true, если операция выполнена успешно; иначе false.</returns>
        public bool RemoveSelectedValue()
        {
            if (IsCollectionValue == false)
                return false;
            if (SelectedValue == null)
                return false;
            if (_assignedValues != null && _assignedValues.Any(x => x == SelectedValue) == false)
                return false;

            if (_model.TryRemoveValueFromValuesCollection(SelectedValue))
            {
                _assignedValues?.Remove(SelectedValue);
                OnPropertyChanged(nameof(AssignedValues));
                OnPropertyChanged(nameof(AssignedValuesString));
                OnPropertyChanged(nameof(AreValuesOverridden));
                OnPropertyChanged(nameof(ValuesOverrideToolTip));
                NotifyStateVisibilityPropertiesChanged();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Получить отображаемое значение родительского атрибута для подсказки.
        /// </summary>
        private static string FormatValue(TreeLeaveModel? value)
        {
            return string.IsNullOrWhiteSpace(value?.Name) ? "<не задано>" : value.Name;
        }

        /// <summary>
        /// Получить отображаемую коллекцию значений родительского атрибута для подсказки.
        /// </summary>
        private static string FormatValues(IEnumerable<TreeLeaveModel>? values)
        {
            var names = values?
                .Select(x => x.Name)
                .Where(x => string.IsNullOrWhiteSpace(x) == false)
                .ToArray();

            return names is { Length: > 0 }
                ? string.Join("; ", names)
                : "<не задано>";
        }

        #endregion
    }
}
