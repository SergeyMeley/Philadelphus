using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Presentation.Wpf.UI.Models.Tables
{
    /// <summary>
    /// Column descriptor for child collection table.
    /// </summary>
    public sealed class ChildCollectionTableColumn
    {
        public ChildCollectionTableColumn(
            string key,
            string header,
            int order,
            Func<IChildrenModel, object?> valueGetter,
            bool isReadOnly = true,
            bool isAttribute = false,
            Func<IChildrenModel, Func<object?, object?>?>? setterFactory = null,
            Func<IChildrenModel, IEnumerable<object>?>? valueOptionsGetter = null,
            string? bindingKey = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(header);
            ArgumentOutOfRangeException.ThrowIfNegative(order);
            ArgumentNullException.ThrowIfNull(valueGetter);

            Key = key;
            BindingKey = string.IsNullOrWhiteSpace(bindingKey) ? key : bindingKey;
            Header = header;
            Order = order;
            ValueGetter = valueGetter;
            IsReadOnly = isReadOnly;
            IsAttribute = isAttribute;
            SetterFactory = setterFactory;
            ValueOptionsGetter = valueOptionsGetter;
        }

        public string Key { get; }

        public string BindingKey { get; }

        public string Header { get; }

        public int Order { get; }

        public bool IsReadOnly { get; }

        public bool IsAttribute { get; }

        private Func<IChildrenModel, object?> ValueGetter { get; }

        private Func<IChildrenModel, Func<object?, object?>?>? SetterFactory { get; }

        private Func<IChildrenModel, IEnumerable<object>?>? ValueOptionsGetter { get; }

        public bool HasValueOptions => ValueOptionsGetter != null;

        public object? GetValue(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            return ValueGetter(child);
        }

        public Func<object?, object?>? GetSetter(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            return IsReadOnly
                ? null
                : SetterFactory?.Invoke(child);
        }

        public IEnumerable<object>? GetValueOptions(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            return ValueOptionsGetter?.Invoke(child);
        }
    }
}
