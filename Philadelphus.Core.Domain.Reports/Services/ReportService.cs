using AutoMapper;
using Philadelphus.Core.Domain.Entities.Infrastructure.DataStorages;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Infrastructure.Persistence.Entities.Infrastructure.DataStorages;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using Philadelphus.Infrastructure.Persistence.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Philadelphus.Core.Domain.Reports.Services
{
    public class ReportService : IReportService
    {
        private readonly IMapper _mapper;

        public ReportService(
            IMapper mapper)
        {
            _mapper = mapper;
        }

        public async Task<List<ReportInfoModel>> GetReportsAsync(IEnumerable<IDataStorageModel> dataStorageModels)
        {
            return await Task.Run( async () =>
            {
                var dbReports = new List<ReportInfo>();
                foreach (var repository in dataStorageModels.Where(x => x.HasReportsInfrastructureRepository).Select(x => x.ReportsInfrastructureRepository))
                {
                    dbReports.AddRange(await repository.GetAvailableReportsAsync("reports"));
                }
                return _mapper.Map<List<ReportInfoModel>>(dbReports);
            });
        }

        public async Task<DataTable> ExecuteReportAsync(ReportInfoModel report, Dictionary<string, object> parameters, IReportsInfrastructureRepository repository)
        {
            var dbReport = _mapper.Map<ReportInfo>(report);
            return await repository.ExecuteReportAsync(dbReport, parameters);
        }

        public async Task RefreshMaterializedViewAsync(ReportInfoModel report, IReportsInfrastructureRepository repository)
        {
            var dbReport = _mapper.Map<ReportInfo>(report);
            await repository.RefreshMaterializedViewAsync(dbReport);
        }
    }
}
