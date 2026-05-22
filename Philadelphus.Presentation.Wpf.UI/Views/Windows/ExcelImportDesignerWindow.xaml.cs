using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Philadelphus.Core.Domain.ImportExport.Excel;
using Philadelphus.Core.Domain.Services.Interfaces;
using Philadelphus.Presentation.Wpf.UI.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using IOPath = System.IO.Path;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    public partial class ExcelImportDesignerWindow : Window
    {
        private const string NoParentRelationOption = "(Нет родителя)";
        private const double DiagramCardWidth = 280;
        private const double DiagramCardMinHeight = 210;
        private const double DiagramCardMaxWidth = 520;
        private const double DiagramGridPadding = 40;
        private const double DiagramGridColumnGap = 48;
        private const double DiagramGridRowGap = 48;
        private const double DiagramGridRowHeight = 260;
        private const double DiagramZoomMin = 0.35;
        private const double DiagramZoomMax = 2.5;
        private const double DiagramZoomStep = 1.1;

        // Card movement and relation creation are intentionally separate gestures:
        // dragging the card moves the diagram, dragging a column creates a parent-column -> child-column link.
        private enum DiagramDragMode
        {
            None,
            MoveSheet,
            CreateRelation
        }

        private sealed class DiagramColumnTag
        {
            public DiagramColumnTag(ExcelImportSheetSchema sheet, ExcelImportColumnProfile column)
            {
                Sheet = sheet;
                Column = column;
            }

            public ExcelImportSheetSchema Sheet { get; }

            public ExcelImportColumnProfile Column { get; }
        }

        private sealed class DiagramRelationVisual
        {
            public required ExcelImportRelationSchema Relation { get; init; }

            public required Line Line { get; init; }

            public required Polygon ArrowHead { get; init; }
        }

        private readonly IServiceProvider _serviceProvider;
        private readonly ConversionService _conversionService;
        private readonly ExcelPreviewService _previewService;
        private readonly IExcelImportSchemaBuilder _schemaBuilder;
        private readonly IExcelImportSchemaTemplateStorage _templateStorage;
        private readonly ExcelImportPipeline _importPipeline;
        private Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryModel? _repository;
        private IPhiladelphusRepositoryService? _repositoryService;
        private Action? _refreshRepositoryView;
        private string _selectedFilePath = string.Empty;
        private ExcelPreviewWorkbookInfo? _workbookPreview;
        private ExcelImportSchema? _schema;
        private ExcelImportSheetSchema? _currentSheet;
        private bool _isUpdatingSheetControls;
        private bool _isUpdatingRelationControls;
        private ExcelImportSheetSchema? _draggedSheet;
        private ExcelImportColumnProfile? _draggedColumn;
        private Border? _draggedBorder;
        private DiagramDragMode _diagramDragMode = DiagramDragMode.None;
        private Line? _relationPreviewLine;
        private readonly Dictionary<string, FrameworkElement> _diagramColumnElements = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<DiagramRelationVisual> _diagramRelationVisuals = new();
        private bool _diagramInitialLayoutPending;
        private double _diagramZoom = 1.0;
        private Point _dragStartPoint;
        private double _dragStartLeft;
        private double _dragStartTop;

        internal bool CompletedImport { get; private set; }

        public Array AvailableColumnRoles => Enum.GetValues(typeof(ExcelImportColumnRole));
        public List<ExcelImportDefinitionScopeItem> AvailableDefinitionScopes { get; } = ExcelImportDisplayOptionsHelper.CreateDefinitionScopeItems();
        public List<ExcelImportValueModeItem> AvailableValueModes { get; } = ExcelImportDisplayOptionsHelper.CreateValueModeItems();
        public List<ExcelImportEntityKindItem> AvailableEntityKinds { get; } = ExcelImportDisplayOptionsHelper.CreateEntityKindItems();

        public ExcelImportDesignerWindow(
            IServiceProvider serviceProvider,
            ConversionService conversionService,
            ExcelPreviewService previewService,
            IExcelImportSchemaBuilder schemaBuilder,
            IExcelImportSchemaTemplateStorage templateStorage,
            ExcelImportPipeline importPipeline)
        {
            InitializeComponent();
            DataContext = this;
            _serviceProvider = serviceProvider;
            _conversionService = conversionService;
            _previewService = previewService;
            _schemaBuilder = schemaBuilder;
            _templateStorage = templateStorage;
            _importPipeline = importPipeline;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            ResetDiagramDragState();
            SyncSchemaFromRootControls();
            base.OnClosing(e);
        }

        internal void Initialize(
            Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubModel shrub,
            Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryModel repository,
            IPhiladelphusRepositoryService repositoryService,
            Action refreshRepositoryView)
        {
            _repository = repository;
            _repositoryService = repositoryService;
            _refreshRepositoryView = refreshRepositoryView;
            InitializeRootsList(shrub);
            ChkCreateNewRoot.IsChecked = true;
            ChkCreateNewRoot_Checked(this, new RoutedEventArgs());
        }

        internal void LoadWorkbook(string filePath)
        {
            if (File.Exists(filePath) == false)
                return;

            LoadSchemaFromWorkbook(filePath);
        }

        internal void LoadSchema(ExcelImportSchema schema)
        {
            _schema = schema;
            _diagramInitialLayoutPending = true;
            ExcelImportSchemaNormalizer.EnsureEditableState(_schema);
            ExcelImportSchemaNormalizer.RefreshRelationProjection(_schema);

            _selectedFilePath = _schema.SourceFilePath;
            TxtFilePath.Text = string.IsNullOrWhiteSpace(_selectedFilePath)
                ? "Файл не выбран"
                : IOPath.GetFileName(_selectedFilePath);

            _workbookPreview = string.IsNullOrWhiteSpace(_selectedFilePath) || File.Exists(_selectedFilePath) == false
                ? null
                : _previewService.GetWorkbookPreview(_selectedFilePath);

            ApplyRootControlsFromSchema();
            BindSchema();
        }

        private void InitializeRootsList(Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubModel shrub)
        {
            var roots = _conversionService.GetExistingRootsFromStorage(shrub);
            CmbExistingRoots.ItemsSource = roots;
            CmbExistingRoots.DisplayMemberPath = nameof(Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers.TreeRootModel.Name);
            CmbExistingRoots.SelectedValuePath = nameof(Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers.TreeRootModel.Name);

            if (roots.Count > 0)
            {
                CmbExistingRoots.SelectedIndex = 0;
            }
        }

        private void ApplyRootControlsFromSchema()
        {
            if (_schema == null)
                return;

            // Книга Excel всегда является корнем результата, поэтому выбор существующего корня отключен.
            _schema.CreateNewRoot = true;
            TxtRootName.Text = _schema.RootName;
            ChkCreateNewRoot.IsChecked = true;
            ChkCreateNewRoot_Checked(this, new RoutedEventArgs());
        }

        private bool TrySyncSchemaFromImportParameters(bool isNewRoot, string rootName)
        {
            if (_schema == null)
            {
                MessageBox.Show("Сначала выберите Excel-файл и настройте схему импорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            _schema.SourceFilePath = _selectedFilePath;
            _schema.CreateNewRoot = true;
            _schema.RootName = rootName;
            ExcelImportSchemaNormalizer.NormalizeForExecution(_schema);
            RefreshRelationViews();
            return true;
        }

        private void SyncSchemaFromRootControls()
        {
            if (_schema == null)
                return;

            _schema.SourceFilePath = _selectedFilePath;
            _schema.CreateNewRoot = true;
            _schema.RootName = TxtRootName.Text?.Trim() ?? string.Empty;
            ExcelImportSchemaNormalizer.RefreshRelationProjection(_schema);
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Excel Files|*.xlsx;*.xls",
                Title = "Выберите файл Excel"
            };

            if (dialog.ShowDialog() != true)
                return;

            LoadSchemaFromWorkbook(dialog.FileName);
        }

        private void LoadSchemaFromWorkbook(string filePath)
        {
            _selectedFilePath = filePath;
            TxtFilePath.Text = IOPath.GetFileName(filePath);
            _workbookPreview = _previewService.GetWorkbookPreview(filePath);
            _schema = _schemaBuilder.CreateDraftSchema(filePath, IOPath.GetFileNameWithoutExtension(filePath));
            _diagramInitialLayoutPending = true;
            _schema.CreateNewRoot = true;
            ApplyRootControlsFromSchema();
            BindSchema();
        }

        private void BindSchema()
        {
            if (_schema == null)
                return;

            ExcelImportSchemaNormalizer.EnsureEditableState(_schema);
            RefreshRelationViews();
            LstSchemaSheets.ItemsSource = _schema.Sheets;
            CmbRelationChildSheet.ItemsSource = _schema.Sheets.Select(x => x.SourceName).ToList();

            if (_schema.Sheets.Count > 0)
            {
                LstSchemaSheets.SelectedIndex = 0;
            }

            RenderDiagram();
            ClearPreviewResult();
        }

        private void LstSchemaSheets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _currentSheet = LstSchemaSheets.SelectedItem as ExcelImportSheetSchema;
            BindCurrentSheet();
        }

        private void BindCurrentSheet()
        {
            _isUpdatingSheetControls = true;
            try
            {
                if (_currentSheet == null || string.IsNullOrWhiteSpace(_selectedFilePath))
                {
                    TxtSheetDisplayName.Text = string.Empty;
                    TxtCurrentSheetSource.Text = string.Empty;
                    CmbSheetEntityKind.SelectedValue = ExcelImportEntityKind.Node;
                    DgSheetColumns.ItemsSource = null;
                    DgSheetPreview.ItemsSource = null;
                    TxtSheetPreviewInfo.Text = "Лист не выбран.";
                    return;
                }

                ExcelImportProfileEditorHelper.SyncSheetFieldsFromProfile(_currentSheet);

                TxtSheetDisplayName.Text = _currentSheet.DisplayName;
                TxtCurrentSheetSource.Text = _currentSheet.SourceName;
                _currentSheet.EntityKind = ExcelImportEntityKind.Node;
                _currentSheet.Profile.EntityKind = ExcelImportEntityKind.Node;
                CmbSheetEntityKind.SelectedValue = ExcelImportEntityKind.Node;

                var headers = ExcelImportProfileEditorHelper.BuildHeaderOptions(_currentSheet.Profile.Columns, sort: false);
                CmbSheetKeyColumn.ItemsSource = headers;

                CmbSheetKeyColumn.SelectedItem = string.IsNullOrWhiteSpace(_currentSheet.RowKeyColumnName) ? null : _currentSheet.RowKeyColumnName;

                DgSheetColumns.ItemsSource = _currentSheet.Profile.Columns;

                var preview = _previewService.GetPreview(_selectedFilePath, _currentSheet.Profile.SourceSelection);
                TxtSheetPreviewInfo.Text = $"Лист: {preview.SourceName}. Строк данных: {preview.TotalRowCount}. Колонок: {preview.TotalColumnCount}.";
                DgSheetPreview.ItemsSource = ExcelPreviewTableBuilder.Build(preview).DefaultView;
            }
            finally
            {
                _isUpdatingSheetControls = false;
            }

            BindRelationEditor(_currentSheet?.SourceName);
            LstSchemaSheets.Items.Refresh();
            DgRelations.Items.Refresh();
            RenderDiagram();
        }

        private void TxtSheetDisplayName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdatingSheetControls || _currentSheet == null)
                return;

            _currentSheet.DisplayName = TxtSheetDisplayName.Text.Trim();
            LstSchemaSheets.Items.Refresh();
            RenderDiagram();
        }

        private void CmbSheetKeyColumn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSheetControls || _currentSheet == null)
                return;

            _currentSheet.RowKeyColumnName = CmbSheetKeyColumn.SelectedItem as string ?? string.Empty;
            RenderDiagram();
        }

        private void CmbSheetEntityKind_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingSheetControls || _currentSheet == null)
                return;

            ApplySheetEntityKind(_currentSheet, ExcelImportEntityKind.Node);
            LstSchemaSheets.Items.Refresh();
            RenderDiagram();
        }

        private void ApplySheetEntityKind(ExcelImportSheetSchema sheet, ExcelImportEntityKind entityKind)
        {
            sheet.EntityKind = entityKind;
            sheet.Profile.EntityKind = entityKind;
        }

        private void DgSheetColumns_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (_currentSheet == null)
                    return;

                ExcelImportProfileEditorHelper.SyncSheetFieldsFromProfile(_currentSheet);
                BindCurrentSheet();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void SheetEnabledChanged(object sender, RoutedEventArgs e)
        {
            RefreshRelationViews();
            RenderDiagram();
        }

        private void RefreshRelationViews()
        {
            if (_schema == null)
                return;

            ExcelImportSchemaNormalizer.RefreshRelationProjection(_schema);
            DgRelations.ItemsSource = _schema.Relations;
            DgRelations.Items.Refresh();
        }

        private void DgRelations_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var relation = DgRelations.SelectedItem as ExcelImportRelationSchema;
            BindRelationEditor(relation?.ChildSourceName);
        }

        private void BindRelationEditor(string? childSourceName)
        {
            _isUpdatingRelationControls = true;
            try
            {
                if (_schema == null)
                    return;

                var childSheets = _schema.Sheets.Select(x => x.SourceName).ToList();
                CmbRelationChildSheet.ItemsSource = childSheets;
                CmbRelationChildSheet.SelectedItem = string.IsNullOrWhiteSpace(childSourceName) ? null : childSourceName;

                var childSheet = GetSheet(childSourceName);
                if (childSheet == null)
                {
                    CmbRelationParentSheet.ItemsSource = null;
                    CmbRelationParentKey.ItemsSource = null;
                    CmbRelationChildKey.ItemsSource = null;
                    return;
                }

                var parentOptions = ExcelImportProfileEditorHelper.BuildParentSourceOptions(
                    _schema.Sheets.Select(x => x.SourceName),
                    childSheet.SourceName,
                    NoParentRelationOption);
                CmbRelationParentSheet.ItemsSource = parentOptions;
                CmbRelationParentSheet.SelectedItem = ExcelImportProfileEditorHelper.GetSelectedParentOption(
                    childSheet.Profile.Relation.ParentSourceName,
                    parentOptions,
                    NoParentRelationOption);

                var childHeaders = ExcelImportProfileEditorHelper.BuildHeaderOptions(childSheet.Profile.Columns, sort: false);
                CmbRelationChildKey.ItemsSource = childHeaders;
                CmbRelationChildKey.SelectedItem = string.IsNullOrWhiteSpace(childSheet.Profile.Relation.ChildKeyColumnName)
                    ? null
                    : childSheet.Profile.Relation.ChildKeyColumnName;

                RefreshRelationParentKeyOptions(childSheet);
            }
            finally
            {
                _isUpdatingRelationControls = false;
            }
        }

        private void RefreshRelationParentKeyOptions(ExcelImportSheetSchema childSheet)
        {
            var parentSheet = GetSheet(childSheet.Profile.Relation.ParentSourceName);
            if (parentSheet == null)
            {
                CmbRelationParentKey.ItemsSource = null;
                CmbRelationParentKey.SelectedItem = null;
                return;
            }

            var headers = ExcelImportProfileEditorHelper.BuildHeaderOptions(parentSheet.Profile.Columns, sort: false);
            CmbRelationParentKey.ItemsSource = headers;
            CmbRelationParentKey.SelectedItem = string.IsNullOrWhiteSpace(childSheet.Profile.Relation.ParentKeyColumnName)
                ? null
                : childSheet.Profile.Relation.ParentKeyColumnName;
        }

        private void CmbRelationChildSheet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingRelationControls)
                return;

            BindRelationEditor(CmbRelationChildSheet.SelectedItem as string);
        }

        private void CmbRelationParentSheet_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingRelationControls)
                return;

            var childSheet = GetSheet(CmbRelationChildSheet.SelectedItem as string);
            if (childSheet == null)
                return;

            var parentName = CmbRelationParentSheet.SelectedItem as string;
            if (TryApplyRelationParent(childSheet, parentName, selectChildSheet: false) == false)
            {
                BindRelationEditor(childSheet.SourceName);
            }
        }

        private void CmbRelationParentKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingRelationControls)
                return;

            var childSheet = GetSheet(CmbRelationChildSheet.SelectedItem as string);
            if (childSheet == null)
                return;

            ExcelImportProfileEditorHelper.SetRelationParentKey(childSheet, CmbRelationParentKey.SelectedItem as string);
            RefreshRelationUi(childSheet, bindEditor: false, selectChildSheet: false);
        }

        private void CmbRelationChildKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingRelationControls)
                return;

            var childSheet = GetSheet(CmbRelationChildSheet.SelectedItem as string);
            if (childSheet == null)
                return;

            ExcelImportProfileEditorHelper.SetRelationChildKey(childSheet, CmbRelationChildKey.SelectedItem as string);
            RefreshRelationUi(childSheet, bindEditor: false, selectChildSheet: false);
        }

        private void BtnAddRelation_Click(object sender, RoutedEventArgs e)
        {
            if (_schema == null)
                return;

            var targetSheet = _schema.Sheets.FirstOrDefault(x => string.IsNullOrWhiteSpace(x.Profile.Relation.ParentSourceName));
            if (targetSheet == null)
            {
                MessageBox.Show("Все листы уже имеют настроенную связь или книга не загружена.", "Связи", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            BindRelationEditor(targetSheet.SourceName);
        }

        private void BtnRemoveRelation_Click(object sender, RoutedEventArgs e)
        {
            var childSheet = GetSheet(CmbRelationChildSheet.SelectedItem as string);
            if (childSheet == null)
                return;

            ClearSheetRelation(childSheet);
        }

        private void RenderDiagram()
        {
            ResetDiagramDragState();
            DiagramCanvas.Children.Clear();
            _diagramColumnElements.Clear();
            _diagramRelationVisuals.Clear();

            if (_schema == null)
                return;

            var visibleSheets = _schema.Sheets.Where(x => x.IsEnabled).ToList();
            if (_diagramInitialLayoutPending)
            {
                ArrangeDiagramCardsByGrid(visibleSheets);
                _diagramInitialLayoutPending = false;
            }

            foreach (var sheet in visibleSheets)
            {
                var card = BuildDiagramCard(sheet);
                Canvas.SetLeft(card, sheet.CanvasX);
                Canvas.SetTop(card, sheet.CanvasY);
                DiagramCanvas.Children.Add(card);
            }

            DiagramCanvas.UpdateLayout();
            foreach (var relation in ExcelImportSchemaNormalizer.BuildRelationProjection(_schema).Where(x => x.IsEnabled))
            {
                AddRelationVisual(relation);
            }
        }

        private void ArrangeDiagramCardsByGrid(IReadOnlyList<ExcelImportSheetSchema> visibleSheets)
        {
            if (visibleSheets.Count == 0)
                return;

            var availableWidth = DiagramScrollViewer.ViewportWidth;
            if (double.IsNaN(availableWidth) || availableWidth <= 0)
                availableWidth = DiagramCanvas.Width;

            var maxCardWidth = visibleSheets.Max(ResolveDiagramCardWidth);
            var cellWidth = maxCardWidth + DiagramGridColumnGap;
            var columnCount = Math.Max(1, (int)Math.Floor((availableWidth - DiagramGridPadding * 2 + DiagramGridColumnGap) / cellWidth));
            columnCount = Math.Min(columnCount, visibleSheets.Count);

            var rowCount = (int)Math.Ceiling((double)visibleSheets.Count / columnCount);
            var canvasWidth = DiagramGridPadding * 2 + columnCount * maxCardWidth + Math.Max(0, columnCount - 1) * DiagramGridColumnGap;
            var canvasHeight = DiagramGridPadding * 2 + rowCount * DiagramGridRowHeight + Math.Max(0, rowCount - 1) * DiagramGridRowGap;
            DiagramCanvas.Width = Math.Max(1400, canvasWidth);
            DiagramCanvas.Height = Math.Max(900, canvasHeight);

            for (var index = 0; index < visibleSheets.Count; index++)
            {
                var sheet = visibleSheets[index];
                var row = index / columnCount;
                var column = index % columnCount;
                var cardWidth = ResolveDiagramCardWidth(sheet);

                sheet.CanvasX = DiagramGridPadding
                    + column * (maxCardWidth + DiagramGridColumnGap)
                    + (maxCardWidth - cardWidth) / 2;
                sheet.CanvasY = DiagramGridPadding + row * (DiagramGridRowHeight + DiagramGridRowGap);
            }
        }

        private Border BuildDiagramCard(ExcelImportSheetSchema sheet)
        {
            var border = new Border
            {
                Width = ResolveDiagramCardWidth(sheet),
                MinHeight = DiagramCardMinHeight,
                BorderBrush = ReferenceEquals(sheet, _currentSheet) ? Brushes.SteelBlue : Brushes.Silver,
                BorderThickness = new Thickness(1),
                Background = Brushes.White,
                Padding = new Thickness(8),
                CornerRadius = new CornerRadius(6),
                Cursor = Cursors.SizeAll,
                Tag = sheet
            };

            var layout = new Grid();
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var title = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(sheet.DisplayName) ? sheet.SourceName : sheet.DisplayName,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(title, 0);
            layout.Children.Add(title);

            var relation = new TextBlock
            {
                Text = string.IsNullOrWhiteSpace(sheet.Profile.Relation.ParentSourceName)
                    ? "Без родителя"
                    : string.IsNullOrWhiteSpace(sheet.Profile.Relation.ParentKeyColumnName)
                        || string.IsNullOrWhiteSpace(sheet.Profile.Relation.ChildKeyColumnName)
                            ? $"Родитель: {sheet.Profile.Relation.ParentSourceName}; ключи не заданы"
                            : $"Родитель: {sheet.Profile.Relation.ParentSourceName} ({sheet.Profile.Relation.ParentKeyColumnName} -> {sheet.Profile.Relation.ChildKeyColumnName})",
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 6, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };

            var relationPanel = new Grid();
            relationPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            relationPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            Grid.SetColumn(relation, 0);
            relationPanel.Children.Add(relation);

            if (string.IsNullOrWhiteSpace(sheet.Profile.Relation.ParentSourceName) == false)
            {
                var removeRelationButton = new Button
                {
                    Content = "x",
                    Width = 28,
                    Height = 24,
                    Padding = new Thickness(0),
                    Margin = new Thickness(8, 2, 0, 0),
                    Cursor = Cursors.Hand,
                    ToolTip = "Удалить связь с родителем",
                    VerticalAlignment = VerticalAlignment.Top
                };
                removeRelationButton.Click += (_, _) => ClearSheetRelation(sheet);
                Grid.SetColumn(removeRelationButton, 1);
                relationPanel.Children.Add(removeRelationButton);
            }

            Grid.SetRow(relationPanel, 1);
            layout.Children.Add(relationPanel);

            var columnsTitle = new TextBlock
            {
                Text = "Колонки",
                FontSize = 11,
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 8, 0, 3)
            };
            Grid.SetRow(columnsTitle, 2);
            layout.Children.Add(columnsTitle);

            var columnsPanel = new StackPanel();
            foreach (var column in sheet.Profile.Columns.OrderBy(x => x.ColumnIndex))
            {
                columnsPanel.Children.Add(BuildDiagramColumnItem(sheet, column, border));
            }

            var columnsScroll = new ScrollViewer
            {
                Content = columnsPanel,
                MaxHeight = ResolveDiagramColumnsMaxHeight(sheet),
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
            };
            Grid.SetRow(columnsScroll, 3);
            layout.Children.Add(columnsScroll);
            border.Child = layout;

            border.MouseLeftButtonDown += DiagramCard_MouseLeftButtonDown;
            border.MouseMove += DiagramCard_MouseMove;
            border.MouseLeftButtonUp += DiagramCard_MouseLeftButtonUp;
            border.LostMouseCapture += DiagramCard_LostMouseCapture;

            return border;
        }

        private static double ResolveDiagramCardWidth(ExcelImportSheetSchema sheet)
        {
            var relationText = string.IsNullOrWhiteSpace(sheet.Profile.Relation.ParentSourceName)
                ? "Без родителя"
                : $"{sheet.Profile.Relation.ParentSourceName} ({sheet.Profile.Relation.ParentKeyColumnName} -> {sheet.Profile.Relation.ChildKeyColumnName})";

            var maxTextLength = new[]
            {
                string.IsNullOrWhiteSpace(sheet.DisplayName) ? sheet.SourceName : sheet.DisplayName,
                relationText
            }
            .Concat(sheet.Profile.Columns.Select(column =>
                string.IsNullOrWhiteSpace(column.DataTypeNodeName)
                    ? column.HeaderName
                    : $"{column.HeaderName} · {column.DataTypeNodeName}"))
            .DefaultIfEmpty(string.Empty)
            .Max(text => text?.Length ?? 0);

            // Ширина карточки подстраивается под длинные имена колонок, но не раздувает всю диаграмму бесконечно.
            var estimatedWidth = 80 + maxTextLength * 6.5;
            return Math.Clamp(estimatedWidth, DiagramCardWidth, DiagramCardMaxWidth);
        }

        private static double ResolveDiagramColumnsMaxHeight(ExcelImportSheetSchema sheet)
        {
            var visibleRowsHeight = Math.Max(1, sheet.Profile.Columns.Count) * 34;
            return Math.Clamp(visibleRowsHeight, 96, 260);
        }

        private void DiagramScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                return;

            var oldZoom = _diagramZoom;
            var factor = e.Delta > 0 ? DiagramZoomStep : 1 / DiagramZoomStep;
            var newZoom = Math.Clamp(oldZoom * factor, DiagramZoomMin, DiagramZoomMax);
            if (Math.Abs(newZoom - oldZoom) < 0.001)
            {
                e.Handled = true;
                return;
            }

            var mouseOnCanvas = e.GetPosition(DiagramCanvas);
            var mouseOnViewer = e.GetPosition(DiagramScrollViewer);

            _diagramZoom = newZoom;
            DiagramCanvasScaleTransform.ScaleX = newZoom;
            DiagramCanvasScaleTransform.ScaleY = newZoom;
            e.Handled = true;

            Dispatcher.BeginInvoke(new Action(() =>
            {
                DiagramScrollViewer.ScrollToHorizontalOffset((mouseOnCanvas.X * newZoom) - mouseOnViewer.X);
                DiagramScrollViewer.ScrollToVerticalOffset((mouseOnCanvas.Y * newZoom) - mouseOnViewer.Y);
                UpdateRelationVisuals();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private Border BuildDiagramColumnItem(ExcelImportSheetSchema sheet, ExcelImportColumnProfile column, Border parentBorder)
        {
            var isIgnored = column.Role == ExcelImportColumnRole.Ignore;
            var isRelationColumn = IsRelationColumn(sheet, column);
            var columnBorder = new Border
            {
                Tag = new DiagramColumnTag(sheet, column),
                Background = isIgnored
                    ? Brushes.Gainsboro
                    : isRelationColumn ? Brushes.Honeydew : Brushes.AliceBlue,
                BorderBrush = isIgnored
                    ? Brushes.LightGray
                    : isRelationColumn ? Brushes.SeaGreen : Brushes.LightSteelBlue,
                BorderThickness = isRelationColumn ? new Thickness(2) : new Thickness(1),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(6, 3, 6, 3),
                Margin = new Thickness(0, 0, 0, 4),
                Cursor = Cursors.Hand,
                ToolTip = isRelationColumn
                    ? "Колонка участвует в связи. Потяните колонку, чтобы переназначить связь"
                    : "Потяните эту колонку на колонку дочернего листа, чтобы задать связь"
            };

            var text = string.IsNullOrWhiteSpace(column.DataTypeNodeName)
                ? column.HeaderName
                : $"{column.HeaderName} · {column.DataTypeNodeName}";
            if (isIgnored)
            {
                text += " · игнор";
            }

            columnBorder.Child = new TextBlock
            {
                Text = text,
                FontSize = 11,
                Foreground = isIgnored ? Brushes.DimGray : Brushes.Black,
                TextTrimming = TextTrimming.CharacterEllipsis
            };
            columnBorder.PreviewMouseLeftButtonDown += (_, e) => BeginColumnRelationDrag(sheet, column, parentBorder, columnBorder, e);
            _diagramColumnElements[GetDiagramColumnKey(sheet.SourceName, column.HeaderName)] = columnBorder;

            return columnBorder;
        }

        private void DiagramCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is not Border border || border.Tag is not ExcelImportSheetSchema sheet)
                return;

            if (IsInsideButton(e.OriginalSource as DependencyObject))
                return;

            ResetDiagramDragState();
            _draggedSheet = sheet;
            _draggedBorder = border;
            _diagramDragMode = DiagramDragMode.MoveSheet;
            _dragStartPoint = e.GetPosition(DiagramCanvas);
            _dragStartLeft = GetCanvasLeft(border);
            _dragStartTop = GetCanvasTop(border);
            border.CaptureMouse();
            e.Handled = true;
        }

        private void DiagramCard_MouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedSheet == null || _draggedBorder == null)
                return;

            if (e.LeftButton != MouseButtonState.Pressed)
            {
                ResetDiagramDragState();
                return;
            }

            var currentPoint = e.GetPosition(DiagramCanvas);
            if (_diagramDragMode == DiagramDragMode.CreateRelation)
            {
                UpdateRelationPreviewLine(currentPoint);
                return;
            }

            if (_diagramDragMode != DiagramDragMode.MoveSheet)
                return;

            var offsetX = currentPoint.X - _dragStartPoint.X;
            var offsetY = currentPoint.Y - _dragStartPoint.Y;
            Canvas.SetLeft(_draggedBorder, _dragStartLeft + offsetX);
            Canvas.SetTop(_draggedBorder, _dragStartTop + offsetY);
            UpdateRelationVisuals();
        }

        private void DiagramCard_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_draggedSheet == null || _draggedBorder == null)
                return;

            var draggedSheet = _draggedSheet;
            var draggedColumn = _draggedColumn;
            var draggedBorder = _draggedBorder;
            var dragMode = _diagramDragMode;
            var dropPoint = e.GetPosition(DiagramCanvas);
            var targetColumn = dragMode == DiagramDragMode.CreateRelation && draggedColumn != null
                ? FindDiagramColumnDropTarget(dropPoint, draggedColumn)
                : null;

            if (dragMode == DiagramDragMode.MoveSheet)
            {
                draggedSheet.CanvasX = GetCanvasLeft(draggedBorder);
                draggedSheet.CanvasY = GetCanvasTop(draggedBorder);
            }

            ResetDiagramDragState();

            if (dragMode == DiagramDragMode.CreateRelation && draggedColumn != null && targetColumn != null)
            {
                TryApplyRelationColumns(
                    targetColumn.Sheet,
                    draggedSheet.SourceName,
                    draggedColumn.HeaderName,
                    targetColumn.Column.HeaderName,
                    selectChildSheet: true);
            }

            RenderDiagram();
            e.Handled = true;
        }

        private void DiagramCard_LostMouseCapture(object sender, MouseEventArgs e)
        {
            if (ReferenceEquals(sender, _draggedBorder) == false)
                return;

            if (_diagramDragMode == DiagramDragMode.MoveSheet && _draggedSheet != null && _draggedBorder != null)
            {
                _draggedSheet.CanvasX = GetCanvasLeft(_draggedBorder);
                _draggedSheet.CanvasY = GetCanvasTop(_draggedBorder);
            }

            ResetDiagramDragState(releaseMouseCapture: false);
        }

        private void BeginColumnRelationDrag(
            ExcelImportSheetSchema parentSheet,
            ExcelImportColumnProfile parentColumn,
            Border parentBorder,
            FrameworkElement sourceColumnElement,
            MouseButtonEventArgs e)
        {
            ResetDiagramDragState();
            _draggedSheet = parentSheet;
            _draggedColumn = parentColumn;
            _draggedBorder = parentBorder;
            _diagramDragMode = DiagramDragMode.CreateRelation;
            _dragStartPoint = GetElementCenter(sourceColumnElement);
            parentBorder.CaptureMouse();
            CreateRelationPreviewLine(_dragStartPoint);
            e.Handled = true;
        }

        // The diagram is only a visual/editor surface over the canonical relation state
        // stored in sheet.Profile.Relation. The relation grid and import pipeline read the same data.
        private void AddRelationVisual(ExcelImportRelationSchema relation)
        {
            if (TryGetRelationColumnElements(relation, out var parentColumn, out var childColumn) == false)
                return;

            var start = GetColumnConnectionPoint(parentColumn, childColumn);
            var end = GetColumnConnectionPoint(childColumn, parentColumn);

            var line = new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = Brushes.SeaGreen,
                StrokeThickness = 2.5,
                IsHitTestVisible = false
            };
            var arrowHead = CreateArrowHead(start, end, Brushes.SeaGreen);

            var visual = new DiagramRelationVisual
            {
                Relation = relation,
                Line = line,
                ArrowHead = arrowHead
            };

            // Стрелки должны быть поверх карточек: связь должна явно выходить из колонки и приходить в колонку.
            DiagramCanvas.Children.Add(line);
            DiagramCanvas.Children.Add(arrowHead);
            _diagramRelationVisuals.Add(visual);
        }

        private void UpdateRelationVisuals()
        {
            DiagramCanvas.UpdateLayout();
            foreach (var visual in _diagramRelationVisuals)
            {
                if (TryGetRelationColumnElements(visual.Relation, out var parentColumn, out var childColumn) == false)
                    continue;

                var start = GetColumnConnectionPoint(parentColumn, childColumn);
                var end = GetColumnConnectionPoint(childColumn, parentColumn);
                visual.Line.X1 = start.X;
                visual.Line.Y1 = start.Y;
                visual.Line.X2 = end.X;
                visual.Line.Y2 = end.Y;
                visual.ArrowHead.Points = CreateArrowHeadPoints(start, end);
            }
        }

        private bool TryGetRelationColumnElements(
            ExcelImportRelationSchema relation,
            out FrameworkElement parentColumn,
            out FrameworkElement childColumn)
        {
            var parentKey = GetDiagramColumnKey(relation.ParentSourceName, relation.ParentKeyColumnName);
            var childKey = GetDiagramColumnKey(relation.ChildSourceName, relation.ChildKeyColumnName);
            var hasParent = _diagramColumnElements.TryGetValue(parentKey, out var parent);
            var hasChild = _diagramColumnElements.TryGetValue(childKey, out var child);
            parentColumn = parent!;
            childColumn = child!;
            return hasParent && hasChild;
        }

        private bool IsRelationColumn(ExcelImportSheetSchema sheet, ExcelImportColumnProfile column)
        {
            if (_schema == null)
                return false;

            return ExcelImportSchemaNormalizer.BuildRelationProjection(_schema)
                .Where(x => x.IsEnabled)
                .Any(relation =>
                    (string.Equals(relation.ParentSourceName, sheet.SourceName, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(relation.ParentKeyColumnName, column.HeaderName, StringComparison.OrdinalIgnoreCase))
                    || (string.Equals(relation.ChildSourceName, sheet.SourceName, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(relation.ChildKeyColumnName, column.HeaderName, StringComparison.OrdinalIgnoreCase)));
        }

        private static string GetDiagramColumnKey(string sourceName, string columnName)
        {
            return $"{sourceName}\u001f{columnName}";
        }

        private Point GetColumnConnectionPoint(FrameworkElement sourceColumn, FrameworkElement targetColumn)
        {
            var sourceRect = GetElementCanvasRect(sourceColumn);
            var targetRect = GetElementCanvasRect(targetColumn);
            var sourceCenter = new Point(sourceRect.Left + sourceRect.Width / 2, sourceRect.Top + sourceRect.Height / 2);
            var targetCenter = new Point(targetRect.Left + targetRect.Width / 2, targetRect.Top + targetRect.Height / 2);

            var x = targetCenter.X >= sourceCenter.X ? sourceRect.Right : sourceRect.Left;
            return new Point(x, sourceCenter.Y);
        }

        private Rect GetElementCanvasRect(FrameworkElement element)
        {
            var topLeft = element.TransformToAncestor(DiagramCanvas).Transform(new Point(0, 0));
            var width = element.ActualWidth > 0 ? element.ActualWidth : element.Width;
            var height = element.ActualHeight > 0 ? element.ActualHeight : element.Height;
            return new Rect(topLeft, new Size(width, height));
        }

        private void CreateRelationPreviewLine(Point start)
        {
            RemoveRelationPreviewLine();
            _relationPreviewLine = new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = start.X,
                Y2 = start.Y,
                Stroke = Brushes.SteelBlue,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 3 },
                IsHitTestVisible = false
            };
            DiagramCanvas.Children.Add(_relationPreviewLine);
        }

        private void UpdateRelationPreviewLine(Point currentPoint)
        {
            if (_relationPreviewLine == null)
                return;

            _relationPreviewLine.X2 = currentPoint.X;
            _relationPreviewLine.Y2 = currentPoint.Y;
        }

        private void RemoveRelationPreviewLine()
        {
            if (_relationPreviewLine == null)
                return;

            DiagramCanvas.Children.Remove(_relationPreviewLine);
            _relationPreviewLine = null;
        }

        private void ResetDiagramDragState(bool releaseMouseCapture = true)
        {
            var capturedBorder = _draggedBorder;

            RemoveRelationPreviewLine();
            _draggedSheet = null;
            _draggedColumn = null;
            _draggedBorder = null;
            _diagramDragMode = DiagramDragMode.None;

            if (releaseMouseCapture && capturedBorder?.IsMouseCaptured == true)
            {
                capturedBorder.ReleaseMouseCapture();
            }
        }

        private static Point GetBorderCenter(Border border)
        {
            var left = GetCanvasLeft(border);
            var top = GetCanvasTop(border);
            var width = border.ActualWidth > 0 ? border.ActualWidth : border.Width;
            var height = border.ActualHeight > 0 ? border.ActualHeight : border.Height;
            return new Point(left + width / 2, top + height / 2);
        }

        private Point GetElementCenter(FrameworkElement element)
        {
            try
            {
                var topLeft = element.TransformToAncestor(DiagramCanvas).Transform(new Point(0, 0));
                var width = element.ActualWidth > 0 ? element.ActualWidth : element.Width;
                var height = element.ActualHeight > 0 ? element.ActualHeight : element.Height;
                return new Point(topLeft.X + width / 2, topLeft.Y + height / 2);
            }
            catch (InvalidOperationException)
            {
                return _draggedBorder == null
                    ? new Point()
                    : GetBorderCenter(_draggedBorder);
            }
        }

        private static Polygon CreateArrowHead(Point start, Point end, Brush brush)
        {
            return new Polygon
            {
                Fill = brush,
                Stroke = brush,
                Points = CreateArrowHeadPoints(start, end),
                IsHitTestVisible = false
            };
        }

        private static PointCollection CreateArrowHeadPoints(Point start, Point end)
        {
            var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
            const double length = 11;
            const double width = 6;
            var back = new Point(
                end.X - length * Math.Cos(angle),
                end.Y - length * Math.Sin(angle));
            var normalX = Math.Cos(angle + Math.PI / 2);
            var normalY = Math.Sin(angle + Math.PI / 2);

            return new PointCollection
            {
                end,
                new Point(back.X + width * normalX, back.Y + width * normalY),
                new Point(back.X - width * normalX, back.Y - width * normalY)
            };
        }

        private DiagramColumnTag? FindDiagramColumnDropTarget(Point dropPoint, ExcelImportColumnProfile draggedColumn)
        {
            return FindDiagramColumnDropTarget(DiagramCanvas, dropPoint, draggedColumn);
        }

        private DiagramColumnTag? FindDiagramColumnDropTarget(
            DependencyObject source,
            Point dropPoint,
            ExcelImportColumnProfile draggedColumn)
        {
            int childrenCount;
            try
            {
                childrenCount = VisualTreeHelper.GetChildrenCount(source);
            }
            catch (InvalidOperationException)
            {
                return null;
            }

            for (var i = childrenCount - 1; i >= 0; i--)
            {
                var child = VisualTreeHelper.GetChild(source, i);
                if (child is FrameworkElement element
                    && element.Tag is DiagramColumnTag tag
                    && ReferenceEquals(tag.Column, draggedColumn) == false
                    && IsPointInsideElement(element, dropPoint))
                {
                    return tag;
                }

                var nestedTarget = FindDiagramColumnDropTarget(child, dropPoint, draggedColumn);
                if (nestedTarget != null)
                    return nestedTarget;
            }

            return null;
        }

        private bool IsPointInsideElement(FrameworkElement element, Point canvasPoint)
        {
            if (element.IsVisible == false)
                return false;

            try
            {
                var topLeft = element.TransformToAncestor(DiagramCanvas).Transform(new Point(0, 0));
                var width = element.ActualWidth > 0 ? element.ActualWidth : element.Width;
                var height = element.ActualHeight > 0 ? element.ActualHeight : element.Height;
                return width > 0
                    && height > 0
                    && new Rect(topLeft, new Size(width, height)).Contains(canvasPoint);
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        private static double GetCanvasLeft(FrameworkElement element)
        {
            var left = Canvas.GetLeft(element);
            return double.IsNaN(left) ? 0 : left;
        }

        private static double GetCanvasTop(FrameworkElement element)
        {
            var top = Canvas.GetTop(element);
            return double.IsNaN(top) ? 0 : top;
        }

        private static bool IsInsideButton(DependencyObject? source)
        {
            while (source != null)
            {
                if (source is Button || source is ComboBox)
                    return true;

                try
                {
                    source = VisualTreeHelper.GetParent(source);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }

            return false;
        }

        private bool TryApplyRelationParent(
            ExcelImportSheetSchema childSheet,
            string? parentSourceName,
            bool selectChildSheet)
        {
            if (_schema == null)
                return false;

            if (ExcelImportProfileEditorHelper.TrySetRelationParent(
                    _schema.Sheets,
                    childSheet,
                    parentSourceName,
                    NoParentRelationOption,
                    out var errorMessage) == false)
            {
                MessageBox.Show(errorMessage, "Связи", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            RefreshRelationUi(childSheet, bindEditor: true, selectChildSheet);
            return true;
        }

        private bool TryApplyRelationColumns(
            ExcelImportSheetSchema childSheet,
            string parentSourceName,
            string parentKeyColumnName,
            string childKeyColumnName,
            bool selectChildSheet)
        {
            if (_schema == null)
                return false;

            if (ExcelImportProfileEditorHelper.TrySetRelationColumns(
                    _schema.Sheets,
                    childSheet,
                    parentSourceName,
                    parentKeyColumnName,
                    childKeyColumnName,
                    out var errorMessage) == false)
            {
                MessageBox.Show(errorMessage, "Связи", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            RefreshRelationUi(childSheet, bindEditor: true, selectChildSheet);
            return true;
        }

        private void ClearSheetRelation(ExcelImportSheetSchema childSheet)
        {
            ExcelImportProfileEditorHelper.ClearRelation(childSheet);
            RefreshRelationUi(childSheet, bindEditor: true, selectChildSheet: false);
        }

        private void RefreshRelationUi(ExcelImportSheetSchema? childSheet, bool bindEditor, bool selectChildSheet)
        {
            RefreshRelationViews();

            var selectionChanged = false;
            if (childSheet != null && selectChildSheet && ReferenceEquals(LstSchemaSheets.SelectedItem, childSheet) == false)
            {
                LstSchemaSheets.SelectedItem = childSheet;
                selectionChanged = true;
            }

            if (childSheet != null && bindEditor && selectionChanged == false)
            {
                BindRelationEditor(childSheet.SourceName);
            }

            if (selectionChanged == false)
            {
                RenderDiagram();
            }

            ClearPreviewResult();
        }

        private void BtnRefreshPreview_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TryGetImportParameters(out var isNewRoot, out var rootName, out _) == false)
                    return;

                if (TrySyncSchemaFromImportParameters(isNewRoot, rootName) == false)
                    return;

                var profiles = _importPipeline.GetProfilesForExecution(_schema!);
                var validationResult = _importPipeline.Validate(_schema!);
                if (validationResult.HasErrors)
                {
                    MessageBox.Show(ExcelImportValidationMessageBuilder.Build(validationResult), "Ошибка проверки данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (profiles.Count == 0)
                {
                    MessageBox.Show("Не выбраны источники Excel для предпросмотра импорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var previewViewModel = _importPipeline.BuildRepositoryPreview(_schema!, isNewRoot ? null : rootName);
                RepositoryExplorerPreview.DataContext = previewViewModel;
                RepositoryExplorerPreviewConfigurator.ConfigureAsReadonly(RepositoryExplorerPreview);
                TxtPreviewSummary.Text = _importPipeline.BuildPreviewSummary(previewViewModel);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось построить предпросмотр: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnImport_Click(object sender, RoutedEventArgs e)
        {
            if (_repository == null || _repositoryService == null)
            {
                MessageBox.Show("Не инициализирован контекст активного репозитория.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                if (TryGetImportParameters(out var isNewRoot, out var rootName, out var existingRoot) == false)
                    return;

                if (TrySyncSchemaFromImportParameters(isNewRoot, rootName) == false)
                    return;

                var profiles = _importPipeline.GetProfilesForExecution(_schema!);
                if (profiles.Count == 0)
                {
                    MessageBox.Show("Не выбраны источники Excel для импорта.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var validationResult = _importPipeline.Validate(_schema!);
                if (validationResult.HasErrors)
                {
                    MessageBox.Show(ExcelImportValidationMessageBuilder.Build(validationResult), "Ошибка проверки данных", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var progressWindow = _serviceProvider.GetRequiredService<ImportProgressWindow>();
                progressWindow.Initialize("Конструктор импорта Excel", "Подготовка операции...");
                progressWindow.Show();

                var schema = _schema!;
                var repository = _repository;
                var repositoryService = _repositoryService;

                CompletedImport = true;
                Close();

                IProgress<string> statusProgress = new Progress<string>(status => progressWindow.UpdateStatus(status));
                _ = System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        statusProgress.Report("Чтение Excel и формирование PHJSON...");
                        statusProgress.Report("Импорт дерева в Чубушник...");
                        _importPipeline.ImportToRepository(schema, repository, repositoryService, existingRoot);
                        Dispatcher.Invoke(() =>
                        {
                            _refreshRepositoryView?.Invoke();
                            progressWindow.Complete("Импорт завершен. Сохраните репозиторий для записи в хранилище.");
                        });
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.Invoke(() => progressWindow.Fail($"Ошибка импорта: {ex.Message}"));
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось выполнить импорт: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLoadTemplate_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Import Schema|*.phimportschema.json|JSON|*.json",
                Title = "Выберите шаблон схемы импорта"
            };

            if (dialog.ShowDialog() != true)
                return;

            var loadedSchema = _templateStorage.Load(dialog.FileName);
            var schemaFilePath = string.IsNullOrWhiteSpace(_selectedFilePath) ? loadedSchema.SourceFilePath : _selectedFilePath;
            if (string.IsNullOrWhiteSpace(schemaFilePath) || File.Exists(schemaFilePath) == false)
            {
                MessageBox.Show("Для загруженного шаблона не найден Excel-файл. Сначала выберите книгу вручную.", "Шаблон импорта", MessageBoxButton.OK, MessageBoxImage.Warning);
                _schema = loadedSchema;
                ApplyRootControlsFromSchema();
                BindSchema();
                return;
            }

            _selectedFilePath = schemaFilePath;
            TxtFilePath.Text = IOPath.GetFileName(schemaFilePath);
            _workbookPreview = _previewService.GetWorkbookPreview(schemaFilePath);
            _schema = loadedSchema;
            _schema.SourceFilePath = schemaFilePath;
            _schema.RootName = string.IsNullOrWhiteSpace(_schema.RootName) ? IOPath.GetFileNameWithoutExtension(schemaFilePath) : _schema.RootName;
            ApplyRootControlsFromSchema();
            BindSchema();
        }

        private void BtnSaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (_schema == null)
            {
                MessageBox.Show("Сначала выберите Excel-файл и настройте схему импорта.", "Шаблон импорта", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ExcelImportSchemaNormalizer.RefreshRelationProjection(_schema);
            _schema.SourceFilePath = _selectedFilePath;
            _schema.RootName = TxtRootName.Text.Trim();
            _schema.CreateNewRoot = true;

            var dialog = new SaveFileDialog
            {
                Filter = "Import Schema|*.phimportschema.json|JSON|*.json",
                FileName = $"{_schema.Name}.phimportschema.json"
            };

            if (dialog.ShowDialog() != true)
                return;

            _templateStorage.Save(dialog.FileName, _schema);
            MessageBox.Show("Шаблон схемы импорта сохранен.", "Шаблон импорта", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ClearPreviewResult()
        {
            RepositoryExplorerPreview.DataContext = null;
            TxtPreviewSummary.Text = "Предпросмотр результата еще не построен.";
        }

        private bool TryGetImportParameters(
            out bool isNewRoot,
            out string rootName,
            out Philadelphus.Core.Domain.Entities.MainEntities.PhiladelphusRepositoryMembers.ShrubMembers.WorkingTreeMembers.TreeRootModel? existingRoot)
        {
            isNewRoot = true;
            rootName = string.Empty;
            existingRoot = null;

            if (string.IsNullOrWhiteSpace(_selectedFilePath))
            {
                MessageBox.Show("Сначала выберите файл Excel.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            rootName = TxtRootName.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(rootName))
            {
                MessageBox.Show("Укажите наименование корня.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private void ChkCreateNewRoot_Checked(object sender, RoutedEventArgs e)
        {
            TxtRootName.IsEnabled = true;
            CmbExistingRoots.IsEnabled = false;
        }

        private void ChkCreateNewRoot_Unchecked(object sender, RoutedEventArgs e)
        {
            ChkCreateNewRoot.IsChecked = true;
            TxtRootName.IsEnabled = true;
            CmbExistingRoots.IsEnabled = false;
        }

        private ExcelImportSheetSchema? GetSheet(string? sourceName)
        {
            return _schema?.Sheets.FirstOrDefault(x => string.Equals(x.SourceName, sourceName, StringComparison.OrdinalIgnoreCase));
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
