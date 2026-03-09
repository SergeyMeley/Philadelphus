using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Presentation.Wpf.UI.ViewModels;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для DetailsWindow.xaml
    /// </summary>
    public partial class DetailsWindow : Window
    {
        public DetailsWindow(IMainEntityVM<IMainEntityModel> vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
