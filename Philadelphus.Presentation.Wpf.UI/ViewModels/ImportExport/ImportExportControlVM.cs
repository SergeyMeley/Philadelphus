using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.ImportExport.Services.Interfaces;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.ViewModels.ImportExport;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.Views.Windows;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ImportExport
{
    /// <summary>
    /// Модель представления команд импорта, экспорта и конвертации.
    /// </summary>
    public class ImportExportControlVM : ViewModelBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IImportExportService _importExportService;
        private readonly IPhiladelphusRepositoryService _repositoryService;
        private readonly RepositoryExplorerControlVM _repositoryExplorerControlVM;
        private readonly IRelayCommandFactory _commandFactory;

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="ImportExportControlVM" />.
        /// </summary>
        /// <param name="serviceProvider">Поставщик сервисов приложения.</param>
        /// <param name="importExportService">Сервис импорта-экспорта.</param>
        /// <param name="repositoryService">Доменный сервис репозитория.</param>
        /// <param name="repositoryExplorerControlVM">Модель представления обозревателя репозитория.</param>
        /// <param name="commandFactory">Фабрика синхронных команд.</param>
        public ImportExportControlVM(
            IServiceProvider serviceProvider,
            IImportExportService importExportService,
            IPhiladelphusRepositoryService repositoryService,
            RepositoryExplorerControlVM repositoryExplorerControlVM,
            IRelayCommandFactory commandFactory)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);
            ArgumentNullException.ThrowIfNull(importExportService);
            ArgumentNullException.ThrowIfNull(repositoryService);
            ArgumentNullException.ThrowIfNull(repositoryExplorerControlVM);
            ArgumentNullException.ThrowIfNull(commandFactory);

            _serviceProvider = serviceProvider;
            _importExportService = importExportService;
            _repositoryService = repositoryService;
            _repositoryExplorerControlVM = repositoryExplorerControlVM;
            _commandFactory = commandFactory;

            RefreshAdapters();
        }

        /// <summary>
        /// Доступные адаптеры импорта-экспорта.
        /// </summary>
        public ObservableCollection<ImportExportAdapterVM> AvailableAdapters { get; } = new();

        /// <summary>
        /// Доступные операции импорта.
        /// </summary>
        public ObservableCollection<ImportExportOperationVM> ImportOperations { get; } = new();

        /// <summary>
        /// Доступные операции экспорта.
        /// </summary>
        public ObservableCollection<ImportExportOperationVM> ExportOperations { get; } = new();

        /// <summary>
        /// Доступные операции конвертации.
        /// </summary>
        public ObservableCollection<ImportExportOperationVM> ConversionOperations { get; } = new();

        /// <summary>
        /// Текущий этап операции импорта-экспорта.
        /// </summary>
        public string CurrentProcess { get; private set; } = string.Empty;

        /// <summary>
        /// Текущий прогресс операции импорта-экспорта.
        /// </summary>
        public string CurrentProgress { get; private set; } = string.Empty;

        /// <summary>
        /// Команда обновления списка доступных адаптеров.
        /// </summary>
        public IRelayCommand RefreshAdaptersCommand => _commandFactory.Create(_ => RefreshAdapters());

        private void RefreshAdapters()
        {
            AvailableAdapters.Clear();
            ImportOperations.Clear();
            ExportOperations.Clear();
            ConversionOperations.Clear();

            foreach (var adapterInfo in _importExportService.GetAvailableAdapters())
            {
                AvailableAdapters.Add(new ImportExportAdapterVM(adapterInfo));
            }

            foreach (var adapter in AvailableAdapters)
            {
                AddImportOperation(adapter);
                AddExportOperation(adapter);
            }

            foreach (var sourceAdapter in AvailableAdapters.Where(x => x.CanImport))
            {
                foreach (var targetAdapter in AvailableAdapters.Where(x => x.CanExport))
                {
                    AddConversionOperation(sourceAdapter, targetAdapter);
                }
            }
        }

        private void AddImportOperation(ImportExportAdapterVM adapter)
        {
            if (adapter.CanImport == false)
            {
                return;
            }

            ImportOperations.Add(new ImportExportOperationVM(
                $"Импорт из {adapter.FileFormat}",
                _commandFactory.Create(
                    _ => ImportFromFile(adapter.FileFormat, adapter.AdapterName),
                    _ => CanImportToRepository())));
        }

        private void AddExportOperation(ImportExportAdapterVM adapter)
        {
            if (adapter.CanExport == false)
            {
                return;
            }

            ExportOperations.Add(new ImportExportOperationVM(
                $"Экспорт в {adapter.FileFormat}",
                _commandFactory.Create(
                    _ => ExportToFile(adapter.FileFormat, adapter.AdapterName),
                    _ => CanExportSelectedWorkingTree())));
        }

        private void AddConversionOperation(
            ImportExportAdapterVM sourceAdapter,
            ImportExportAdapterVM targetAdapter)
        {
            if (IsSameAdapter(sourceAdapter, targetAdapter))
            {
                return;
            }

            ConversionOperations.Add(new ImportExportOperationVM(
                $"Конвертировать {sourceAdapter.FileFormat} в {targetAdapter.FileFormat}",
                _commandFactory.Create(
                    _ => ConvertFile(
                        sourceAdapter.FileFormat,
                        sourceAdapter.AdapterName,
                        targetAdapter.FileFormat,
                        targetAdapter.AdapterName),
                    _ => CanConvertFile())));
        }

        private void ExportToFile(string fileFormat, string adapterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileFormat);
            ArgumentException.ThrowIfNullOrWhiteSpace(adapterName);

            if (_repositoryExplorerControlVM.SelectedRepositoryMember?.Model is not TreeRootModel treeRoot)
            {
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = BuildFileDialogFilter(fileFormat),
                DefaultExt = NormalizeFileFormat(fileFormat),
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            _importExportService.ExportToFile(
                fileFormat,
                adapterName,
                treeRoot.OwningWorkingTree,
                saveFileDialog.FileName);

            OpenFile(saveFileDialog.FileName);
        }

        private void ImportFromFile(string fileFormat, string adapterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileFormat);
            ArgumentException.ThrowIfNullOrWhiteSpace(adapterName);

            // Временное решение до закрытия задачи #65496139:
            // Excel-импорт пока должен идти через старый дизайнер, иначе теряются настройки схемы и атрибуты.
            if (IsTemporaryExcelImport(fileFormat))
            {
                ImportFromExcelDesigner();
                return;
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = BuildFileDialogFilter(fileFormat),
                DefaultExt = NormalizeFileFormat(fileFormat)
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            var importedTree = _importExportService.ImportFromFile(
                fileFormat,
                adapterName,
                openFileDialog.FileName,
                _repositoryExplorerControlVM.PhiladelphusRepositoryVM.Model,
                _repositoryService,
                OnProcessChanged,
                OnProgressChanged);

            if (importedTree.ContentRoot != null)
            {
                _repositoryExplorerControlVM.AddTreeRoot(importedTree.ContentRoot);
            }
        }

        private void ImportFromExcelDesigner()
        {
            if (_repositoryExplorerControlVM.PhiladelphusRepositoryVM.Model.ContentShrub == null)
            {
                _serviceProvider.GetRequiredService<INotificationService>().SendModalWindow<ImportExportControlVM>(
                    "Активный репозиторий не содержит рабочего дерева для импорта.",
                    NotificationCriticalLevelModel.Warning);
                return;
            }

            var window = _serviceProvider.GetRequiredService<ExcelImportDesignerWindow>();
            window.Initialize(
                _repositoryExplorerControlVM.PhiladelphusRepositoryVM.Model.ContentShrub,
                _repositoryExplorerControlVM.PhiladelphusRepositoryVM.Model,
                _repositoryService,
                _repositoryExplorerControlVM.RefreshRepositoryView);
            window.ShowDialog();
        }

        private void ConvertFile(
            string sourceFileFormat,
            string sourceAdapterName,
            string targetFileFormat,
            string targetAdapterName)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceFileFormat);
            ArgumentException.ThrowIfNullOrWhiteSpace(sourceAdapterName);
            ArgumentException.ThrowIfNullOrWhiteSpace(targetFileFormat);
            ArgumentException.ThrowIfNullOrWhiteSpace(targetAdapterName);

            var openFileDialog = new OpenFileDialog
            {
                Filter = BuildFileDialogFilter(sourceFileFormat),
                DefaultExt = NormalizeFileFormat(sourceFileFormat)
            };

            if (openFileDialog.ShowDialog() != true)
            {
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = BuildFileDialogFilter(targetFileFormat),
                DefaultExt = NormalizeFileFormat(targetFileFormat),
                AddExtension = true
            };

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            _importExportService.ConvertFile(
                sourceFileFormat,
                sourceAdapterName,
                openFileDialog.FileName,
                targetFileFormat,
                targetAdapterName,
                saveFileDialog.FileName);

            OpenFile(saveFileDialog.FileName);
        }

        private bool CanImportToRepository()
        {
            return _repositoryExplorerControlVM.IsRepositoryLoading == false;
        }

        private bool CanExportSelectedWorkingTree()
        {
            return _repositoryExplorerControlVM.IsRepositoryLoading == false
                && _repositoryExplorerControlVM.SelectedRepositoryMember is TreeRootVM;
        }

        private bool CanConvertFile()
        {
            return _repositoryExplorerControlVM.IsRepositoryLoading == false;
        }

        private static bool IsSameAdapter(
            ImportExportAdapterVM sourceAdapter,
            ImportExportAdapterVM targetAdapter)
        {
            ArgumentNullException.ThrowIfNull(sourceAdapter);
            ArgumentNullException.ThrowIfNull(targetAdapter);

            return string.Equals(sourceAdapter.FileFormat, targetAdapter.FileFormat, StringComparison.OrdinalIgnoreCase)
                && string.Equals(sourceAdapter.AdapterName, targetAdapter.AdapterName, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildFileDialogFilter(string fileFormat)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(fileFormat);

            var normalizedFileFormat = NormalizeFileFormat(fileFormat);
            return $"{normalizedFileFormat.ToUpperInvariant()} файлы (*{normalizedFileFormat})|*{normalizedFileFormat}|Все файлы (*.*)|*.*";
        }

        private static string NormalizeFileFormat(string fileFormat)
        {
            var normalizedFileFormat = fileFormat.Trim().ToLowerInvariant();
            return normalizedFileFormat.StartsWith(".", StringComparison.Ordinal)
                ? normalizedFileFormat
                : $".{normalizedFileFormat}";
        }

        private static bool IsTemporaryExcelImport(string fileFormat)
        {
            return string.Equals(NormalizeFileFormat(fileFormat), ".xlsx", StringComparison.OrdinalIgnoreCase);
        }

        private static void OpenFile(string file)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = file,
                UseShellExecute = true
            });
        }

        private void OnProcessChanged(string currentProcess)
        {
            ArgumentNullException.ThrowIfNull(currentProcess);

            _serviceProvider.GetRequiredService<IDispatcherService>().Invoke(() =>
            {
                CurrentProcess = currentProcess;
                OnPropertyChanged(nameof(CurrentProcess));
            });
        }

        private void OnProgressChanged(int currentNumber, int totalCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(currentNumber);
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(totalCount);

            _serviceProvider.GetRequiredService<IDispatcherService>().Invoke(() =>
            {
                CurrentProgress = $"{currentNumber} / {totalCount} ({Math.Round((double)currentNumber / totalCount * 100, 1)} %)";
                OnPropertyChanged(nameof(CurrentProgress));
            });
        }
    }
}
