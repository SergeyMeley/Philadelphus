using Philadelphus.Presentation.ViewModels.ImportExport;
using System.Windows;
using System.Windows.Controls;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    /// <summary>
    /// Окно конструктора импорта из Excel. Логика и состояние — в <see cref="ExcelImportDesignerVM" />
    /// (DataContext), диаграмма связей — в DiagramBehavior. В code-behind остались только мосты
    /// событий контролов, не выражаемых биндингом (завершение редактирования ячейки, вкл/выкл листа).
    /// </summary>
    public partial class ExcelImportDesignerWindow : Window
    {
        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ExcelImportDesignerWindow" />.
        /// DataContext (ExcelImportDesignerVM) задаётся снаружи через IWindowService.
        /// </summary>
        public ExcelImportDesignerWindow()
        {
            InitializeComponent();
        }

        private void DgSheetColumns_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(
                new Action(() => (DataContext as ExcelImportDesignerVM)?.OnSheetColumnEdited()),
                System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SheetEnabledChanged(object sender, RoutedEventArgs e)
        {
            (DataContext as ExcelImportDesignerVM)?.OnSheetEnabledChanged();
        }
    }
}
