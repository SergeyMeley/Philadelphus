using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System.Windows.Controls;

namespace Philadelphus.Presentation.Wpf.UI.Views.Controls.RepositoryExplorer
{
    /// <summary>
    /// Логика взаимодействия для RepositoryExplorerLeavesList.xaml
    /// </summary>
    public partial class RepositoryExplorerLeavesList : UserControl
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RepositoryExplorerLeavesList" />.
        /// </summary>
        public RepositoryExplorerLeavesList()
        {
            InitializeComponent();
        }

        private RepositoryExplorerControlVM ViewModel
        {
            get
            {
                return (RepositoryExplorerControlVM)DataContext;
            }
        }

        private void SelectedElementLeavesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedElementLeavesDataGrid.SelectedItem is IMainEntityVM<IMainEntityModel> selectedLeave)
            {
                ViewModel.SelectedRepositoryMember = selectedLeave;
            }
        }
    }
}
