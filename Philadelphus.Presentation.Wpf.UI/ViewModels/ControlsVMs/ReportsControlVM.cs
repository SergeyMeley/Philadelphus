using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Core.Domain.Reports.Services;
using Philadelphus.Core.Domain.Services.Implementations;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.TablesExport.Enums;
using Philadelphus.Core.Domain.TablesExport.Factories;
using Philadelphus.Core.Domain.TablesExport.Models;
using Philadelphus.Core.Domain.TablesExport.Services;
using Philadelphus.Presentation.Wpf.UI.Infrastructure;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Serilog;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs
{
    public class ReportsControlVM : ControlBaseVM
    {
        private readonly IReportService _reportService;
        private readonly ITablesExportService _xlsxExportService;
        private readonly ITablesExportService _jsonExportService;
        private readonly ITablesExportService _xmlExportService;
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
            ITablesExportServiceFactory tablesExportServiceFactory,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            ApplicationCommandsVM applicationCommandsVM) 
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            _reportService = reportService;
            _xlsxExportService = tablesExportServiceFactory.Create(TablesExportFormat.Xlsx);
            _jsonExportService = tablesExportServiceFactory.Create(TablesExportFormat.Json);
            _xmlExportService = tablesExportServiceFactory.Create(TablesExportFormat.Xml);
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
        public RelayCommand ExportToXlsxCommand
        {
            get
            {
                return new RelayCommand(
                    async obj =>
                    {
                        ExportTableAsync(_xlsxExportService);
                    },
                    ce =>
                    {
                        return ReportResult != null;
                    });
            }
        }

        public RelayCommand ExportToJsonCommand
        {
            get
            {
                return new RelayCommand(
                    async obj =>
                    {
                        ExportTableAsync(_jsonExportService);
                    },
                    ce =>
                    {
                        return ReportResult != null;
                    });
            }
        }

        public RelayCommand ExportToXmlCommand
        {
            get
            {
                return new RelayCommand(
                    async obj =>
                    {
                        ExportTableAsync(_xmlExportService);
                    },
                    ce =>
                    {
                        return ReportResult != null;
                    });
            }
        }

        private async Task ExportTableAsync(ITablesExportService tablesExportService)
        {
            if (ReportResult == null || ReportResult.Rows.Count == 0)
                return;

            // колонки
            var columns = ReportResult.Columns
                .Cast<DataColumn>()
                .Select(c => new TableExportColumn<DataRow>
                {
                    Header = c.ColumnName,
                    ValueSelector = row => row[c],
                    Width = 20
                })
                .ToList();

            // локальная функция → превращаем DataTable в IAsyncEnumerable
            async IAsyncEnumerable<DataRow> GetData(
                [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
            {
                foreach (DataRow row in ReportResult.Rows)
                {
                    ct.ThrowIfCancellationRequested();

                    yield return row;

                    // даём возможность не блокировать поток
                    await Task.Yield();
                }
            }

            var path = await tablesExportService.ExportAsync(
                GetData(),
                columns,
                SelectedReportInfo?.Name ?? "report");

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = path,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _notificationService.SendTextMessage<ReportsControlVM>(
                    $"Файл сохранён, но не удалось открыть: {ex.Message}",
                    NotificationCriticalLevelModel.Warning);
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
