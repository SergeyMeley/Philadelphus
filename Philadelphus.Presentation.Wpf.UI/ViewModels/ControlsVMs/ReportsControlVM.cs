using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Core.Domain.Reports.Services;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class ReportsControlVM : ControlBaseVM
    {
        private readonly IReportService _reportService;
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;

        private DataStorageVM _selectedDataStorageVM;
        private ReportInfoModel _selectedReportInfo;
        private IEnumerable<ReportInfoModel> _reportInfos;
        private Dictionary<string, object> _parametersValues;
        private DataTable _reportResult;

        public IEnumerable<DataStorageVM> DataStoragesVMs
        {
            get
            {
                return _dataStoragesCollectionVM.DataStoragesVMs.Where(x => x.Model.HasReportsInfrastructureRepository);
            }
        }
        public CompositeCollection DisplayedDataStoragesVMs 
        { 
            get
            {
                return new CompositeCollection()
            {
                new CollectionContainer { Collection = new[] { new { Name = "не выбрано" } } },
                new CollectionContainer { Collection = DataStoragesVMs },
            }; ;
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
                _selectedDataStorageVM = value;
                OnPropertyChanged(nameof(SelectedDataStorageVM));
                UpdateReportInfos();
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
        public bool PreliminaryRefresh { get; set; }

        public DataTable ReportResult
        {
            get
            {
                return _reportResult;
            }
            set
            {
                _reportResult = value;
                OnPropertyChanged();
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

        public RelayCommand ExecuteReportCommand
        {
            get
            {
                return new RelayCommand(
                    async obj =>
                    {
                        await ExecuteReportAsync();
                    },
                    ce =>
                    {
                        return SelectedReportInfo != null;
                    });
            }
        }
        public RelayCommand ExportToExcelCommand
        {
            get
            {
                return new RelayCommand(
                    async obj =>
                    {
                        _notificationService.SendTextMessage<ReportsControlVM>(
                            $"Экспорт отчета в Excel не реализован.",
                            criticalLevel: NotificationCriticalLevelModel.Warning);
                    },
                    ce =>
                    {
                        return SelectedReportInfo != null;
                    });
            }
        }
        private async void UpdateReportInfos()
        {
            if (SelectedDataStorageVM != null)
            {
                var storages = new[] { SelectedDataStorageVM.Model };
                ReportInfos = await _reportService.GetReportsListAsync(storages);
            }
        }

        private async Task ExecuteReportAsync()
        {
            if (SelectedReportInfo == null) 
                return;

            ReportResult = null;

            try
            {
                var result = await _reportService.ExecuteReportAsync(
                    SelectedReportInfo,
                    SelectedDataStorageVM.Model.ReportsInfrastructureRepository,
                    PreliminaryRefresh);
                ReportResult = result;
            }
            catch (Exception ex)
            {
            }
        }
    }
}
