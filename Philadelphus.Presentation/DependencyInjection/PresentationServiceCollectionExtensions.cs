using Philadelphus.Presentation.Factories.Implementations;
using Philadelphus.Presentation.Factories.Interfaces;
using Philadelphus.Presentation.Services;
using Philadelphus.Presentation.Services.Implementations;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.ControlsVMs.NotificationsVMs;
using Philadelphus.Presentation.ViewModels.ControlsVMs.TabItemsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.ViewModels.ImportExport;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Регистрация платформо-нейтрального презентационного слоя (ViewModel, фабрики, сервисы).
    /// Платформенные реализации (IWindowService, IDialogService, IFileDialogService,
    /// IImportProgressReporter, командные фабрики, View) регистрируются в конкретном приложении.
    /// </summary>
    public static class PresentationServiceCollectionExtensions
    {
        /// <summary>
        /// Регистрирует общие сервисы, ViewModel и фабрики проекта Philadelphus.Presentation.
        /// </summary>
        public static IServiceCollection AddPhiladelphusPresentation(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Сервисы презентации
            services.AddSingleton<IConfigurationService, ConfigurationService>();

            // Регистрация ViewModel
            // Общие ViewModel
            services.AddSingleton<ApplicationVM>();
            services.AddSingleton<ApplicationCommandsVM>();
            services.AddSingleton<IApplicationCommandsVM>(sp => sp.GetRequiredService<ApplicationCommandsVM>());
            services.AddTransient<MainWindowNotificationsVM>();
            services.AddTransient<MessageLogControlVM>();
            services.AddTransient<PopUpNotificationsControlVM>();
            services.AddTransient<ModalWindowNotificationsControlVM>();
            // ViewModel окон
            services.AddTransient<LaunchWindowVM>();
            // ViewModel контролов
            services.AddSingleton<ApplicationSettingsControlVM>();
            services.AddTransient<ApplicationSettingsTabItemControlVM>();
            services.AddTransient<LaunchWindowTabItemControlVM>();
            services.AddTransient<StorageCreationControlVM>();
            services.AddTransient<RepositoryCreationControlVM>();
            services.AddTransient<ReportsControlVM>();
            services.AddTransient<FormulaTestControlVM>();
            //services.AddTransient<MainWindowVM>();                    // Заменено на фабрику
            //services.AddTransient<RepositoryExplorerControlVM>();     // Заменено на фабрику
            // ViewModel сущностей
            services.AddSingleton<DataStoragesCollectionVM>();
            services.AddSingleton<PhiladelphusRepositoryCollectionVM>();        // Не менять. Приводит к ошибкам обновления интерфейса
            services.AddSingleton<PhiladelphusRepositoryHeadersCollectionVM>(); // Не менять. Приводит к ошибкам обновления интерфейса

            // Регистрация фабрик
            services.AddTransient<IMainWindowVMFactory, MainWindowVMFactory>();
            services.AddTransient<IRepositoryExplorerControlVMFactory, RepositoryExplorerControlVMFactory>();
            services.AddTransient<IExtensionsControlVMFactory, ExtensionsControlVMFactory>();
            services.AddTransient<IExcelImportDesignerVMFactory, ExcelImportDesignerVMFactory>();

            // Excel-импорт (презентация)
            services.AddTransient<ExcelImportPresentationPipeline>();
            services.AddTransient<ExcelImportRepositoryPreviewBuilder>();
            services.AddTransient<ExcelImportPresentationSessionState>();
            services.AddTransient<ImportFromExcelVM>();

            return services;
        }
    }
}
