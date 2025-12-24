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
    /// Логика взаимодействия для DataStorageHeadersListBox.xaml
    /// </summary>
    public partial class DataStorageHeadersListBox : UserControl
    {
        public static readonly DependencyProperty DataStorageHeadersItemsSourceProperty =
        DependencyProperty.Register("DataStorageHeadersItemsSource", typeof(IEnumerable),
        typeof(DataStorageHeadersListBox), new PropertyMetadata(null));

        public static readonly DependencyProperty DataStorageHeadersSelectedItemProperty =
            DependencyProperty.Register("DataStorageHeadersSelectedItem", typeof(object),
            typeof(DataStorageHeadersListBox), new PropertyMetadata(null));

        public IEnumerable DataStorageHeadersItemsSource
        {
            get { return (IEnumerable)GetValue(DataStorageHeadersItemsSourceProperty); }
            set { SetValue(DataStorageHeadersItemsSourceProperty, value); }
        }

        public object DataStorageHeadersSelectedItem
        {
            get { return GetValue(DataStorageHeadersSelectedItemProperty); }
            set { SetValue(DataStorageHeadersSelectedItemProperty, value); }
        }
        
        public DataStorageHeadersListBox()
        {
            InitializeComponent();
        }
    }
}
