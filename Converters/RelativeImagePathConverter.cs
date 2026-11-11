using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace HouseholdGoodsIS.Converters;

public class RelativeImagePathConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var fallback = parameter as string ?? "pack://application:,,,/Resources/Images/product_placeholder.png";
        var path = value as string;
        try
        {
            if (string.IsNullOrWhiteSpace(path)) return Load(fallback);
            var fullPath = Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
            return File.Exists(fullPath) ? Load(fullPath) : Load(fallback);
        }
        catch
        {
            return Load(fallback);
        }
    }

    private static BitmapImage Load(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(path, UriKind.Absolute);
        image.EndInit();
        image.Freeze();
        return image;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

