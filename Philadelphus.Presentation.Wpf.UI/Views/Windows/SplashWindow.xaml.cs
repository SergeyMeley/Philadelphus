using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Philadelphus.Presentation.Wpf.UI.Views.Windows
{
    public partial class SplashWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private DispatcherTimer _dotTimer;
        private int _dotCount;
        private bool _isClosing = false;
        private readonly DispatcherPriority _animationPriority = DispatcherPriority.Background;

        public SplashWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
            Loaded += SplashWindow_Loaded;
        }

        private void SplashWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StartAnimations();
            // Запускаем инициализацию приложения (не ждём)
            _ = InitializeApplicationAsync();
        }

        private async Task InitializeApplicationAsync()
        {
            try
            {
                await UpdateLoadingTextAsync("Инициализация модулей...");
                await Task.Run(() => SimulateWork(2000));
                await Task.Delay(100);

                await UpdateLoadingTextAsync("Загрузка данных...");
                await Task.Run(() => SimulateWork(3000));
                await Task.Delay(100);

                await UpdateLoadingTextAsync("Подготовка интерфейса...");
                await Task.Run(() => SimulateWork(2500));
                await Task.Delay(100);

                await ProcessWithProgress(10, 800);

                await UpdateLoadingTextAsync("Запуск приложения...");

                // Даём анимации поработать перед созданием окна
                await Task.Delay(800);

                // Создаём окно с низким приоритетом, чтобы не блокировать анимацию
                LaunchWindow mainWindow = null;
                await Dispatcher.InvokeAsync(() =>
                {
                    mainWindow = _serviceProvider.GetRequiredService<LaunchWindow>();
                }, DispatcherPriority.Background); // Изменено на Background

                // Даём время на финальную отрисовку
                await Task.Delay(300);

                // Если окно создалось быстро, даём анимации ещё поработать
                if (!_isClosing)
                {
                    await Task.Delay(400);
                }

                CloseWithAnimation(mainWindow);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        // Заглушка для длительной работы (заменить на реальные вызовы)
        private void SimulateWork(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds);
        }

        private async Task ProcessWithProgress(int steps, int delayPerStep)
        {
            for (int i = 0; i < steps; i++)
            {
                int progress = (i + 1) * 10;

                // Обновляем текст
                await UpdateLoadingTextAsync($"Обработка... {progress}%");
                await Task.Delay(100); // время для анимации

                // Выполняем работу в фоне
                await Task.Run(() => SimulateWork(delayPerStep));

                // На последних шагах даём больше времени анимации
                if (progress >= 80)
                    await Task.Delay(300);
                else
                    await Task.Delay(50);
            }
        }

        private async Task UpdateLoadingTextAsync(string message)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                if (LoadingText != null && !_isClosing)
                {
                    LoadingText.Text = message;
                    _dotCount = 0;
                    // Принудительно обрабатываем очередь рендера
                    Dispatcher.Invoke(() => { }, DispatcherPriority.Render);
                }
            }, _animationPriority);
        }

        private void StartAnimations()
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (MainBorder == null || TitleText == null) return;

                // Анимация появления окна
                var fadeIn = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
                };
                var scaleX = new DoubleAnimation
                {
                    From = 0.8,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.6),
                    EasingFunction = new ElasticEase { Oscillations = 1, Springiness = 3 }
                };
                var scaleY = new DoubleAnimation
                {
                    From = 0.8,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.6),
                    EasingFunction = new ElasticEase { Oscillations = 1, Springiness = 3 }
                };

                if (!(MainBorder.RenderTransform is ScaleTransform))
                    MainBorder.RenderTransform = new ScaleTransform(0.8, 0.8);

                MainBorder.BeginAnimation(OpacityProperty, fadeIn, HandoffBehavior.Compose);
                MainBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleX, HandoffBehavior.Compose);
                MainBorder.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleY, HandoffBehavior.Compose);

                // Анимация заголовка
                if (!(TitleText.RenderTransform is TranslateTransform))
                    TitleText.RenderTransform = new TranslateTransform(0, 20);

                var titleFade = new DoubleAnimation
                {
                    From = 0,
                    To = 1,
                    Duration = TimeSpan.FromSeconds(0.5),
                    BeginTime = TimeSpan.FromSeconds(0.3)
                };
                var titleMove = new DoubleAnimation
                {
                    From = 20,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.5),
                    BeginTime = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut, Amplitude = 0.3 }
                };

                TitleText.BeginAnimation(OpacityProperty, titleFade, HandoffBehavior.Compose);
                TitleText.RenderTransform.BeginAnimation(TranslateTransform.YProperty, titleMove, HandoffBehavior.Compose);

                // Анимации спиннера, пульсация текста и точки
                StartSpinnerAnimations();
                StartPulseAnimation();
                StartDotAnimation();
            }), _animationPriority);
        }

        private void StartSpinnerAnimations()
        {
            if (RotateOuter == null || RotateInner == null || RotateDot == null) return;

            RotateOuter.BeginAnimation(RotateTransform.AngleProperty, null);
            RotateInner.BeginAnimation(RotateTransform.AngleProperty, null);
            RotateDot.BeginAnimation(RotateTransform.AngleProperty, null);

            var outer = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(2),
                RepeatBehavior = RepeatBehavior.Forever
            };
            var inner = new DoubleAnimation
            {
                From = 360,
                To = 0,
                Duration = TimeSpan.FromSeconds(1.5),
                RepeatBehavior = RepeatBehavior.Forever
            };
            var dot = new DoubleAnimation
            {
                From = 0,
                To = 360,
                Duration = TimeSpan.FromSeconds(1),
                RepeatBehavior = RepeatBehavior.Forever
            };

            RotateOuter.BeginAnimation(RotateTransform.AngleProperty, outer, HandoffBehavior.Compose);
            RotateInner.BeginAnimation(RotateTransform.AngleProperty, inner, HandoffBehavior.Compose);
            RotateDot.BeginAnimation(RotateTransform.AngleProperty, dot, HandoffBehavior.Compose);
        }

        private void StartPulseAnimation()
        {
            if (LoadingText == null) return;
            LoadingText.BeginAnimation(OpacityProperty, null);
            LoadingText.BeginAnimation(TextBlock.FontSizeProperty, null);

            var opacity = new DoubleAnimation
            {
                From = 0.4,
                To = 1.0,
                Duration = TimeSpan.FromSeconds(1.2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };
            var fontSize = new DoubleAnimation
            {
                From = 20,
                To = 22,
                Duration = TimeSpan.FromSeconds(1.2),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            LoadingText.BeginAnimation(OpacityProperty, opacity, HandoffBehavior.Compose);
            LoadingText.BeginAnimation(TextBlock.FontSizeProperty, fontSize, HandoffBehavior.Compose);
        }

        private void StartDotAnimation()
        {
            _dotTimer?.Stop();
            _dotTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _dotTimer.Tick += (s, e) =>
            {
                if (LoadingText != null && !_isClosing)
                {
                    _dotCount = (_dotCount + 1) % 4;
                    // Сохраняем основную часть текста (без точек)
                    string baseText = LoadingText.Text.Split('.')[0];
                    LoadingText.Text = baseText + new string('.', _dotCount);
                }
            };
            _dotTimer.Start();
        }

        public void UpdateLoadingText(string message)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (LoadingText != null && !_isClosing)
                {
                    LoadingText.Text = message;
                    _dotCount = 0;
                }
            }), _animationPriority);
        }

        public void UpdateProgress(int percent)
        {
            UpdateLoadingText($"Загрузка... {percent}%");
        }

        public void CloseWithAnimation(Window nextWindow)
        {
            if (_isClosing) return;
            _isClosing = true;

            _dotTimer?.Stop();

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromSeconds(0.5)
            };
            fadeOut.Completed += (s, e) =>
            {
                if (nextWindow != null)
                {
                    nextWindow.Topmost = true;
                    nextWindow.Show();
                    nextWindow.Activate();
                    nextWindow.Topmost = false;
                }
                Close();
            };

            MainBorder.BeginAnimation(OpacityProperty, fadeOut, HandoffBehavior.Compose);
        }

        protected override void OnClosed(EventArgs e)
        {
            _dotTimer?.Stop();
            _dotTimer = null;
            base.OnClosed(e);
        }
    }
}