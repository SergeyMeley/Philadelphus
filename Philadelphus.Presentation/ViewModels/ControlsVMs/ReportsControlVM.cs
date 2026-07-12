using AutoMapper;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Reports.Models;
using Philadelphus.Core.Domain.Reports.Services;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Core.Domain.TablesExport.Enums;
using Philadelphus.Core.Domain.TablesExport.Factories;
using Philadelphus.Core.Domain.TablesExport.Models;
using Philadelphus.Core.Domain.TablesExport.Services;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Serilog;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

namespace Philadelphus.Presentation.ViewModels.ControlsVMs
{
    /// <summary>
    /// Модель представления для отчета.
    /// </summary>
    public class ReportsControlVM : ControlBaseVM
    {
        private readonly IReportService _reportService;
        private readonly ITablesExportService _xlsxExportService;
        private readonly ITablesExportService _jsonExportService;
        private readonly ITablesExportService _xmlExportService;
        private readonly DataStoragesCollectionVM _dataStoragesCollectionVM;
        private readonly IRelayCommandFactory _commandFactory;

        private DataStorageVM? _selectedDataStorageVM;
        private PhiladelphusRepositoryVM? _repositoryVM;
        private ReportInfoModel _selectedReportInfo;
        private IEnumerable<ReportInfoModel> _reportInfos;
        private Dictionary<string, object> _parametersValues;
        private DataTable _reportResult;

        public IEnumerable<DataStorageVM> DataStoragesVMs
        {
            get
            {
                var repositoryStorageUuids = _repositoryVM?.DataStorages
                    .Select(x => x.Uuid)
                    .ToHashSet();

                return (_dataStoragesCollectionVM.DataStoragesVMs
                        ?? Enumerable.Empty<DataStorageVM>())
                    .Where(x => x.Model?.HasReportsInfrastructureRepository == true)
                    .Where(x => repositoryStorageUuids == null || repositoryStorageUuids.Contains(x.Uuid));
            }
        }
        public DataStorageVM? SelectedDataStorageVM
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

        /// <summary>
        /// Привязывает выбор хранилища отчётов к настройкам открытого репозитория.
        /// </summary>
        public void BindToRepository(PhiladelphusRepositoryVM repositoryVM)
        {
            ArgumentNullException.ThrowIfNull(repositoryVM);
            if (_repositoryVM != null)
                _repositoryVM.PropertyChanged -= RepositoryPropertyChanged;

            _repositoryVM = repositoryVM;
            _repositoryVM.PropertyChanged += RepositoryPropertyChanged;
            OnPropertyChanged(nameof(DataStoragesVMs));
            ApplyRepositoryDefaultDataStorage();
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

        /// <summary>
        /// Предварительное обновление.
        /// </summary>
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

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ReportsControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="mapper">Экземпляр AutoMapper.</param>
        /// <param name="logger">Логгер.</param>
        /// <param name="notificationService">Сервис уведомлений.</param>
        /// <param name="reportService">Параметр reportService.</param>
        /// <param name="tablesExportServiceFactory">Параметр tablesExportServiceFactory.</param>
        /// <param name="dataStoragesCollectionVM">Коллекция моделей представления хранилищ данных.</param>
        /// <param name="applicationCommandsVM">Модель представления команд приложения.</param>
        /// <param name="commandFactory">Фабрика синхронных команд.</param>
        /// <exception cref="ArgumentNullException">Если обязательный аргумент равен null.</exception>
        public ReportsControlVM(
            IServiceProvider serviceProvider,
            IMapper mapper,
            ILogger logger,
            INotificationService notificationService,
            IReportService reportService,
            ITablesExportServiceFactory tablesExportServiceFactory,
            DataStoragesCollectionVM dataStoragesCollectionVM,
            IApplicationCommandsVM applicationCommandsVM,
            IRelayCommandFactory commandFactory)
            : base(serviceProvider, mapper, logger, notificationService, applicationCommandsVM)
        {
            ArgumentNullException.ThrowIfNull(reportService);
            ArgumentNullException.ThrowIfNull(tablesExportServiceFactory);
            ArgumentNullException.ThrowIfNull(dataStoragesCollectionVM);
            ArgumentNullException.ThrowIfNull(commandFactory);

            _reportService = reportService;
            _xlsxExportService = tablesExportServiceFactory.Create(TablesExportFormat.Xlsx);
            _jsonExportService = tablesExportServiceFactory.Create(TablesExportFormat.Json);
            _xmlExportService = tablesExportServiceFactory.Create(TablesExportFormat.Xml);
            _dataStoragesCollectionVM = dataStoragesCollectionVM;
            _commandFactory = commandFactory;
        }

        public IRelayCommand ExecuteReportCommand
        {
            get
            {
                return _commandFactory.Create(
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
        public IRelayCommand ExportToXlsxCommand
        {
            get
            {
                return _commandFactory.Create(
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

        public IRelayCommand ExportToJsonCommand
        {
            get
            {
                return _commandFactory.Create(
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

        public IRelayCommand ExportToXmlCommand
        {
            get
            {
                return _commandFactory.Create(
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
            ArgumentNullException.ThrowIfNull(tablesExportService);

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
                return;
            }

            ReportInfos = Enumerable.Empty<ReportInfoModel>();
            SelectedReportInfo = null;
        }

        private void RepositoryPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PhiladelphusRepositoryVM.DefaultReportsDataStorage))
                ApplyRepositoryDefaultDataStorage();
            else if (e.PropertyName == nameof(PhiladelphusRepositoryVM.DataStorages))
            {
                OnPropertyChanged(nameof(DataStoragesVMs));
                if (SelectedDataStorageVM != null
                    && DataStoragesVMs.All(x => x.Uuid != SelectedDataStorageVM.Uuid))
                {
                    ApplyRepositoryDefaultDataStorage();
                }
            }
        }

        private void ApplyRepositoryDefaultDataStorage()
        {
            var defaultStorageUuid = _repositoryVM?.DefaultReportsDataStorage?.Uuid;
            SelectedDataStorageVM = defaultStorageUuid.HasValue
                ? DataStoragesVMs.SingleOrDefault(x => x.Uuid == defaultStorageUuid.Value)
                : null;
        }

        private async Task ExecuteReportAsync()
        {
            if (SelectedReportInfo == null) 
                return;
            if (SelectedDataStorageVM?.Model == null)
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
                _logger.Error(ex, "Ошибка выполнения отчёта '{ReportName}'", SelectedReportInfo.Name);
                _notificationService.SendTextMessage<ReportsControlVM>(
                    $"Не удалось выполнить отчёт '{SelectedReportInfo.Name}'. Подробнее:\r\n{ex.Message}",
                    NotificationCriticalLevelModel.Error);
            }
        }
    }
}
