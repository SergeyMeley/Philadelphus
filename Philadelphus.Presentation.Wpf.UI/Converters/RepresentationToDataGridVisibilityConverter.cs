using Philadelphus.Presentation.Wpf.UI.Models.Entities.Enums;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    public class RepresentationToDataGridVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((PropertyGridRepresentations)value == PropertyGridRepresentations.DataGrid)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
