using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls.Primitives;
using global::Avalonia;
using global::Avalonia.Collections;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Shapes;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Media;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.ViewModels.ImportExport;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Avalonia-аналог WPF <c>DiagramBehavior</c>: рисует диаграмму связей листов конструктора импорта Excel.
    /// Данные читает из <see cref="ExcelImportDesignerVM"/> (DataContext), связи удаляет через команды VM.
    /// </summary>
    /// <remarks>
    /// Этап 1 (текущий): отрисовка карточек листов, линий связей со стрелками, кнопка удаления связи,
    /// выбор листа кликом, зум по Ctrl+колесо. Перетаскивание листов и создание связи перетягиванием
    /// колонки — последующие этапы (см. docs/avalonia-migration/15, пункт F).
    /// Подключается attached-свойством <see cref="IsEnabledProperty"/> к <see cref="ScrollViewer"/>,
    /// чей <c>Content</c> — <see cref="LayoutTransformControl"/> с <see cref="Canvas"/> внутри
    /// и <see cref="ScaleTransform"/> в <c>LayoutTransform</c> (аналог WPF Canvas.LayoutTransform).
    /// Цвета карточек пока жёсткие (как в WPF) — тематизация под Dark в общем тех-долге темы.
    /// </remarks>
    public class DiagramBehavior
    {
        private DiagramBehavior()
        {
        }

        /// <summary>Включение диаграммы на ScrollViewer.</summary>
        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<DiagramBehavior, ScrollViewer, bool>("IsEnabled");

        public static bool GetIsEnabled(ScrollViewer o) => o.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(ScrollViewer o, bool value) => o.SetValue(IsEnabledProperty, value);

        private static readonly AttachedProperty<DiagramController?> ControllerProperty =
            AvaloniaProperty.RegisterAttached<DiagramBehavior, ScrollViewer, DiagramController?>("Controller");

        static DiagramBehavior()
        {
            IsEnabledProperty.Changed.AddClassHandler<ScrollViewer>(OnIsEnabledChanged);
        }

        private static void OnIsEnabledChanged(ScrollViewer viewer, AvaloniaPropertyChangedEventArgs e)
        {
            var existing = viewer.GetValue(ControllerProperty);
            if (existing != null)
            {
                existing.Detach();
                viewer.SetValue(ControllerProperty, null);
            }

            if (e.NewValue is true)
            {
                var controller = new DiagramController(viewer);
                viewer.SetValue(ControllerProperty, controller);
                controller.Attach();
            }
        }

        /// <summary>
        /// Состояние диаграммы для одного ScrollViewer: подписки, отрисовка, зум.
        /// </summary>
        private sealed class DiagramController
        {
            private const double CardWidth = 280;
            private const double CardMinHeight = 210;
            private const double CardMaxWidth = 520;
            private const double GridPadding = 40;
            private const double GridColumnGap = 48;
            private const double GridRowGap = 48;
            private const double GridRowHeight = 260;
            private const double ZoomMin = 0.35;
            private const double ZoomMax = 2.5;
            private const double ZoomStep = 1.1;

            private readonly ScrollViewer _viewer;
            private readonly Dictionary<string, Control> _columnElements = new(StringComparer.OrdinalIgnoreCase);

            private Canvas? _canvas;
            private ScaleTransform? _scale;
            private ExcelImportDesignerVM? _vm;
            private bool _initialLayoutPending;
            private double _zoom = 1.0;

            private readonly Dictionary<ExcelImportSheetSchema, Border> _cardElements = new();
            private DragMode _dragMode = DragMode.None;
            private ExcelImportSheetSchema? _draggedSheet;
            private ExcelImportColumnProfile? _draggedColumn;
            private Border? _draggedBorder;
            private Line? _relationPreviewLine;
            private Point _dragStartPointer;
            private double _dragStartLeft;
            private double _dragStartTop;
            private bool _isDragging;

            private enum DragMode
            {
                None,
                MoveSheet,
                CreateRelation
            }

            private sealed class ColumnTag
            {
                public ColumnTag(ExcelImportSheetSchema sheet, ExcelImportColumnProfile column)
                {
                    Sheet = sheet;
                    Column = column;
                }

                public ExcelImportSheetSchema Sheet { get; }

                public ExcelImportColumnProfile Column { get; }
            }

            public DiagramController(ScrollViewer viewer)
            {
                _viewer = viewer;
            }

            public void Attach()
            {
                _viewer.Loaded += OnLoaded;
                _viewer.DataContextChanged += OnDataContextChanged;
                _viewer.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheel, RoutingStrategies.Tunnel);

                ResolveCanvas();
                HookVm(_viewer.DataContext as ExcelImportDesignerVM);
                _initialLayoutPending = true;
                RenderDiagram();
            }

            public void Detach()
            {
                _viewer.Loaded -= OnLoaded;
                _viewer.DataContextChanged -= OnDataContextChanged;
                _viewer.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheel);
                HookVm(null);
            }

            private void OnLoaded(object? sender, RoutedEventArgs e)
            {
                ResolveCanvas();
                _initialLayoutPending = true;
                RenderDiagram();
            }

            private void ResolveCanvas()
            {
                var transform = _viewer.Content as LayoutTransformControl;
                _canvas = transform?.Child as Canvas;
                _scale = transform?.LayoutTransform as ScaleTransform;
            }

            private void OnDataContextChanged(object? sender, EventArgs e)
            {
                HookVm(_viewer.DataContext as ExcelImportDesignerVM);
                _initialLayoutPending = true;
                RenderDiagram();
            }

            private void HookVm(ExcelImportDesignerVM? newVm)
            {
                if (_vm != null)
                {
                    _vm.PropertyChanged -= OnVmPropertyChanged;
                    _vm.SchemaReloaded -= OnSchemaReloaded;
                }

                _vm = newVm;

                if (_vm != null)
                {
                    _vm.PropertyChanged += OnVmPropertyChanged;
                    _vm.SchemaReloaded += OnSchemaReloaded;
                }
            }

            private void OnSchemaReloaded()
            {
                _initialLayoutPending = true;
                RenderDiagram();
            }

            private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(ExcelImportDesignerVM.Sheets)
                    || e.PropertyName == nameof(ExcelImportDesignerVM.Relations))
                {
                    RenderDiagram();
                }
                else if (e.PropertyName == nameof(ExcelImportDesignerVM.SelectedSheet))
                {
                    // Подсветку выбора обновляем «на месте», без ререндера — иначе пересоздание
                    // карточек ломает захват указателя при перетаскивании.
                    UpdateSelectionHighlight();
                }
            }

            // ====== Отрисовка ======

            private void RenderDiagram()
            {
                if (_canvas == null)
                    return;

                _canvas.Children.Clear();
                _columnElements.Clear();
                _cardElements.Clear();

                if (_vm == null)
                    return;

                var visibleSheets = _vm.Sheets.Where(x => x.IsEnabled).ToList();
                if (_initialLayoutPending)
                {
                    ArrangeCardsByGrid(visibleSheets);
                    _initialLayoutPending = false;
                }

                foreach (var sheet in visibleSheets)
                {
                    var card = BuildCard(sheet);
                    Canvas.SetLeft(card, sheet.CanvasX);
                    Canvas.SetTop(card, sheet.CanvasY);
                    _canvas.Children.Add(card);
                }

                // Связи рисуем после прохода компоновки, когда у колонок есть реальные позиции/размеры.
                Dispatcher.UIThread.Post(BuildRelationVisuals, DispatcherPriority.Loaded);
            }

            private void ArrangeCardsByGrid(IReadOnlyList<ExcelImportSheetSchema> visibleSheets)
            {
                if (_canvas == null || visibleSheets.Count == 0)
                    return;

                var availableWidth = _viewer.Viewport.Width;
                if (double.IsNaN(availableWidth) || availableWidth <= 0)
                    availableWidth = _canvas.Width;

                var maxCardWidth = visibleSheets.Max(ResolveCardWidth);
                var cellWidth = maxCardWidth + GridColumnGap;
                var columnCount = Math.Max(1, (int)Math.Floor((availableWidth - GridPadding * 2 + GridColumnGap) / cellWidth));
                columnCount = Math.Min(columnCount, visibleSheets.Count);

                var rowCount = (int)Math.Ceiling((double)visibleSheets.Count / columnCount);
                var canvasWidth = GridPadding * 2 + columnCount * maxCardWidth + Math.Max(0, columnCount - 1) * GridColumnGap;
                var canvasHeight = GridPadding * 2 + rowCount * GridRowHeight + Math.Max(0, rowCount - 1) * GridRowGap;
                _canvas.Width = Math.Max(1400, canvasWidth);
                _canvas.Height = Math.Max(900, canvasHeight);

                for (var index = 0; index < visibleSheets.Count; index++)
                {
                    var sheet = visibleSheets[index];
                    var row = index / columnCount;
                    var column = index % columnCount;
                    var cardWidth = ResolveCardWidth(sheet);

                    sheet.CanvasX = GridPadding
                        + column * (maxCardWidth + GridColumnGap)
                        + (maxCardWidth - cardWidth) / 2;
                    sheet.CanvasY = GridPadding + row * (GridRowHeight + GridRowGap);
                }
            }

            private Border BuildCard(ExcelImportSheetSchema sheet)
            {
                var border = new Border
                {
                    Width = ResolveCardWidth(sheet),
                    MinHeight = CardMinHeight,
                    BorderBrush = ReferenceEquals(sheet, _vm?.SelectedSheet) ? Brushes.SteelBlue : Brushes.Silver,
                    BorderThickness = new Thickness(1),
                    Background = Brushes.White,
                    Padding = new Thickness(8),
                    CornerRadius = new CornerRadius(6),
                    Tag = sheet
                };

                var layout = new Grid { RowDefinitions = new RowDefinitions("Auto,Auto,Auto,*") };

                var title = new TextBlock
                {
                    Text = string.IsNullOrWhiteSpace(sheet.DisplayName) ? sheet.SourceName : sheet.DisplayName,
                    FontWeight = FontWeight.SemiBold,
                    TextWrapping = TextWrapping.Wrap
                };
                Grid.SetRow(title, 0);
                layout.Children.Add(title);

                var relationText = new TextBlock
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

                var relationPanel = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto") };
                Grid.SetColumn(relationText, 0);
                relationPanel.Children.Add(relationText);

                if (string.IsNullOrWhiteSpace(sheet.Profile.Relation.ParentSourceName) == false)
                {
                    var removeRelationButton = new Button
                    {
                        Content = "x",
                        Width = 28,
                        Height = 24,
                        Padding = new Thickness(0),
                        Margin = new Thickness(8, 2, 0, 0),
                        VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Top
                    };
                    ToolTip.SetTip(removeRelationButton, "Удалить связь с родителем");
                    removeRelationButton.Click += (_, _) => _vm?.ClearSheetRelation(sheet);
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
                    columnsPanel.Children.Add(BuildColumnItem(sheet, column));
                }

                var columnsScroll = new ScrollViewer
                {
                    Content = columnsPanel,
                    MaxHeight = ResolveColumnsMaxHeight(sheet),
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
                };
                Grid.SetRow(columnsScroll, 3);
                layout.Children.Add(columnsScroll);
                border.Child = layout;

                _cardElements[sheet] = border;

                // Выбор листа + перетаскивание карточки по Canvas (этап 2).
                border.PointerPressed += OnCardPointerPressed;
                border.PointerMoved += OnCardPointerMoved;
                border.PointerReleased += OnCardPointerReleased;
                border.PointerCaptureLost += OnCardPointerCaptureLost;

                return border;
            }

            private void UpdateSelectionHighlight()
            {
                foreach (var pair in _cardElements)
                {
                    pair.Value.BorderBrush = ReferenceEquals(pair.Key, _vm?.SelectedSheet)
                        ? Brushes.SteelBlue
                        : Brushes.Silver;
                }
            }

            // ====== Перетаскивание карточек ======

            private void OnCardPointerPressed(object? sender, PointerPressedEventArgs e)
            {
                if (_canvas == null || sender is not Border border || border.Tag is not ExcelImportSheetSchema sheet)
                    return;

                // Нажатие на кнопке (удаление связи) не начинает перетаскивание.
                if ((e.Source as Visual)?.FindAncestorOfType<Button>(includeSelf: true) != null)
                    return;

                var point = e.GetCurrentPoint(_canvas);
                if (point.Properties.IsLeftButtonPressed == false)
                    return;

                if (_vm != null)
                    _vm.SelectedSheet = sheet;

                _dragMode = DragMode.MoveSheet;
                _draggedSheet = sheet;
                _draggedColumn = null;
                _draggedBorder = border;
                _dragStartPointer = point.Position;
                _dragStartLeft = GetCanvasLeft(border);
                _dragStartTop = GetCanvasTop(border);
                _isDragging = true;
                e.Pointer.Capture(border);
                e.Handled = true;
            }

            // Нажатие на колонке-источнике начинает создание/переназначение связи (этап 3).
            private void OnColumnPointerPressed(object? sender, PointerPressedEventArgs e)
            {
                if (_canvas == null || sender is not Border columnBorder || columnBorder.Tag is not ColumnTag tag)
                    return;

                var point = e.GetCurrentPoint(_canvas);
                if (point.Properties.IsLeftButtonPressed == false)
                    return;

                if (_cardElements.TryGetValue(tag.Sheet, out var card) == false)
                    return;

                if (_vm != null)
                    _vm.SelectedSheet = tag.Sheet;

                _dragMode = DragMode.CreateRelation;
                _draggedSheet = tag.Sheet;
                _draggedColumn = tag.Column;
                _draggedBorder = card;
                _isDragging = true;

                var rect = GetCanvasRect(columnBorder);
                var start = rect != null
                    ? new Point(rect.Value.X + rect.Value.Width / 2, rect.Value.Y + rect.Value.Height / 2)
                    : point.Position;
                CreateRelationPreviewLine(start);

                e.Pointer.Capture(card);
                e.Handled = true;
            }

            private void OnCardPointerMoved(object? sender, PointerEventArgs e)
            {
                if (_dragMode == DragMode.None || _canvas == null)
                    return;

                var point = e.GetCurrentPoint(_canvas);
                if (point.Properties.IsLeftButtonPressed == false)
                {
                    FinishDrag(point.Position);
                    return;
                }

                var current = point.Position;
                if (_dragMode == DragMode.MoveSheet && _draggedBorder != null)
                {
                    Canvas.SetLeft(_draggedBorder, _dragStartLeft + (current.X - _dragStartPointer.X));
                    Canvas.SetTop(_draggedBorder, _dragStartTop + (current.Y - _dragStartPointer.Y));
                    BuildRelationVisualsRefresh();
                }
                else if (_dragMode == DragMode.CreateRelation && _relationPreviewLine != null)
                {
                    _relationPreviewLine.EndPoint = current;
                }

                e.Handled = true;
            }

            private void OnCardPointerReleased(object? sender, PointerReleasedEventArgs e)
            {
                if (_dragMode == DragMode.None)
                    return;

                var drop = _canvas != null ? e.GetPosition(_canvas) : (Point?)null;
                FinishDrag(drop);
                e.Pointer.Capture(null);
                e.Handled = true;
            }

            private void OnCardPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
            {
                if (_dragMode == DragMode.None)
                    return;

                FinishDrag(null);
            }

            private void FinishDrag(Point? dropPoint)
            {
                var mode = _dragMode;
                var sourceSheet = _draggedSheet;
                var sourceColumn = _draggedColumn;
                var border = _draggedBorder;

                if (mode == DragMode.MoveSheet && sourceSheet != null && border != null)
                {
                    sourceSheet.CanvasX = GetCanvasLeft(border);
                    sourceSheet.CanvasY = GetCanvasTop(border);
                }

                RemoveRelationPreviewLine();
                _dragMode = DragMode.None;
                _draggedSheet = null;
                _draggedColumn = null;
                _draggedBorder = null;
                _isDragging = false;

                if (mode == DragMode.CreateRelation && sourceSheet != null && sourceColumn != null && dropPoint != null)
                {
                    var target = FindColumnDropTarget(dropPoint.Value, sourceColumn);
                    if (target != null && _vm != null)
                    {
                        // Тянем от родительской колонки (источник) к дочерней (цель).
                        _vm.ApplyRelationFromColumns(
                            target.Sheet,
                            sourceSheet.SourceName,
                            sourceColumn.HeaderName,
                            target.Column.HeaderName,
                            selectChildSheet: true);
                        return; // VM перерисует диаграмму (изменились Relations).
                    }
                }

                BuildRelationVisualsRefresh();
            }

            private ColumnTag? FindColumnDropTarget(Point dropPoint, ExcelImportColumnProfile draggedColumn)
            {
                foreach (var element in _columnElements.Values)
                {
                    if (element.Tag is not ColumnTag tag || ReferenceEquals(tag.Column, draggedColumn))
                        continue;

                    var rect = GetCanvasRect(element);
                    if (rect != null && rect.Value.Contains(dropPoint))
                        return tag;
                }

                return null;
            }

            private void CreateRelationPreviewLine(Point start)
            {
                if (_canvas == null)
                    return;

                RemoveRelationPreviewLine();
                _relationPreviewLine = new Line
                {
                    StartPoint = start,
                    EndPoint = start,
                    Stroke = Brushes.SteelBlue,
                    StrokeThickness = 2,
                    StrokeDashArray = new AvaloniaList<double> { 4, 3 },
                    IsHitTestVisible = false
                };
                _canvas.Children.Add(_relationPreviewLine);
            }

            private void RemoveRelationPreviewLine()
            {
                if (_relationPreviewLine != null && _canvas != null)
                    _canvas.Children.Remove(_relationPreviewLine);

                _relationPreviewLine = null;
            }

            private static double GetCanvasLeft(Control element)
            {
                var value = Canvas.GetLeft(element);
                return double.IsNaN(value) ? 0 : value;
            }

            private static double GetCanvasTop(Control element)
            {
                var value = Canvas.GetTop(element);
                return double.IsNaN(value) ? 0 : value;
            }

            private Border BuildColumnItem(ExcelImportSheetSchema sheet, ExcelImportColumnProfile column)
            {
                var isIgnored = column.Role == ExcelImportColumnRole.Ignore;
                var isRelationColumn = IsRelationColumn(sheet, column);
                var columnBorder = new Border
                {
                    Tag = new ColumnTag(sheet, column),
                    Background = isIgnored
                        ? Brushes.Gainsboro
                        : isRelationColumn ? Brushes.Honeydew : Brushes.AliceBlue,
                    BorderBrush = isIgnored
                        ? Brushes.LightGray
                        : isRelationColumn ? Brushes.SeaGreen : Brushes.LightSteelBlue,
                    BorderThickness = isRelationColumn ? new Thickness(2) : new Thickness(1),
                    CornerRadius = new CornerRadius(4),
                    Padding = new Thickness(6, 3, 6, 3),
                    Margin = new Thickness(0, 0, 0, 4)
                };

                var text = string.IsNullOrWhiteSpace(column.DataTypeNodeName)
                    ? column.HeaderName
                    : $"{column.HeaderName} · {column.DataTypeNodeName}";
                if (isIgnored)
                    text += " · игнор";

                columnBorder.Child = new TextBlock
                {
                    Text = text,
                    FontSize = 11,
                    Foreground = isIgnored ? Brushes.DimGray : Brushes.Black,
                    TextTrimming = TextTrimming.CharacterEllipsis
                };

                columnBorder.PointerPressed += OnColumnPointerPressed;
                _columnElements[GetColumnKey(sheet.SourceName, column.HeaderName)] = columnBorder;
                return columnBorder;
            }

            private static double ResolveCardWidth(ExcelImportSheetSchema sheet)
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

                var estimatedWidth = 80 + maxTextLength * 6.5;
                return Math.Clamp(estimatedWidth, CardWidth, CardMaxWidth);
            }

            private static double ResolveColumnsMaxHeight(ExcelImportSheetSchema sheet)
            {
                var visibleRowsHeight = Math.Max(1, sheet.Profile.Columns.Count) * 34;
                return Math.Clamp(visibleRowsHeight, 96, 260);
            }

            private bool IsRelationColumn(ExcelImportSheetSchema sheet, ExcelImportColumnProfile column)
            {
                if (_vm == null)
                    return false;

                return _vm.Relations
                    .Where(x => x.IsEnabled)
                    .Any(relation =>
                        (string.Equals(relation.ParentSourceName, sheet.SourceName, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(relation.ParentKeyColumnName, column.HeaderName, StringComparison.OrdinalIgnoreCase))
                        || (string.Equals(relation.ChildSourceName, sheet.SourceName, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(relation.ChildKeyColumnName, column.HeaderName, StringComparison.OrdinalIgnoreCase)));
            }

            private static string GetColumnKey(string sourceName, string columnName) => $"{sourceName}{columnName}";

            // ====== Связи ======

            private void BuildRelationVisuals()
            {
                if (_canvas == null || _vm == null)
                    return;

                foreach (var relation in _vm.Relations.Where(x => x.IsEnabled))
                {
                    if (TryGetRelationColumns(relation, out var parentColumn, out var childColumn) == false)
                        continue;

                    var start = GetColumnConnectionPoint(parentColumn, childColumn);
                    var end = GetColumnConnectionPoint(childColumn, parentColumn);
                    if (start == null || end == null)
                        continue;

                    _canvas.Children.Add(new Line
                    {
                        StartPoint = start.Value,
                        EndPoint = end.Value,
                        Stroke = Brushes.SeaGreen,
                        StrokeThickness = 2.5,
                        IsHitTestVisible = false
                    });
                    _canvas.Children.Add(CreateArrowHead(start.Value, end.Value, Brushes.SeaGreen));
                }
            }

            private bool TryGetRelationColumns(ExcelImportRelationSchema relation, out Control parent, out Control child)
            {
                var hasParent = _columnElements.TryGetValue(GetColumnKey(relation.ParentSourceName, relation.ParentKeyColumnName), out var p);
                var hasChild = _columnElements.TryGetValue(GetColumnKey(relation.ChildSourceName, relation.ChildKeyColumnName), out var c);
                parent = p!;
                child = c!;
                return hasParent && hasChild;
            }

            private Point? GetColumnConnectionPoint(Control sourceColumn, Control targetColumn)
            {
                var sourceRect = GetCanvasRect(sourceColumn);
                var targetRect = GetCanvasRect(targetColumn);
                if (sourceRect == null || targetRect == null)
                    return null;

                var sourceCenter = new Point(sourceRect.Value.X + sourceRect.Value.Width / 2, sourceRect.Value.Y + sourceRect.Value.Height / 2);
                var targetCenter = new Point(targetRect.Value.X + targetRect.Value.Width / 2, targetRect.Value.Y + targetRect.Value.Height / 2);

                var x = targetCenter.X >= sourceCenter.X ? sourceRect.Value.Right : sourceRect.Value.X;
                return new Point(x, sourceCenter.Y);
            }

            private Rect? GetCanvasRect(Control element)
            {
                if (_canvas == null)
                    return null;

                var topLeft = element.TranslatePoint(new Point(0, 0), _canvas);
                if (topLeft == null)
                    return null;

                var size = element.Bounds.Size;
                if (size.Width <= 0 || size.Height <= 0)
                    return null;

                return new Rect(topLeft.Value, size);
            }

            private static Polygon CreateArrowHead(Point start, Point end, IBrush brush)
            {
                var angle = Math.Atan2(end.Y - start.Y, end.X - start.X);
                const double length = 11;
                const double width = 6;
                var back = new Point(end.X - length * Math.Cos(angle), end.Y - length * Math.Sin(angle));
                var normalX = Math.Cos(angle + Math.PI / 2);
                var normalY = Math.Sin(angle + Math.PI / 2);

                var points = new Points
                {
                    end,
                    new Point(back.X + width * normalX, back.Y + width * normalY),
                    new Point(back.X - width * normalX, back.Y - width * normalY)
                };

                return new Polygon
                {
                    Fill = brush,
                    Stroke = brush,
                    Points = points,
                    IsHitTestVisible = false
                };
            }

            // ====== Зум (Ctrl + колесо) ======

            private void OnPointerWheel(object? sender, PointerWheelEventArgs e)
            {
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control) == false)
                    return;

                if (_canvas == null || _scale == null)
                    return;

                var oldZoom = _zoom;
                var factor = e.Delta.Y > 0 ? ZoomStep : 1 / ZoomStep;
                var newZoom = Math.Clamp(oldZoom * factor, ZoomMin, ZoomMax);
                if (Math.Abs(newZoom - oldZoom) < 0.001)
                {
                    e.Handled = true;
                    return;
                }

                var mouseOnCanvas = e.GetPosition(_canvas);
                var mouseOnViewer = e.GetPosition(_viewer);

                _zoom = newZoom;
                _scale.ScaleX = newZoom;
                _scale.ScaleY = newZoom;
                e.Handled = true;

                // Якорим точку под курсором: смещение пересчитываем после прохода компоновки.
                Dispatcher.UIThread.Post(() =>
                {
                    var x = Math.Max(0, mouseOnCanvas.X * newZoom - mouseOnViewer.X);
                    var y = Math.Max(0, mouseOnCanvas.Y * newZoom - mouseOnViewer.Y);
                    _viewer.Offset = new Vector(x, y);
                    BuildRelationVisualsRefresh();
                }, DispatcherPriority.Background);
            }

            private void BuildRelationVisualsRefresh()
            {
                if (_canvas == null)
                    return;

                // Перерисовываем линии связей под новые позиции колонок.
                var lines = _canvas.Children.OfType<Shape>().ToList();
                foreach (var shape in lines)
                    _canvas.Children.Remove(shape);

                BuildRelationVisuals();
            }
        }
    }
}
