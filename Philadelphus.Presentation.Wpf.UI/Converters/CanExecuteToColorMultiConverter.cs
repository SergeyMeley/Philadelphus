using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    public class CanExecuteToColorMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length > 0 && values[0] is bool canExecute)
            {
                return canExecute ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
            }
            return System.Windows.Media.Brushes.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
