using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Media;
using global::Avalonia.Threading;
using global::Avalonia.VisualTree;

namespace Philadelphus.Presentation.Avalonia.Behaviors
{
    /// <summary>
    /// Автопрокрутка DataGrid к последнему (новейшему) элементу при добавлении строк, с краткой
    /// подсветкой реально новых строк. Avalonia-аналог WPF DataGridAutoScrollBehavior.
    /// </summary>
    /// <remarks>
    /// Изменения дебаунсятся; скролл выполняется дважды (сразу и после реализации строк), т.к.
    /// Avalonia ScrollIntoView к последнему ряду нередко «не доезжает» до измерения. Подсветка —
    /// best-effort: контейнеры строк виртуализированы, фон строки ставится/снимается по таймеру.
    /// </remarks>
    public class DataGridAutoScrollBehavior
    {
        private DataGridAutoScrollBehavior()
        {
        }

        /// <summary>Включение автопрокрутки на DataGrid.</summary>
        public static readonly AttachedProperty<bool> IsEnabledProperty =
            AvaloniaProperty.RegisterAttached<DataGridAutoScrollBehavior, DataGrid, bool>("IsEnabled");

        public static bool GetIsEnabled(DataGrid o) => o.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DataGrid o, bool value) => o.SetValue(IsEnabledProperty, value);

        /// <summary>Кисть кратковременной подсветки новых строк.</summary>
        public static readonly AttachedProperty<IBrush?> HighlightBrushProperty =
            AvaloniaProperty.RegisterAttached<DataGridAutoScrollBehavior, DataGrid, IBrush?>(
                "HighlightBrush", new SolidColorBrush(Color.Parse("#ABD0DB")));

        public static IBrush? GetHighlightBrush(DataGrid o) => o.GetValue(HighlightBrushProperty);
        public static void SetHighlightBrush(DataGrid o, IBrush? value) => o.SetValue(HighlightBrushProperty, value);

        private static readonly ConditionalWeakTable<DataGrid, State> States = new();

        private sealed class State
        {
            public INotifyCollectionChanged? Collection;
            public NotifyCollectionChangedEventHandler? CollectionHandler;
            public DispatcherTimer Timer = null!;
            public readonly HashSet<object> NewItems = new();
        }

        static DataGridAutoScrollBehavior()
        {
            IsEnabledProperty.Changed.AddClassHandler<DataGrid>(OnIsEnabledChanged);
        }

        private static void OnIsEnabledChanged(DataGrid grid, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                Attach(grid);
            }
            else
            {
                Detach(grid);
            }
        }

        private static void Attach(DataGrid grid)
        {
            if (States.TryGetValue(grid, out _))
            {
                return;
            }

            var state = new State
            {
                Timer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(120) },
            };
            state.Timer.Tick += (_, _) =>
            {
                state.Timer.Stop();
                Flush(grid, state);
            };
            States.Add(grid, state);

            grid.PropertyChanged += OnGridPropertyChanged;
            grid.DetachedFromVisualTree += OnGridDetached;
            Subscribe(grid, state);
        }

        private static void Detach(DataGrid grid)
        {
            grid.PropertyChanged -= OnGridPropertyChanged;
            grid.DetachedFromVisualTree -= OnGridDetached;

            if (States.TryGetValue(grid, out var state))
            {
                Unsubscribe(state);
                state.Timer.Stop();
                state.NewItems.Clear();
                States.Remove(grid);
            }
        }

        private static void OnGridDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            if (sender is DataGrid grid)
            {
                Detach(grid);
            }
        }

        private static void OnGridPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (sender is DataGrid grid && e.Property == DataGrid.ItemsSourceProperty && States.TryGetValue(grid, out var state))
            {
                Subscribe(grid, state);
                RestartDebounce(state);
            }
        }

        private static void Subscribe(DataGrid grid, State state)
        {
            Unsubscribe(state);

            if (grid.ItemsSource is INotifyCollectionChanged collection)
            {
                state.Collection = collection;
                state.CollectionHandler = (_, e) => OnCollectionChanged(state, e);
                collection.CollectionChanged += state.CollectionHandler;
            }
        }

        private static void Unsubscribe(State state)
        {
            if (state.Collection != null && state.CollectionHandler != null)
            {
                state.Collection.CollectionChanged -= state.CollectionHandler;
            }

            state.Collection = null;
            state.CollectionHandler = null;
        }

        private static void OnCollectionChanged(State state, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                state.NewItems.Clear();
            }
            else if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item != null)
                    {
                        state.NewItems.Add(item);
                    }
                }
            }

            RestartDebounce(state);
        }

        private static void RestartDebounce(State state)
        {
            state.Timer.Stop();
            state.Timer.Start();
        }

        private static void Flush(DataGrid grid, State state)
        {
            var newItems = state.NewItems.ToList();
            state.NewItems.Clear();

            ScrollToEnd(grid);

            // Второй проход после реализации строк: ScrollIntoView к последнему ряду «не доезжает»,
            // пока ряд не измерен. Заодно подсвечиваем новые строки.
            Dispatcher.UIThread.Post(
                () =>
                {
                    ScrollToEnd(grid);
                    HighlightRows(grid, newItems);
                },
                DispatcherPriority.Loaded);
        }

        private static void ScrollToEnd(DataGrid grid)
        {
            if (grid.ItemsSource is not IEnumerable items)
            {
                return;
            }

            object? last = null;
            foreach (var item in items)
            {
                last = item;
            }

            if (last != null)
            {
                grid.ScrollIntoView(last, null);
            }
        }

        private static void HighlightRows(DataGrid grid, IReadOnlyCollection<object> items)
        {
            var brush = GetHighlightBrush(grid);
            if (brush == null || items.Count == 0)
            {
                return;
            }

            var rows = grid.GetVisualDescendants().OfType<DataGridRow>()
                .Where(row => row.DataContext != null && items.Contains(row.DataContext))
                .ToList();

            if (rows.Count == 0)
            {
                return;
            }

            foreach (var row in rows)
            {
                row.Background = brush;
            }

            DispatcherTimer? clearTimer = null;
            clearTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(900) };
            clearTimer.Tick += (_, _) =>
            {
                clearTimer!.Stop();
                foreach (var row in rows)
                {
                    row.ClearValue(global::Avalonia.Controls.Primitives.TemplatedControl.BackgroundProperty);
                }
            };
            clearTimer.Start();
        }
    }
}
