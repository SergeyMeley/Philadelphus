using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Philadelphus.WpfApplication.Converters
{
    public class IsAvailableToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush result = Brushes.DarkRed;
            if (value == null)
                result = Brushes.Black;
            if (value is bool)
            {
                if ((bool)value)
                    result = Brushes.Green;
                else
                    result = Brushes.Red;
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
