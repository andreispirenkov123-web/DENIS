using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace HouseholdGoodsIS.Converters;

public class StatusToBrushConverter : IValueConverter
{
    private static readonly Brush Green = new SolidColorBrush(Color.FromRgb(0x10, 0x7C, 0x10));
    private static readonly Brush Blue = new SolidColorBrush(Color.FromRgb(0x00, 0x78, 0xD7));
    private static readonly Brush Orange = new SolidColorBrush(Color.FromRgb(0xCA, 0x50, 0x10));
    private static readonly Brush Red = new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C));
    private static readonly Brush Grey = new SolidColorBrush(Color.FromRgb(0x76, 0x76, 0x76));

    static StatusToBrushConverter()
    {
        Green.Freeze(); Blue.Freeze(); Orange.Freeze(); Red.Freeze(); Grey.Freeze();
    }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        (value as string) switch
        {
            "Выдан клиенту" or "Завершён" or "Оплачено" => Green,
            "В доставке" or "Оплачен" => Blue,
            "На таможне" or "Куплен" => Orange,
            "Отменён" or "Заблокирован" => Red,
            _ => Grey
        };

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
