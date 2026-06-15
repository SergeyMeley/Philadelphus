using global::Avalonia.Media;

using Philadelphus.Presentation.Enums;

namespace Philadelphus.Presentation.Avalonia.Helpers
{
    /// <summary>
    /// Материализация <see cref="ConverterColor" /> в Avalonia-кисть (кэшированные <see cref="Brushes" />).
    /// </summary>
    internal static class ConverterColorBrushes
    {
        public static IBrush ToBrush(ConverterColor color)
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
