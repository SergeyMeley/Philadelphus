using System;
using System.Globalization;

using global::Avalonia.Data.Converters;

namespace Philadelphus.Presentation.Avalonia.Converters
{
    /// <summary>
    /// Связывает <see cref="DateTime"/>? (модель) с <see cref="DateTimeOffset"/>? (Avalonia
    /// <c>DatePicker.SelectedDate</c>). В WPF DatePicker работает с DateTime?, в Avalonia — с DateTimeOffset?.
    /// </summary>
    public sealed class NullableDateTimeToOffsetConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Unspecified трактуется как локальное время — без исключения для Utc/Local.
            return value is DateTime dateTime
                ? new DateTimeOffset(DateTime.SpecifyKind(dateTime, DateTimeKind.Unspecified))
                : (DateTimeOffset?)null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is DateTimeOffset dateTimeOffset
                ? dateTimeOffset.DateTime
                : (DateTime?)null;
        }
    }
}
