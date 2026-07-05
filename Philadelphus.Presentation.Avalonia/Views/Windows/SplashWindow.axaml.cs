using global::Avalonia.Controls;
using global::Avalonia.Threading;

namespace Philadelphus.Presentation.Avalonia.Views.Windows
{
    /// <summary>
    /// Простое окно запуска с анимацией (название приложения + индикатор), показывается, пока
    /// поднимается Host. Не повторяет WPF SplashWindow — это лёгкая визуальная заглушка.
    /// </summary>
    public partial class SplashWindow : Window
    {
        private readonly DispatcherTimer _heartbeatTimer;
        private readonly TextBlock _elapsedText;
        private readonly TextBlock _statusText;
        private readonly DateTime _startedAt = DateTime.Now;

        public SplashWindow()
        {
            InitializeComponent();

            _elapsedText = this.GetControl<TextBlock>("ElapsedText");
            _statusText = this.GetControl<TextBlock>("StatusText");

            _heartbeatTimer = new DispatcherTimer(DispatcherPriority.Normal)
            {
                Interval = TimeSpan.FromMilliseconds(160),
            };

            _heartbeatTimer.Tick += OnHeartbeatTimerTick;
            _heartbeatTimer.Start();
            UpdateElapsedText();
        }

        public void SetStatus(string status)
        {
            _statusText.Text = string.IsNullOrWhiteSpace(status)
                ? "Загрузка…"
                : status;
            UpdateElapsedText();
        }

        protected override void OnClosed(EventArgs e)
        {
            _heartbeatTimer.Stop();
            _heartbeatTimer.Tick -= OnHeartbeatTimerTick;
            base.OnClosed(e);
        }

        private void OnHeartbeatTimerTick(object? sender, EventArgs e)
        {
            UpdateElapsedText();
        }

        private void UpdateElapsedText()
        {
            var seconds = (int)(DateTime.Now - _startedAt).TotalSeconds;
            _elapsedText.Text = seconds < 2
                ? "Подготовка приложения"
                : $"Подготовка приложения, {seconds} с";
        }
    }
}
