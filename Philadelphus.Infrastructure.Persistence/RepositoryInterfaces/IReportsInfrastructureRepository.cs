using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    /// <summary>
    /// Задает контракт для работы с репозиторем БД отчетов.
    /// </summary>
    public interface IReportsInfrastructureRepository : IInfrastructureRepository
    {
        /// <summary>
        /// Получает список доступных отчетов.
        /// </summary>
        /// <param name="schemaName">Имя схемы.</param>
        /// <returns>Задача, представляющая асинхронную операцию. Результат содержит возвращаемые данные.</returns>
        public Task<List<ReportInfo>> GetAvailableReportsAsync(string schemaName);
        
        /// <summary>
        /// Получить отчет.
        /// </summary>
        /// <param name="report">Отчет.</param>
        /// <returns>Задача, представляющая асинхронную операцию. Результат содержит возвращаемые данные.</returns>
        public Task<DataTable> ExecuteReportAsync(ReportInfo report);
        
        /// <summary>
        /// Обновить материализованное представление.
        /// </summary>
        /// <param name="report">Отчет.</param>
        /// <returns>Задача, представляющая асинхронную операцию.</returns>
        public Task RefreshMaterializedViewAsync(ReportInfo report);
    }
}
