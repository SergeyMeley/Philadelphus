using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System.Windows;
using System.Windows.Controls;

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
