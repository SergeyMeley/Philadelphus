using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-конвертер значений для LastLaunchToDaysAgoConverter.
    /// </summary>
    public class LastLaunchToDaysAgoConverter : IValueConverter
    {
        /// <summary>
        /// Преобразует значение для Convert.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="targetType">Целевой тип преобразования.</param>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        /// <param name="culture">Культура преобразования.</param>
        /// <returns>Преобразованное значение.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is DateTime lastLaunch))
                return "никогда не запускался";

            // Если дата в будущем (ошибка в данных)
            if (lastLaunch > DateTime.Now)
                return "в будущем";

            var timeSpan = DateTime.Now - lastLaunch;
            int days = (int)timeSpan.TotalDays;

            return GetFormattedText(days);
        }

        private string GetFormattedText(int days)
        {
            if (days == 0)
            {
                var timeSpan = DateTime.Now - DateTime.Today;
                if (timeSpan.TotalHours < 1)
                    return "менее часа назад";
                else if (timeSpan.TotalHours < 24)
                    return "менее дня назад";
            }

            string daysWord = GetDaysWord(days);
            return $"{days} {daysWord} назад";
        }

        private string GetDaysWord(int days)
        {
            int lastDigit = days % 10;
            int lastTwoDigits = days % 100;

            // Исключения для 11-14
            if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
                return "дней";

            return lastDigit switch
            {
                1 => "день",
                2 => "дня",
                3 => "дня",
                4 => "дня",
                _ => "дней"
            };
        }

        /// <summary>
        /// Преобразует значение для ConvertBack.
        /// </summary>
        /// <param name="value">Значение.</param>
        /// <param name="targetType">Целевой тип преобразования.</param>
        /// <param name="parameter">Дополнительный параметр преобразования.</param>
        /// <param name="culture">Культура преобразования.</param>
        /// <returns>Преобразованное значение.</returns>
        /// <exception cref="NotImplementedException">Метод еще не реализован.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
