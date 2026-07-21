using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

using Philadelphus.Presentation.ViewModels.ControlsVMs;
using Philadelphus.Presentation.ViewModels.EntitiesVMs.MainEntitiesVMs.ElementsContentVMs;

namespace Philadelphus.Presentation.Avalonia.Behaviors;

/// <summary>
/// Управляет popup расширенного поиска над ячейкой значения выбранного атрибута.
/// </summary>
public sealed class AttributeValueLookupPopupBehavior
{
    private const string ValueCellClass = "attributeValue";

    private AttributeValueLookupPopupBehavior()
    {
    }

    /// <summary>
    /// Popup, которым управляет таблица атрибутов.
    /// </summary>
    public static readonly AttachedProperty<Popup?> PopupProperty =
        AvaloniaProperty.RegisterAttached<AttributeValueLookupPopupBehavior, DataGrid, Popup?>("Popup");

    private static readonly AttachedProperty<Controller?> ControllerProperty =
        AvaloniaProperty.RegisterAttached<AttributeValueLookupPopupBehavior, DataGrid, Controller?>("Controller");

    public static Popup? GetPopup(DataGrid dataGrid) =>
        dataGrid.GetValue(PopupProperty);

    public static void SetPopup(DataGrid dataGrid, Popup? value) =>
        dataGrid.SetValue(PopupProperty, value);

    static AttributeValueLookupPopupBehavior()
    {
        PopupProperty.Changed.AddClassHandler<DataGrid>(OnPopupChanged);
    }

    private static void OnPopupChanged(
        DataGrid dataGrid,
        AvaloniaPropertyChangedEventArgs eventArgs)
    {
        dataGrid.GetValue(ControllerProperty)?.Dispose();
        dataGrid.SetValue(
            ControllerProperty,
            eventArgs.NewValue is Popup popup
                ? new Controller(dataGrid, popup)
                : null);
    }

    private sealed class Controller : IDisposable
    {
        private static readonly TimeSpan CloseDelay = TimeSpan.FromMilliseconds(350);

        private readonly DataGrid _dataGrid;
        private readonly Popup _popup;
        private readonly Control _popupContent;
        private readonly DispatcherTimer _closeTimer;
        private readonly HashSet<ComboBox> _comboBoxes = [];
        private readonly HashSet<ComboBox> _openDropDowns = [];
        private bool _isEditing;
        private bool _suppressGridPointerExited;

        public Controller(DataGrid dataGrid, Popup popup)
        {
            _dataGrid = dataGrid;
            _popup = popup;
            _popupContent = popup.Child
                ?? throw new ArgumentException("Popup не содержит визуального содержимого.", nameof(popup));
            _closeTimer = new DispatcherTimer { Interval = CloseDelay };
            _closeTimer.Tick += OnCloseTimerTick;

            _dataGrid.PointerMoved += OnDataGridPointerMoved;
            _dataGrid.PointerExited += OnDataGridPointerExited;
            _dataGrid.SelectionChanged += OnSelectionChanged;
            _dataGrid.PreparingCellForEdit += OnPreparingCellForEdit;
            _dataGrid.CellEditEnding += OnCellEditEnding;
            _dataGrid.AddHandler(
                InputElement.KeyDownEvent,
                OnKeyDown,
                RoutingStrategies.Tunnel,
                handledEventsToo: true);
            _popupContent.PointerEntered += OnPopupPointerEntered;
            _popupContent.PointerExited += OnPointerExited;
            _popup.PropertyChanged += OnPopupPropertyChanged;
            _popup.AddHandler(
                InputElement.KeyDownEvent,
                OnKeyDown,
                RoutingStrategies.Tunnel,
                handledEventsToo: true);
        }

        public void Dispose()
        {
            _closeTimer.Stop();
            _closeTimer.Tick -= OnCloseTimerTick;
            _dataGrid.PointerMoved -= OnDataGridPointerMoved;
            _dataGrid.PointerExited -= OnDataGridPointerExited;
            _dataGrid.SelectionChanged -= OnSelectionChanged;
            _dataGrid.PreparingCellForEdit -= OnPreparingCellForEdit;
            _dataGrid.CellEditEnding -= OnCellEditEnding;
            _dataGrid.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
            _popupContent.PointerEntered -= OnPopupPointerEntered;
            _popupContent.PointerExited -= OnPointerExited;
            _popup.PropertyChanged -= OnPopupPropertyChanged;
            _popup.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
            foreach (var comboBox in _comboBoxes)
            {
                comboBox.DropDownOpened -= OnDropDownOpened;
                comboBox.DropDownClosed -= OnDropDownClosed;
            }
            _comboBoxes.Clear();
            _openDropDowns.Clear();
            Close();
        }

        private void OnDataGridPointerMoved(object? sender, PointerEventArgs eventArgs)
        {
            if (TryGetSelectedValueCell(eventArgs.Source, out var cell))
            {
                _closeTimer.Stop();
                _popup.PlacementTarget = cell;
                if (_popup.IsOpen == false)
                {
                    _suppressGridPointerExited = true;
                    _popup.IsOpen = true;
                    Dispatcher.UIThread.Post(
                        CompletePopupOpening,
                        DispatcherPriority.Background);
                }
            }
            else
            {
                ScheduleClose();
            }
        }

        private bool TryGetSelectedValueCell(object? source, out DataGridCell? cell)
        {
            cell = (source as Visual)?.FindAncestorOfType<DataGridCell>(includeSelf: true);
            var row = cell?.FindAncestorOfType<DataGridRow>();
            return _isEditing == false
                && cell?.Classes.Contains(ValueCellClass) == true
                && row?.DataContext is ElementAttributeVM attribute
                && ReferenceEquals(_dataGrid.SelectedItem, attribute)
                && _popup.DataContext is AttributeValueLookupHostVM lookup
                && ReferenceEquals(lookup.Attribute, attribute);
        }

        private void OnPopupPointerEntered(object? sender, PointerEventArgs eventArgs) =>
            _closeTimer.Stop();

        private void CompletePopupOpening()
        {
            _suppressGridPointerExited = false;
            foreach (var comboBox in _popupContent
                .GetVisualDescendants()
                .OfType<ComboBox>())
            {
                if (_comboBoxes.Add(comboBox))
                {
                    comboBox.DropDownOpened += OnDropDownOpened;
                    comboBox.DropDownClosed += OnDropDownClosed;
                }
            }
        }

        private void OnDropDownOpened(object? sender, EventArgs eventArgs)
        {
            if (sender is ComboBox comboBox)
                _openDropDowns.Add(comboBox);
            _closeTimer.Stop();
        }

        private void OnDropDownClosed(object? sender, EventArgs eventArgs)
        {
            if (sender is ComboBox comboBox)
                _openDropDowns.Remove(comboBox);
        }

        private void OnDataGridPointerExited(object? sender, PointerEventArgs eventArgs)
        {
            if (_suppressGridPointerExited == false)
                ScheduleClose();
        }

        private void OnPointerExited(object? sender, PointerEventArgs eventArgs) =>
            ScheduleClose();

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs eventArgs) =>
            Close();

        private void OnPreparingCellForEdit(
            object? sender,
            DataGridPreparingCellForEditEventArgs eventArgs)
        {
            _isEditing = true;
            Close();
        }

        private void OnCellEditEnding(
            object? sender,
            DataGridCellEditEndingEventArgs eventArgs) =>
            _isEditing = false;

        private void OnPopupPropertyChanged(
            object? sender,
            AvaloniaPropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.Property == StyledElement.DataContextProperty)
                Close();
        }

        private void OnKeyDown(object? sender, KeyEventArgs eventArgs)
        {
            if (_popup.IsOpen && eventArgs.Key == Key.Escape)
            {
                Close();
                eventArgs.Handled = true;
            }
        }

        private void ScheduleClose()
        {
            if (_popup.IsOpen == false || _closeTimer.IsEnabled)
                return;

            _closeTimer.Start();
        }

        private void OnCloseTimerTick(object? sender, EventArgs eventArgs)
        {
            _closeTimer.Stop();
            if (_popupContent.IsPointerOver == false
                && _openDropDowns.Count == 0)
                Close();
        }

        private void Close()
        {
            _closeTimer.Stop();
            foreach (var comboBox in _openDropDowns.ToArray())
                comboBox.IsDropDownOpen = false;
            _openDropDowns.Clear();
            _popup.IsOpen = false;
            _popup.PlacementTarget = null;
        }
    }
}
