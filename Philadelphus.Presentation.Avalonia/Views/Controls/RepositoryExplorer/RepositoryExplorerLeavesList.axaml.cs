using global::Avalonia.Controls;

using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;

namespace Philadelphus.Presentation.Avalonia.Views.Controls.RepositoryExplorer
{
    /// <summary>
    /// Логика взаимодействия для RepositoryExplorerLeavesList.axaml.
    /// </summary>
    public partial class RepositoryExplorerLeavesList : UserControl
    {
        private RepositoryExplorerControlVM? ViewModel => DataContext as RepositoryExplorerControlVM;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RepositoryExplorerLeavesList" />.
        /// </summary>
        public RepositoryExplorerLeavesList()
        {
            InitializeComponent();
        }

        private void SelectedElementLeavesDataGrid_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (ViewModel is { } viewModel
                && SelectedElementLeavesDataGrid.SelectedItem is IMainEntityVM<IMainEntityModel> selectedLeave)
            {
                viewModel.SelectedRepositoryMember = selectedLeave;
            }
        }
    }
}
