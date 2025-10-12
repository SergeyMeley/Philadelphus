using Philadelphus.WpfApplication.ViewModels.MainEntitiesViewModels;
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

namespace Philadelphus.WpfApplication.Views
{
    /// <summary>
    /// Логика взаимодействия для RepositorySettingsWindow.xaml
    /// </summary>
    public partial class RepositorySettingsWindow : Window
    {
        internal RepositoryCollectionVM ViewModel { get { return (RepositoryCollectionVM)DataContext; } }
        public RepositorySettingsWindow()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }
    }
}
