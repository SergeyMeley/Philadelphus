using Microsoft.Xaml.Behaviors;
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
    public class DataGridAutoScrollBehavior : Behavior<DataGrid>
    {
        private INotifyCollectionChanged _currentCollection;

        // Храним информацию о каждой анимируемой строке
        private class RowAnimationInfo
        {
            public DataGridRow Row { get; set; }
            public Brush OriginalBackground { get; set; }
            public DispatcherTimer Timer { get; set; }
        }

        private Dictionary<DataGridRow, RowAnimationInfo> _animatedRows = new Dictionary<DataGridRow, RowAnimationInfo>();

        // Свойства для настройки анимации
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
                new PropertyMetadata(new Duration(TimeSpan.FromMilliseconds(1500))));

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

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;

            var descriptor = DependencyPropertyDescriptor.FromProperty(
                ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            descriptor.AddValueChanged(AssociatedObject, OnItemsSourceChanged);

            if (AssociatedObject.IsLoaded)
            {
                SubscribeToCollection();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;

            var descriptor = DependencyPropertyDescriptor.FromProperty(
                ItemsControl.ItemsSourceProperty, typeof(DataGrid));
            descriptor.RemoveValueChanged(AssociatedObject, OnItemsSourceChanged);

            UnsubscribeFromCollection();
            ClearAllHighlights();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SubscribeToCollection();
        }

        private void OnItemsSourceChanged(object sender, EventArgs e)
        {
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
            if (_currentCollection != null)
            {
                _currentCollection.CollectionChanged -= OnCollectionChanged;
                _currentCollection = null;
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Collection changed: {e.Action}, NewItems count: {e.NewItems?.Count ?? 0}");

            // Анимация только для добавленных элементов
            if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach (var newItem in e.NewItems)
                {
                    var item = newItem;
                    AssociatedObject.Dispatcher.BeginInvoke(new System.Action(() =>
                    {
                        if (AssociatedObject.Items.Count > 0)
                        {
                            AssociatedObject.ScrollIntoView(item);

                            AssociatedObject.Dispatcher.BeginInvoke(new System.Action(() =>
                            {
                                HighlightNewRowWithAnimation(item);
                            }), DispatcherPriority.Render);
                        }
                    }), DispatcherPriority.Render);
                }
            }
            // При очистке коллекции убираем все выделения
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ClearAllHighlights();
            }
        }

        private void HighlightNewRowWithAnimation(object item)
        {
            var row = AssociatedObject.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;

            if (row != null)
            {
                // Проверяем, нет ли уже анимации на этой строке
                if (_animatedRows.ContainsKey(row))
                {
                    return;
                }

                // Сохраняем оригинальный фон
                var originalBackground = row.Background;

                // Создаем новый SolidColorBrush для анимации
                var highlightBrush = new SolidColorBrush(HighlightColor);
                row.Background = highlightBrush;

                // Создаем анимацию цвета
                var colorAnimation = new ColorAnimation
                {
                    From = HighlightColor,
                    To = Colors.Transparent,
                    Duration = AnimationDuration,
                    FillBehavior = FillBehavior.Stop
                };

                // Применяем анимацию
                highlightBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);

                // Создаем информацию о строке
                var animationInfo = new RowAnimationInfo
                {
                    Row = row,
                    OriginalBackground = originalBackground
                };

                // Добавляем в словарь
                _animatedRows[row] = animationInfo;

                // Запускаем таймер для восстановления фона
                var timer = new DispatcherTimer
                {
                    Interval = AnimationDuration.TimeSpan
                };

                timer.Tick += (s, e) =>
                {
                    timer.Stop();
                    if (_animatedRows.TryGetValue(row, out var info))
                    {
                        if (row != null)
                        {
                            // Останавливаем анимацию
                            if (row.Background is SolidColorBrush brush)
                            {
                                brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
                            }
                            // Восстанавливаем оригинальный фон
                            row.Background = info.OriginalBackground;
                        }
                        _animatedRows.Remove(row);
                    }
                };

                timer.Start();
                animationInfo.Timer = timer;
            }
            else
            {
                // Если строка еще не создана, ждем
                EventHandler handler = null;
                handler = (s, e) =>
                {
                    AssociatedObject.ItemContainerGenerator.StatusChanged -= handler;
                    HighlightNewRowWithAnimation(item);
                };

                AssociatedObject.ItemContainerGenerator.StatusChanged += handler;
            }
        }

        private void ClearAllHighlights()
        {
            foreach (var kvp in _animatedRows)
            {
                if (kvp.Key != null && kvp.Value.OriginalBackground != null)
                {
                    if (kvp.Key.Background is SolidColorBrush brush)
                    {
                        brush.BeginAnimation(SolidColorBrush.ColorProperty, null);
                    }
                    kvp.Key.Background = kvp.Value.OriginalBackground;
                }
                kvp.Value.Timer?.Stop();
            }
            _animatedRows.Clear();
        }
    }
}