using System.Windows;
using System.Windows.Controls;

namespace Philadelphus.Presentation.Wpf.UI.Views.Controls.DetailedInformationGrids
{
    /// <summary>
    /// Логика взаимодействия для DataStorageDetails.xaml
    /// </summary>
    public partial class DataStorageDetails : UserControl
    {
        //TODO: Мб удалить
        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register("IsReadOnly", typeof(bool),
            typeof(MainEntityDetails), new PropertyMetadata(null));
        public bool IsReadOnly
        {
            get { return (bool)GetValue(IsReadOnlyProperty); }
            set { SetValue(IsReadOnlyProperty, value); }
        }

        public DataStorageDetails()
        {
            InitializeComponent();
        }
    }
}
