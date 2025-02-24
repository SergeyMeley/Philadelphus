using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.WpfApplication.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Philadelphus.WpfApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ApplicationViewModel _viewModel = new ApplicationViewModel();
        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
            var qwe = MainTreeView.SelectedItem;
        }

        private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _viewModel.SelectedEntity = (IMainEntity)MainTreeView.SelectedItem;
        }
    }
}