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
        private static readonly string BaseUri = "pack://application:,,,/Philadelphus.Presentation.Wpf.UI;component/Icons/";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string iconPath = value switch
            {
                PhiladelphusRepositoryVM => "without_a_license/icons8_icon_repository.png",
                TreeRootVM => "without_a_license/Flaticon_icon_root.png",
                TreeNodeVM => "without_a_license/Flaticon_icon_node.png",
                TreeLeaveVM => "without_a_license/Flaticon_icon_leave.png",
                _ => "Flaticon_icon_empty.png"
            };

            var uri = new Uri(BaseUri + iconPath, UriKind.Absolute);
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = uri;
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
