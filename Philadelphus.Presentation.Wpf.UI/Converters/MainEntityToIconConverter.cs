using Philadelphus.Core.Domain.Entities.Enums;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs;
using Philadelphus.Presentation.Wpf.UI.ViewModels.EntitiesVMs.MainEntitiesVMs.RepositoryMembersVMs.RootMembersVMs;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Philadelphus.Presentation.Wpf.UI.Converters
{
    public class MainEntityToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = Environment.ExpandEnvironmentVariables(@"C:\Users\%USERNAME%\Downloads");
            if (Directory.Exists(path) == false)
                path = Path.GetTempPath();

            string fullPath = string.Empty;

            if (value is PhiladelphusRepositoryVM)
                fullPath = Path.Combine(path, "icons8_icon_repository.png");
            else if (value is TreeRootVM)
                fullPath = Path.Combine(path, "Flaticon_icon_root2.png");
            else if (value is TreeNodeVM)
                fullPath = Path.Combine(path, "Flaticon_icon_node.png");
            else if (value is TreeLeaveVM)
                fullPath = Path.Combine(path, "Flaticon_icon_leave.png");

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

            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
