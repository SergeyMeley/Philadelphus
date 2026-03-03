using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
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

namespace Philadelphus.Presentation.Wpf.UI.Views.Controls.RepositoryExplorer
{
    /// <summary>
    /// Логика взаимодействия для RepositoryExplorerShrub.xaml
    /// </summary>
    public partial class RepositoryExplorerShrub : UserControl
    {
        public RepositoryExplorerControlVM ViewModel { get { return (RepositoryExplorerControlVM)DataContext; } }
        public RepositoryExplorerShrub()
        {
            InitializeComponent();
        }

        private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.SelectedRepositoryMember = (MainEntityBaseVM)MainTreeView.SelectedItem;
        }
    }
}
