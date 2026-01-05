using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Philadelphus.Presentation.Wpf.UI.Views.Controls.CollectionControls.ListBoxes
{
    /// <summary>
    /// Логика взаимодействия для AvailableRepositoriesListBox.xaml
    /// </summary>
    public partial class AvailableRepositoriesListBox : UserControl
    {
        public static readonly DependencyProperty RepositoriesItemsSourceProperty =
            DependencyProperty.Register("RepositoriesItemsSource", typeof(IEnumerable),
            typeof(AvailableRepositoriesListBox), new PropertyMetadata(null));

        public static readonly DependencyProperty RepositoriesSelectedItemProperty =
            DependencyProperty.Register("RepositoriesSelectedItem", typeof(object),
            typeof(AvailableRepositoriesListBox), new PropertyMetadata(null));
        public IEnumerable RepositoriesItemsSource
        {
            get { return (IEnumerable)GetValue(RepositoriesItemsSourceProperty); }
            set { SetValue(RepositoriesItemsSourceProperty, value); }
        }
        public object RepositoriesSelectedItem
        {
            get { return GetValue(RepositoriesSelectedItemProperty); }
            set { SetValue(RepositoriesSelectedItemProperty, value); }
        }

        public AvailableRepositoriesListBox()
        {
            InitializeComponent();
        }
    }
}
