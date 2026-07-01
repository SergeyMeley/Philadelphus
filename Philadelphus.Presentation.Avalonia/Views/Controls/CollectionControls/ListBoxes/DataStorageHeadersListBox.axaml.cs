using System.Collections;

using global::Avalonia;
using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Controls.CollectionControls.ListBoxes
{
    /// <summary>
    /// Логика взаимодействия для DataStorageHeadersListBox.axaml
    /// </summary>
    public partial class DataStorageHeadersListBox : UserControl
    {
        public static readonly StyledProperty<IEnumerable?> DataStorageHeadersItemsSourceProperty =
            AvaloniaProperty.Register<DataStorageHeadersListBox, IEnumerable?>(nameof(DataStorageHeadersItemsSource));

        public static readonly StyledProperty<object?> DataStorageHeadersSelectedItemProperty =
            AvaloniaProperty.Register<DataStorageHeadersListBox, object?>(
                nameof(DataStorageHeadersSelectedItem),
                defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

        public IEnumerable? DataStorageHeadersItemsSource
        {
            get => GetValue(DataStorageHeadersItemsSourceProperty);
            set => SetValue(DataStorageHeadersItemsSourceProperty, value);
        }

        public object? DataStorageHeadersSelectedItem
        {
            get => GetValue(DataStorageHeadersSelectedItemProperty);
            set => SetValue(DataStorageHeadersSelectedItemProperty, value);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="DataStorageHeadersListBox" />.
        /// </summary>
        public DataStorageHeadersListBox()
        {
            InitializeComponent();
        }
    }
}
