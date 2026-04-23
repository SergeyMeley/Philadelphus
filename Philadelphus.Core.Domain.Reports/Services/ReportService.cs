using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Services
{
    /// <summary>
    /// Сервис отчетов
    /// </summary>
    public class ReportService : IReportService
    {
        private readonly IMapper _mapper;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;

        /// <summary>
        /// Сервис отчетов
        /// </summary>
        /// <param name="mapper">Автомаппер</param>
        /// <param name="logger">Логгер</param>
        /// <param name="notificationService">Сервис уведомлений</param>
        public ReportService(
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService)
        {
            _mapper = mapper;
            _logger = logger;
            _notificationService = notificationService;
        }

        /// <summary>
        /// Получить список отчетов из хранилищ
        /// </summary>
        /// <param name="dataStorageModels">Хранилища</param>
        /// <returns></returns>
        public async Task<List<ReportInfoModel>> GetReportsListAsync(
            IEnumerable<IDataStorageModel> dataStorageModels)
        {
            return await Task.Run(async () =>
            {
                var storagesDescs = new List<string>();
                foreach (var item in dataStorageModels)
                {
                    storagesDescs.Add($"'{item.Name}' [{item.Uuid}]");
                }

                var storagesDescsString = string.Join(", ", storagesDescs);

                _notificationService.SendTextMessage<ReportService>(
                    $"Начало получения списка отчетов. Хранилища: {storagesDescsString }.",
                    criticalLevel: NotificationCriticalLevelModel.Info);

                var dbReports = new List<ReportInfo>();
                foreach (var repository in dataStorageModels.Where(x => x.HasReportsInfrastructureRepository).Select(x => x.ReportsInfrastructureRepository))
                {
                    dbReports.AddRange(await repository.GetAvailableReportsAsync("reports"));
                }

                var result = _mapper.Map<List<ReportInfoModel>>(dbReports);

                _notificationService.SendTextMessage<ReportService>(
                    $"Получение списка отчетов успешно выполнено. Найдено отчетов - {result.Count()} шт.",
                    criticalLevel: NotificationCriticalLevelModel.Ok);

                return result;
            });
        }

        /// <summary>
        /// Получить отчет
        /// </summary>
        /// <param name="report">Отчет</param>
        /// <param name="repository">Репозиторий БД</param>
        /// <param name="preliminaryRefresh">Флаг предварительного обновления данных:
        /// 1. Материализованное представление в БД.
        /// 2. Любой тип в кэше</param>
        /// <returns></returns>
        public async Task<DataTable> ExecuteReportAsync(
            ReportInfoModel report,
            IReportsInfrastructureRepository repository,
            bool preliminaryRefresh = true)
        {
            _notificationService.SendTextMessage<ReportService>(
                $"Начало получения отчета. Отчет - '{report.Name}', параметров - {report.Parameters?.Count} шт.",
                criticalLevel: NotificationCriticalLevelModel.Info);

            var sw = new Stopwatch();
            sw.Start();

            var dbReport = _mapper.Map<ReportInfo>(report);

            if (preliminaryRefresh)
            {
                try
                {
                    await RefreshMaterializedViewAsync(report, repository);
                    await RefreshCachedReportAsync(report, repository);
                }
                catch (Exception ex)
                {
                    _notificationService.SendTextMessage<ReportService>(
                        $"Ошибка обновления материализованного представления '{report.Name}'. Неизвестная ошибка, обратитесь к разработчкику. \r\nПодробнее:\r\n{ex.Message}\r\n{ex.StackTrace}",
                        criticalLevel: NotificationCriticalLevelModel.Error);

                }
                _notificationService.SendTextMessage<ReportService>(
                    $"Отчет '{report.Name}' обновлен из БД",
                    criticalLevel: NotificationCriticalLevelModel.Info);
            }

            DataTable result = default;
            try
            {
                result = await repository.ExecuteReportAsync(dbReport);
            }
            catch (Exception ex)
            {
                _notificationService.SendTextMessage<ReportService>(
                    $"Ошибка получения отчета '{report.Name}'. Неизвестная ошибка, обратитесь к разработчкику. \r\nПодробнее:\r\n{ex.Message}\r\n{ex.StackTrace}",
                    criticalLevel: NotificationCriticalLevelModel.Error);
            }

            sw.Stop();

            if (result != default)
            {
                _notificationService.SendTextMessage<ReportService>(
                    $"Получение отчета '{report.Name}' успешно выполнено. Строк - {result?.Rows?.Count} шт. Время выполнения - {sw.ElapsedMilliseconds} мс.",
                    criticalLevel: NotificationCriticalLevelModel.Ok);
            }

            return result;
        }

        /// <summary>
        /// Обновить отчет в кэше
        /// </summary>
        /// <param name="report">Отчет</param>
        /// <param name="repository">Репозиторий БД</param>
        /// <returns></returns>
        public async Task RefreshCachedReportAsync(
            ReportInfoModel report,
            IReportsInfrastructureRepository repository)
        {
            _notificationService.SendTextMessage<ReportService>(
                $"Отчет не кэширован. Обновление не выполнено.",
                criticalLevel: NotificationCriticalLevelModel.Info);
        }

        /// <summary>
        /// Обновить материализованное представление в БД
        /// </summary>
        /// <param name="report">Отчет</param>
        /// <param name="repository">Репозиторий БД</param>
        /// <returns></returns>
        public async Task RefreshMaterializedViewAsync(ReportInfoModel report, IReportsInfrastructureRepository repository)
        {
            var sw = new Stopwatch();
            sw.Start();

            var dbReport = _mapper.Map<ReportInfo>(report);

            try
            {
                await repository.RefreshMaterializedViewAsync(dbReport);
            }
            catch (Exception ex)
            {
                _notificationService.SendTextMessage<ReportService>(
                    $"Ошибка обновления представления. Произошла непредвиденная ошибка, обратитесь к разработчику. \r\nПодробности: \r\n{ex.StackTrace}",
                    criticalLevel: NotificationCriticalLevelModel.Error);
            }

            sw.Stop();

            _notificationService.SendTextMessage<ReportService>(
                $"Представление обновлено в БД. Представление - {report.Name}, время обновления - {sw.ElapsedMilliseconds} мс.",
                criticalLevel: NotificationCriticalLevelModel.Info);
        }
    }
}
