using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Philadelphus.WpfApplication.Views.Controls.CollectionControls.ListBoxes
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
