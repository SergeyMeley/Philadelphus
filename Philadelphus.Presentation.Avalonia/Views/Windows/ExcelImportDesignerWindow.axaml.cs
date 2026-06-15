using global::Avalonia.Controls;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Окно конструктора импорта из Excel. DataContext (ExcelImportDesignerVM) задаётся снаружи через IWindowService.
    /// Мосты событий контролов (завершение редактирования ячейки, вкл/выкл листа) будут добавлены при
    /// переносе DataGrid-части из WPF.
    /// </summary>
    public partial class ExcelImportDesignerWindow : Window
    {
        public ExcelImportDesignerWindow()
        {
            InitializeComponent();
        }
    }
}
