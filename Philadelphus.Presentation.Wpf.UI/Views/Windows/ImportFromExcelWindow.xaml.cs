using Philadelphus.Presentation.Wpf.UI.ViewModels.ImportExport;
using System.Windows;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    public partial class ImportFromExcelWindow : Window
    {
        private readonly ImportFromExcelViewModel _viewModel;

        public ImportFromExcelWindow(ImportFromExcelViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        internal void InitializeForConvert()
        {
            _viewModel.InitializeForConvert();
            ConfigureWindowAppearance();
        }

        private void ConfigureWindowAppearance()
        {
            Title = "Конвертер Excel -> PHJSON";
            TxtWizardIntro.Text = "Конвертер Excel в PHJSON.";
            BtnMainAction.Content = "Конвертировать в PHJSON";
            GroupRootSettings.Header = "Параметры PHJSON";
            GroupSourceSelection.Header = "Выбор источников Excel";
            ChkUseAllWorksheets.Content = "Конвертировать все источники книги";
        }
    }
}
