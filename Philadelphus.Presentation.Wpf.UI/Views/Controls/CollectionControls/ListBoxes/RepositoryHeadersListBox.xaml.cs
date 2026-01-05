using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace Philadelphus.Presentation.Wpf.UI.Views.Controls.CollectionControls.ListBoxes
{
    /// <summary>
    /// Логика взаимодействия для RepositoryHeadersListBox.xaml
    /// </summary>
    public partial class RepositoryHeadersListBox : UserControl
    {
        public static readonly DependencyProperty RepositoryHeadersItemsSourceProperty =
        DependencyProperty.Register("RepositoryHeadersItemsSource", typeof(IEnumerable),
        typeof(RepositoryHeadersListBox), new PropertyMetadata(null));

        public static readonly DependencyProperty RepositoryHeadersSelectedItemProperty =
            DependencyProperty.Register("RepositoryHeadersSelectedItem", typeof(object),
            typeof(RepositoryHeadersListBox), new PropertyMetadata(null));

        public IEnumerable RepositoryHeadersItemsSource
        {
            get { return (IEnumerable)GetValue(RepositoryHeadersItemsSourceProperty); }
            set { SetValue(RepositoryHeadersItemsSourceProperty, value); }
        }

        public object RepositoryHeadersSelectedItem
        {
            get { return GetValue(RepositoryHeadersSelectedItemProperty); }
            set { SetValue(RepositoryHeadersSelectedItemProperty, value); }
        }
        public RepositoryHeadersListBox()
        {
            InitializeComponent();
        }
    }
}
