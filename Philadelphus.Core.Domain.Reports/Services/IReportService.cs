using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Services
{
    /// <summary>
    /// Сервис отчетов
    /// </summary>
    public interface IReportService
    {
        /// <summary>
        /// Получить список отчетов из хранилищ
        /// </summary>
        /// <param name="dataStorageModels">Хранилища</param>
        /// <returns>Результат выполнения операции.</returns>
        public Task<List<ReportInfoModel>> GetReportsListAsync(
            IEnumerable<IDataStorageModel> dataStorageModels);

        /// <summary>
        /// Получить отчет
        /// </summary>
        /// <param name="report">Отчет</param>
        /// <param name="repository">Репозиторий БД</param>
        /// <param name="preliminaryRefresh">Флаг предварительного обновления данных:
        /// 1. Материализованное представление в БД.
        /// 2. Любой тип в кэше</param>
        /// <returns>Результат выполнения операции.</returns>
        public Task<DataTable> ExecuteReportAsync(
            ReportInfoModel report,
            IReportsInfrastructureRepository repository,
            bool preliminaryRefresh = true);

        /// <summary>
        /// Обновить отчет в кэше
        /// </summary>
        /// <param name="report">Отчет</param>
        /// <param name="repository">Репозиторий БД</param>
        /// <returns>Результат выполнения операции.</returns>
        public Task RefreshCachedReportAsync(
            ReportInfoModel report,
            IReportsInfrastructureRepository repository);

        /// <summary>
        /// Обновить материализованное представление в БД
        /// </summary>
        /// <param name="report">Отчет</param>
        /// <param name="repository">Репозиторий БД</param>
        /// <returns>Результат выполнения операции.</returns>
        public Task RefreshMaterializedViewAsync(
            ReportInfoModel report, 
            IReportsInfrastructureRepository repository);
    }
}
