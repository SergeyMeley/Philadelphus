using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Philadelphus.Presentation.Wpf.UI.Models.Tables
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
        private readonly IReadOnlyDictionary<string, Func<object?>> _refreshers;
        private readonly IReadOnlyDictionary<string, string> _keyAliases;
        private readonly IReadOnlyDictionary<string, string> _logicalKeys;

        public ChildCollectionTableRow(
            IReadOnlyDictionary<string, object?> cells,
            IReadOnlyDictionary<string, Func<object?, object?>?>? setters = null,
            IReadOnlyDictionary<string, IEnumerable<object>?>? valueOptions = null,
            IReadOnlyDictionary<string, Func<object?>>? refreshers = null,
            IReadOnlyDictionary<string, string>? keyAliases = null,
            Guid sourceUuid = default,
            Action<Guid, string>? cellChanged = null)
        {
            ArgumentNullException.ThrowIfNull(cells);

            _cells = new Dictionary<string, object?>(cells);
            _setters = setters == null
                ? new ReadOnlyDictionary<string, Func<object?, object?>?>(new Dictionary<string, Func<object?, object?>?>())
                : new ReadOnlyDictionary<string, Func<object?, object?>?>(new Dictionary<string, Func<object?, object?>?>(setters));
            _valueOptions = valueOptions == null
                ? new Dictionary<string, IEnumerable<object>?>()
                : new Dictionary<string, IEnumerable<object>?>(valueOptions);
            _refreshers = refreshers == null
                ? new ReadOnlyDictionary<string, Func<object?>>(new Dictionary<string, Func<object?>>())
                : new ReadOnlyDictionary<string, Func<object?>>(new Dictionary<string, Func<object?>>(refreshers));
            _keyAliases = keyAliases == null
                ? new ReadOnlyDictionary<string, string>(new Dictionary<string, string>())
                : new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(keyAliases));
            _logicalKeys = new ReadOnlyDictionary<string, string>(
                _keyAliases
                    .GroupBy(x => x.Value, StringComparer.Ordinal)
                    .ToDictionary(x => x.Key, x => x.First().Key, StringComparer.Ordinal));

            Cells = new ReadOnlyDictionary<string, object?>(_cells);
            ValueOptions = new ReadOnlyDictionary<string, IEnumerable<object>?>(_valueOptions);
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
                foreach (var refresher in _refreshers)
                {
                    _cells[refresher.Key] = refresher.Value();
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cells)));
                CellChanged?.Invoke(SourceUuid, ResolveLogicalKey(cellKey));
            }
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
    }
}
