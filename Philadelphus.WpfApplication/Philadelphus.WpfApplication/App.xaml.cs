using Philadelphus.WpfApplication.ViewModels;
using System.Configuration;
using System.Data;
using System.Windows;

namespace Philadelphus.WpfApplication
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        ApplicationViewModel _viewModel;
        public App()
        {
            _viewModel = new ApplicationViewModel();
        }
    }

}
