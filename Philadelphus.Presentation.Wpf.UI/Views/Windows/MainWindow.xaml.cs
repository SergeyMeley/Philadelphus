using System.ComponentModel;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Считаем ВСЕ открытые MainWindow, кроме текущего
            var otherMainWindows = Application.Current.Windows
                .OfType<MainWindow>()
                .Count(w => w != this);

            if (otherMainWindows == 0)
            {
                // Нет других — закрываем app целиком (игнорирует скрытые окна)
                Application.Current.Shutdown();
            }
            else
            {
                //// Есть другие — прячем текущее
                //this.Visibility = Visibility.Hidden;
                //e.Cancel = true;
            }

            base.OnClosing(e);
        }

    }
}