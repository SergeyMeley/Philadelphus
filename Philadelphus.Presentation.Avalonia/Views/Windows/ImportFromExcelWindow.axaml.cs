using global::Avalonia.Controls;

using Philadelphus.Presentation.ViewModels.ImportExport;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Окно мастера конвертации Excel → PHJSON. VM инжектируется через DI (как в WPF).
    /// </summary>
    public partial class ImportFromExcelWindow : Window
    {
        private readonly ImportFromExcelVM _viewModel;

        public ImportFromExcelWindow(ImportFromExcelVM viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        /// <summary>
        /// Перевод окна в режим «конвертация» (как в WPF). Тонкая настройка надписей в каркасе опущена —
        /// будет добавлена при переносе разметки мастера.
        /// </summary>
        internal void InitializeForConvert()
        {
            _viewModel.InitializeForConvert();
            Title = "Конвертер Excel -> PHJSON";
        }
    }
}
