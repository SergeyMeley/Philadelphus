using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    public class UtcToLocalTimeConverter : IValueConverter
    {
        public enum DateTimeStyle
        {
            // Российские форматы
            RussianShort,       // 02.04.2026
            RussianLong,        // 02 апреля 2026 г.
            RussianFull,        // 02.04.2026 15:30:45
            RussianTime,        // 15:30:45

            // ISO 8601 форматы
            Iso8601,            // 2026-04-02T15:30:45
            Iso8601WithZone,    // 2026-04-02T15:30:45+03:00
            Iso8601Utc,         // 2026-04-02T15:30:45Z
            Iso8601Date,        // 2026-04-02
            Iso8601Time,        // 15:30:45
            Iso8601Basic,       // 20260402T153045

            // Смешанные
            Auto,               // Автоматический выбор
            LikeIso8601WithZone // 2026-04-02 15:30:45 UTC+03:00
        }

        private readonly CultureInfo _russianCulture = new CultureInfo("ru-RU");

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                var localTime = dateTime.ToLocalTime();
                var utcTime = dateTime.ToUniversalTime();

                // Получаем стиль из параметра
                DateTimeStyle style = DateTimeStyle.Iso8601;
                if (parameter is string paramStr && Enum.TryParse<DateTimeStyle>(paramStr, true, out var parsedStyle))
                {
                    style = parsedStyle;
                }

                // Получаем смещение часового пояса
                var offset = TimeZoneInfo.Local.GetUtcOffset(localTime);
                string offsetString = offset.ToString(@"hh\:mm");
                string offsetWithSign = offset >= TimeSpan.Zero ? $"+{offsetString}" : offsetString;

                return style switch
                {
                    // Российские форматы
                    DateTimeStyle.RussianShort => localTime.ToString("dd.MM.yyyy", _russianCulture),
                    DateTimeStyle.RussianLong => localTime.ToString("dd MMMM yyyy 'г.'", _russianCulture),
                    DateTimeStyle.RussianFull => localTime.ToString("dd.MM.yyyy HH:mm:ss", _russianCulture),
                    DateTimeStyle.RussianTime => localTime.ToString("HH:mm:ss", _russianCulture),

                    // ISO 8601 форматы
                    DateTimeStyle.Iso8601 => localTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                    DateTimeStyle.Iso8601WithZone => $"{localTime:yyyy-MM-ddTHH:mm:ss}{offsetWithSign}",
                    DateTimeStyle.Iso8601Utc => $"{utcTime:yyyy-MM-ddTHH:mm:ss}Z",
                    DateTimeStyle.Iso8601Date => localTime.ToString("yyyy-MM-dd"),
                    DateTimeStyle.Iso8601Time => localTime.ToString("HH:mm:ss"),
                    DateTimeStyle.Iso8601Basic => localTime.ToString("yyyyMMddTHHmmss"),

                    DateTimeStyle.Auto => AutoFormat(localTime),
                    DateTimeStyle.LikeIso8601WithZone => $"{localTime:yyyy-MM-dd HH:mm:ss} UTC{offsetWithSign}",

                    _ => localTime.ToString("yyyy-MM-ddTHH:mm:ss")
                };
            }
            return value;
        }

        private string AutoFormat(DateTime dateTime)
        {
            // Автоматический выбор формата в зависимости от контекста
            if (dateTime.Date == DateTime.Today)
                return $"Сегодня в {dateTime:HH:mm}";
            if (dateTime.Date == DateTime.Today.AddDays(-1))
                return $"Вчера в {dateTime:HH:mm}";
            if (dateTime.Year == DateTime.Today.Year)
                return dateTime.ToString("dd MMMM HH:mm", _russianCulture);

            return dateTime.ToString("dd.MM.yyyy HH:mm", _russianCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && !string.IsNullOrEmpty(stringValue))
            {
                // Пробуем распарсить ISO 8601
                if (DateTime.TryParse(stringValue, null, DateTimeStyles.RoundtripKind, out var dateTime))
                {
                    return dateTime;
                }

                // Пробуем распарсить российские форматы
                if (DateTime.TryParse(stringValue, _russianCulture, DateTimeStyles.None, out dateTime))
                {
                    return dateTime;
                }
            }
            return DateTime.MinValue;
        }
    }
}
