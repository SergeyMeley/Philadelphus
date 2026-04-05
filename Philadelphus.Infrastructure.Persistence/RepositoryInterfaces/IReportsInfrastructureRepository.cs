using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Philadelphus.Infrastructure.Persistence.RepositoryInterfaces
{
    public interface IReportsInfrastructureRepository : IInfrastructureRepository
    {
        public Task<List<ReportInfo>> GetAvailableReportsAsync(string schemaName);
        public Task<DataTable> ExecuteReportAsync(ReportInfo report, Dictionary<string, object> parameters);
        public Task RefreshMaterializedViewAsync(ReportInfo report);
    }
}
