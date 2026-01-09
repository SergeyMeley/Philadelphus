using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using System.ComponentModel;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для LaunchWindow.xaml
    /// </summary>
    public partial class LaunchWindow : Window
    {
        public LaunchWindow(LaunchWindowVM vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            var mainWindows = Application.Current.Windows
                .OfType<MainWindow>()
                .Count();

            if (mainWindows != 0)
            {
                this.Visibility = Visibility.Hidden;
                e.Cancel = true;
            }

            base.OnClosing(e);
        }
    }
}
