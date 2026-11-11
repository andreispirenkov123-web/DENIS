using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HouseholdGoodsIS.Converters;

public class StockToBrushConverter : IValueConverter
{
    private static readonly Brush Green = new SolidColorBrush(Color.FromRgb(0x10, 0x7C, 0x10));
    private static readonly Brush Orange = new SolidColorBrush(Color.FromRgb(0xCA, 0x50, 0x10));
    private static readonly Brush Red = new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C));

    static StockToBrushConverter()
    {
        Green.Freeze(); Orange.Freeze(); Red.Freeze();
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var stock = value is int i ? i : 0;
        if (stock <= 3) return Red;
        if (stock <= 7) return Orange;
        return Green;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
