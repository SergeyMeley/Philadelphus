using Microsoft.Extensions.DependencyInjection;

using Philadelphus.Core.Domain.ImportExport.Contracts;
using Philadelphus.Core.Domain.ImportExport.Services.Implementations;
using Philadelphus.Core.Domain.ImportExport.Services.Interfaces;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Infrastructure.ImportExport.Phjson;
using Philadelphus.Presentation.Avalonia.Services;
using Philadelphus.Presentation.Avalonia.Views.Windows;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.ImportExport;

namespace Philadelphus.Presentation.Avalonia.Infrastructure.Startup
{
    internal static class ImportExportServiceCollectionExtensions
    {
        public static void RegisterImportExport(IServiceCollection services)
        {
            RegisterImportExportCore(services);
            RegisterImportExportAdapters(services);
            RegisterExcelImportInfrastructure(services);
            RegisterExcelImportPresentation(services);
        }

        private static void RegisterImportExportCore(IServiceCollection services)
        {
            services.AddTransient<IImportExportService, ImportExportService>();
        }

        private static void RegisterImportExportAdapters(IServiceCollection services)
        {
            services.AddTransient<IImportExportAdapter, JsonImportExportAdapter>();
        }

        private static void RegisterExcelImportInfrastructure(IServiceCollection services)
        {
            services.AddSingleton<ConversionService>();
            services.AddSingleton<ExcelPreviewService>();
            services.AddSingleton<IExcelDataTypeDetector, ExcelDataTypeDetector>();
            services.AddSingleton<IExcelImportSourceReader, ExcelImportSourceReader>();
            services.AddSingleton<IExcelImportSchemaBuilder, ExcelImportSchemaBuilder>();
            services.AddSingleton<IExcelImportProfileResolver, ExcelImportProfileResolver>();
            services.AddSingleton<IExcelImportProfileValidator, ExcelImportProfileValidator>();
            services.AddSingleton<IExcelImportInheritanceResolver, ExcelImportInheritanceResolver>();
            services.AddSingleton<IExcelImportSettingsReader, ExcelImportSettingsReader>();
            services.AddSingleton<IExcelImportSchemaTemplateStorage, ExcelImportSchemaTemplateStorage>();

            services.AddTransient<IImportExportAdapter, ExcelImportExportAdapter>();
            services.AddTransient<ExcelImportExportAdapter>();
            services.AddTransient<ExcelImportPipeline>();
            services.AddTransient<ExcelImportSessionState>();
        }

        private static void RegisterExcelImportPresentation(IServiceCollection services)
        {
            services.AddSingleton<IFileDialogService, AvaloniaFileDialogService>();
            services.AddSingleton<IMessageDialogService, AvaloniaMessageDialogService>();
            services.AddTransient<IImportProgressReporter, AvaloniaImportProgressReporter>();

            services.AddTransient<ImportFromExcelWindow>();
            services.AddTransient<ExcelImportDesignerWindow>();
            services.AddTransient<ImportProgressWindow>();
        }
    }
}
