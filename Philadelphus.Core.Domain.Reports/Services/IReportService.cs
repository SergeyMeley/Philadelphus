using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Services
{
    public interface IReportService
    {
        public Task<List<ReportInfoModel>> GetReportsAsync(IEnumerable<IDataStorageModel> dataStorageModels);
        public Task<DataTable> ExecuteReportAsync(ReportInfoModel report, Dictionary<string, object> parameters, IReportsInfrastructureRepository repository);
        public Task RefreshMaterializedViewAsync(ReportInfoModel report, IReportsInfrastructureRepository repository);
    }
}
