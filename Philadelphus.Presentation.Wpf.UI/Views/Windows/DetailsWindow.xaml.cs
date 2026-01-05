using Philadelphus.Presentation.Wpf.UI.ViewModels;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для DetailsWindow.xaml
    /// </summary>
    public partial class DetailsWindow : Window
    {
        public DetailsWindow(ViewModelBase vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
