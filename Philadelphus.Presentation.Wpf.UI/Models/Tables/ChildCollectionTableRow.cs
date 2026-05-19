using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Philadelphus.Presentation.Wpf.UI.Models.Tables
{
    /// <summary>
    /// Row values for child collection table.
    /// </summary>
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

        public Guid SourceUuid { get; }

        public IReadOnlyDictionary<string, object?> Cells { get; }

        public IReadOnlyDictionary<string, IEnumerable<object>?> ValueOptions { get; }

        private Action<Guid, string>? CellChanged { get; }

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

        private string ResolveLogicalKey(string cellKey)
        {
            return _logicalKeys.TryGetValue(cellKey, out var logicalKey)
                ? logicalKey
                : cellKey;
        }
    }
}
