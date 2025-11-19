using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs;
using System.Windows;
using System.Windows.Controls;

namespace Philadelphus.WpfApplication.Views.Controls
{
    /// <summary>
    /// Логика взаимодействия для RepositoryExplorer.xaml
    /// </summary>
    public partial class RepositoryExplorer : UserControl
    {
        public TreeRepositoryVM ViewModel { get { return (TreeRepositoryVM)DataContext; } }
        public RepositoryExplorer()
        {
            InitializeComponent();
        }

        private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.SelectedRepositoryMember = (MainEntityBaseVM)MainTreeView.SelectedItem;
        }
    }
}
