using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Entities.MainEntityContent.Attributes;
using Philadelphus.Core.Domain.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using System.Collections.ObjectModel;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs
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
                    _model.Value = value;
                }
                OnPropertyChanged(nameof(State));
                OnPropertyChanged(nameof(AssignedValue));
            }
        }
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

        public VisibilityScope Visibility
        {
            get
            {
                return _model.Visibility;
            }
            set
            {
                if (_model.State != State.Initialized)
                {
                    MessageBox.Show("На данный момент переопределение области видимости невозможно");
                    return;
                }

                _model.Visibility = value;
                OnPropertyChanged(nameof(Visibility));
            }
        }

        /// <summary>
        /// Список областей видимости.
        /// </summary>
        public List<VisibilityScopeItem> VisibilityScopesList { get; }

        public OverrideType Override
        {
            get
            {
                return _model.Override;
            }
            set
            {
                _model.Override = value;
                OnPropertyChanged(nameof(Override));
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
            IPhiladelphusRepositoryService service) 
            : base(elementAttribute, dataStoragesCollectionVM, service)
        {
            ArgumentNullException.ThrowIfNull(elementAttribute);
            ArgumentNullException.ThrowIfNull(elementAttribute.Values);
            ArgumentNullException.ThrowIfNull(service);

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

        internal void NotifyChildsPropertyChangedRecursive()
        {
            OnPropertyChanged(nameof(State));
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
                OnPropertyChanged(nameof(State));
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
                OnPropertyChanged(nameof(State));
                return true;
            }

            return false;
        }

        #endregion
    }
}
