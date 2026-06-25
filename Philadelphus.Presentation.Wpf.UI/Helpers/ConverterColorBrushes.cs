using Philadelphus.Presentation.Enums;
using System.Windows.Media;

namespace Philadelphus.Presentation.Wpf.UI.Helpers
{
    /// <summary>
    /// Материализация <see cref="ConverterColor" /> в WPF-кисть (кэшированные Brushes).
    /// </summary>
    internal static class ConverterColorBrushes
    {
        public static Brush ToBrush(ConverterColor color)
            => color switch
            {
                ConverterColor.Black => Brushes.Black,
                ConverterColor.White => Brushes.White,
                ConverterColor.Green => Brushes.Green,
                ConverterColor.Red => Brushes.Red,
                ConverterColor.DarkRed => Brushes.DarkRed,
                ConverterColor.IndianRed => Brushes.IndianRed,
                ConverterColor.OrangeRed => Brushes.OrangeRed,
                ConverterColor.DeepPink => Brushes.DeepPink,
                ConverterColor.Cyan => Brushes.Cyan,
                ConverterColor.YellowGreen => Brushes.YellowGreen,
                _ => Brushes.Transparent
            };
    }
}
