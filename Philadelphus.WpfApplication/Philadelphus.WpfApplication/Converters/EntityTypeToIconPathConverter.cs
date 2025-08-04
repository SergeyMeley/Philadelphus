using Philadelphus.Business.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Philadelphus.WpfApplication.Converters
{
    public class EntityTypeToIconPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = @"C:\Users\%username%\Downloads";
            if (Path.Exists(path) ==false) 
            {
                path=Path.GetTempPath();
            }
            string fullPath = string.Empty;
            switch ((EntityTypes)value)
            {
                case EntityTypes.Repository:
                    fullPath = Path.Combine(path, "icons8_icon_repository.png");
                    break;
                case EntityTypes.Root:
                    fullPath = Path.Combine(path, "Flaticon_icon_root2.png");
                    break;
                case EntityTypes.Node:
                    fullPath = Path.Combine(path, "Flaticon_icon_node.png");
                    break;
                case EntityTypes.Leave:
                    fullPath = Path.Combine(path, "Flaticon_icon_leave.png");
                    break;
                default:
                    fullPath = Path.Combine(path, "Flaticon_icon_empty.png");
                    break; 
            }
            if (File.Exists(fullPath) == false)
            {
                fullPath = Path.Combine(path, "Flaticon_icon_empty.png");
                if (File.Exists(fullPath) == false)
                {
                    fullPath = Path.Combine(path, "icon_ERROR.png");
                    if (File.Exists(fullPath) == false)
                    {
                        using (Bitmap bmp = new Bitmap(10, 10))
                        {
                            bmp.Save(fullPath, ImageFormat.Png);
                        }
                    }
                }
            }
            return fullPath;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
