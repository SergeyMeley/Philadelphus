using System.Collections.ObjectModel;
using System.ComponentModel;

using Philadelphus.Core.Domain.Entities.Enums;

namespace Philadelphus.Presentation.Models.Tables
{
    /// <summary>
    /// Хранит значения одной строки таблицы наследников выбранного элемента.
    /// </summary>
    /// <remarks>
    /// Ключи внутренних словарей совпадают с <see cref="ChildCollectionTableColumn.BindingKey"/>,
    /// но индексатор дополнительно принимает логические ключи колонок. Это позволяет WPF работать
    /// с безопасными binding path-ами, а остальной код оставляет привязку к доменным именам свойств
    /// и атрибутов.
    /// </remarks>
    public sealed class ChildCollectionTableRow : INotifyPropertyChanged
    {
        private readonly Dictionary<string, object?> _cells;
        private readonly IReadOnlyDictionary<string, Func<object?, object?>?> _setters;
        private readonly Dictionary<string, IEnumerable<object>?> _valueOptions;
        private readonly Dictionary<string, bool> _cellEnabledStates;
        private readonly Dictionary<string, string?> _cellToolTips;
        private readonly IReadOnlyDictionary<string, Func<object?>> _refreshers;
        private readonly Dictionary<string, bool> _valueOverrideStates;
        private readonly IReadOnlyDictionary<string, Func<bool>> _valueOverrideStateRefreshers;
        private readonly Dictionary<string, string?> _valueOverrideToolTips;
        private readonly IReadOnlyDictionary<string, Func<string?>> _valueOverrideToolTipRefreshers;
        private readonly Action<Action>? _cellChangedDispatcher;

        // Канал «отображаемый текст значения атрибута» (результат формулы / код ошибки / значение) —
        // для режима просмотра ячейки. Обновляется тем же каскадом, что и _cells.
        private readonly Dictionary<string, string?> _displayTexts;
        private readonly IReadOnlyDictionary<string, Func<string?>> _displayTextRefreshers;

        // Канал «текст редактирования» (формула / ссылка «[uuid]») — для редактируемого ComboBox,
        // ПОЛНОСТЬЮ как ячейка значения в таблице атрибутов (см. AttributeValueText).
        private readonly IReadOnlyDictionary<string, Func<string?>> _editTextGetters;
        private readonly IReadOnlyDictionary<string, Action<string?>> _editTextSetters;

        private readonly IReadOnlyDictionary<string, string> _keyAliases;
        private readonly IReadOnlyDictionary<string, string> _logicalKeys;
        private readonly Func<State>? _parentOwnerAggregateStateGetter;
        private readonly Func<State>? _stateGetter;
        private readonly Func<State>? _childContentAggregateStateGetter;
        private readonly Func<string>? _stateVisibilityToolTipGetter;

        public ChildCollectionTableRow(
            IReadOnlyDictionary<string, object?> cells,
            IReadOnlyDictionary<string, Func<object?, object?>?>? setters = null,
            IReadOnlyDictionary<string, IEnumerable<object>?>? valueOptions = null,
            IReadOnlyDictionary<string, Func<object?>>? refreshers = null,
            IReadOnlyDictionary<string, string>? keyAliases = null,
            Guid sourceUuid = default,
            Action<Guid, string>? cellChanged = null,
            IReadOnlyDictionary<string, bool>? valueOverrideStates = null,
            IReadOnlyDictionary<string, Func<bool>>? valueOverrideStateRefreshers = null,
            IReadOnlyDictionary<string, string?>? valueOverrideToolTips = null,
            IReadOnlyDictionary<string, Func<string?>>? valueOverrideToolTipRefreshers = null,
            IReadOnlyDictionary<string, string?>? displayTexts = null,
            IReadOnlyDictionary<string, Func<string?>>? displayTextRefreshers = null,
            IReadOnlyDictionary<string, Func<string?>>? editTextGetters = null,
            IReadOnlyDictionary<string, Action<string?>>? editTextSetters = null,
            Func<State>? parentOwnerAggregateStateGetter = null,
            Func<State>? stateGetter = null,
            Func<State>? childContentAggregateStateGetter = null,
            Func<string>? stateVisibilityToolTipGetter = null,
            IReadOnlyDictionary<string, bool>? cellEnabledStates = null,
            IReadOnlyDictionary<string, string?>? cellToolTips = null,
            Action<Action>? cellChangedDispatcher = null)
        {
            ArgumentNullException.ThrowIfNull(cells);

            _cells = new Dictionary<string, object?>(cells);
            _setters = setters == null
                ? new ReadOnlyDictionary<string, Func<object?, object?>?>(new Dictionary<string, Func<object?, object?>?>())
                : new ReadOnlyDictionary<string, Func<object?, object?>?>(new Dictionary<string, Func<object?, object?>?>(setters));
            _valueOptions = valueOptions == null
                ? new Dictionary<string, IEnumerable<object>?>()
                : new Dictionary<string, IEnumerable<object>?>(valueOptions);
            _cellEnabledStates = cellEnabledStates == null
                ? new Dictionary<string, bool>()
                : new Dictionary<string, bool>(cellEnabledStates);
            _cellToolTips = cellToolTips == null
                ? new Dictionary<string, string?>()
                : new Dictionary<string, string?>(cellToolTips);
            _refreshers = refreshers == null
                ? new ReadOnlyDictionary<string, Func<object?>>(new Dictionary<string, Func<object?>>())
                : new ReadOnlyDictionary<string, Func<object?>>(new Dictionary<string, Func<object?>>(refreshers));
            _valueOverrideStates = valueOverrideStates == null
                ? new Dictionary<string, bool>()
                : new Dictionary<string, bool>(valueOverrideStates);
            _valueOverrideStateRefreshers = valueOverrideStateRefreshers == null
                ? new ReadOnlyDictionary<string, Func<bool>>(new Dictionary<string, Func<bool>>())
                : new ReadOnlyDictionary<string, Func<bool>>(new Dictionary<string, Func<bool>>(valueOverrideStateRefreshers));
            _valueOverrideToolTips = valueOverrideToolTips == null
                ? new Dictionary<string, string?>()
                : new Dictionary<string, string?>(valueOverrideToolTips);
            _valueOverrideToolTipRefreshers = valueOverrideToolTipRefreshers == null
                ? new ReadOnlyDictionary<string, Func<string?>>(new Dictionary<string, Func<string?>>())
                : new ReadOnlyDictionary<string, Func<string?>>(new Dictionary<string, Func<string?>>(valueOverrideToolTipRefreshers));
            _displayTexts = displayTexts == null
                ? new Dictionary<string, string?>()
                : new Dictionary<string, string?>(displayTexts);
            _displayTextRefreshers = displayTextRefreshers == null
                ? new ReadOnlyDictionary<string, Func<string?>>(new Dictionary<string, Func<string?>>())
                : new ReadOnlyDictionary<string, Func<string?>>(new Dictionary<string, Func<string?>>(displayTextRefreshers));
            _editTextGetters = editTextGetters == null
                ? new ReadOnlyDictionary<string, Func<string?>>(new Dictionary<string, Func<string?>>())
                : new ReadOnlyDictionary<string, Func<string?>>(new Dictionary<string, Func<string?>>(editTextGetters));
            _editTextSetters = editTextSetters == null
                ? new ReadOnlyDictionary<string, Action<string?>>(new Dictionary<string, Action<string?>>())
                : new ReadOnlyDictionary<string, Action<string?>>(new Dictionary<string, Action<string?>>(editTextSetters));
            _keyAliases = keyAliases == null
                ? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>())
                : new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(keyAliases));
            _logicalKeys = new ReadOnlyDictionary<string, string>(
                _keyAliases
                    .GroupBy(x => x.Value, StringComparer.Ordinal)
                    .ToDictionary(x => x.Key, x => x.First().Key, StringComparer.Ordinal));
            _parentOwnerAggregateStateGetter = parentOwnerAggregateStateGetter;
            _stateGetter = stateGetter;
            _childContentAggregateStateGetter = childContentAggregateStateGetter;
            _stateVisibilityToolTipGetter = stateVisibilityToolTipGetter;
            _cellChangedDispatcher = cellChangedDispatcher;

            Cells = new ReadOnlyDictionary<string, object?>(_cells);
            ValueOptions = new ReadOnlyDictionary<string, IEnumerable<object>?>(_valueOptions);
            CellEnabledStates = new ReadOnlyDictionary<string, bool>(_cellEnabledStates);
            CellToolTips = new ReadOnlyDictionary<string, string?>(_cellToolTips);
            ValueOverrideStates = new ReadOnlyDictionary<string, bool>(_valueOverrideStates);
            ValueOverrideToolTips = new ReadOnlyDictionary<string, string?>(_valueOverrideToolTips);
            DisplayTexts = new ReadOnlyDictionary<string, string?>(_displayTexts);
            EditText = new EditTextAccessor(this);
            SourceUuid = sourceUuid;
            CellChanged = cellChanged;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Uuid исходной доменной модели, которая представлена этой строкой.
        /// </summary>
        public Guid SourceUuid { get; }

        /// <summary>
        /// Текущие значения ячеек, индексированные техническими binding-ключами.
        /// </summary>
        public IReadOnlyDictionary<string, object?> Cells { get; }

        /// <summary>
        /// Допустимые значения combo-box ячеек, индексированные техническими binding-ключами.
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<object>?> ValueOptions { get; }

        /// <summary>
        /// Доступность интерактивного содержимого ячеек, индексированная binding-ключами.
        /// </summary>
        public IReadOnlyDictionary<string, bool> CellEnabledStates { get; }

        /// <summary>
        /// Подсказки состояния ячеек, индексированные binding-ключами.
        /// </summary>
        public IReadOnlyDictionary<string, string?> CellToolTips { get; }

        /// <summary>
        /// Признаки переопределения значений атрибутов, индексированные техническими binding-ключами.
        /// </summary>
        public IReadOnlyDictionary<string, bool> ValueOverrideStates { get; }

        /// <summary>
        /// Подсказки переопределения значений атрибутов, индексированные техническими binding-ключами.
        /// </summary>
        public IReadOnlyDictionary<string, string?> ValueOverrideToolTips { get; }

        /// <summary>
        /// Отображаемый текст значения атрибута (режим просмотра), индексированный binding-ключами.
        /// Эквивалент <c>DisplayedValueText</c> ячейки таблицы атрибутов.
        /// </summary>
        public IReadOnlyDictionary<string, string?> DisplayTexts { get; }

        /// <summary>
        /// Редактируемый текст значения атрибута (формула / ссылка «[uuid]»), привязывается TwoWay
        /// к <c>Text</c> редактируемого ComboBox — ПОЛНОСТЬЮ как в таблице атрибутов.
        /// </summary>
        public EditTextAccessor EditText { get; }

        public State ParentOwnerAggregateState => _parentOwnerAggregateStateGetter?.Invoke() ?? State.SavedOrLoaded;

        public State State => _stateGetter?.Invoke() ?? State.SavedOrLoaded;

        public State ChildContentAggregateState => _childContentAggregateStateGetter?.Invoke() ?? State.SavedOrLoaded;

        public string StateVisibilityToolTip => _stateVisibilityToolTipGetter?.Invoke() ?? string.Empty;

        private Action<Guid, string>? CellChanged { get; }

        /// <summary>
        /// Читает или записывает значение ячейки по логическому ключу колонки или по binding-ключу.
        /// </summary>
        public object? this[string key]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return null;
                }

                return Cells.TryGetValue(ResolveCellKey(key), out var value)
                    ? value
                    : null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return;
                }

                var cellKey = ResolveCellKey(key);
                if (_setters.TryGetValue(cellKey, out var setter) == false || setter == null)
                {
                    return;
                }

                _cells[cellKey] = setter(value);
                RaiseAfterCellChange(cellKey);
            }
        }

        /// <summary>
        /// Читает редактируемый текст значения атрибута по логическому ключу или binding-ключу.
        /// </summary>
        internal string? GetEditText(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return null;
            }

            return _editTextGetters.TryGetValue(ResolveCellKey(key), out var getter)
                ? getter()
                : null;
        }

        /// <summary>
        /// Разбирает введённый текст значения атрибута и применяет его к модели, затем запускает
        /// тот же каскад пересчёта/уведомлений, что и обычная правка ячейки.
        /// </summary>
        internal void SetEditText(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            var cellKey = ResolveCellKey(key);
            if (_editTextSetters.TryGetValue(cellKey, out var setter) == false)
            {
                return;
            }

            setter(value);
            RaiseAfterCellChange(cellKey);
        }

        /// <summary>
        /// Пересчитывает зависимые ячейки/подсветку/подсказки и поднимает уведомления после правки.
        /// </summary>
        private void RaiseAfterCellChange(string cellKey)
        {
            foreach (var refresher in _refreshers)
            {
                _cells[refresher.Key] = refresher.Value();
            }

            foreach (var refresher in _displayTextRefreshers)
            {
                _displayTexts[refresher.Key] = refresher.Value();
            }

            foreach (var refresher in _valueOverrideStateRefreshers)
            {
                _valueOverrideStates[refresher.Key] = refresher.Value();
            }

            foreach (var refresher in _valueOverrideToolTipRefreshers)
            {
                _valueOverrideToolTips[refresher.Key] = refresher.Value();
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cells)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayTexts)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueOverrideStates)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueOverrideToolTips)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ParentOwnerAggregateState)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(State)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ChildContentAggregateState)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StateVisibilityToolTip)));
            EditText.RaiseChanged();
            NotifyCellChanged(cellKey);
        }

        private void NotifyCellChanged(string cellKey)
        {
            var cellChanged = CellChanged;
            if (cellChanged == null)
            {
                return;
            }

            var sourceUuid = SourceUuid;
            var logicalKey = ResolveLogicalKey(cellKey);
            void Notify() => cellChanged(sourceUuid, logicalKey);

            if (_cellChangedDispatcher == null)
            {
                Notify();
                return;
            }

            // Обновление VM может заменить ItemsSource таблицы. Откладываем его до завершения
            // текущего цикла ввода, чтобы DataGrid успел штатно закрыть редактор ячейки.
            _cellChangedDispatcher(Notify);
        }

        private string ResolveCellKey(string key)
        {
            return _keyAliases.TryGetValue(key, out var bindingKey)
                ? bindingKey
                : key;
        }

        /// <summary>
        /// Возвращает логический ключ для уведомлений ViewModel после редактирования ячейки.
        /// </summary>
        private string ResolveLogicalKey(string cellKey)
        {
            return _logicalKeys.TryGetValue(cellKey, out var logicalKey)
                ? logicalKey
                : cellKey;
        }

        /// <summary>
        /// Индексируемый доступ к редактируемому тексту значения атрибута для XAML-привязки
        /// (<c>Text="{Binding EditText[ключ], Mode=TwoWay}"</c>). Делегирует в строку.
        /// </summary>
        public sealed class EditTextAccessor : INotifyPropertyChanged
        {
            private readonly ChildCollectionTableRow _row;

            internal EditTextAccessor(ChildCollectionTableRow row)
            {
                _row = row;
            }

            public event PropertyChangedEventHandler? PropertyChanged;

            public string? this[string key]
            {
                get => _row.GetEditText(key);
                set => _row.SetEditText(key, value);
            }

            internal void RaiseChanged()
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
            }
        }
    }
}
