using Microsoft.Extensions.DependencyInjection;

using Philadelphus.Presentation.Avalonia.Services;
using Philadelphus.Presentation.Avalonia.Views.Windows;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.ImportExport;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Startup
{
    internal static class AvaloniaWindowRegistry
    {
        public static void Register(IServiceProvider services)
        {
            var windowService = services.GetRequiredService<AvaloniaWindowService>();
            windowService.Register<MainWindowVM, MainWindow>();
            windowService.Register<LaunchWindowVM, LaunchWindow>();
            windowService.Register<FormulaTestControlVM, FormulaEditorWindow>();
            windowService.Register<RepositoryExplorerControlVM, AttributeValuesCollectionWindow>();
            windowService.Register<DetailsWindowVM, DetailsWindow>();
            windowService.Register<AboutWindowVM, AboutWindow>();
            windowService.Register<ExcelImportDesignerVM, ExcelImportDesignerWindow>();
        }

        public static LaunchWindow CreateLaunchWindow(IServiceProvider services)
        {
            var launchWindow = services.GetRequiredService<LaunchWindow>();
            launchWindow.DataContext = services.GetRequiredService<LaunchWindowVM>();

            return launchWindow;
        }
    }
}
