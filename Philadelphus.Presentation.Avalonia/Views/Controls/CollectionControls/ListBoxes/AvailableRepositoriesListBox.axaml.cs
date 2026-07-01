using System.Collections;

using global::Avalonia;
using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Controls.CollectionControls.ListBoxes
{
    /// <summary>
    /// Логика взаимодействия для AvailableRepositoriesListBox.axaml
    /// </summary>
    public partial class AvailableRepositoriesListBox : UserControl
    {
        public static readonly StyledProperty<IEnumerable?> RepositoriesItemsSourceProperty =
            AvaloniaProperty.Register<AvailableRepositoriesListBox, IEnumerable?>(nameof(RepositoriesItemsSource));

        public static readonly StyledProperty<object?> RepositoriesSelectedItemProperty =
            AvaloniaProperty.Register<AvailableRepositoriesListBox, object?>(
                nameof(RepositoriesSelectedItem),
                defaultBindingMode: global::Avalonia.Data.BindingMode.TwoWay);

        public IEnumerable? RepositoriesItemsSource
        {
            get => GetValue(RepositoriesItemsSourceProperty);
            set => SetValue(RepositoriesItemsSourceProperty, value);
        }

        public object? RepositoriesSelectedItem
        {
            get => GetValue(RepositoriesSelectedItemProperty);
            set => SetValue(RepositoriesSelectedItemProperty, value);
        }

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="AvailableRepositoriesListBox" />.
        /// </summary>
        public AvailableRepositoriesListBox()
        {
            InitializeComponent();
        }
    }
}
