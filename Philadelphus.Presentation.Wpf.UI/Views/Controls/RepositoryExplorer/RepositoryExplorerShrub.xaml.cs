using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
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
        /// <summary>
        /// Выполняет операцию ViewModel.
        /// </summary>
        /// <param name="RepositoryExplorerControlVM">Параметр RepositoryExplorerControlVM.</param>
        /// <returns>Результат выполнения операции.</returns>
        public RepositoryExplorerControlVM ViewModel { get { return (RepositoryExplorerControlVM)DataContext; } }
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RepositoryExplorerShrub" />.
        /// </summary>
        public RepositoryExplorerShrub()
        {
            InitializeComponent();
        }

        private void MainTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.SelectedRepositoryMember = MainTreeView.SelectedItem as IMainEntityVM<IMainEntityModel>;
        }
    }
}
