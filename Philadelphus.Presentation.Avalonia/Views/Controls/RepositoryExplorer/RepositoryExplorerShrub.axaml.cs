using global::Avalonia.Controls;

using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;

namespace Philadelphus.Presentation.Avalonia.Views.Controls.RepositoryExplorer
{
    /// <summary>
    /// Логика взаимодействия для RepositoryExplorerShrub.axaml.
    /// </summary>
    public partial class RepositoryExplorerShrub : UserControl
    {
        /// <summary>
        /// Модель представления обозревателя репозитория.
        /// </summary>
        public RepositoryExplorerControlVM? ViewModel => DataContext as RepositoryExplorerControlVM;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RepositoryExplorerShrub" />.
        /// </summary>
        public RepositoryExplorerShrub()
        {
            InitializeComponent();
        }

        private void MainTreeView_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ViewModel is { } viewModel)
            {
                viewModel.SelectedRepositoryTreeMember = MainTreeView.SelectedItem as IMainEntityVM<IMainEntityModel>;
            }
        }
    }
}
