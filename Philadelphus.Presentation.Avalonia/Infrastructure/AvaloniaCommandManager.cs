using global::Avalonia.Controls;
using global::Avalonia.Input;

namespace Philadelphus.Presentation.Avalonia.Infrastructure
{
    /// <summary>
    /// Avalonia-аналог WPF <c>CommandManager.RequerySuggested</c>. В Avalonia нет авто-переопроса
    /// доступности команд, поэтому переопрашиваем CanExecute по событиям ввода (клик/клавиша) —
    /// как делает WPF. Без этого команды с динамическим CanExecute (например, «Выбрать и начать
    /// работу!») не включаются после изменения состояния (выбора элемента и т.п.).
    /// </summary>
    internal static class AvaloniaCommandManager
    {
        private static bool _initialized;

        /// <summary>
        /// Событие, по которому команды переопрашивают свою доступность (аналог RequerySuggested).
        /// </summary>
        public static event EventHandler? RequerySuggested;

        /// <summary>
        /// Принудительно инициировать переопрос доступности всех команд.
        /// </summary>
        public static void RaiseRequerySuggested() => RequerySuggested?.Invoke(null, EventArgs.Empty);

        /// <summary>
        /// Подписаться на ввод приложения. Вызывается один раз при старте.
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            // Глобальные class-handler'ы на TopLevel (окне): переопрос после отпускания указателя
            // и нажатия клавиши — когда выбор/состояние уже обновлены (события дошли по bubble).
            // handledEventsToo — чтобы срабатывало даже если контрол обработал событие (например,
            // выбор в ListBox помечает указатель как handled).
            InputElement.PointerReleasedEvent.AddClassHandler<TopLevel>(
                (_, _) => RaiseRequerySuggested(),
                handledEventsToo: true);

            InputElement.KeyUpEvent.AddClassHandler<TopLevel>(
                (_, _) => RaiseRequerySuggested(),
                handledEventsToo: true);
        }
    }
}
