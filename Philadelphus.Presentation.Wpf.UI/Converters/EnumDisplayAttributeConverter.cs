using Philadelphus.Core.Domain.Entities.Enums;
using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-конвертер значений DisplayAttribute для enum.
    /// </summary>
    public class EnumDisplayAttributeConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Enum enumValue)
            {
                return value?.ToString();
            }

            return parameter?.ToString() == "Description"
                ? enumValue.GetDisplayDescription()
                : enumValue.GetDisplayName();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
