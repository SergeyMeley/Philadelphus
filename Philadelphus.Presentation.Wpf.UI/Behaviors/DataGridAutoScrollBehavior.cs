using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Philadelphus.Presentation.Wpf.UI.Behaviors
{
    /// <summary>
    /// Представляет объект DataGridAutoScrollBehavior.
    /// </summary>
    public class DataGridAutoScrollBehavior : Behavior<DataGrid>
    {
        private const int MaxHighlightedRowsPerBatch = 20;

        private sealed class RowHighlightState
        {
            /// <summary>
            /// Исходное локальное значение фона.
            /// </summary>
            public object OriginalBackgroundLocalValue { get; set; } = DependencyProperty.UnsetValue;
            
            /// <summary>
            /// Кисть подсветки.
            /// </summary>
            public SolidColorBrush HighlightBrush { get; set; } = null!;
           
            /// <summary>
            /// Обработчик выгрузки.
            /// </summary>
            public RoutedEventHandler UnloadedHandler { get; set; } = null!;
        }

        private INotifyCollectionChanged _currentCollection;
        private readonly Queue<object> _pendingItems = new Queue<object>();
        private readonly Dictionary<DataGridRow, RowHighlightState> _highlightedRows = new Dictionary<DataGridRow, RowHighlightState>();
        private DispatcherTimer _flushTimer;
        private bool _flushScheduled;
        private bool _isDetached = true;

        public static readonly DependencyProperty HighlightColorProperty =
            DependencyProperty.Register(
                nameof(HighlightColor),
                typeof(Color),
                typeof(DataGridAutoScrollBehavior),
                new PropertyMetadata(Colors.LightGreen));

        public static readonly DependencyProperty AnimationDurationProperty =
            DependencyProperty.Register(
                nameof(AnimationDuration),
                typeof(Duration),
                typeof(DataGridAutoScrollBehavior),
                new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(700))));

        public static readonly DependencyProperty HighlightRetryCountProperty =
            DependencyProperty.Register(
                nameof(HighlightRetryCount),
                typeof(int),
                typeof(DataGridAutoScrollBehavior),
                new PropertyMetadata(3));

        public Color HighlightColor
        {
            get => (Color)GetValue(HighlightColorProperty);
            set => SetValue(HighlightColorProperty, value);
        }

        public Duration AnimationDuration
        {
            get => (Duration)GetValue(AnimationDurationProperty);
            set => SetValue(AnimationDurationProperty, value);
        }

        public int HighlightRetryCount
        {
            get => (int)GetValue(HighlightRetryCountProperty);
            set => SetValue(HighlightRetryCountProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            _isDetached = false;
            AssociatedObject.Loaded += OnLoaded;

            var descriptor = DependencyPropertyDescriptor.FromProperty(
                ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            descriptor.AddValueChanged(AssociatedObject, OnItemsSourceChanged);

            _flushTimer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(150)
            };
            _flushTimer.Tick += FlushTimerOnTick;

            if (AssociatedObject.IsLoaded)
            {
                SubscribeToCollection();
            }
        }

        protected override void OnDetaching()
        {
            _isDetached = true;
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;

            var descriptor = DependencyPropertyDescriptor.FromProperty(
                ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            descriptor.RemoveValueChanged(AssociatedObject, OnItemsSourceChanged);

            UnsubscribeFromCollection();
            ClearAllHighlights();

            if (_flushTimer != null)
            {
                _flushTimer.Stop();
                _flushTimer.Tick -= FlushTimerOnTick;
                _flushTimer = null;
            }

            _pendingItems.Clear();
            _flushScheduled = false;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_isDetached)
            {
                return;
            }

            SubscribeToCollection();
        }

        private void OnItemsSourceChanged(object sender, EventArgs e)
        {
            if (_isDetached)
            {
                return;
            }

            SubscribeToCollection();
        }

        private void SubscribeToCollection()
        {
            UnsubscribeFromCollection();

            if (AssociatedObject.ItemsSource is INotifyCollectionChanged collection)
            {
                _currentCollection = collection;
                _currentCollection.CollectionChanged += OnCollectionChanged;
            }
        }

        private void UnsubscribeFromCollection()
        {
            if (_currentCollection == null)
            {
                return;
            }

            _currentCollection.CollectionChanged -= OnCollectionChanged;
            _currentCollection = null;
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_isDetached)
            {
                return;
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _pendingItems.Clear();
                _flushScheduled = false;
                ClearAllHighlights();
                return;
            }

            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems == null || e.NewItems.Count == 0)
            {
                return;
            }

            foreach (var item in e.NewItems)
            {
                if (item != null)
                {
                    _pendingItems.Enqueue(item);
                }
            }

            if (_flushScheduled || _flushTimer == null)
            {
                return;
            }

            _flushScheduled = true;
            _flushTimer.Start();
        }

        private void FlushTimerOnTick(object sender, EventArgs e)
        {
            if (_isDetached)
            {
                _flushTimer.Stop();
                _flushScheduled = false;
                _pendingItems.Clear();
                return;
            }

            _flushTimer.Stop();
            _flushScheduled = false;

            if (_pendingItems.Count == 0 || AssociatedObject.Items.Count == 0)
            {
                return;
            }

            var batchItems = new List<object>();
            while (_pendingItems.Count > 0)
            {
                batchItems.Add(_pendingItems.Dequeue());
            }

            if (batchItems.Count == 0)
            {
                return;
            }

            var lastItem = batchItems[batchItems.Count - 1];
            if (lastItem == null)
            {
                return;
            }

            AssociatedObject.ScrollIntoView(lastItem);
            HighlightBatchRows(batchItems);
        }

        private void HighlightBatchRows(List<object> batchItems)
        {
            var startIndex = Math.Max(0, batchItems.Count - MaxHighlightedRowsPerBatch);
            var retryCount = Math.Max(0, HighlightRetryCount);
            for (var i = startIndex; i < batchItems.Count; i++)
            {
                HighlightSingleRow(batchItems[i], retryCount);
            }
        }

        private void HighlightSingleRow(object item, int attemptsLeft)
        {
            if (_isDetached)
            {
                return;
            }

            var row = AssociatedObject.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            if (row == null)
            {
                if (attemptsLeft > 0)
                {
                    AssociatedObject.Dispatcher.BeginInvoke(
                        new Action(() =>
                        {
                            if (_isDetached)
                            {
                                return;
                            }

                            HighlightSingleRow(item, attemptsLeft - 1);
                        }),
                        DispatcherPriority.Render);
                }

                return;
            }

            ClearRowHighlight(row, restoreOriginalBackground: true);

            var originalBackgroundLocalValue = row.ReadLocalValue(Control.BackgroundProperty);
            var highlightBrush = new SolidColorBrush(HighlightColor);
            RoutedEventHandler unloadedHandler = (_, _) => ClearRowHighlight(row, restoreOriginalBackground: true);
            row.Unloaded += unloadedHandler;

            _highlightedRows[row] = new RowHighlightState
            {
                OriginalBackgroundLocalValue = originalBackgroundLocalValue,
                HighlightBrush = highlightBrush,
                UnloadedHandler = unloadedHandler
            };

            row.Background = highlightBrush;

            var colorAnimation = new ColorAnimation
            {
                From = HighlightColor,
                To = Colors.Transparent,
                Duration = AnimationDuration,
                FillBehavior = FillBehavior.Stop
            };

            colorAnimation.Completed += (_, _) =>
            {
                ClearRowHighlight(row, restoreOriginalBackground: true);
            };

            highlightBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation, HandoffBehavior.SnapshotAndReplace);
        }

        private void ClearAllHighlights()
        {
            foreach (var row in new List<DataGridRow>(_highlightedRows.Keys))
            {
                ClearRowHighlight(row, restoreOriginalBackground: true);
            }
        }

        private void ClearRowHighlight(DataGridRow row, bool restoreOriginalBackground)
        {
            if (row == null || !_highlightedRows.TryGetValue(row, out var state))
            {
                return;
            }

            if (state.UnloadedHandler != null)
            {
                row.Unloaded -= state.UnloadedHandler;
            }

            state.HighlightBrush?.BeginAnimation(SolidColorBrush.ColorProperty, null);
            _highlightedRows.Remove(row);

            if (!restoreOriginalBackground || !ReferenceEquals(row.Background, state.HighlightBrush))
            {
                return;
            }

            if (state.OriginalBackgroundLocalValue == DependencyProperty.UnsetValue)
            {
                row.ClearValue(Control.BackgroundProperty);
                return;
            }

            row.SetValue(Control.BackgroundProperty, state.OriginalBackgroundLocalValue);
        }
    }
}
