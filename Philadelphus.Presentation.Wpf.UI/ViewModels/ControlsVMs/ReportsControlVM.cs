using AutoMapper;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Core.Domain.Reports.Services;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Infrastructure.Persistence.Entities.Reports;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Serilog;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class ReportsControlVM : ControlBaseVM
    {
        private readonly IReportService _reportService;
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;

        private DataStorageVM _selectedDataStorageVM;
        private ReportInfoModel _selectedReportInfo;
        private IEnumerable<ReportInfoModel> _reportInfos;

        public IEnumerable<DataStorageVM> DataStoragesVMs
        {
            get
            {
                return _dataStoragesCollectionVM.DataStoragesVMs.Where(x => x.Model.HasReportsInfrastructureRepository);
            }
        }
        public DataStorageVM SelectedDataStorageVM
        {
            get
            {
                return _selectedDataStorageVM;
            }
            set
            {
                if (_selectedDataStorageVM != value)
                {
                    _selectedDataStorageVM = value;
                    OnPropertyChanged(nameof(SelectedDataStorageVM));
                    UpdateReportInfos();
                }
            }
        }

        public IEnumerable<ReportInfoModel> ReportInfos 
        { 
            get
            {
                return _reportInfos;
            }
            set
            {
                _reportInfos = value;
                OnPropertyChanged(nameof(ReportInfos));
            }
        }
        public ReportInfoModel SelectedReportInfo
        {
            get
            {
                return _selectedReportInfo;
            }
            set
            {
                if (_selectedReportInfo != value)
                {
                    _selectedReportInfo = value;
                    OnPropertyChanged(nameof(SelectedReportInfo));
                }
            }
        }

        public ReportsControlVM(
            IServiceProvider serviceProvider, 
            IMapper mapper, 
            ILogger logger, 
            INotificationService notificationService,
            IReportService reportService,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            ApplicationCommandsVM applicationCommandsVM) 
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _reportService = reportService;
            _dataStoragesCollectionVM = dataStoragesCollectionVM;
        }

        private async void UpdateReportInfos()
        {
            var storages = DataStoragesVMs.Select(x => x.Model);
            ReportInfos = await _reportService.GetReportsAsync(storages);
        }
    }
}
