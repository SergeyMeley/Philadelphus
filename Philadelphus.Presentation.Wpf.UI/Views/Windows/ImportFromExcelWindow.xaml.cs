using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;
using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Core.Domain.Entities.MainEntities;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers;
using Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers;
using Philadelphus.Core.Domain.Helpers;
using Philadelphus.Core.Domain.ImportExport.Excel;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Factories.Interfaces;
using Philadelphus.Presentation.Wpf.UI.ViewModels.ControlsVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.InfrastructureVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Input;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ImportFromExcelWindow.xaml
    /// </summary>
    public partial class ImportFromExcelWindow : Window
    {
        internal enum WindowMode
        {
            ConvertToPhjson,
            ImportTreeFromXlsx
        }

        private readonly ConversionService _service;
        private readonly ExcelPreviewService _previewService;
        private readonly IServiceProvider _serviceProvider;
        private const string NoParentRelationOption = "(Нет родителя)";
        private WindowMode _mode = WindowMode.ConvertToPhjson;
        private PhiladelphusRepositoryModel? _repository;
        private IPhiladelphusRepositoryService? _repositoryService;
        private Action? _refreshRepositoryView;
        private string _selectedFilePath = string.Empty;
        private ExcelPreviewWorkbookInfo? _workbookPreview;
        private ExcelImportProfile? _currentImportProfile;
        private readonly Dictionary<string, ExcelImportProfile> _importProfilesBySource = new(StringComparer.OrdinalIgnoreCase);
        private bool _isUpdatingRelationControls;

        public Array AvailableColumnRoles => Enum.GetValues(typeof(ExcelImportColumnRole));
        public List<VisibilityScopeItem> AvailableVisibilityScopes { get; } = Enum.GetValues<VisibilityScope>()
            .Select(x => new VisibilityScopeItem
            {
                Value = x,
                DisplayName = GetDisplayName(x)
            })
            .ToList();
        public List<OverrideTypeItem> AvailableOverrideTypes { get; } = Enum.GetValues<OverrideType>()
            .Select(x => new OverrideTypeItem
            {
                Value = x,
                DisplayName = GetDisplayName(x)
            })
            .ToList();
        public List<ExcelImportDefinitionScopeItem> AvailableDefinitionScopes { get; } = Enum.GetValues<ExcelImportDefinitionScope>()
            .Select(x => new ExcelImportDefinitionScopeItem
            {
                Value = x,
                DisplayName = GetDisplayName(x)
            })
            .ToList();
        public List<ExcelImportValueModeItem> AvailableValueModes { get; } = Enum.GetValues<ExcelImportValueMode>()
            .Select(x => new ExcelImportValueModeItem
            {
                Value = x,
                DisplayName = GetDisplayName(x)
            })
            .ToList();

        public ImportFromExcelWindow(IServiceProvider serviceProvider, ConversionService service, ExcelPreviewService previewService, ExcelImportPreviewService importPreviewService)
        {
            InitializeComponent();
            DataContext = this;
            _serviceProvider = serviceProvider;
            _service = service;
            _previewService = previewService;
        }

        internal void InitializeForConvert(ShrubModel shrub)
        {
            _mode = WindowMode.ConvertToPhjson;
            _repository = null;
            _repositoryService = null;
            _refreshRepositoryView = null;
            ConfigureWindowAppearance();
            InitializeRootsList(shrub);
        }

        internal void InitializeForImport(
            ShrubModel shrub,
            PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action refreshRepositoryView)
        {
            _mode = WindowMode.ImportTreeFromXlsx;
            _repository = repository;
            _repositoryService = repositoryService;
            _refreshRepositoryView = refreshRepositoryView;
            ConfigureWindowAppearance();
            InitializeRootsList(shrub);
        }

        private void InitializeRootsList(ShrubModel shrub)
        {
            var roots = _service.GetExistingRootsFromStorage(shrub);
            CmbExistingRoots.ItemsSource = roots;
            CmbExistingRoots.DisplayMemberPath = nameof(TreeRootModel.Name);
            CmbExistingRoots.SelectedValuePath = nameof(TreeRootModel.Name);

            if (roots.Count > 0)
            {
                CmbExistingRoots.SelectedIndex = 0;
            }
        }

        private void ConfigureWindowAppearance()
        {
            switch (_mode)
            {
                case WindowMode.ImportTreeFromXlsx:
                    Title = "Импорт дерева из Excel";
                    TxtWizardIntro.Text = "Импорт дерева из Excel.";
                    BtnMainAction.Content = "Импортировать";
                    BtnPreviewResult.Visibility = Visibility.Visible;
                    GroupColumnSettings.Visibility = Visibility.Visible;
                    GroupRootSettings.Header = "Параметры импорта в TreeRoot";
                    GroupSourceSelection.Header = "Предпросмотр Excel";
                    GroupColumnSettings.Header = "Настройка колонок";
                    ChkCreateNewRoot.Visibility = Visibility.Visible;
                    GridExistingRoots.Visibility = Visibility.Visible;
                    ChkUseAllWorksheets.Content = "Импортировать все листы книги";
                    break;
                default:
                    Title = "Конвертер Excel -> JSON";
                    TxtWizardIntro.Text = "Конвертер Excel в PHJSON.";
                    BtnMainAction.Content = "Конвертировать в PHJSON";
                    BtnPreviewResult.Visibility = Visibility.Collapsed;
                    GroupColumnSettings.Visibility = Visibility.Collapsed;
                    GroupRootSettings.Header = "Параметры PHJSON";
                    GroupSourceSelection.Header = "Выбор листов Excel";
                    ChkCreateNewRoot.Visibility = Visibility.Collapsed;
                    GridExistingRoots.Visibility = Visibility.Collapsed;
                    ChkUseAllWorksheets.Content = "Конвертировать все листы книги";
                    ChkCreateNewRoot.IsChecked = true;
                    TxtRootName.IsEnabled = true;
                    break;
            }
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Выберите файл Excel"
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedFilePath = dialog.FileName;
                TxtFilePath.Text = Path.GetFileName(_selectedFilePath);
                TxtRootName.Text = Path.GetFileNameWithoutExtension(_selectedFilePath);
                LoadWorkbookPreview();
            }
        }

        private void ChkCreateNewRoot_Checked(object sender, RoutedEventArgs e)
        {
            TxtRootName.IsEnabled = true;
            CmbExistingRoots.IsEnabled = false;
        }

        private void ChkCreateNewRoot_Unchecked(object sender, RoutedEventArgs e)
        {
            TxtRootName.IsEnabled = false;
            CmbExistingRoots.IsEnabled = true;
        }

        private async void BtnConvert_Click(object sender, RoutedEventArgs e)
        {
            if (TryGetImportParameters(out var isNewRoot, out var rootName, out var existingRoot) == false)
                return;

            try
            {
                if (_mode == WindowMode.ImportTreeFromXlsx)
                {
                    if (_repository == null || _repositoryService == null)
                    {
                        MessageBox.Show("Не инициализирован контекст активного репозитория.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var importProfiles = GetImportProfilesForExecution();
                    if (importProfiles.Count == 0)
                    {
                        MessageBox.Show("Не выбраны листы Excel для импорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    var validationResult = ValidateImportProfiles(importProfiles);
                    if (validationResult.HasErrors)
                    {
                        MessageBox.Show(
                            BuildValidationMessage(validationResult),
                            "Ошибка проверки данных",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }

                    var progressWindow = _serviceProvider.GetRequiredService<ImportProgressWindow>();
                    progressWindow.Initialize("Импорт дерева из Excel", "Подготовка операции...");
                    progressWindow.Show();

                    var filePath = _selectedFilePath;
                    var repository = _repository;
                    var repositoryService = _repositoryService;

                    Close();

                    IProgress<string> statusProgress = new Progress<string>(status => progressWindow.UpdateStatus(status));

                    try
                    {
                        await Task.Run(() =>
                        {
                            statusProgress.Report("Чтение Excel и формирование PHJSON...");
                            var jsonResult = _service.ProcessFile(filePath, isNewRoot, rootName, importProfiles);

                            statusProgress.Report("Импорт дерева в Чубушник...");
                            JsonImportExportHelper.ParseJson(jsonResult, repositoryService, repository, existingRoot);
                        });

                        _refreshRepositoryView?.Invoke();
                        progressWindow.Complete("Импорт завершен. Сохраните репозиторий для записи в хранилище.");
                    }
                    catch (Exception ex)
                    {
                        progressWindow.Fail($"Ошибка импорта: {ex.Message}");
                    }

                    return;
                }

                SetBusyState(true);
                var exportProfiles = GetImportProfilesForExecution();
                if (exportProfiles.Count == 0)
                {
                    SetBusyState(false);
                    MessageBox.Show("Не выбраны листы Excel для конвертации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (exportProfiles.Count > 0)
                {
                    var validationResult = ValidateImportProfiles(exportProfiles);
                    if (validationResult.HasErrors)
                    {
                        SetBusyState(false);
                        MessageBox.Show(
                            BuildValidationMessage(validationResult),
                            "Ошибка проверки данных",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                        return;
                    }
                }

                var jsonResult = _service.ProcessFile(_selectedFilePath, isNewRoot, rootName, exportProfiles);

                var saveDialog = new SaveFileDialog
                {
                    Filter = "PHJSON Files|*.phjson",
                    FileName = Path.GetFileNameWithoutExtension(_selectedFilePath) + ".phjson"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, jsonResult);
                    SetBusyState(false);
                    MessageBox.Show("Файл успешно создан!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
                else
                {
                    SetBusyState(false);
                }
            }
            catch (Exception ex)
            {
                SetBusyState(false);
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetBusyState(bool isBusy)
        {
            BtnMainAction.IsEnabled = !isBusy;
            BtnPreviewResult.IsEnabled = !isBusy;
            ChkCreateNewRoot.IsEnabled = !isBusy;
            TxtRootName.IsEnabled = !isBusy && (_mode == WindowMode.ConvertToPhjson || ChkCreateNewRoot.IsChecked == true);
            CmbExistingRoots.IsEnabled = !isBusy && _mode == WindowMode.ImportTreeFromXlsx && ChkCreateNewRoot.IsChecked != true;
            ChkUseAllWorksheets.IsEnabled = !isBusy;
            LstPreviewSources.IsEnabled = !isBusy;
            DgColumnSettings.IsEnabled = !isBusy;
            CmbParentSource.IsEnabled = !isBusy && CmbParentSource.ItemsSource != null;
            CmbParentKeyColumn.IsEnabled = !isBusy && CmbParentKeyColumn.ItemsSource != null;
            CmbCurrentKeyColumn.IsEnabled = !isBusy && CmbCurrentKeyColumn.ItemsSource != null;
            ProgressOperation.Visibility = isBusy ? Visibility.Visible : Visibility.Collapsed;
            Mouse.OverrideCursor = isBusy ? Cursors.Wait : null;
        }

        private bool TryGetImportParameters(out bool isNewRoot, out string rootName, out TreeRootModel? existingRoot)
        {
            isNewRoot = _mode == WindowMode.ConvertToPhjson || ChkCreateNewRoot.IsChecked == true;
            rootName = string.Empty;
            existingRoot = null;

            if (string.IsNullOrWhiteSpace(_selectedFilePath))
            {
                MessageBox.Show("Сначала выберите файл Excel!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            existingRoot = isNewRoot ? null : CmbExistingRoots.SelectedItem as TreeRootModel;
            rootName = isNewRoot
                ? TxtRootName.Text?.Trim() ?? string.Empty
                : existingRoot?.Name?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(rootName))
            {
                MessageBox.Show("Укажите наименование корня!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void BtnPreviewResult_Click(object sender, RoutedEventArgs e)
        {
            if (_mode != WindowMode.ImportTreeFromXlsx)
                return;

            if (TryGetImportParameters(out var isNewRoot, out var rootName, out var existingRoot) == false)
                return;

            try
            {
                var importProfiles = GetImportProfilesForExecution();
                if (importProfiles.Count == 0)
                {
                    MessageBox.Show("Не выбраны листы Excel для предпросмотра импорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var validationResult = ValidateImportProfiles(importProfiles);
                if (validationResult.HasErrors)
                {
                    MessageBox.Show(
                        BuildValidationMessage(validationResult),
                        "Ошибка проверки данных",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var jsonResult = _service.ProcessFile(_selectedFilePath, isNewRoot, rootName, importProfiles);
                var previewViewModel = BuildRepositoryExplorerPreview(jsonResult, isNewRoot ? null : rootName);
                var previewWindow = _serviceProvider.GetRequiredService<ImportTreePreviewWindow>();
                previewWindow.Owner = this;
                previewWindow.Initialize(
                    previewViewModel,
                    BuildPreviewSummary(previewViewModel));
                previewWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось построить предпросмотр импорта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PreviewSourceType_Checked(object sender, RoutedEventArgs e)
        {
            RefreshPreviewSourceList();
        }

        private void ChkUseAllWorksheets_Changed(object sender, RoutedEventArgs e)
        {
            UpdateSelectionMode();
            UpdatePreviewInfoSuffix();
        }

        private void LstPreviewSources_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LoadSelectedPreview();
            UpdatePreviewInfoSuffix();
        }

        private void LoadWorkbookPreview()
        {
            try
            {
                _workbookPreview = _previewService.GetWorkbookPreview(_selectedFilePath);
                _importProfilesBySource.Clear();
                _currentImportProfile = null;
                RefreshPreviewSourceList();
                UpdateAllWorksheetStatuses();
                RefreshRelationControls();
            }
            catch (Exception ex)
            {
                _workbookPreview = null;
                _currentImportProfile = null;
                LstPreviewSources.ItemsSource = null;
                DgPreview.ItemsSource = null;
                DgColumnSettings.ItemsSource = null;
                TxtPreviewInfo.Text = $"Не удалось загрузить предпросмотр: {ex.Message}";
                RefreshRelationControls();
            }
        }

        private void RefreshPreviewSourceList()
        {
            if (LstPreviewSources == null || DgPreview == null || TxtPreviewInfo == null || DgColumnSettings == null)
                return;

            if (_workbookPreview == null)
            {
                LstPreviewSources.ItemsSource = null;
                DgPreview.ItemsSource = null;
                DgColumnSettings.ItemsSource = null;
                TxtPreviewInfo.Text = "Файл не выбран.";
                return;
            }

            var sources = _workbookPreview.Worksheets;
            LstPreviewSources.ItemsSource = sources;

            if (sources.Count == 0)
            {
                LstPreviewSources.SelectedItem = null;
                DgPreview.ItemsSource = null;
                DgColumnSettings.ItemsSource = null;
                TxtPreviewInfo.Text = "В файле нет листов с данными.";
                return;
            }

            UpdateSelectionMode();
            LstPreviewSources.SelectedIndex = 0;
        }

        private void LoadSelectedPreview()
        {
            if (LstPreviewSources == null || DgPreview == null || TxtPreviewInfo == null || DgColumnSettings == null)
                return;

            if (LstPreviewSources.SelectedItem is not ExcelPreviewSourceInfo selectedSource || string.IsNullOrWhiteSpace(_selectedFilePath))
            {
                DgPreview.ItemsSource = null;
                DgColumnSettings.ItemsSource = null;
                return;
            }

            try
            {
                var sourceSelection = new ExcelImportSourceSelection
                {
                    SourceName = selectedSource.Name,
                    SourceType = selectedSource.SourceType
                };
                var preview = _previewService.GetPreview(_selectedFilePath, sourceSelection);
                _currentImportProfile = GetOrCreateImportProfile(sourceSelection);

                TxtPreviewInfo.Text = $"Активный лист: {preview.SourceName}. Строк данных: {preview.TotalRowCount}. Колонок: {preview.TotalColumnCount}.";
                DgPreview.ItemsSource = BuildPreviewTable(preview).DefaultView;
                DgColumnSettings.ItemsSource = _currentImportProfile.Columns;
                RefreshRelationControls();
                UpdatePreviewInfoSuffix();
            }
            catch (Exception ex)
            {
                DgPreview.ItemsSource = null;
                DgColumnSettings.ItemsSource = null;
                TxtPreviewInfo.Text = $"Не удалось построить предпросмотр: {ex.Message}";
                RefreshRelationControls();
            }
        }

        private ExcelImportProfile GetOrCreateImportProfile(ExcelImportSourceSelection sourceSelection)
        {
            if (_importProfilesBySource.TryGetValue(sourceSelection.SourceName, out var profile))
                return profile;

            profile = _previewService.BuildImportProfile(_selectedFilePath, sourceSelection);
            _importProfilesBySource[sourceSelection.SourceName] = profile;
            return profile;
        }

        private List<ExcelImportProfile> GetImportProfilesForExecution()
        {
            if (_workbookPreview == null)
                return new List<ExcelImportProfile>();

            if (ChkUseAllWorksheets.IsChecked == true)
            {
                return _workbookPreview.Worksheets
                    .Select(source => GetOrCreateImportProfile(new ExcelImportSourceSelection
                    {
                        SourceName = source.Name,
                        SourceType = ExcelPreviewSourceType.Worksheet
                    }))
                    .ToList();
            }

            return LstPreviewSources.SelectedItems
                .OfType<ExcelPreviewSourceInfo>()
                .Select(source => GetOrCreateImportProfile(new ExcelImportSourceSelection
                {
                    SourceName = source.Name,
                    SourceType = ExcelPreviewSourceType.Worksheet
                }))
                .ToList();
        }

        private ExcelImportValidationResult ValidateImportProfiles(IEnumerable<ExcelImportProfile> importProfiles)
        {
            var validator = _serviceProvider.GetRequiredService<IExcelImportProfileValidator>();
            var result = validator.ValidateProfiles(_selectedFilePath, importProfiles);

            UpdateAllWorksheetStatuses();

            return result;
        }

        private void UpdateSelectionMode()
        {
            if (LstPreviewSources == null)
                return;

            if (ChkUseAllWorksheets.IsChecked == true)
            {
                LstPreviewSources.SelectionMode = SelectionMode.Single;
                if (LstPreviewSources.Items.Count > 0 && LstPreviewSources.SelectedIndex < 0)
                {
                    LstPreviewSources.SelectedIndex = 0;
                }

                return;
            }

            LstPreviewSources.SelectionMode = SelectionMode.Extended;
        }

        private void UpdatePreviewInfoSuffix()
        {
            if (TxtPreviewInfo == null || LstPreviewSources == null)
                return;

            var baseText = TxtPreviewInfo.Text?.Split(" Будет обработано:").FirstOrDefault() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(baseText))
                return;

            var processedText = ChkUseAllWorksheets.IsChecked == true
                ? " Будет обработано: все листы книги."
                : $" Будет обработано: выбранных листов {LstPreviewSources.SelectedItems.Count}.";

            TxtPreviewInfo.Text = baseText + processedText;
        }

        private void DgColumnSettings_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            if (_currentImportProfile == null)
            {
                MessageBox.Show("Сначала выберите лист Excel для настройки колонок.", "Настройка колонок", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DgColumnSettings_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateAllWorksheetStatuses();
                if (_currentImportProfile?.SourceSelection?.SourceName != null)
                {
                    UpdateCurrentWorksheetStatus(_currentImportProfile.SourceSelection.SourceName);
                }
            }), DispatcherPriority.Background);
        }

        private void RefreshRelationControls()
        {
            if (CmbParentSource == null || CmbParentKeyColumn == null || CmbCurrentKeyColumn == null)
                return;

            _isUpdatingRelationControls = true;
            try
            {
                if (_currentImportProfile == null || _workbookPreview == null)
                {
                    CmbParentSource.ItemsSource = null;
                    CmbParentSource.SelectedItem = null;
                    CmbParentKeyColumn.ItemsSource = null;
                    CmbParentKeyColumn.SelectedItem = null;
                    CmbCurrentKeyColumn.ItemsSource = null;
                    CmbCurrentKeyColumn.SelectedItem = null;
                    CmbParentSource.IsEnabled = false;
                    CmbParentKeyColumn.IsEnabled = false;
                    CmbCurrentKeyColumn.IsEnabled = false;
                    return;
                }

                var parentOptions = new List<string> { NoParentRelationOption };
                parentOptions.AddRange(_workbookPreview.Worksheets
                    .Select(x => x.Name)
                    .Where(x => string.Equals(x, _currentImportProfile.SourceSelection.SourceName, StringComparison.OrdinalIgnoreCase) == false)
                    .OrderBy(x => x));

                CmbParentSource.ItemsSource = parentOptions;
                CmbCurrentKeyColumn.ItemsSource = _currentImportProfile.Columns
                    .Select(x => x.HeaderName)
                    .OrderBy(x => x)
                    .ToList();

                CmbParentSource.IsEnabled = true;
                CmbCurrentKeyColumn.IsEnabled = true;

                var selectedParent = string.IsNullOrWhiteSpace(_currentImportProfile.Relation.ParentSourceName)
                    ? NoParentRelationOption
                    : _currentImportProfile.Relation.ParentSourceName;
                CmbParentSource.SelectedItem = parentOptions.Contains(selectedParent) ? selectedParent : NoParentRelationOption;

                CmbCurrentKeyColumn.SelectedItem = string.IsNullOrWhiteSpace(_currentImportProfile.Relation.ChildKeyColumnName)
                    ? null
                    : _currentImportProfile.Relation.ChildKeyColumnName;

                RefreshParentKeyColumnOptions();
            }
            finally
            {
                _isUpdatingRelationControls = false;
            }
        }

        private void RefreshParentKeyColumnOptions()
        {
            if (CmbParentKeyColumn == null)
                return;

            if (_currentImportProfile == null || string.IsNullOrWhiteSpace(_currentImportProfile.Relation.ParentSourceName))
            {
                CmbParentKeyColumn.ItemsSource = null;
                CmbParentKeyColumn.SelectedItem = null;
                CmbParentKeyColumn.IsEnabled = false;
                return;
            }

            if (_importProfilesBySource.TryGetValue(_currentImportProfile.Relation.ParentSourceName, out var parentProfile) == false)
            {
                var parentSelection = new ExcelImportSourceSelection
                {
                    SourceName = _currentImportProfile.Relation.ParentSourceName,
                    SourceType = ExcelPreviewSourceType.Worksheet
                };
                parentProfile = GetOrCreateImportProfile(parentSelection);
            }

            var parentHeaders = parentProfile.Columns
                .Select(x => x.HeaderName)
                .OrderBy(x => x)
                .ToList();

            CmbParentKeyColumn.ItemsSource = parentHeaders;
            CmbParentKeyColumn.IsEnabled = true;
            CmbParentKeyColumn.SelectedItem = string.IsNullOrWhiteSpace(_currentImportProfile.Relation.ParentKeyColumnName)
                ? null
                : _currentImportProfile.Relation.ParentKeyColumnName;
        }

        private void CmbParentSource_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingRelationControls || _currentImportProfile == null)
                return;

            var selectedParent = CmbParentSource.SelectedItem as string;
            if (string.IsNullOrWhiteSpace(selectedParent) || selectedParent == NoParentRelationOption)
            {
                _currentImportProfile.Relation.ParentSourceName = string.Empty;
                _currentImportProfile.Relation.ParentKeyColumnName = string.Empty;
                _currentImportProfile.Relation.ChildKeyColumnName = string.Empty;
                RefreshRelationControls();
            }
            else
            {
                _currentImportProfile.Relation.ParentSourceName = selectedParent;
                RefreshParentKeyColumnOptions();
            }

            UpdateAllWorksheetStatuses();
        }

        private void CmbParentKeyColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingRelationControls || _currentImportProfile == null)
                return;

            _currentImportProfile.Relation.ParentKeyColumnName = CmbParentKeyColumn.SelectedItem as string ?? string.Empty;
            UpdateAllWorksheetStatuses();
        }

        private void CmbCurrentKeyColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingRelationControls || _currentImportProfile == null)
                return;

            _currentImportProfile.Relation.ChildKeyColumnName = CmbCurrentKeyColumn.SelectedItem as string ?? string.Empty;
            UpdateAllWorksheetStatuses();
        }

        private static DataTable BuildPreviewTable(ExcelPreviewTable preview)
        {
            var table = new DataTable();

            foreach (var header in preview.Headers)
            {
                var columnName = string.IsNullOrWhiteSpace(header) ? "Колонка" : header;
                var uniqueColumnName = columnName;
                var suffix = 1;

                while (table.Columns.Contains(uniqueColumnName))
                {
                    suffix++;
                    uniqueColumnName = $"{columnName} ({suffix})";
                }

                table.Columns.Add(uniqueColumnName);
            }

            foreach (var row in preview.Rows)
            {
                var rowValues = row.Cast<object>().ToArray();
                table.Rows.Add(rowValues);
            }

            return table;
        }

        private static string BuildValidationMessage(ExcelImportValidationResult validationResult)
        {
            var lines = validationResult.Errors
                .Take(15)
                .Select(error =>
                {
                    if (error.IsConfigurationError)
                    {
                        return string.IsNullOrWhiteSpace(error.ColumnName)
                            ? $"Лист \"{error.SourceName}\": {error.Message}"
                            : $"Лист \"{error.SourceName}\", настройка колонки \"{error.ColumnName}\": {error.Message}";
                    }

                    return $"Лист \"{error.SourceName}\", строка {error.RowNumber}, колонка \"{error.ColumnName}\": {error.Message}";
                })
                .ToList();

            if (validationResult.Errors.Count > 15)
            {
                lines.Add($"... и еще {validationResult.Errors.Count - 15} ошибок.");
            }

            lines.Insert(0, "Операция остановлена. В настройках или данных импорта найдены ошибки.");
            lines.Insert(1, string.Empty);

            return string.Join(Environment.NewLine, lines);
        }

        private void UpdateAllWorksheetStatuses()
        {
            if (_workbookPreview == null)
                return;

            foreach (var worksheet in _workbookPreview.Worksheets)
            {
                UpdateCurrentWorksheetStatus(worksheet.Name);
            }

            LstPreviewSources.Items.Refresh();
        }

        private void UpdateCurrentWorksheetStatus(string sourceName)
        {
            if (_workbookPreview == null)
                return;

            var worksheet = _workbookPreview.Worksheets.FirstOrDefault(x => string.Equals(x.Name, sourceName, StringComparison.OrdinalIgnoreCase));
            if (worksheet == null)
                return;

            if (_importProfilesBySource.TryGetValue(sourceName, out var profile) == false)
            {
                worksheet.StatusText = "Не настроен";
                return;
            }

            var validation = _previewService.ValidateProfileConfiguration(profile);
            worksheet.StatusText = validation.HasErrors
                ? "Ошибка конфигурации"
                : "Готов к импорту";
        }

        private static string GetDisplayName(Enum value)
        {
            var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
            var display = member?.GetCustomAttribute<DisplayAttribute>();
            return display?.Name ?? value.ToString();
        }

        private RepositoryExplorerControlVM BuildRepositoryExplorerPreview(string json, string? targetExistingRootName)
        {
            var repositoryService = _serviceProvider.GetRequiredService<IPhiladelphusRepositoryService>();
            var repositoryCollectionService = _serviceProvider.GetRequiredService<IPhiladelphusRepositoryCollectionService>();
            var repositoryExplorerFactory = _serviceProvider.GetRequiredService<IRepositoryExplorerControlVMFactory>();
            var dataStoragesCollectionVm = _serviceProvider.GetRequiredService<DataStoragesCollectionVM>();

            var previewStorage = dataStoragesCollectionVm.MainDataStorageVM?.Model
                ?? dataStoragesCollectionVm.DataStoragesVMs?.Select(x => x.Model).FirstOrDefault(x => x != null);

            if (previewStorage == null)
                throw new InvalidOperationException("Не удалось получить временное хранилище для предпросмотра.");

            var previewRepository = repositoryCollectionService.CreateNewPhiladelphusRepository(previewStorage, needAutoName: false);
            previewRepository.Name = "Предпросмотр импорта";
            previewRepository.Description = "Временный репозиторий для предпросмотра дерева из Excel";

            repositoryService.GetShrubContent(previewRepository);

            TreeRootModel? previewTargetRoot = null;
            if (string.IsNullOrWhiteSpace(targetExistingRootName) == false)
            {
                var previewWorkingTree = repositoryService.CreateWorkingTree(previewRepository, previewStorage, needAutoName: false, withoutInfoNotifications: true);
                previewWorkingTree.Name = $"Предпросмотр: {targetExistingRootName}";
                previewTargetRoot = repositoryService.CreateTreeRoot(previewWorkingTree, needAutoName: false, withoutInfoNotifications: true);
                previewTargetRoot.Name = targetExistingRootName;
            }

            var previewTree = JsonImportExportHelper.ParseJson(json, repositoryService, previewRepository, previewTargetRoot);

            var previewRepositoryVm = new PhiladelphusRepositoryVM(previewRepository, dataStoragesCollectionVm, repositoryService);
            previewRepositoryVm.Childs.Clear();

            var rootForPreview = previewTargetRoot ?? previewTree.ContentRoot;
            if (rootForPreview != null && rootForPreview.IsSystemBase == false)
            {
                previewRepositoryVm.Childs.Add(new TreeRootVM(rootForPreview, dataStoragesCollectionVm, repositoryService));
            }

            var previewExplorerVm = repositoryExplorerFactory.Create(previewRepositoryVm, skipInitialLoad: true);
            previewExplorerVm.SelectedRepositoryMember = previewRepositoryVm.Childs.FirstOrDefault();
            return previewExplorerVm;
        }

        private static string BuildPreviewSummary(RepositoryExplorerControlVM previewViewModel)
        {
            var roots = previewViewModel.PhiladelphusRepositoryVM.Childs.Count;
            var nodes = previewViewModel.PhiladelphusRepositoryVM.Childs.Sum(CountNodes);
            var leaves = previewViewModel.PhiladelphusRepositoryVM.Childs.Sum(CountLeaves);
            return $"Предпросмотр дерева. Корней: {roots}. Узлов: {nodes}. Листьев: {leaves}.";
        }

        private static int CountNodes(TreeRootVM root)
        {
            return root.ChildNodes.Sum(node => 1 + CountChildNodes(node));
        }

        private static int CountChildNodes(TreeNodeVM node)
        {
            return node.ChildNodes.Sum(child => 1 + CountChildNodes(child));
        }

        private static int CountLeaves(TreeRootVM root)
        {
            return root.ChildNodes.Sum(CountLeaves);
        }

        private static int CountLeaves(TreeNodeVM node)
        {
            return node.ChildLeaves.Count + node.ChildNodes.Sum(CountLeaves);
        }
    }
}
