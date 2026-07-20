using Philadelphus.Core.Domain.Interfaces;

namespace Philadelphus.Presentation.Models.Tables
{
    /// <summary>
    /// Описывает колонку таблицы наследников выбранного элемента.
    /// </summary>
    /// <remarks>
    /// Модель не зависит от WPF: она хранит логический ключ колонки, порядок,
    /// правила чтения/записи значения и источник допустимых значений для combo-box ячеек.
    /// </remarks>
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
            string? bindingKey = null,
            string? headerToolTip = null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(key);
            ArgumentException.ThrowIfNullOrWhiteSpace(header);
            ArgumentOutOfRangeException.ThrowIfNegative(order);
            ArgumentNullException.ThrowIfNull(valueGetter);

            Key = key;
            BindingKey = string.IsNullOrWhiteSpace(bindingKey) ? key : bindingKey;
            Header = header;
            HeaderToolTip = string.IsNullOrWhiteSpace(headerToolTip) ? null : headerToolTip;
            Order = order;
            ValueGetter = valueGetter;
            IsReadOnly = isReadOnly;
            IsAttribute = isAttribute;
            SetterFactory = setterFactory;
            ValueOptionsGetter = valueOptionsGetter;
        }

        /// <summary>
        /// Логический ключ колонки: имя свойства, полный путь свойства или DeclaringUuid атрибута.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Технический ключ для binding-индексаторов WPF.
        /// </summary>
        /// <remarks>
        /// Для динамических атрибутов отличается от <see cref="Key"/>, чтобы отделить доменную
        /// идентификацию колонки от технического пути binding-а.
        /// </remarks>
        public string BindingKey { get; }

        /// <summary>
        /// Отображаемый заголовок колонки.
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Подсказка заголовка, обычно берется из DisplayAttribute.Description.
        /// </summary>
        public string? HeaderToolTip { get; }

        /// <summary>
        /// Стабильный порядок отображения колонки.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Признак запрета редактирования ячеек колонки.
        /// </summary>
        public bool IsReadOnly { get; }

        /// <summary>
        /// Признак динамической колонки атрибута.
        /// </summary>
        public bool IsAttribute { get; }

        private Func<IChildrenModel, object?> ValueGetter { get; }

        private Func<IChildrenModel, Func<object?, object?>?>? SetterFactory { get; }

        private Func<IChildrenModel, IEnumerable<object>?>? ValueOptionsGetter { get; }

        /// <summary>
        /// Возвращает true, если колонка должна отображаться как выбор из списка.
        /// </summary>
        public bool HasValueOptions => ValueOptionsGetter != null;

        /// <summary>
        /// Получает отображаемое значение ячейки для конкретного наследника.
        /// </summary>
        public object? GetValue(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            return ValueGetter(child);
        }

        /// <summary>
        /// Возвращает setter ячейки для конкретного наследника или null для readonly-значений.
        /// </summary>
        public Func<object?, object?>? GetSetter(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            return IsReadOnly
                ? null
                : SetterFactory?.Invoke(child);
        }

        /// <summary>
        /// Получает список допустимых значений для combo-box ячейки.
        /// </summary>
        public IEnumerable<object>? GetValueOptions(IChildrenModel child)
        {
            ArgumentNullException.ThrowIfNull(child);

            return ValueOptionsGetter?.Invoke(child);
        }
    }
}
