using AutoMapper;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Services.Interfaces;

namespace Philadelphus.Core.Domain.ImportExport.Mapping
{
    /// <summary>
    /// Предоставляет методы расширения для передачи параметров импорта через контекст AutoMapper.
    /// </summary>
    internal static class ImportExportMappingExtensions
    {
        private const string RepositoryKey = "ImportExport.Repository";
        private const string RepositoryServiceKey = "ImportExport.RepositoryService";
        private const string LeavePolymorphismServiceKey = "ImportExport.LeavePolymorphismService";
        private const string RefreshProcessKey = "ImportExport.RefreshProcess";
        private const string RefreshProgressKey = "ImportExport.RefreshProgress";

        /// <summary>
        /// Добавляет репозиторий в контекст маппинга импорта.
        /// </summary>
        /// <param name="items">Параметры контекста AutoMapper.</param>
        /// <param name="repository">Репозиторий, в который выполняется импорт.</param>
        public static void SetImportRepository(this IDictionary<string, object> items, PhiladelphusRepositoryModel repository)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(repository);

            items[RepositoryKey] = repository;
        }

        /// <summary>
        /// Добавляет доменный сервис репозитория в контекст маппинга импорта.
        /// </summary>
        /// <param name="items">Параметры контекста AutoMapper.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        public static void SetImportRepositoryService(
            this IDictionary<string, object> items,
            IPhiladelphusRepositoryService repositoryService)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(repositoryService);

            items[RepositoryServiceKey] = repositoryService;
        }

        /// <summary>
        /// Добавляет сервис полиморфизма листов в контекст маппинга импорта.
        /// </summary>
        /// <param name="items">Параметры контекста AutoMapper.</param>
        /// <param name="service">Сервис заполнения и разрешения полиморфных связей.</param>
        public static void SetImportLeavePolymorphismService(
            this IDictionary<string, object> items,
            ILeavePolymorphismService service)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(service);

            items[LeavePolymorphismServiceKey] = service;
        }

        /// <summary>
        /// Добавляет действие обновления описания процесса в контекст маппинга импорта.
        /// </summary>
        /// <param name="items">Параметры контекста AutoMapper.</param>
        /// <param name="refreshProcess">Действие обновления описания процесса.</param>
        public static void SetImportRefreshProcess(this IDictionary<string, object> items, Action<string> refreshProcess)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(refreshProcess);

            items[RefreshProcessKey] = refreshProcess;
        }

        /// <summary>
        /// Добавляет действие обновления прогресса в контекст маппинга импорта.
        /// </summary>
        /// <param name="items">Параметры контекста AutoMapper.</param>
        /// <param name="refreshProgress">Действие обновления прогресса.</param>
        public static void SetImportRefreshProgress(this IDictionary<string, object> items, Action<int, int> refreshProgress)
        {
            ArgumentNullException.ThrowIfNull(items);
            ArgumentNullException.ThrowIfNull(refreshProgress);

            items[RefreshProgressKey] = refreshProgress;
        }

        /// <summary>
        /// Возвращает репозиторий из контекста маппинга импорта.
        /// </summary>
        /// <param name="context">Контекст AutoMapper.</param>
        /// <returns>Репозиторий, в который выполняется импорт.</returns>
        public static PhiladelphusRepositoryModel GetImportRepository(this ResolutionContext context)
        {
            return GetRequiredContextItem<PhiladelphusRepositoryModel>(context, RepositoryKey);
        }

        /// <summary>
        /// Возвращает доменный сервис репозитория из контекста маппинга импорта.
        /// </summary>
        /// <param name="context">Контекст AutoMapper.</param>
        /// <returns>Доменный сервис репозитория.</returns>
        public static IPhiladelphusRepositoryService GetImportRepositoryService(this ResolutionContext context)
        {
            return GetRequiredContextItem<IPhiladelphusRepositoryService>(context, RepositoryServiceKey);
        }

        /// <summary>
        /// Возвращает сервис полиморфизма листов из контекста маппинга импорта.
        /// </summary>
        /// <param name="context">Контекст AutoMapper.</param>
        /// <returns>Сервис заполнения и разрешения полиморфных связей.</returns>
        public static ILeavePolymorphismService GetImportLeavePolymorphismService(
            this ResolutionContext context)
        {
            return GetRequiredContextItem<ILeavePolymorphismService>(
                context,
                LeavePolymorphismServiceKey);
        }

        /// <summary>
        /// Возвращает действие обновления описания процесса из контекста маппинга импорта.
        /// </summary>
        /// <param name="context">Контекст AutoMapper.</param>
        /// <returns>Действие обновления описания процесса.</returns>
        public static Action<string> GetImportRefreshProcess(this ResolutionContext context)
        {
            return GetRequiredContextItem<Action<string>>(context, RefreshProcessKey);
        }

        /// <summary>
        /// Возвращает действие обновления прогресса из контекста маппинга импорта.
        /// </summary>
        /// <param name="context">Контекст AutoMapper.</param>
        /// <returns>Действие обновления прогресса.</returns>
        public static Action<int, int> GetImportRefreshProgress(this ResolutionContext context)
        {
            return GetRequiredContextItem<Action<int, int>>(context, RefreshProgressKey);
        }

        private static T GetRequiredContextItem<T>(ResolutionContext context, string key)
        {
            ArgumentNullException.ThrowIfNull(context);

            if (context.Items.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }

            throw new InvalidOperationException($"В контексте AutoMapper не найден параметр '{key}'.");
        }
    }
}
