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

        public ChildCollectionTableRow(
            IReadOnlyDictionary<string, object?> cells,
            IReadOnlyDictionary<string, Func<object?, object?>?>? setters = null,
            IReadOnlyDictionary<string, IEnumerable<object>?>? valueOptions = null,
            IReadOnlyDictionary<string, Func<object?>>? refreshers = null,
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

                return Cells.TryGetValue(key, out var value)
                    ? value
                    : null;
            }
            set
            {
                if (string.IsNullOrWhiteSpace(key))
                {
                    return;
                }

                if (_setters.TryGetValue(key, out var setter) == false || setter == null)
                {
                    return;
                }

                _cells[key] = setter(value);
                foreach (var refresher in _refreshers)
                {
                    _cells[refresher.Key] = refresher.Value();
                }

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Item[]"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Cells)));
                CellChanged?.Invoke(SourceUuid, key);
            }
        }
    }
}
