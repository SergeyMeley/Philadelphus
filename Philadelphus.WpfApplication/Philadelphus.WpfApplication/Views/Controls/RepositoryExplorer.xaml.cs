using Philadelphus.Business.Entities.MainEntities;
using Philadelphus.WpfApplication.ViewModels;
using System;
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

namespace Philadelphus.WpfApplication.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для RepositoryExplorer.xaml
    /// </summary>
    public partial class RepositoryExplorer : UserControl
    {
        public RepositoryExplorerViewModel ViewModel { get { return (RepositoryExplorerViewModel)DataContext; } }
        public RepositoryExplorer()
        {
            InitializeComponent();
        }

        private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.SelectedEntity = (IMainEntity)MainTreeView.SelectedItem;
        }
    }
}
