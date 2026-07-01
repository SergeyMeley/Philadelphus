using System.Collections;

using global::Avalonia;
using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Controls.CollectionControls.ListBoxes
{
    /// <summary>
    /// Логика взаимодействия для RepositoryHeadersListBox.axaml
    /// </summary>
    public partial class RepositoryHeadersListBox : UserControl
    {
        public static readonly StyledProperty<IEnumerable?> RepositoryHeadersItemsSourceProperty =
            AvaloniaProperty.Register<RepositoryHeadersListBox, IEnumerable?>(nameof(RepositoryHeadersItemsSource));

        public static readonly StyledProperty<object?> RepositoryHeadersSelectedItemProperty =
            AvaloniaProperty.Register<RepositoryHeadersListBox, object?>(
                nameof(RepositoryHeadersSelectedItem),
                defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

        public IEnumerable? RepositoryHeadersItemsSource
        {
            get => GetValue(RepositoryHeadersItemsSourceProperty);
            set => SetValue(RepositoryHeadersItemsSourceProperty, value);
        }

        public object? RepositoryHeadersSelectedItem
        {
            get => GetValue(RepositoryHeadersSelectedItemProperty);
            set => SetValue(RepositoryHeadersSelectedItemProperty, value);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RepositoryHeadersListBox" />.
        /// </summary>
        public RepositoryHeadersListBox()
        {
            InitializeComponent();
        }
    }
}
