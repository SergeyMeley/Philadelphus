using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.Infrastructure;
using Philadelphus.Presentation.Services.Interfaces;
using Philadelphus.Presentation.ViewModels;
using Philadelphus.Presentation.ViewModels.ImportExport;
using Philadelphus.Presentation.Wpf.UI.Services;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.ViewModels.ImportExport
{
    public class ImportFromExcelVM : ViewModelBase
    {
        private readonly ExcelImportPresentationSessionState _session;
        private readonly IFileDialogService _fileDialogService;
        private readonly IMessageDialogService _messageDialogService;
        private readonly IRelayCommandFactory _commandFactory;
        private readonly IAsyncRelayCommandFactory _asyncCommandFactory;

        private string _filePathDisplay = "Файл не выбран";
        private string _rootName = string.Empty;
        private ExcelImportSourceItemVM? _selectedSourceItem;
        private DataView? _previewView;
        private bool _useAllWorksheets = true;
        private bool _isBusy;

        public ImportFromExcelVM(
            ExcelImportPresentationSessionState session,
            IFileDialogService fileDialogService,
            IMessageDialogService messageDialogService,
            IRelayCommandFactory commandFactory,
            IAsyncRelayCommandFactory asyncCommandFactory)
        {
            ArgumentNullException.ThrowIfNull(commandFactory);
            ArgumentNullException.ThrowIfNull(asyncCommandFactory);

            _session = session;
            _fileDialogService = fileDialogService;
            _messageDialogService = messageDialogService;
            _commandFactory = commandFactory;
            _asyncCommandFactory = asyncCommandFactory;

            SelectFileCommand = _commandFactory.Create(_ => SelectFile());
            MainActionCommand = _asyncCommandFactory.Create(_ => ExecuteMainActionAsync());
        }

        public ObservableCollection<ExcelImportSourceItemVM> SourceItems { get; } = new();

        public ICommand SelectFileCommand { get; }

        public ICommand MainActionCommand { get; }

        public string FilePathDisplay
        {
            get => _filePathDisplay;
            private set => SetProperty(ref _filePathDisplay, value);
        }

        public string RootName
        {
            get => _rootName;
            set => SetProperty(ref _rootName, value);
        }

        public bool UseAllWorksheets
        {
            get => _useAllWorksheets;
            set
            {
                if (SetProperty(ref _useAllWorksheets, value))
                {
                    ApplyUseAllWorksheets();
                    RefreshWorksheetStatuses();
                    UpdatePreviewInfoSuffix();
                    OnPropertyChanged(nameof(CanChooseIndividualSheets));
                }
            }
        }

        public bool CanChooseIndividualSheets => UseAllWorksheets == false;

        public ExcelImportSourceItemVM? SelectedSourceItem
        {
            get => _selectedSourceItem;
            set
            {
                if (SetProperty(ref _selectedSourceItem, value))
                {
                    SelectCurrentSheet(value);
                }
            }
        }

        public DataView? PreviewView
        {
            get => _previewView;
            private set => SetProperty(ref _previewView, value);
        }

        public string PreviewInfo { get; private set; } = "Файл не выбран.";

        public bool IsBusy
        {
            get => _isBusy;
            private set => SetProperty(ref _isBusy, value);
        }

        public void InitializeForConvert()
        {
            FilePathDisplay = "Файл не выбран";
            RootName = string.Empty;
            PreviewView = null;
            SourceItems.Clear();
            SelectedSourceItem = null;
            PreviewInfo = "Файл не выбран.";
            UseAllWorksheets = true;
            OnPropertyChanged(nameof(PreviewInfo));
        }

        private void SelectFile()
        {
            var filePath = _fileDialogService.OpenExcelFile();
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            try
            {
                RootName = Path.GetFileNameWithoutExtension(filePath);
                _session.LoadWorkbook(filePath, RootName);
                FilePathDisplay = Path.GetFileName(filePath);
                RebuildSourceItemsFromSession();
                SelectedSourceItem = SourceItems.FirstOrDefault();
                RefreshWorksheetStatuses();
            }
            catch (Exception ex)
            {
                PreviewView = null;
                SourceItems.Clear();
                _messageDialogService.ShowError($"Не удалось загрузить предпросмотр: {ex.Message}", "Ошибка");
            }
        }

        private void RebuildSourceItemsFromSession()
        {
            SourceItems.Clear();

            if (_session.Schema == null || _session.WorkbookPreview == null)
                return;

            var previewSources = _session.WorkbookPreview.Tables
                .Concat(_session.WorkbookPreview.NamedRanges)
                .Concat(_session.WorkbookPreview.Worksheets)
                .ToList();

            foreach (var sheet in _session.Schema.Sheets)
            {
                var source = previewSources.FirstOrDefault(x =>
                    x.SourceType == sheet.SourceType
                    && string.Equals(x.Name, sheet.SourceName, StringComparison.OrdinalIgnoreCase))
                    ?? new ExcelPreviewSourceInfo
                    {
                        Name = sheet.SourceName,
                        SourceType = sheet.SourceType
                    };

                SourceItems.Add(new ExcelImportSourceItemVM(sheet, source, OnSourceItemIncludedChanged));
            }

            ApplyUseAllWorksheets();
        }

        private void OnSourceItemIncludedChanged(ExcelImportSourceItemVM item, bool isIncluded)
        {
            if (UseAllWorksheets)
                return;

            item.Sheet.IsEnabled = isIncluded;
            RefreshWorksheetStatuses();
            UpdatePreviewInfoSuffix();
        }

        private void ApplyUseAllWorksheets()
        {
            if (_session.Schema == null)
                return;

            if (UseAllWorksheets)
            {
                _session.SetAllSheetsEnabled(true);
                foreach (var item in SourceItems)
                {
                    item.SetIncludedSilently(true);
                }
            }
            else
            {
                foreach (var item in SourceItems)
                {
                    item.Sheet.IsEnabled = item.IsIncluded;
                }
            }
        }

        private void SelectCurrentSheet(ExcelImportSourceItemVM? item)
        {
            if (item == null)
            {
                _session.CurrentSheet = null;
                PreviewView = null;
                PreviewInfo = "Источник не выбран.";
                OnPropertyChanged(nameof(PreviewInfo));
                return;
            }

            _session.CurrentSheet = item.Sheet;
            LoadSelectedPreview();
        }

        private void LoadSelectedPreview()
        {
            if (_session.CurrentSheet == null || string.IsNullOrWhiteSpace(_session.SelectedFilePath))
            {
                PreviewView = null;
                return;
            }

            try
            {
                var preview = _session.GetPreview(_session.CurrentSheet);
                PreviewInfo = $"Активный источник: {preview.SourceName}. Строк данных: {preview.TotalRowCount}. Колонок: {preview.TotalColumnCount}.";
                PreviewView = ExcelPreviewTableBuilder.Build(preview).DefaultView;
                OnPropertyChanged(nameof(PreviewInfo));
                UpdatePreviewInfoSuffix();
            }
            catch (Exception ex)
            {
                PreviewView = null;
                PreviewInfo = $"Не удалось построить предпросмотр: {ex.Message}";
                OnPropertyChanged(nameof(PreviewInfo));
            }
        }

        private void RefreshWorksheetStatuses()
        {
            foreach (var item in SourceItems)
            {
                var validation = _session.ValidateProfileConfiguration(item.Sheet.Profile);
                item.StatusText = validation.HasErrors
                    ? "Ошибка конфигурации"
                    : item.Sheet.IsEnabled ? "Готов к конвертации" : "Отключен";
            }
        }

        private void UpdatePreviewInfoSuffix()
        {
            var baseText = PreviewInfo.Split(" Будет обработано:").FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baseText))
                return;

            var processedText = UseAllWorksheets
                ? " Будет обработано: все источники книги."
                : $" Будет обработано: выбранных источников {SourceItems.Count(x => x.IsIncluded)}.";

            PreviewInfo = baseText + processedText;
            OnPropertyChanged(nameof(PreviewInfo));
        }

        private Task ExecuteMainActionAsync()
        {
            if (TrySyncSchemaFromConvertParameters() == false)
                return Task.CompletedTask;

            ConvertToPhjson();
            return Task.CompletedTask;
        }

        private void ConvertToPhjson()
        {
            try
            {
                IsBusy = true;

                var profiles = _session.GetProfilesForExecution();
                if (profiles.Count == 0)
                {
                    _messageDialogService.ShowWarning("Не выбраны источники Excel для конвертации.", "Ошибка");
                    return;
                }

                var validationResult = _session.Validate();
                if (validationResult.HasErrors)
                {
                    _messageDialogService.ShowWarning(ExcelImportValidationMessageBuilder.Build(validationResult), "Ошибка проверки данных");
                    return;
                }

                var jsonResult = _session.BuildJson();
                var defaultFileName = Path.GetFileNameWithoutExtension(_session.SelectedFilePath) + ".phjson";
                var savePath = _fileDialogService.SavePhjsonFile(defaultFileName);
                if (string.IsNullOrWhiteSpace(savePath))
                    return;

                File.WriteAllText(savePath, jsonResult);
                _messageDialogService.ShowInformation("Файл успешно создан!", "Успех");
                Process.Start(new ProcessStartInfo
                {
                    FileName = savePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                _messageDialogService.ShowError($"Произошла ошибка: {ex.Message}", "Ошибка");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool TrySyncSchemaFromConvertParameters()
        {
            if (_session.Schema == null || string.IsNullOrWhiteSpace(_session.SelectedFilePath))
            {
                _messageDialogService.ShowWarning("Сначала выберите файл Excel.", "Ошибка");
                return false;
            }

            var rootName = RootName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(rootName))
            {
                _messageDialogService.ShowWarning("Укажите наименование корня.", "Ошибка");
                return false;
            }

            ApplyUseAllWorksheets();
            _session.SyncRootSettings(createNewRoot: true, rootName);
            return true;
        }
    }
}