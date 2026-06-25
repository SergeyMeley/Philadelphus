using Microsoft.Xaml.Behaviors;
using Philadelphus.Infrastructure.ImportExport.Excel;
using Philadelphus.Presentation.ViewModels.ImportExport;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    /// <summary>
    /// Поведение, рисующее диаграмму связей листов конструктора импорта Excel.
    /// Прикрепляется к ScrollViewer (его Content — Canvas). Данные читает из ExcelImportDesignerVM
    /// (DataContext), связи создаёт/удаляет через команды VM. Вынесено из code-behind на Фазе 3
    /// (см. docs/avalonia-migration/10).
    /// </summary>
    public class DiagramBehavior : Behavior<ScrollViewer>
    {
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

        private readonly Dictionary<string, FrameworkElement> _diagramColumnElements = new(StringComparer.OrdinalIgnoreCase);
        private readonly List<DiagramRelationVisual> _diagramRelationVisuals = new();
        private ExcelImportSheetSchema? _draggedSheet;
        private ExcelImportColumnProfile? _draggedColumn;
        private Border? _draggedBorder;
        private DiagramDragMode _diagramDragMode = DiagramDragMode.None;
        private Line? _relationPreviewLine;
        private bool _diagramInitialLayoutPending;
        private double _diagramZoom = 1.0;
        private Point _dragStartPoint;
        private double _dragStartLeft;
        private double _dragStartTop;

        private Canvas? _canvas;
        private ScaleTransform? _scaleTransform;
        private ExcelImportDesignerVM? _vm;

        // Мосты к именованным элементам исходного code-behind: тело методов перенесено без изменений.
        private Canvas DiagramCanvas => _canvas!;
        private ScrollViewer DiagramScrollViewer => AssociatedObject;
        private ScaleTransform DiagramCanvasScaleTransform => _scaleTransform!;
        private ExcelImportDesignerVM? Vm => _vm;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewMouseWheel += OnPreviewMouseWheel;
            AssociatedObject.DataContextChanged += OnDataContextChanged;
            AssociatedObject.Loaded += OnLoaded;

            ResolveCanvas();
            HookVm(AssociatedObject.DataContext as ExcelImportDesignerVM);
            RenderDiagram();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.PreviewMouseWheel -= OnPreviewMouseWheel;
            AssociatedObject.DataContextChanged -= OnDataContextChanged;
            AssociatedObject.Loaded -= OnLoaded;
            HookVm(null);
            base.OnDetaching();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            ResolveCanvas();
            _diagramInitialLayoutPending = true;
            RenderDiagram();
        }

        private void ResolveCanvas()
        {
            _canvas = AssociatedObject.Content as Canvas;
            _scaleTransform = _canvas?.LayoutTransform as ScaleTransform;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            HookVm(e.NewValue as ExcelImportDesignerVM);
            _diagramInitialLayoutPending = true;
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
            _diagramInitialLayoutPending = true;
            RenderDiagram();
        }

        private void OnVmPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ExcelImportDesignerVM.Sheets)
                || e.PropertyName == nameof(ExcelImportDesignerVM.Relations)
                || e.PropertyName == nameof(ExcelImportDesignerVM.SelectedSheet))
            {
                RenderDiagram();
            }
        }

        // ====== Диаграмма связей (перенесено из ExcelImportDesignerWindow.xaml.cs) ======

        private void RenderDiagram()
        {
            if (_canvas == null)
                return;

            ResetDiagramDragState();
            DiagramCanvas.Children.Clear();
            _diagramColumnElements.Clear();
            _diagramRelationVisuals.Clear();

            if (Vm == null)
                return;

            var visibleSheets = Vm.Sheets.Where(x => x.IsEnabled).ToList();
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
            foreach (var relation in Vm.Relations.Where(x => x.IsEnabled))
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
                BorderBrush = ReferenceEquals(sheet, Vm?.SelectedSheet) ? Brushes.SteelBlue : Brushes.Silver,
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
                removeRelationButton.Click += (_, _) => Vm?.ClearSheetRelation(sheet);
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

            var estimatedWidth = 80 + maxTextLength * 6.5;
            return Math.Clamp(estimatedWidth, DiagramCardWidth, DiagramCardMaxWidth);
        }

        private static double ResolveDiagramColumnsMaxHeight(ExcelImportSheetSchema sheet)
        {
            var visibleRowsHeight = Math.Max(1, sheet.Profile.Columns.Count) * 34;
            return Math.Clamp(visibleRowsHeight, 96, 260);
        }

        private void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                return;

            if (_canvas == null || _scaleTransform == null)
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
                Vm?.ApplyRelationFromColumns(
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
            if (Vm == null)
                return false;

            return Vm.Relations
                .Where(x => x.IsEnabled)
                .Any(relation =>
                    (string.Equals(relation.ParentSourceName, sheet.SourceName, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(relation.ParentKeyColumnName, column.HeaderName, StringComparison.OrdinalIgnoreCase))
                    || (string.Equals(relation.ChildSourceName, sheet.SourceName, StringComparison.OrdinalIgnoreCase)
                        && string.Equals(relation.ChildKeyColumnName, column.HeaderName, StringComparison.OrdinalIgnoreCase)));
        }

        private static string GetDiagramColumnKey(string sourceName, string columnName)
        {
            return $"{sourceName}{columnName}";
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
    }
}
