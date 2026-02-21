using Microsoft.Extensions.DependencyInjection;
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
using System.Windows.Shapes;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        public SplashWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider; 

            InitializeComponent();

            Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Даём время UI отрисоваться
            await Task.Delay(100);

            // Запускаем основное приложение
            var window = _serviceProvider.GetRequiredService<LaunchWindow>();
            window.Topmost = true;
            window.Show();
            window.Activate();
            window.Topmost = false;

            Close();
        }
    }
}
