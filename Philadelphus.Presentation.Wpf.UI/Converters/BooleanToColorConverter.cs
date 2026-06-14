using Philadelphus.Presentation.Converters.Logic;
using System.Globalization;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    /// <summary>
    /// WPF-обёртка IValueConverter для bool/null → цвет (CanExecute и IsAvailable — идентичная семантика).
    /// Логика вынесена в <see cref="BooleanToColorLogic" />, материализация — в <see cref="ConverterColorBrushes" />.
    /// </summary>
    public class BooleanToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => ConverterColorBrushes.ToBrush(BooleanToColorLogic.ResolveColor(value));

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
