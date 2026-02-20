using Philadelphus.Core.Domain.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    public class StateToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is State state)
            {
                switch (state)
                {
                    case State.Initialized:
                        return Brushes.DeepPink;
                        break;
                    case State.Changed:
                        return Brushes.Yellow;
                        break;
                    case State.SavedOrLoaded:
                        return Brushes.Green;
                        break;
                    case State.ForSoftDelete:
                        return Brushes.Red;
                        break;
                    case State.ForHardDelete:
                        return Brushes.OrangeRed;
                        break;
                    case State.SoftDeleted:
                        return Brushes.IndianRed;
                        break;
                    default:
                        break;
                }
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
