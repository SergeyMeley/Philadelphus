using Philadelphus.WpfApplication.ViewModels.MainEntitiesVMs;
using System.Windows;

namespace Philadelphus.WpfApplication.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для RepositorySettingsWindow.xaml
    /// </summary>
    public partial class RepositorySettingsWindow : Window
    {
        public RepositorySettingsWindow(TreeRepositoryCollectionVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
