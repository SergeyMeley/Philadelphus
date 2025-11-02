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
        public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register("ItemsSource", typeof(IEnumerable),
        typeof(RepositoryHeadersListBox), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object),
            typeof(RepositoryHeadersListBox), new PropertyMetadata(null));

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
        public RepositoryHeadersListBox()
        {
            InitializeComponent();
        }
    }
}
