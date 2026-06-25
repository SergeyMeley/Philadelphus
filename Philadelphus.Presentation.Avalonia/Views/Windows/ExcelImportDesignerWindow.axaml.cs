using global::Avalonia.Controls;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;

using Philadelphus.Presentation.ViewModels.ImportExport;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Окно конструктора импорта из Excel. Логика и состояние — в <see cref="ExcelImportDesignerVM" />
    /// (DataContext, задаётся снаружи через IWindowService). В code-behind — только мосты событий
    /// контролов, не выражаемых биндингом (завершение редактирования ячейки, вкл/выкл листа),
    /// и список типов данных для combo (в WPF был x:Array-ресурсом).
    /// </summary>
    public partial class ExcelImportDesignerWindow : Window
    {
        /// <summary>
        /// Доступные типы данных колонки (в WPF — статический ресурс AvailableDataTypesResource).
        /// </summary>
        public string[] DataTypeOptions { get; } = { "Текст", "Число", "Целое число", "Дробное число" };

        public ExcelImportDesignerWindow()
        {
            InitializeComponent();
        }

        private void DgSheetColumns_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            // Откладываем: к моменту вызова правка ячейки уже зафиксирована в источнике.
            Dispatcher.UIThread.Post(
                () => (DataContext as ExcelImportDesignerVM)?.OnSheetColumnEdited(),
                DispatcherPriority.Background);
        }

        private void SheetEnabledChanged(object? sender, RoutedEventArgs e)
        {
            (DataContext as ExcelImportDesignerVM)?.OnSheetEnabledChanged();
        }
    }
}
