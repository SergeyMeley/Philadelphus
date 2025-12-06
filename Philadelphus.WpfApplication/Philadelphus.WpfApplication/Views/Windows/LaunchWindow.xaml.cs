using Philadelphus.WpfApplication.ViewModels.ControlsVMs;
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

namespace Philadelphus.WpfApplication.Views.Windows
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
    }
}
